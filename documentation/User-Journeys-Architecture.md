# Architecture et Parcours Utilisateurs - FitnessApp

## 🎯 Vue d'ensemble

Cette documentation présente l'architecture globale de FitnessApp et les parcours utilisateurs pour faciliter l'écriture de tests d'intégration pertinents.

## 📐 Architecture Modulaire Globale

```mermaid
graph TB
    subgraph "API Layer"
        API[FitnessApp.API]
        MW[Middleware & Auth]
    end

    subgraph "Modules Métier"
        AUTH[Authentication]
        USERS[Users]
        WORKOUTS[Workouts]
        EXERCISES[Exercises]
        TRACKING[Tracking]
        PROGRAMS[Programs]
        COACH[Coach]
        OBJECTIVES[Objectives]
        NOTIFICATIONS[Notifications]
        PAYMENTS[Payments]
        CONTENT[Content]
    end

    subgraph "Infrastructure"
        DB[(PostgreSQL)]
        CACHE[Redis Cache]
        FILES[File Storage]
    end

    API --> AUTH
    API --> USERS
    API --> WORKOUTS
    API --> EXERCISES
    API --> TRACKING
    API --> PROGRAMS
    API --> COACH
    API --> OBJECTIVES
    API --> NOTIFICATIONS
    API --> PAYMENTS
    API --> CONTENT

    AUTH --> DB
    USERS --> DB
    WORKOUTS --> DB
    EXERCISES --> DB
    TRACKING --> DB
    PROGRAMS --> DB

    CONTENT --> FILES
    API --> CACHE
```

## 🚀 Parcours Utilisateur : Inscription et Premier Profil

### Diagramme de Séquence

```mermaid
sequenceDiagram
    participant C as Client Mobile/Web
    participant API as API Gateway
    participant AUTH as Auth Module
    participant USERS as Users Module
    participant TRACKING as Tracking Module
    participant DB as Database

    Note over C: 1. Vérification de disponibilité
    C->>API: GET /auth/exists/email?email=user@email.com
    API->>AUTH: CheckEmailExists
    AUTH->>DB: Query email existence
    DB->>AUTH: Result
    AUTH->>API: {exists: false}
    API->>C: Email disponible

    Note over C: 2. Inscription
    C->>API: POST /auth/register
    API->>AUTH: RegisterAsync
    AUTH->>AUTH: Validate & Hash Password
    AUTH->>DB: Create AuthUser
    AUTH->>API: {accessToken, refreshToken, userId}
    API->>C: 201 Created + Tokens

    Note over C: 3. Création du profil
    C->>API: POST /users/profile (avec Bearer token)
    API->>USERS: CreateUserProfile
    USERS->>USERS: Create UserProfile + PhysicalMeasurements
    USERS->>DB: Save UserProfile

    Note over C: 4. Synchronisation automatique vers Tracking
    API->>API: MeasurementSyncMediatorService
    API->>TRACKING: RecordUserMetric (Weight)
    TRACKING->>DB: Save UserMetric
    API->>TRACKING: RecordUserMetric (Height)
    TRACKING->>DB: Save UserMetric

    USERS->>API: UserProfileResponse
    API->>C: 201 Created Profile
```

### Points de Test d'Intégration

1. **Validation Email** : Tester les regex et formats email
2. **Sécurité Mot de Passe** : Vérifier les règles de complexité
3. **Cohérence des Tokens** : Valider JWT et refresh tokens
4. **Synchronisation Cross-Module** : Vérifier que les métriques sont créées dans Tracking
5. **Gestion des Erreurs** : Tester rollback en cas d'échec partiel

## 🏃‍♀️ Parcours Utilisateur : Session d'Entraînement Complète

### Diagramme de Flux

