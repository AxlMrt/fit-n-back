---
applyTo: "**"
---

# AI Knowledge Base - FitnessApp

## 🏗️ Architecture

**Stack:** .NET 9, ASP.NET Core, PostgreSQL, EF Core, Clean Architecture + DDD + Modular Monolith

**Modules Fonctionnels (6/12 implémentés):**

- ✅ **Authentication** - JWT + Refresh tokens, sécurité robuste
- ✅ **Users** - Profils complets, unités métrique/impérial, sync cross-module
- ✅ **Exercises** - Bibliothèque avec médias, groupes musculaires, équipements
- ✅ **Workouts** - Templates publics + workouts personnels, phases structurées
- ✅ **Content** - Gestion médias MinIO/Azure, transcodage vidéo, liaison exercices
- ✅ **Tracking** - Sessions, métriques utilisateur, synchronisation temps réel

**Modules en Attente (6/12):**

- 🔄 **Programs** - Plans d'entraînement multi-semaines (placeholder)
- 🔄 **Coach** - Recommandations IA basées sur performances (placeholder)
- 🔄 **Objectives** - Gestion objectifs SMART avec progression (placeholder)
- 🔄 **Notifications** - Système d'engagement et rappels (placeholder)
- 🔄 **Payments** - Abonnements Stripe/PayPal (placeholder)
- 🔄 **Authorization** - Policies avancées et claims (partiel)

**Communication:** MediatR events pour découplage cross-module

## 🧪 Tests - État VALIDÉ (181+ tests ✅)

### Tests d'Intégration HTTP (102+ tests)

- ✅ **AuthenticationHttpIntegrationTests** (19) - JWT, refresh tokens, login/logout
- ✅ **ExerciseHttpIntegrationTests** (14) - CRUD, permissions admin/user
- ✅ **WorkoutHttpIntegrationTests** (16) - Templates + workouts personnels
- ✅ **UserProfileHttpIntegrationTests** (18) - Profils, mesures physiques
- ✅ **UserTrackingSynchronizationHttpTests** (11) - Sync cross-module
- ✅ **TrackingHttpIntegrationTests** (15) - Métriques utilisateur
- ✅ **MediatREventPropagationTests** (3) - Communication inter-modules
- ✅ **InfrastructureTests** (8) - Health checks, DB

### Tests d'Intégration Restants

- ❌ **Performance Tests** - Tests sous charge et temps de réponse
- ❌ **Robustness Tests** - Gestion des échecs partiels, concurrence

### Tests Unitaires (79+ tests ✅)

- ✅ **ExerciseTests** (61) - Domain + Application + Infrastructure
- ✅ **WorkoutTests** (14) - Domain + services
- ✅ **ContentTests** (4) - Gestion médias

### Tests Unitaires Manquants

- ❌ **AuthenticationTests** (0) - Services, validation, sécurité
- ❌ **UserTests** (0) - Profils, conversions unités
- ❌ **TrackingTests** (0) - Métriques, synchronisation

---

## 🔧 Fonctionnalités Principales

### Authentication & Users

- **JWT + Refresh tokens** avec rotation automatique et révocation
- **Profils utilisateur** avec unités métrique/impérial préservées
- **Synchronisation cross-module** Users → Tracking via MediatR
- **Validation robuste** avec FluentValidation et règles métier
- **Sécurité** : BCrypt hashing, token validation middleware

### Exercises & Workouts

- **Bibliothèque d'exercices** avec médias (images/vidéos via Content)
- **Templates publics** (gérés par admins) + workouts personnels
- **Permissions strictes** : Admin CRUD complet, User lecture + création personnelle
- **Structure phases** : échauffement, effort, récupération
- **Workouts types** : Template, UserCreated, AIGenerated (préparé)

### Tracking & Content

- **Métriques utilisateur** avec conversion automatique d'unités
- **Gestion médias** via MinIO/Azure Blob Storage
- **Transcodage vidéo** automatique via background workers
- **Événements cross-module** pour synchronisation temps réel
- **Sessions workout** avec ExerciseSets et UserMetrics

---

## 🧪 Découvertes Techniques Importantes

### Pattern Tests HTTP Authentiques

