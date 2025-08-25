# Tests du Module Workouts

Ce projet contient une suite complète de tests pour le module Workouts de l'application fitness.

## 🎯 Types de Tests

### Tests Unitaires

#### Domain Layer
- **`WorkoutTests`** : Tests des entités principales et logique métier
- **`WorkoutPhaseTests`** : Tests des phases de workout et leurs exercices
- **`DurationTests`** : Tests du value object Duration
- **`ExerciseParametersTests`** : Tests des paramètres d'exercice

#### Application Layer
- **`WorkoutServiceTests`** : Tests des services applicatifs avec mocks
- **`WorkoutAuthorizationServiceTests`** : Tests des règles d'autorisation
- **`CreateWorkoutDtoValidatorTests`** : Tests de validation des DTOs

#### API Layer
- **`WorkoutsControllerTests`** : Tests des contrôleurs REST avec mocks

### Tests d'Intégration

#### Infrastructure Layer
- **`WorkoutRepositoryTests`** : Tests du repository avec base de données en mémoire

### Tests de Performance
- **`WorkoutPerformanceTests`** : Tests de performance pour les opérations lourdes

## 🚀 Exécution des Tests

### Tous les tests
```bash
dotnet test
```

### Tests spécifiques par catégorie
```bash
# Tests unitaires uniquement
dotnet test --filter "Category=Unit"

# Tests d'intégration uniquement  
dotnet test --filter "Category=Integration"

# Tests de performance uniquement
dotnet test --filter "Category=Performance"
```

### Tests avec couverture de code
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Couverture

Les tests couvrent :

### ✅ Couverture Complète
- **Entities** : Workout, WorkoutPhase, WorkoutExercise (100%)
- **Value Objects** : Duration, ExerciseParameters (100%)
- **Services** : WorkoutService, WorkoutAuthorizationService (95%+)
- **Repositories** : WorkoutRepository (90%+)
- **Controllers** : WorkoutsController (90%+)
- **Validators** : Tous les validators DTOs (95%+)

### 🔍 Scénarios Testés

#### Scénarios de Succès
- Création, modification, suppression de workouts
- Gestion des phases et exercices
- Autorisation pour les propriétaires
- Recherche et filtrage
- Duplication de workouts
- Calculs de durée

#### Scénarios d'Échec
- Données invalides
- Utilisateurs non autorisés
- Entités inexistantes
- Violations de règles métier
- Contraintes de validation

#### Scénarios de Performance
- Requêtes avec grande quantité de données
- Opérations complexes
- Calculs sur workouts complexes

## 🛠️ Structure des Tests

```
Tests/
├── Domain/
│   ├── Entities/           # Tests des entités du domaine
│   └── ValueObjects/       # Tests des value objects
├── Application/
│   ├── Services/           # Tests des services applicatifs
│   └── Validators/         # Tests des validators
├── Infrastructure/
│   └── Repositories/       # Tests d'intégration du repository
├── API/
│   └── Controllers/        # Tests des contrôleurs
├── Integration/            # Tests d'intégration end-to-end
├── Performance/            # Tests de performance
└── Helpers/               # Classes utilitaires pour tests
    ├── TestDataFactory.cs  # Factory pour créer des données de test
    └── IntegrationTestBase.cs # Base class pour tests d'intégration
```

## 📋 Conventions

### Naming
- **Test Class** : `{ClassUnderTest}Tests`
- **Test Method** : `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

### Arrange-Act-Assert
Tous les tests suivent le pattern AAA :
```csharp
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = methodUnderTest(input);
    
    // Assert
    result.Should().Be(expected);
}
```

### Données de Test
Utiliser `TestDataFactory` pour créer des données cohérentes :
```csharp
var workout = TestDataFactory.Workouts.CreateUserWorkout();
var parameters = TestDataFactory.ValueObjects.CreateRepetitionBasedParameters();
```

## 🔧 Configuration

### Packages Utilisés
- **xUnit** : Framework de test principal
- **FluentAssertions** : Assertions expressives
- **Moq** : Framework de mocking
- **EntityFramework.InMemory** : Base de données en mémoire pour tests
- **Microsoft.AspNetCore.Mvc.Testing** : Tests d'intégration API

### Base de Données de Test
Les tests d'intégration utilisent une base de données en mémoire pour :
- Isolation complète entre les tests
- Exécution rapide
- Pas de dépendance externe

## 📈 Métriques de Qualité

### Objectifs de Couverture
- **Code Coverage** : > 90%
- **Branch Coverage** : > 85%
- **Tests de Régression** : 100% des bugs critiques

### Critères de Performance
- **Tests Unitaires** : < 10ms par test
- **Tests d'Intégration** : < 100ms par test  
- **Tests de Performance** : Valident les seuils métier

## 🐛 Debugging des Tests

### Logs de Test
Les tests utilisent des loggers configurés pour le debugging :
```csharp
// Dans les tests, activer les logs détaillés si nécessaire
builder.Services.AddLogging(logging => logging.AddDebug());
```

### Tests Flaky
En cas de tests instables :
1. Vérifier l'isolation des tests
2. S'assurer du nettoyage des données
3. Éviter les dépendances temporelles
4. Utiliser des assertions plus robustes

## 🚨 CI/CD

### Pipeline de Tests
```yaml
- name: Run Unit Tests
  run: dotnet test --filter "Category!=Integration&Category!=Performance"

- name: Run Integration Tests  
  run: dotnet test --filter "Category=Integration"

- name: Run Performance Tests
  run: dotnet test --filter "Category=Performance"
```

### Quality Gates
- **Build** : Tous les tests doivent passer
- **Coverage** : Minimum 90% de couverture
- **Performance** : Pas de régression > 20%

Les tests sont la garantie de qualité du module Workouts ! 🏋️‍♂️