```mermaid
flowchart TD
    A[Utilisateur connecté] --> B[Recherche d'exercices]
    B --> C[Sélection d'un workout]
    C --> D[Démarrage session tracking]
    D --> E[Exécution des exercices]
    E --> F[Enregistrement des performances]
    F --> G[Finalisation de la session]
    G --> H[Calculs de progression]
    H --> I[Mise à jour des métriques utilisateur]

    subgraph "Modules impliqués"
        B -.-> EX[Exercises Module]
        C -.-> WO[Workouts Module]
        D -.-> TR[Tracking Module]
        E -.-> TR
        F -.-> TR
        G -.-> TR
        H -.-> CO[Coach Module]
        I -.-> US[Users Module]
    end
```

### Diagramme de Séquence Détaillé

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API
    participant EX as Exercises
    participant WO as Workouts
    participant TR as Tracking
    participant US as Users
    participant CO as Coach

    Note over C: 1. Recherche et sélection
    C->>API: GET /exercises/search?term=squat
    API->>EX: SearchExercises("squat")
    EX->>API: ExerciseListDto[]
    API->>C: Liste des exercices

    C->>API: GET /workouts/active
    API->>WO: GetActiveWorkouts()
    WO->>API: WorkoutDto[]
    API->>C: Workouts disponibles

    Note over C: 2. Démarrage de session
    C->>API: POST /tracking/sessions/start
    API->>TR: StartWorkoutSession(userId, workoutId)
    TR->>TR: Create WorkoutSession
    TR->>API: WorkoutSessionDto
    API->>C: Session créée

    Note over C: 3. Enregistrement des exercices
    loop Pour chaque exercice
        C->>API: POST /tracking/sessions/{id}/exercises
        API->>TR: AddExerciseToSession
        TR->>TR: Record ExerciseSet
        TR->>API: Updated session
        API->>C: Confirmation
    end

    Note over C: 4. Finalisation
    C->>API: POST /tracking/sessions/{id}/complete
    API->>TR: CompleteWorkoutSession
    TR->>TR: Calculate session stats

    Note over C: 5. Analyses et progression
    TR->>CO: TriggerProgressAnalysis(userId)
    CO->>CO: Analyze performance trends
    CO->>US: UpdateProgressMetrics
    US->>US: Update user fitness data

    TR->>API: CompletedSessionDto
    API->>C: Session terminée avec stats
```

### Points de Test d'Intégration

1. **Recherche Cross-Module** : Vérifier les requêtes entre Exercises/Workouts
2. **State Management** : Tester la cohérence d'état des sessions
3. **Calculs de Performance** : Valider les métriques calculées
4. **Synchronisation Modules** : Coach → Users pour progression
5. **Gestion des Timeouts** : Sessions abandonnées

## 👤 Parcours Utilisateur : Gestion Complète du Profil

### Diagramme de Flux États

```mermaid
stateDiagram-v2
    [*] --> Registered : Register
    Registered --> BasicProfile : Create Profile
    BasicProfile --> CompleteProfile : Add Measurements
    CompleteProfile --> ActiveUser : Set Fitness Goals

    ActiveUser --> UpdatedProfile : Update Info
    ActiveUser --> PremiumUser : Subscribe
    ActiveUser --> InactiveUser : Long inactivity

    UpdatedProfile --> ActiveUser : Sync Complete
    PremiumUser --> ActiveUser : Subscription expires
    InactiveUser --> ActiveUser : Return

    state BasicProfile {
        [*] --> PersonalInfo
        PersonalInfo --> PhysicalData : Add measurements
        PhysicalData --> FitnessProfile : Set goals
        FitnessProfile --> [*]
    }

    state UpdatedProfile {
        [*] --> SyncingData
        SyncingData --> ValidationError : Invalid data
        SyncingData --> SyncComplete : Success
        ValidationError --> SyncingData : Retry
        SyncComplete --> [*]
    }
