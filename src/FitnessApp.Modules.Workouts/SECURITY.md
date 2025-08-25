# Guide d'Intégration Sécurisé - Module Workouts

## 🔐 Sécurité et Autorisations

Le module Workouts intègre maintenant un système de sécurité robuste qui garantit que :
- **Les utilisateurs ne peuvent modifier/supprimer que leurs propres workouts**
- **Les workouts systèmes sont protégés**
- **L'accès est contrôlé selon le type de workout**

## 🚀 Configuration avec Sécurité

### 1. Configuration dans Program.cs

```csharp
using FitnessApp.Modules.Workouts;

var builder = WebApplication.CreateBuilder(args);

// Configuration des services (avec authentification)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        // Configuration JWT...
    });

builder.Services.AddAuthorization();

// Ajout du module Workouts (avec sécurité intégrée)
builder.Services.AddWorkoutsModule(connectionString);

var app = builder.Build();

// Configuration du pipeline
app.UseAuthentication(); // IMPORTANT: Avant UseWorkoutsModule
app.UseAuthorization();
app.UseWorkoutsModule();

app.MapControllers();
app.Run();
```

### 2. Configuration des Claims JWT

Assurez-vous que vos tokens JWT contiennent l'ID utilisateur :

```csharp
// Exemples de claims à inclure
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // ID utilisateur
    new Claim("sub", userId.ToString()),                     // Alternative standard
    new Claim("user_id", userId.ToString()),                 // Alternative personnalisée
    new Claim(ClaimTypes.Role, "User")                       // Rôles si nécessaire
};
```

## 🔒 Règles d'Autorisation

### Types de Workouts et Permissions

| Type de Workout | Créateur | Peut Voir | Peut Modifier | Peut Supprimer |
|-----------------|----------|-----------|---------------|----------------|
| **Dynamic** | Système | Tous | Aucun | Aucun |
| **Fixed (System)** | Système | Tous | Aucun | Aucun |
| **Fixed (Coach)** | Coach | Tous | Coach créateur | Coach créateur |
| **UserCreated** | Utilisateur | Créateur uniquement | Créateur uniquement | Créateur uniquement |

### Règles de Duplication
- ✅ **Tout workout visible peut être dupliqué**
- ✅ **Le workout dupliqué devient un `UserCreated` pour l'utilisateur**
- ✅ **L'utilisateur devient propriétaire de la copie**

## 📡 Réponses d'Erreur d'Autorisation

Le système retourne des erreurs claires :

```json
{
  "message": "You don't have permission to modify this workout. You can only modify workouts you created."
}
```

```json
{
  "message": "You don't have permission to delete this workout. You can only delete workouts you created."
}
```

```json
{
  "message": "You must be authenticated to create workouts"
}
```

## 🛡️ Exemples d'Usage Sécurisé

### 1. Création d'un Workout Utilisateur

```http
POST /api/workouts
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "name": "Mon Workout Perso",
  "description": "Workout créé par moi",
  "type": "UserCreated",
  "difficulty": "Beginner",
  "estimatedDurationMinutes": 30,
  "requiredEquipment": "None"
}
```

### 2. Tentative de Modification (Autorisée)

```http
PUT /api/workouts/{workout-id-created-by-user}
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "name": "Mon Workout Modifié",
  "difficulty": "Intermediate"
}
```

### 3. Tentative de Modification (Non Autorisée)

```http
PUT /api/workouts/{workout-id-created-by-another-user}
Authorization: Bearer {jwt-token}

# Retourne 400 Bad Request
{
  "message": "You don't have permission to modify this workout. You can only modify workouts you created."
}
```

### 4. Duplication d'un Workout Système

```http
POST /api/workouts/{system-workout-id}/duplicate
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "newName": "Ma Version du Workout HIIT"
}

# Le workout dupliqué devient un UserCreated pour l'utilisateur connecté
```

## 🔧 Intégration avec le Frontend

### Vérification côté client (optionnelle)

```typescript
interface WorkoutDto {
  id: string;
  name: string;
  type: 'Dynamic' | 'Fixed' | 'UserCreated';
  createdByUserId?: string;
  createdByCoachId?: string;
  // ... autres propriétés
}

class WorkoutService {
  private currentUserId: string;

  // Vérifications côté client (la sécurité reste côté serveur)
  canModifyWorkout(workout: WorkoutDto): boolean {
    return workout.type === 'UserCreated' && 
           workout.createdByUserId === this.currentUserId;
  }

  canDeleteWorkout(workout: WorkoutDto): boolean {
    return this.canModifyWorkout(workout);
  }

  canDuplicateWorkout(workout: WorkoutDto): boolean {
    // Tous les workouts visibles peuvent être dupliqués
    return true;
  }
}
```

### Gestion des erreurs côté client

```typescript
async function updateWorkout(id: string, updateData: any) {
  try {
    const response = await fetch(`/api/workouts/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${getJwtToken()}`
      },
      body: JSON.stringify(updateData)
    });

    if (!response.ok) {
      const error = await response.json();
      
      if (response.status === 400 && error.message?.includes('permission')) {
        showErrorMessage('Vous ne pouvez modifier que vos propres workouts');
        return;
      }
      
      throw new Error('Erreur lors de la mise à jour');
    }

    const updatedWorkout = await response.json();
    return updatedWorkout;
  } catch (error) {
    console.error('Erreur:', error);
    showErrorMessage('Impossible de mettre à jour le workout');
  }
}
```

## 🧪 Tests de Sécurité

### Test Cases Importants

```csharp
[Test]
public async Task UpdateWorkout_UserTrysToModifyAnotherUsersWorkout_ShouldThrowUnauthorized()
{
    // Arrange
    var workout = CreateUserWorkout(otherUserId);
    var currentUserId = Guid.NewGuid();
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<WorkoutDomainException>(
        () => workoutService.UpdateWorkoutAsync(workout.Id, updateDto));
        
    Assert.Contains("permission", exception.Message);
}

[Test]
public async Task DuplicateWorkout_ShouldCreateUserCreatedWorkoutForCurrentUser()
{
    // Arrange
    var systemWorkout = CreateSystemWorkout();
    var currentUserId = Guid.NewGuid();
    
    // Act
    var duplicated = await workoutService.DuplicateWorkoutAsync(
        systemWorkout.Id, "Ma Copie");
    
    // Assert
    Assert.Equal(WorkoutType.UserCreated, duplicated.Type);
    Assert.Equal(currentUserId, duplicated.CreatedByUserId);
}
```

## 📝 Résumé des Améliorations de Sécurité

✅ **Service d'autorisation** : `IWorkoutAuthorizationService`  
✅ **Service utilisateur courant** : `ICurrentUserService`  
✅ **Validation dans tous les endpoints** de modification/suppression  
✅ **Duplication sécurisée** avec changement de propriétaire  
✅ **Messages d'erreur clairs** pour les violations de sécurité  
✅ **Intégration transparente** avec l'authentification JWT  
✅ **Tests unitaires** pour valider les règles de sécurité

Le module est maintenant **production-ready** avec un système de sécurité robuste ! 🎉
