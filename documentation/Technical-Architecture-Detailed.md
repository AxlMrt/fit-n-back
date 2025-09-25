# Architecture Technique Détaillée - FitnessApp

## 🏗️ Architecture en Couches

### Vue d'Ensemble des Couches

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[API Controllers]
        MW[Middleware Pipeline]
        CORS[CORS Policy]
        AUTH[Authentication]
        AUTHZ[Authorization]
    end

    subgraph "Application Services Layer"
        APP_AUTH[Auth Services]
        APP_USERS[User Services]
        APP_WORKOUTS[Workout Services]
        APP_EXERCISES[Exercise Services]
        APP_TRACKING[Tracking Services]
        APP_CONTENT[Content Services]
    end

    subgraph "Domain Layer"
        DOM_AUTH[Auth Domain]
        DOM_USERS[Users Domain]
        DOM_WORKOUTS[Workouts Domain]
        DOM_EXERCISES[Exercises Domain]
        DOM_TRACKING[Tracking Domain]
        DOM_CONTENT[Content Domain]
    end

    subgraph "Infrastructure Layer"
        INF_AUTH[Auth Infrastructure]
        INF_USERS[Users Infrastructure]
        INF_WORKOUTS[Workouts Infrastructure]
        INF_EXERCISES[Exercises Infrastructure]
        INF_TRACKING[Tracking Infrastructure]
        INF_CONTENT[Content Infrastructure]
    end

    subgraph "Data Layer"
        DB[(PostgreSQL)]
        CACHE[(Redis)]
        FILES[(File Storage)]
        QUEUE[(Message Queue)]
    end

    API --> APP_AUTH
    API --> APP_USERS
    API --> APP_WORKOUTS
    API --> APP_EXERCISES
    API --> APP_TRACKING
    API --> APP_CONTENT

    APP_AUTH --> DOM_AUTH
    APP_USERS --> DOM_USERS
    APP_WORKOUTS --> DOM_WORKOUTS
    APP_EXERCISES --> DOM_EXERCISES
    APP_TRACKING --> DOM_TRACKING
    APP_CONTENT --> DOM_CONTENT

    DOM_AUTH --> INF_AUTH
    DOM_USERS --> INF_USERS
    DOM_WORKOUTS --> INF_WORKOUTS
    DOM_EXERCISES --> INF_EXERCISES
    DOM_TRACKING --> INF_TRACKING
    DOM_CONTENT --> INF_CONTENT

    INF_AUTH --> DB
    INF_USERS --> DB
    INF_WORKOUTS --> DB
    INF_EXERCISES --> DB
    INF_TRACKING --> DB
    INF_CONTENT --> FILES
    INF_AUTH --> CACHE
```

## 🔄 Flux de Données et Communications Inter-Modules

### Communication Patterns

```mermaid
graph TD
    subgraph "Direct Dependencies"
        API --> AUTH[Authentication Module]
        API --> USERS[Users Module]
        API --> WORKOUTS[Workouts Module]
        API --> EXERCISES[Exercises Module]
        API --> TRACKING[Tracking Module]
        API --> CONTENT[Content Module]
    end

    subgraph "Cross-Module Communications"
        USERS -.->|Sync via API Mediator| TRACKING
        WORKOUTS -.->|References| EXERCISES
        TRACKING -.->|Analytics| COACH[Coach Module]
        COACH -.->|Recommendations| USERS
        PAYMENTS[Payments Module] -.->|Subscription Updates| USERS
    end

    subgraph "Shared Components"
        SHARED[SharedKernel]
        VALIDATION[Validation Services]
        UNIT_CONV[Unit Converter]
    end

    AUTH --> SHARED
    USERS --> SHARED
    WORKOUTS --> SHARED
    EXERCISES --> SHARED
    TRACKING --> SHARED

    USERS --> UNIT_CONV
    TRACKING --> UNIT_CONV
    ALL[All Modules] --> VALIDATION