```csharp
// ✅ Pattern validé - JSON raw strings
var request = """{"name": "HIIT", "type": "Template"}""";
await Client.PostAsync("/api/v1/workouts", new StringContent(request, UTF8, "application/json"));

// ❌ Anti-pattern - objets C# typés
await Client.PostAsJsonAsync("/api/v1/workouts", new { name = "HIIT" });
```

### Culture-Invariant Formatting

```csharp
// ✅ Solution validée pour JSON
value.ToString(CultureInfo.InvariantCulture) // 15.5 (pas 15,5)
```

### Communication Cross-Module

```csharp
// ✅ Pattern MediatR validé
await _mediator.Publish(new PhysicalMeasurementsUpdatedEvent(...));
```

### Logging Architecture Consolidé

```csharp
// ✅ Pattern validé - Services avec ILogger<T>
public class UserProfileService : IUserProfileService
{
    private readonly ILogger<UserProfileService> _logger;

    // Logs d'affaires avec contexte structuré
    _logger.LogInformation("User profile created successfully. UserId: {UserId}, FitnessLevel: {FitnessLevel}",
        userId, profileDto.FitnessLevel);
}
```

---

## ✨ Fonctionnalités Existantes

### Module Authentication

- **JWT Authentication** avec refresh tokens et rotation
- **Role-based authorization** (User/Admin) + claims personnalisés
- **Token revocation** via distributed cache
- **Password hashing** sécurisé (BCrypt)
- **Validation robuste** avec FluentValidation

### Module Users

- **Profils complets** (âge, sexe, taille, poids, niveau fitness)
- **Unités de mesure personnalisables** (métrique/impérial pour hauteur/poids)
- **Synchronisation cross-module** automatique via événements MediatR
- **Préférences personnalisables** (durée entraînement, matériel, etc.)
- **Système d'abonnements** (freemium/premium)
- **RGPD compliance** (export, suppression données)

### Module Exercises

- **Bibliothèque d'exercices** avec métadonnées complètes
- **Groupes musculaires** et **équipements requis**
- **Contenus multimédia** (images/vidéos via Content module)
- **Catégorisation** par type et difficulté
- **Search et filtres** avancés

### Module Workouts

- **Workouts structurés** en phases (échauffement, effort, récupération)
- **Templates publics** (gérés par admins) vs **Workouts utilisateur**
- **Types multiples** : Template, UserCreated, AIGenerated
- **Gestion complète** : CRUD avec permissions
- **Association exercices** avec sets/reps/poids/durée

### Module Content

- **Gestion centralisée** des médias (images, vidéos)
- **Upload et stockage** via MinIO/Azure
- **Transcodage vidéo** automatique via background workers
- **Liaison exercices ↔ médias** avec table de jonction

### Module Tracking

- **Sessions workout** complètes avec start/end
- **ExerciseSets** détaillés (reps, poids, durée)
- **UserMetrics** avec conversion d'unités automatique
- **Synchronisation cross-module** via MediatR events

---

## 💾 Base de Données et Modèles

### Structure PostgreSQL par Module

```sql
-- Schémas séparés par domaine métier
auth.AuthUsers, auth.RefreshTokens
users.UserProfiles, users.Preferences, users.Subscriptions
exercises.Exercises
workouts.Workouts, workouts.WorkoutPhases, workouts.WorkoutExercises
content.MediaAssets, content.exercise_media_assets
tracking.WorkoutSessions, tracking.ExerciseSets, tracking.UserMetrics
```

### Relations Cross-Module Clés

```
Users ←→ Workouts (créateur/propriétaire)
Workouts ←→ Exercises (via WorkoutExercises)
Exercises ←→ MediaAssets (images/vidéos)
Users ←→ WorkoutSessions (via Tracking)
WorkoutSessions ←→ ExerciseSets (détails performance)
```

---

## 🔥 Logging Architecture - Phase 2 TERMINÉE ✅

### Architecture Consolidée

- **Controllers** : Orchestration uniquement, pas de try/catch redondants
- **Services** : Logs d'affaires avec ILogger<T> et contexte structuré
- **Middleware** : GlobalExceptionMiddleware pour gestion globale des erreurs

### Services avec Logging Intégré

#### UserProfileService ✅

