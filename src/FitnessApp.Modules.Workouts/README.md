# FitnessApp.Modules.Workouts

Module de gestion des séances d'entraînement (workouts) pour l'application fitness.

## 🎯 Objectifs

Ce module gère la création, le stockage et la manipulation des workouts avec les fonctionnalités suivantes :

- **Types de workouts** :
  - Dynamiques : Générés automatiquement selon le profil utilisateur
  - Fixes : Préconfigurés par les coachs ou le système
  - Créés par l'utilisateur : Workouts personnalisés

- **Structure hiérarchique** :
  - Workout → Phases (Échauffement, Effort principal, Récupération) → Exercices
  - Chaque niveau peut être organisé, modifié et réordonné

- **Métadonnées riches** :
  - Niveau de difficulté (Débutant à Expert)
  - Durée estimée et calculée
  - Équipement requis (Flags enum pour combinations)
  - Paramètres d'exercice (répétitions, séries, poids, temps de repos)

## 🏗️ Architecture

Le module suit les principes de l'architecture DDD (Domain-Driven Design) :

### Domain Layer
- **Entités** : `Workout`, `WorkoutPhase`, `WorkoutExercise`
- **Value Objects** : `Duration`, `ExerciseParameters`
- **Enums** : `WorkoutType`, `DifficultyLevel`, `WorkoutPhaseType`, `EquipmentType`
- **Repository Interfaces** : `IWorkoutRepository`
- **Exceptions** : `WorkoutDomainException`

### Application Layer
- **Services** : `IWorkoutService`, `WorkoutService`
- **DTOs** : Create/Update/Query DTOs pour toutes les entités
- **Validators** : FluentValidation pour toutes les opérations

### Infrastructure Layer
- **DbContext** : `WorkoutsDbContext` avec configurations EF Core
- **Repository** : `WorkoutRepository` (implémentation Entity Framework)
- **Migrations** : Gestion automatique du schéma de base de données

### API Layer
- **Controllers** : `WorkoutsController` avec endpoints REST complets

## 🚀 Fonctionnalités

### Gestion des Workouts
- ✅ CRUD complet (Create, Read, Update, Delete)
- ✅ Recherche et filtrage avancés
- ✅ Pagination
- ✅ Duplication de workouts
- ✅ Activation/Désactivation

### Gestion des Phases
- ✅ Ajout/Suppression de phases
- ✅ Réorganisation (drag & drop logique)
- ✅ Types prédéfinis (WarmUp, MainEffort, Recovery, Stretching)

### Gestion des Exercices dans les Phases
- ✅ Ajout/Suppression d'exercices
- ✅ Paramètres flexibles (reps, sets, duration, weight, rest time)
- ✅ Réorganisation au sein d'une phase
- ✅ Intégration avec le module Exercises

## 🔧 Installation et Configuration

### 1. Ajout du module dans Program.cs

```csharp
using FitnessApp.Modules.Workouts;

// Configuration des services
builder.Services.AddWorkoutsModule(connectionString);

// Configuration du pipeline
app.UseWorkoutsModule();
```

### 2. Migrations

```bash
# Générer une migration
dotnet ef migrations add MigrationName --project FitnessApp.Modules.Workouts

# Appliquer les migrations (fait automatiquement au démarrage)
dotnet ef database update --project FitnessApp.Modules.Workouts
```

## 📡 API Endpoints

### Workouts
- `GET /api/workouts` - Liste paginée avec filtres
- `GET /api/workouts/{id}` - Détail d'un workout
- `POST /api/workouts` - Création
- `PUT /api/workouts/{id}` - Mise à jour
- `DELETE /api/workouts/{id}` - Suppression
- `POST /api/workouts/{id}/duplicate` - Duplication
- `POST /api/workouts/{id}/deactivate` - Désactivation
- `POST /api/workouts/{id}/reactivate` - Réactivation

### Workouts par Utilisateur/Coach
- `GET /api/workouts/user/{userId}` - Workouts d'un utilisateur
- `GET /api/workouts/coach/{coachId}` - Workouts d'un coach