```

## 📊 Architecture des Données

### Modèle de Données Cross-Module

```mermaid
erDiagram
    %% Authentication Module
    AuthUsers {
        uuid id PK
        string email UK
        string username UK
        string passwordHash
        string role
        boolean emailConfirmed
        boolean twoFactorEnabled
        datetime createdAt
    }

    RefreshTokens {
        uuid id PK
        uuid userId FK
        string token UK
        datetime expiresAt
        boolean isRevoked
    }

    %% Users Module
    UserProfiles {
        uuid userId PK
        string firstName
        string lastName
        date dateOfBirth
        string gender
        decimal heightCm
        decimal weightKg
        decimal bmi
        string fitnessLevel
        string primaryFitnessGoal
        datetime createdAt
        datetime updatedAt
    }

    Preferences {
        uuid id PK
        uuid userId FK
        string category
        string key
        string value
    }

    Subscriptions {
        uuid userId PK
        string level
        datetime startDate
        datetime endDate
    }

    %% Exercises Module
    Exercises {
        uuid id PK
        string name
        string description
        string type
        integer muscleGroups
        integer equipment
        integer difficulty
        string instructions
        uuid imageContentId FK
        uuid videoContentId FK
    }

    %% Workouts Module
    Workouts {
        uuid id PK
        string name
        string description
        string type
        integer estimatedDuration
        string difficultyLevel
        boolean isActive
        uuid createdBy FK
    }

    WorkoutPhases {
        uuid id PK
        uuid workoutId FK
        string name
        integer orderIndex
        string phaseType
    }

    WorkoutExercises {
        uuid id PK
        uuid phaseId FK
        uuid exerciseId FK
        integer orderIndex
        integer sets
        integer reps
        decimal weight
        integer duration
    }

    %% Tracking Module
    WorkoutSessions {
        uuid id PK
        uuid userId FK
        uuid workoutId FK
        datetime startedAt
        datetime completedAt
        string status
        text notes
    }

    UserMetrics {
        uuid id PK
        uuid userId FK
        string metricType
        decimal value
        string unit
        datetime recordedAt
        text notes
    }

    ExerciseSets {
        uuid id PK
        uuid sessionId FK
        uuid exerciseId FK
        integer setNumber
        integer reps
        decimal weight
        integer duration
        datetime recordedAt
    }

    %% Content Module
    MediaAssets {
        uuid id PK
        string fileName
        string contentType
        long fileSize
        string storageUrl
        datetime uploadedAt
    }

    %% Relationships
    AuthUsers ||--o{ RefreshTokens : has
    AuthUsers ||--|| UserProfiles : corresponds_to
    UserProfiles ||--o{ Preferences : has
    UserProfiles ||--o| Subscriptions : has
    UserProfiles ||--o{ WorkoutSessions : performs
    UserProfiles ||--o{ UserMetrics : tracks
    UserProfiles ||--o{ Workouts : creates

    Exercises ||--o{ WorkoutExercises : included_in
    Exercises ||--o| MediaAssets : has_image
    Exercises ||--o| MediaAssets : has_video

    Workouts ||--o{ WorkoutPhases : contains
    WorkoutPhases ||--o{ WorkoutExercises : includes
    Workouts ||--o{ WorkoutSessions : performed_as

    WorkoutSessions ||--o{ ExerciseSets : contains
    Exercises ||--o{ ExerciseSets : performed
```

## 🛠️ Middleware Pipeline

### Request Processing Pipeline

```mermaid
flowchart TD
    A[Incoming Request] --> B[HTTPS Redirect]
    B --> C[CORS Policy]
    C --> D[Token Validation Middleware]
    D --> E{Token Required?}
    E -->|No| F[Public Endpoint]
    E -->|Yes| G{Token Valid?}
    G -->|No| H[401 Unauthorized]
    G -->|Yes| I[Authentication]
    I --> J[Authorization Policies]
    J --> K{Authorized?}
    K -->|No| L[403 Forbidden]
    K -->|Yes| M[Route to Controller]
    M --> N[Model Validation]
    N --> O[Business Logic]
    O --> P[Response]

    F --> M
    H --> Q[Error Response]
    L --> Q
    P --> R[Response Middleware]
    R --> S[Client Response]
    Q --> S
```

## 🔐 Security Architecture

### Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant MW as Middleware
    participant AUTH as Auth Service
    participant CACHE as Token Cache
    participant AUTHZ as Authorization
    participant CTRL as Controller
    participant BIZ as Business Logic

    C->>MW: Request with Bearer token

    Note over MW: TokenValidationMiddleware
    MW->>CACHE: Check if token revoked
    CACHE->>MW: Token status

    alt Token is revoked
        MW->>C: 401 Token Revoked
    else Token is valid
        MW->>MW: Validate JWT signature
        MW->>AUTH: Extract user claims
        AUTH->>MW: User identity

        Note over MW: Authentication complete
        MW->>AUTHZ: Check authorization policy
        AUTHZ->>AUTHZ: Evaluate role & subscription

        alt Not authorized
            AUTHZ->>C: 403 Forbidden
        else Authorized
            AUTHZ->>CTRL: Forward request
            CTRL->>BIZ: Execute business logic
            BIZ->>CTRL: Result
            CTRL->>C: Response
        end
    end
```

## 📈 Data Synchronization Patterns

### User Metrics Synchronization

```mermaid
sequenceDiagram
    participant API as API Controller
    participant USERS as Users Service
    participant MEDIATOR as Sync Mediator
    participant TRACKING as Tracking Service
    participant CONVERTER as Unit Converter
    participant DB as Database

    Note over API: Profile Update Request
    API->>CONVERTER: Validate & convert units
    CONVERTER->>API: Standardized values

    API->>USERS: Update physical measurements
    USERS->>USERS: Update PhysicalMeasurements VO
    USERS->>USERS: Calculate BMI
    USERS->>DB: Persist UserProfile
    DB->>USERS: Success
    USERS->>API: Updated profile

    Note over API: Fire-and-forget sync
    par Async synchronization
        API->>MEDIATOR: Trigger metrics sync
        MEDIATOR->>TRACKING: Record weight metric
        TRACKING->>DB: Save UserMetric(Weight)
        MEDIATOR->>TRACKING: Record height metric
        TRACKING->>DB: Save UserMetric(Height)
    end

    API->>API: Return success immediately
```

## 🧪 Testing Architecture

### Test Pyramid Structure

```mermaid
graph TB
    subgraph "Test Pyramid"
        E2E[End-to-End Tests<br/>Few, Critical Paths]
        INT[Integration Tests<br/>Module Interactions]
        UNIT[Unit Tests<br/>Business Logic, Value Objects]
    end

    subgraph "Test Categories"
        DOM[Domain Tests<br/>• Value Objects<br/>• Entities<br/>• Business Rules]
        APP[Application Tests<br/>• Services<br/>• Use Cases<br/>• Validation]
        API_TEST[API Tests<br/>• Controllers<br/>• Authentication<br/>• Authorization]
        INFRA[Infrastructure Tests<br/>• Repositories<br/>• Database<br/>• External Services]
    end

    subgraph "Integration Test Focus"
        CROSS[Cross-Module Tests<br/>• User Registration Flow<br/>• Metrics Synchronization<br/>• Workout Session Flow]
        AUTH_INT[Authentication Flow<br/>• Login/Logout<br/>• Token Refresh<br/>• Role Authorization]
        DATA[Data Consistency<br/>• Unit Conversion<br/>• BMI Calculation<br/>• Analytics Pipeline]
    end
```

## 🔧 Configuration & Deployment

### Environment Configuration

```mermaid
graph TD
    subgraph "Configuration Sources"
        APP_SETTINGS[appsettings.json]
        ENV_VARS[Environment Variables]
        USER_SECRETS[User Secrets]
        KEY_VAULT[Azure Key Vault]
    end

    subgraph "Configuration Categories"
        DB_CONFIG[Database Connection Strings]
        JWT_CONFIG[JWT Settings]
        CORS_CONFIG[CORS Policies]
        CACHE_CONFIG[Redis Configuration]
        STORAGE_CONFIG[File Storage Settings]
    end

    subgraph "Environment-Specific"
        DEV[Development<br/>• Local PostgreSQL<br/>• In-Memory Cache<br/>• Local File Storage]
        STAGING[Staging<br/>• Azure PostgreSQL<br/>• Redis Cache<br/>• Azure Blob Storage]
        PROD[Production<br/>• Production Database<br/>• Distributed Cache<br/>• CDN Storage]
    end

    APP_SETTINGS --> DB_CONFIG
    ENV_VARS --> JWT_CONFIG
    USER_SECRETS --> CORS_CONFIG
    KEY_VAULT --> CACHE_CONFIG

    DB_CONFIG --> DEV
    JWT_CONFIG --> STAGING
    CORS_CONFIG --> PROD
```

## 📊 Performance Monitoring Points

### Critical Performance Metrics

```mermaid
graph LR
    subgraph "API Performance"
        A1[Response Time<br/>P95 < 500ms]
        A2[Throughput<br/>>1000 req/min]
        A3[Error Rate<br/><1%]
    end

    subgraph "Database Performance"
        D1[Query Time<br/>P95 < 100ms]
        D2[Connection Pool<br/>Usage < 80%]
        D3[Deadlocks<br/>< 0.1%]
    end

    subgraph "Business Metrics"
        B1[Registration Success<br/>>95%]
        B2[Session Completion<br/>>80%]
        B3[Sync Success Rate<br/>>99%]
    end

    subgraph "Resource Usage"
        R1[CPU Usage<br/><70%]
        R2[Memory Usage<br/><80%]
        R3[Disk I/O<br/><60%]
    end
```

Cette architecture technique détaillée fournit :

1. **Vue en couches** : Séparation claire des responsabilités
2. **Communications inter-modules** : Patterns de communication et dépendances
3. **Modèle de données** : Relations entre entités cross-module
4. **Pipeline middleware** : Traitement des requêtes et sécurité
5. **Synchronisation de données** : Patterns async et cohérence
6. **Architecture de tests** : Structure et focus des tests
7. **Configuration** : Gestion des environnements
8. **Monitoring** : Métriques critiques à surveiller

Cette documentation technique complète le guide des parcours utilisateurs pour une compréhension holistique de l'application avant l'écriture des tests d'intégration.