- **Logs ajoutés** : 8 logs d'affaires stratégiques
- **Couverture** : CreateUserProfileAsync, UpdatePersonalInfoAsync, UpdatePhysicalMeasurementsAsync, UpdateFitnessProfileAsync, DeleteUserProfileAsync
- **Contexte** : UserId, FitnessLevel, FitnessGoal, Height, Weight

#### WorkoutService ✅

- **Logs ajoutés** : 5 logs d'affaires essentiels
- **Couverture** : CreateUserWorkoutAsync, UpdateUserWorkoutAsync, DeleteUserWorkoutAsync, CreateTemplateWorkoutAsync
- **Contexte** : UserId, WorkoutId, WorkoutType

#### MediaAssetService ✅

- **Logs ajoutés** : 8 logs d'affaires complets
- **Couverture** : UploadAsync, DownloadAsync, GetByExerciseIdAsync, DeleteAsync
- **Contexte** : AssetId, ExerciseId, StorageKey, FileName

---

## 📈 Priorités de Développement

### 🎯 Modules Suivants (Priorité Haute)

- [ ] **Programs** - Plans d'entraînement multi-semaines
- [ ] **Coach** - Recommandations IA basées sur performances
- [ ] **Objectives** - Gestion objectifs SMART
- [ ] **Notifications** - Système d'engagement
- [ ] **Payments** - Abonnements, billing
- [ ] **API** - Documentation Swagger complète

### 🔄 Tests Restants à Implémenter

- ❌ **Tests unitaires** Authentication, Users, Tracking (0 tests chacun)
- ❌ **Performance Tests** - Tests sous charge et temps de réponse
- ❌ **Robustness Tests** - Gestion des échecs partiels, concurrence

### 🚀 Roadmap Moyen Terme

- [ ] **Performance optimizations** (caching Redis, pagination)
- [ ] **Notifications push** et système d'engagement
- [ ] **Système de payments** Stripe/PayPal
- [ ] **Analytics dashboard** pour admins
- [ ] **Mobile app** React Native/Flutter
- [ ] **Déploiement cloud** Azure/AWS avec CI/CD

### 💡 Idées et Améliorations

- [ ] **Workouts en temps réel** avec WebSockets
- [ ] **IA conversationnelle** pour coaching personnalisé
- [ ] **Social features** (partage workouts, défis)
- [ ] **Marketplace exercices** communautaire
- [ ] **Intégrations wearables** (Apple Health, Garmin)

---

## 📝 Notes pour Futurs Chats

### Context Important à Rappeler

1. **Architecture modulaire stricte** - Pas de couplage direct entre modules
2. **Tests HTTP authentiques** - Toujours simuler vrais utilisateurs avec JSON
3. **Clean Architecture** - Respecter les couches Domain/Application/Infrastructure
4. **PostgreSQL schémas** - Chaque module a son schéma dédié
5. **JWT + Refresh tokens** - Sécurité robuste avec rotation
6. **Logging consolidé** - ILogger<T> dans services, pas de try/catch controllers

### Anti-Patterns à Éviter

- ❌ Couplage direct entre modules (utiliser médiation)
- ❌ Tests avec objets C# typés (utiliser JSON strings)
- ❌ Logique métier dans les controllers
- ❌ Accès direct à la BDD depuis d'autres modules
- ❌ JSON échappé `\"` dans raw strings `"""` (cause BadRequest 400)
- ❌ Try/catch redondants dans controllers (utiliser GlobalExceptionMiddleware)

### Patterns Validés à Réutiliser

- ✅ HTTP Client integration tests avec TestContainers
- ✅ Module registration via extension methods
- ✅ JWT + Refresh token avec policies
- ✅ Repository pattern avec EF Core
- ✅ Communication cross-module via événements MediatR
- ✅ Conversion unités de mesure avec services dédiés
- ✅ DTOs avec paramètres optionnels pour unités
- ✅ Logging structuré avec ILogger<T> dans services
- ✅ GlobalExceptionMiddleware pour gestion d'erreurs centralisée

---

_Dernière mise à jour : Septembre 2025_
_Version : v2.0 - Architecture consolidée avec logging intégré et 6 modules opérationnels_
