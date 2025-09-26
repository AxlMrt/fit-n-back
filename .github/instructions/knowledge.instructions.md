---
applyTo: "**"
---

# AI Knowledge Base - FitnessApp

## 🏗️ Architecture

**Stack:** .NET 9, ASP.NET Core, PostgreSQL, EF Core, Clean Architecture + DDD + Modular Monolith

**Modules Fonctionnels (6/12):**

- ✅ Authentication, Users, Exercises, Workouts, Content, Tracking
- 🔄 Programs, Coach, Objectives, Notifications, Payments, API

**Communication:** MediatR events pour découplage cross-module

## 🧪 Tests - État VALIDÉ (102/102 ✅)

### Tests d'Intégration HTTP (102 tests)

- ✅ AuthenticationHttpIntegrationTests (19) - JWT, refresh tokens
- ✅ ExerciseHttpIntegrationTests (14) - CRUD, permissions admin/user
- ✅ WorkoutHttpIntegrationTests (16) - Templates + workouts personnels
- ✅ UserProfileHttpIntegrationTests (18) - Profils, mesures physiques
- ✅ UserTrackingSynchronizationHttpTests (11) - Sync cross-module
- ✅ TrackingHttpIntegrationTests (15) - Métriques utilisateur
- ✅ MediatREventPropagationTests (3) - Communication inter-modules
- ✅ InfrastructureTests (8) - Health checks, DB

### Tests d'Intégration Restants

- ❌ **User Journey Tests** - Parcours utilisateur complets end-to-end
- ❌ **Cross-Module Tests** - Communication et synchronisation inter-modules
- ❌ **Performance Tests** - Tests sous charge et temps de réponse
- ❌ **Robustness Tests** - Gestion des échecs partiels, concurrence

### Tests Unitaires (79/79 ✅)

- ✅ ExerciseTests (61) - Domain + Application + Infrastructure
- ✅ WorkoutTests (14) - Domain + services
- ✅ ContentTests (4) - Gestion médias

### Tests Unitaires Manquants

- ❌ **AuthenticationTests** (0) - Services, validation, sécurité
- ❌ **UserTests** (0) - Profils, conversions unités
- ❌ **TrackingTests** (0) - Métriques, synchronisation

---

## 🔧 Fonctionnalités Principales

### Authentication & Users

- **JWT + Refresh tokens** avec policies de sécurité
- **Profils utilisateur** avec unités métrique/impérial préservées
- **Synchronisation cross-module** Users → Tracking via MediatR

### Exercises & Workouts

- **Bibliothèque d'exercices** avec médias (images/vidéos)
- **Templates publics** (gérés par admins) + workouts personnels
- **Permissions** : Admin CRUD complet, User lecture + création personnelle

### Tracking & Content

- **Métriques utilisateur** avec conversion automatique d'unités
- **Gestion médias** via MinIO/Azure Blob Storage
- **Événements cross-module** pour synchronisation temps réel

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

---

## ✨ Fonctionnalités Existantes

### Module Authentication

- **JWT Authentication** avec refresh tokens
- **Role-based authorization** (User/Admin)
- **Email confirmation** et 2FA (préparé)
- **Password hashing** sécurisé (Argon2)

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

### Module Workouts

- **Workouts structurés** en phases (échauffement, effort, récupération)
- **Templates publics** (gérés par admins) vs **Workouts utilisateur**
- **Types multiples** : Template, UserCreated, AIGenerated
- **Gestion complète** : CRUD avec permissions

### Module Content

- **Gestion centralisée** des médias (images, vidéos)
- **Upload et stockage** via MinIO/Azure
- **Transcodage vidéo** et optimisation
- **Liaison exercices ↔ médias**

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
```

- [ ] **Module Coach** - Recommandations IA basées sur performances
- [ ] **Module Programs** - Plans d'entraînement multi-semaines
- [ ] **Module Tracking** - Analytics avancées et métriques
- [ ] **Module Objectives** - Gestion objectifs SMART avec progression

### 🎯 Priorité Haute (Next Sprint)

- [ ] **Tests d'intégration modules restants** (Authentication en cours)
- [ ] **Cross-module integration tests** (workflow complets utilisateur)
- [ ] **API documentation** Swagger complète avec exemples
- [ ] **Seed data** pour développement et démos

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

## 📈 Priorités de Développement

### 🔄 Tests Restants à Implémenter

- ❌ **Tests unitaires** Authentication, Users, Tracking (0 tests chacun)

### 🎯 Modules Suivants (Priorité Haute)

- [ ] **Programs** - Plans d'entraînement multi-semaines
- [ ] **Coach** - Recommandations IA basées sur performances
- [ ] **Objectives** - Gestion objectifs SMART
- [ ] **Notifications** - Système d'engagement
- [ ] **Payments** - Abonnements, billing
- [ ] **API** - Documentation Swagger complète

---

## 📝 Notes pour Futurs Chats

### Context Important à Rappeler

1. **Architecture modulaire stricte** - Pas de couplage direct entre modules
2. **Tests HTTP authentiques** - Toujours simuler vrais utilisateurs avec JSON
3. **Clean Architecture** - Respecter les couches Domain/Application/Infrastructure
4. **PostgreSQL schémas** - Chaque module a son schéma dédié
5. **JWT + Refresh tokens** - Sécurité robuste avec rotation

### Anti-Patterns à Éviter

- ❌ Couplage direct entre modules (utiliser médiation)
- ❌ Tests avec objets C# typés (utiliser JSON strings)
- ❌ Logique métier dans les controllers
- ❌ Accès direct à la BDD depuis d'autres modules
- ❌ JSON échappé `\"` dans raw strings `"""` (cause BadRequest 400)

### Patterns Validés à Réutiliser

- ✅ HTTP Client integration tests avec TestContainers
- ✅ Module registration via extension methods
- ✅ JWT + Refresh token avec policies
- ✅ Repository pattern avec EF Core
- ✅ Communication cross-module via événements MediatR
- ✅ Conversion unités de mesure avec services dédiés
- ✅ DTOs avec paramètres optionnels pour unités
- ✅ MediatR pour communication inter-modules

---

_Dernière mise à jour : Septembre 2025_
_Version : v1.0 - Base architecture validée avec tests intégration_