```

### Diagramme de Séquence : Mise à jour Profil avec Multi-Unités

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API Controller
    participant US as Users Service
    participant SYNC as Sync Mediator
    participant TR as Tracking Service
    participant CONV as Unit Converter
    participant DB as Database

    Note over C: Utilisateur européen met à jour en métrique
    C->>API: PATCH /users/profile/measurements
    Note over C: {height: 180, weight: 75, units: {heightUnit: "cm", weightUnit: "kg"}}

    API->>CONV: ValidateUnits(cm, kg)
    CONV->>API: Valid

    API->>US: UpdatePhysicalMeasurements
    US->>US: Update PhysicalMeasurements Value Object
    US->>US: Calculate BMI
    US->>DB: Save UserProfile
    DB->>US: Success
    US->>API: Updated Profile

    Note over API: Fire-and-forget sync
    API->>SYNC: ProcessMeasurementSync (async)
    SYNC->>CONV: ConvertToStandardUnits(180cm, 75kg)
    CONV->>SYNC: {180, 75} (already standard)

    SYNC->>TR: RecordUserMetric(Weight, 75kg)
    TR->>DB: Save UserMetric
    SYNC->>TR: RecordUserMetric(Height, 180cm)
    TR->>DB: Save UserMetric

    API->>C: 200 OK + Updated Profile

    Note over C: Utilisateur américain met à jour en impérial
    C->>API: PATCH /users/profile/measurements
    Note over C: {height: 5.9, weight: 165, units: {heightUnit: "ft", weightUnit: "lbs"}}

    API->>CONV: ConvertHeight(5.9 ft)
    CONV->>API: 179.8 cm
    API->>CONV: ConvertWeight(165 lbs)
    CONV->>API: 74.8 kg

    API->>US: UpdatePhysicalMeasurements(179.8, 74.8)
    US->>DB: Save standardized values

    API->>SYNC: ProcessMeasurementSync
    SYNC->>TR: RecordUserMetric(Weight, 74.8kg)
    SYNC->>TR: RecordUserMetric(Height, 179.8cm)

    API->>C: 200 OK
```

### Points de Test d'Intégration

1. **Conversion d'Unités** : Tester toutes les combinaisons (cm/ft/in, kg/lbs)
2. **Cohérence des Données** : Vérifier BMI recalculé automatiquement
3. **Synchronisation Asynchrone** : Valider le fire-and-forget vers Tracking
4. **Validation Cross-Module** : Règles business appliquées partout
5. **Gestion des Préférences** : Stockage et récupération des unités favorites

## 🔐 Parcours Authentification et Autorisation

### Diagramme de Flux Sécurité

```mermaid
flowchart TD
    A[Request avec JWT] --> B{Token valide?}
    B -->|Non| C[401 Unauthorized]
    B -->|Oui| D{Token révoqué?}
    D -->|Oui| E[401 Token Revoked]
    D -->|Non| F{Autorisation suffisante?}
    F -->|Non| G[403 Forbidden]
    F -->|Oui| H[Accès autorisé]

    H --> I{Action nécessite rôle?}
    I -->|Admin| J{User is Admin?}
    I -->|Coach| K{User is Coach+?}
    I -->|Premium| L{User has subscription?}

    J -->|Non| M[403 Need Admin Role]
    J -->|Oui| N[Action autorisée]
    K -->|Non| O[403 Need Coach Role]
    K -->|Oui| N
    L -->|Non| P[403 Need Premium]
    L -->|Oui| N
```

### Diagramme de Séquence : Login et Autorisation

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API
    participant MW as Token Middleware
    participant AUTH as Auth Service
    participant AUTHZ as Authorization
    participant CACHE as Token Cache
    participant DB as Database

    Note over C: 1. Login
    C->>API: POST /auth/login
    API->>AUTH: LoginAsync(credentials)
    AUTH->>DB: Verify user credentials
    DB->>AUTH: AuthUser entity
    AUTH->>AUTH: Generate JWT + Refresh Token
    AUTH->>DB: Store refresh token
    AUTH->>API: {accessToken, refreshToken}
    API->>C: 200 OK + Tokens

    Note over C: 2. Requête protégée
    C->>API: GET /users/profile (Bearer token)
    API->>MW: TokenValidationMiddleware
    MW->>CACHE: IsTokenRevoked(token)
    CACHE->>MW: Not revoked
    MW->>MW: Validate JWT signature & expiry
    MW->>API: Token valid

    API->>AUTHZ: Check authorization policy
    AUTHZ->>AUTHZ: Verify user role/subscription
    AUTHZ->>API: Authorized

    API->>API: Execute business logic
    API->>C: 200 OK + Data

    Note over C: 3. Logout
    C->>API: POST /auth/logout
    API->>AUTH: LogoutAsync(userId, token)
    AUTH->>CACHE: RevokeToken(token)
    AUTH->>DB: Invalidate refresh tokens
    API->>C: 200 OK