### Phases
- `POST /api/workouts/{workoutId}/phases` - Ajouter une phase
- `PUT /api/workouts/{workoutId}/phases/{phaseId}` - Modifier une phase
- `DELETE /api/workouts/{workoutId}/phases/{phaseId}` - Supprimer une phase
- `PUT /api/workouts/{workoutId}/phases/{phaseId}/move` - Réorganiser

### Exercices dans les Phases
- `POST /api/workouts/{workoutId}/phases/{phaseId}/exercises` - Ajouter un exercice
- `PUT /api/workouts/{workoutId}/phases/{phaseId}/exercises/{exerciseId}` - Modifier
- `DELETE /api/workouts/{workoutId}/phases/{phaseId}/exercises/{exerciseId}` - Supprimer
- `PUT /api/workouts/{workoutId}/phases/{phaseId}/exercises/{exerciseId}/move` - Réorganiser

## 📊 Structure de Base de Données

### Tables
- `workouts.workouts` - Table principale des workouts
- `workouts.workout_phases` - Phases d'un workout
- `workouts.workout_exercises` - Exercices dans une phase

### Index
- Index sur les champs de recherche fréquents (type, difficulté, équipement)
- Index composites pour les requêtes optimisées
- Contraintes d'unicité sur les ordres dans les hiérarchies

## 🔗 Intégrations

### Modules Dépendants
- **FitnessApp.Modules.Exercises** : Référence aux exercices de la bibliothèque
- **FitnessApp.SharedKernel** : Types partagés et utilitaires

### Modules Intégrés (à venir)
- **FitnessApp.Modules.Users** : Assignation de workouts aux utilisateurs
- **FitnessApp.Modules.Tracking** : Suivi des séances réalisées
- **FitnessApp.Modules.Coach** : Création par les coachs
- **FitnessApp.Modules.Content** : Images et vidéos des workouts

## 🧪 Tests

Les tests unitaires et d'intégration se trouvent dans :
- `tests/FitnessApp.Modules.Workouts.Tests/`

## 📝 Exemples d'Usage

### Création d'un Workout Complet

```csharp
var createDto = new CreateWorkoutDto(
    Name: "HIIT Full Body",
    Description: "High-intensity interval training for full body",
    Type: WorkoutType.Fixed,
    Difficulty: DifficultyLevel.Intermediate,
    EstimatedDurationMinutes: 45,
    RequiredEquipment: EquipmentType.None,
    Phases: new List<CreateWorkoutPhaseDto>
    {
        new(
            Type: WorkoutPhaseType.WarmUp,
            Name: "Échauffement",
            EstimatedDurationMinutes: 5,
            Exercises: new List<CreateWorkoutExerciseDto>
            {
                new(ExerciseId: Guid.NewGuid(), ExerciseName: "Jumping Jacks", DurationSeconds: 30, Sets: 2)
            }
        ),
        new(
            Type: WorkoutPhaseType.MainEffort,
            Name: "Effort Principal",
            EstimatedDurationMinutes: 35
        )
    }
);

var workout = await workoutService.CreateWorkoutAsync(createDto);
```

### Filtrage et Recherche

```csharp
var query = new WorkoutQueryDto
{
    Type = WorkoutType.Fixed,
    Difficulty = DifficultyLevel.Beginner,
    Equipment = EquipmentType.None,
    MaxDurationMinutes = 30,
    SearchTerm = "cardio",
    Page = 1,
    PageSize = 10
};

var results = await workoutService.GetWorkoutsAsync(query);
```

## 🚀 Extensibilité

Le module est conçu pour être facilement extensible :

- **Nouveaux types d'équipement** : Ajout de valeurs dans l'enum `EquipmentType`
- **Nouvelles phases** : Extension de l'enum `WorkoutPhaseType`
- **Paramètres personnalisés** : Extension du Value Object `ExerciseParameters`
- **Algorithmes de génération** : Implémentation d'`IWorkoutGenerationService`

## 🛡️ Validation et Sécurité

- Validation complète avec FluentValidation
- Contraintes métier dans les entités du domaine
- Gestion d'erreurs typées avec exceptions personnalisées
- Isolation des données par utilisateur/coach