```

### Points de Test d'Intégration

1. **Cycle Complet d'Auth** : Login → Request → Logout
2. **Gestion des Tokens** : Validation, révocation, refresh
3. **Politiques d'Autorisation** : Rôles et niveaux d'abonnement
4. **Sécurité Cross-Controller** : Cohérence entre endpoints
5. **Token Expiry & Refresh** : Renouvellement automatique

## 📊 Parcours Analytics et Coaching

### Diagramme de Flux Analytics

```mermaid
flowchart TD
    A[Session d'entraînement terminée] --> B[Trigger Analytics]
    B --> C[Collecte des métriques]
    C --> D[Calcul des KPIs]
    D --> E[Analyse des tendances]
    E --> F[Génération de recommandations]
    F --> G[Mise à jour du profil utilisateur]
    G --> H[Notification de progression]

    subgraph "Sources de données"
        C --> I[Tracking Module]
        C --> J[Users Module]
        C --> K[Workouts Module]
    end

    subgraph "Outputs"
        F --> L[Coach Recommendations]
        G --> M[User Progress Update]
        H --> N[Push Notifications]
    end
```

## 🧪 Matrice des Tests d'Intégration Critiques

### Parcours Cross-Module

| Scénario                  | Modules Impliqués                       | Points de Validation                            |
| ------------------------- | --------------------------------------- | ----------------------------------------------- |
| **Inscription Complète**  | AUTH → USERS → TRACKING                 | Token validity, Profile creation, Metrics sync  |
| **Session Workout**       | EXERCISES → WORKOUTS → TRACKING → COACH | Data flow, State consistency, Analytics trigger |
| **Mise à jour Profil**    | USERS → TRACKING + Unit conversion      | Data synchronization, Unit conversion accuracy  |
| **Progression Analytics** | TRACKING → COACH → USERS                | Calculation accuracy, Profile updates           |
| **Gestion Abonnements**   | USERS → PAYMENTS → Authorization        | Payment processing, Access control updates      |

### Scénarios de Robustesse

| Test Case                      | Description                                       | Expected Behavior                              |
| ------------------------------ | ------------------------------------------------- | ---------------------------------------------- |
| **Sync Failure**               | Tracking module unavailable during profile update | Profile update succeeds, sync logged for retry |
| **Token Expiry**               | JWT expires during long session                   | Graceful refresh or re-auth prompt             |
| **Database Timeout**           | DB connection issues                              | Proper error handling, no data corruption      |
| **Unit Conversion Edge Cases** | Invalid or extreme unit values                    | Validation errors, no corrupt data stored      |
| **Concurrent Updates**         | Multiple clients updating same user data          | Last-write-wins or conflict resolution         |

## 📈 Métriques de Performance à Surveiller

### API Response Times

```mermaid
graph LR
    A[Client Request] --> B[Auth: <100ms]
    B --> C[Business Logic: <500ms]
    C --> D[DB Operations: <200ms]
    D --> E[Total: <800ms]

    F[Critical Paths] --> G[Login: <300ms]
    G --> H[Profile Update: <400ms]
    H --> I[Workout Start: <200ms]
    I --> J[Search: <150ms]
```

Cette documentation fournit une base solide pour créer des tests d'intégration pertinents en couvrant :

- Les parcours utilisateurs critiques
- Les interactions cross-module
- Les points de validation essentiels
- Les scénarios de robustesse
- Les métriques de performance

Les diagrammes facilitent la compréhension des flux de données et des dépendances entre composants, permettant de cibler les tests là où ils auront le plus d'impact.
