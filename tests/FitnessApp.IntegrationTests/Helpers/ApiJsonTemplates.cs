using System.Globalization;
using System.Text;

namespace FitnessApp.IntegrationTests.Helpers;

/// <summary>
/// Helper centralisé pour tous les templates JSON validés et compatibles avec l'API FitnessApp
/// 
/// 🎯 OBJECTIF: Éliminer les incohérences JSON et centraliser tous les formats
/// ✅ Formats validés par les tests d'intégration existants
/// 🔧 Paramètres dynamiques via interpolation de chaînes
/// 📝 Documentation des formats attendus par chaque endpoint
/// </summary>
public static class ApiJsonTemplates
{
    #region 🔐 Authentication Module

    /// <summary>
    /// Inscription utilisateur - Format validé
    /// Endpoint: POST /api/v1/auth/register
    /// Status attendu: 201 Created
    /// </summary>
    public static string RegisterUser(string email, string userName, string password, string? confirmPassword = null) => 
        $$"""
        {
            "email": "{{email}}",
            "userName": "{{userName}}",
            "password": "{{password}}",
            "confirmPassword": "{{confirmPassword ?? password}}"
        }
        """;

    /// <summary>
    /// Connexion utilisateur - Format validé
    /// Endpoint: POST /api/v1/auth/login
    /// Status attendu: 200 OK
    /// </summary>
    public static string LoginUser(string email, string password) => 
        $$"""
        {
            "email": "{{email}}",
            "password": "{{password}}"
        }
        """;

    /// <summary>
    /// Refresh token - Format validé
    /// Endpoint: POST /api/v1/auth/refresh
    /// Status attendu: 200 OK
    /// </summary>
    public static string RefreshToken(string refreshToken) => 
        $$"""
        {
            "refreshToken": "{{refreshToken}}"
        }
        """;

    #endregion

    #region 👤 Users Module

    /// <summary>
    /// Création profil utilisateur - Format validé
    /// Endpoint: POST /api/v1/users/profile
    /// Status attendu: 201 Created
    /// </summary>
    public static string CreateUserProfile(
        string firstName, 
        string lastName, 
        string dateOfBirth, 
        string gender, 
        double? height = null,
        double? weight = null,
        string? fitnessLevel = null, 
        string? fitnessGoal = null) => 
        $$"""
        {
            "firstName": "{{firstName}}",
            "lastName": "{{lastName}}",
            "dateOfBirth": "{{dateOfBirth}}",
            "gender": "{{gender}}"{{(height.HasValue ? $@",
            ""height"": {height.Value.ToString(CultureInfo.InvariantCulture)}" : "")}}{{(weight.HasValue ? $@",
            ""weight"": {weight.Value.ToString(CultureInfo.InvariantCulture)}" : "")}}{{(!string.IsNullOrEmpty(fitnessLevel) ? $@",
            ""fitnessLevel"": ""{fitnessLevel}""" : "")}}{{(!string.IsNullOrEmpty(fitnessGoal) ? $@",
            ""fitnessGoal"": ""{fitnessGoal}""" : "")}}
        }
        """;

    /// <summary>
    /// Mise à jour des informations personnelles - Format validé
    /// Endpoint: PATCH /api/v1/users/profile/personal
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdatePersonalInfo(
        string? firstName = null,
        string? lastName = null,
        string? dateOfBirth = null,
        string? gender = null)
    {
        var fields = new List<string>();
        if (!string.IsNullOrEmpty(firstName)) fields.Add($"\"firstName\": \"{firstName}\"");
        if (!string.IsNullOrEmpty(lastName)) fields.Add($"\"lastName\": \"{lastName}\"");
        if (!string.IsNullOrEmpty(dateOfBirth)) fields.Add($"\"dateOfBirth\": \"{dateOfBirth}\"");
        if (!string.IsNullOrEmpty(gender)) fields.Add($"\"gender\": \"{gender}\"");
        
        return "{\n    " + string.Join(",\n    ", fields) + "\n}";
    }
    
    /// <summary>
    /// Mise à jour complète des informations personnelles - Format validé
    /// Endpoint: PATCH /api/v1/users/profile/personal
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdatePersonalInfoComplete(
        string firstName,
        string lastName,
        string dateOfBirth,
        string gender) =>
        $$"""
        {
            "firstName": "{{firstName}}",
            "lastName": "{{lastName}}",
            "dateOfBirth": "{{dateOfBirth}}",
            "gender": "{{gender}}"
        }
        """;

    /// <summary>
    /// Mise à jour des mesures physiques - Format validé
    /// Endpoint: PATCH /api/v1/users/profile/measurements
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdatePhysicalMeasurements(
        double height, 
        double weight, 
        string heightUnit = "cm", 
        string weightUnit = "kg") => 
        $$"""
        {
            "height": {{height.ToString(CultureInfo.InvariantCulture)}},
            "weight": {{weight.ToString(CultureInfo.InvariantCulture)}},
            "units": {
                "heightUnit": "{{heightUnit}}",
                "weightUnit": "{{weightUnit}}"
            }
        }
        """;

    /// <summary>
    /// Mise à jour partielle des mesures physiques - permet height et/ou weight optionnels
    /// </summary>
    public static string UpdatePhysicalMeasurementsPartial(
        double? height = null, 
        double? weight = null, 
        string heightUnit = "cm", 
        string weightUnit = "kg")
    {
        var fields = new List<string>();
        
        if (height.HasValue)
            fields.Add($"\"height\": {height.Value.ToString(CultureInfo.InvariantCulture)}");
        
        if (weight.HasValue)
            fields.Add($"\"weight\": {weight.Value.ToString(CultureInfo.InvariantCulture)}");
        
        fields.Add($$"""
            "units": {
                "heightUnit": "{{heightUnit}}",
                "weightUnit": "{{weightUnit}}"
            }
            """);
        
        return $"{{ {string.Join(", ", fields)} }}";
    }

    /// <summary>
    /// Mise à jour des préférences fitness - Format validé
    /// Endpoint: PATCH /api/v1/users/profile/fitness
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdateFitnessPreferences(string fitnessLevel, string fitnessGoal) => 
        $$"""
        {
            "fitnessLevel": "{{fitnessLevel}}",
            "fitnessGoal": "{{fitnessGoal}}"
        }
        """;

    /// <summary>
    /// Mise à jour des préférences générales - Format validé
    /// Endpoint: PATCH /api/v1/users/profile/preferences
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdateUserPreferences(
        int? preferredWorkoutDuration = null,
        string[]? availableEquipment = null,
        string[]? preferredWorkoutTypes = null) => 
        $$"""
        {{{(preferredWorkoutDuration.HasValue ? $@"
            ""preferredWorkoutDuration"": {preferredWorkoutDuration.Value}," : "")}}{{(availableEquipment?.Length > 0 ? $@"
            ""availableEquipment"": [{string.Join(", ", availableEquipment.Select(e => $@"""{e}"""))}]," : "")}}{{(preferredWorkoutTypes?.Length > 0 ? $@"
            ""preferredWorkoutTypes"": [{string.Join(", ", preferredWorkoutTypes.Select(t => $@"""{t}"""))}]" : "")}}
        }
        """;

    #endregion

    #region 💪 Exercises Module

    /// <summary>
    /// Création d'exercice - Format validé (Admin uniquement)
    /// Endpoint: POST /api/v1/exercises
    /// Status attendu: 201 Created
    /// </summary>
    public static string CreateExercise(
        string name,
        string description,
        string type,
        string[] muscleGroups,
        string difficulty,
        string[] equipment,
        string? instructions = null)
    {
        var muscleGroupsJson = string.Join(", ", muscleGroups.Select(mg => $"\"{mg}\""));
        var equipmentJson = string.Join(", ", equipment.Select(eq => $"\"{eq}\""));
        var instructionsJson = instructions != null ? $",\n            \"instructions\": \"{instructions}\"" : "";
        
        return $$"""
        {
            "name": "{{name}}",
            "description": "{{description}}",
            "type": "{{type}}",
            "muscleGroups": [{{muscleGroupsJson}}],
            "difficulty": "{{difficulty}}",
            "equipment": [{{equipmentJson}}]{{instructionsJson}}
        }
        """;
    }

    /// <summary>
    /// Mise à jour d'exercice - Format validé (Admin uniquement)
    /// Endpoint: PUT /api/v1/exercises/{id}
    /// Status attendu: 200 OK
    /// </summary>
    public static string UpdateExercise(
        string? name = null,
        string? description = null,
        string? type = null,
        string[]? muscleGroups = null,
        string? difficulty = null,
        string[]? equipment = null,
        string? instructions = null)
    {
        var jsonParts = new List<string>();
        
        if (!string.IsNullOrEmpty(name))
            jsonParts.Add($"\"name\": \"{name}\"");
        
        if (description != null)
            jsonParts.Add($"\"description\": \"{description}\"");
        
        if (!string.IsNullOrEmpty(type))
            jsonParts.Add($"\"type\": \"{type}\"");
        
        if (muscleGroups != null && muscleGroups.Length > 0)
        {
            var muscleGroupsJson = string.Join(", ", muscleGroups.Select(mg => $"\"{mg}\""));
            jsonParts.Add($"\"muscleGroups\": [{muscleGroupsJson}]");
        }
        
        if (!string.IsNullOrEmpty(difficulty))
            jsonParts.Add($"\"difficulty\": \"{difficulty}\"");
        
        if (equipment != null)
        {
            var equipmentJson = string.Join(", ", equipment.Select(eq => $"\"{eq}\""));
            jsonParts.Add($"\"equipment\": [{equipmentJson}]");
        }
        
        if (!string.IsNullOrEmpty(instructions))
            jsonParts.Add($"\"instructions\": \"{instructions}\"");
        
        return $@"{{
            {string.Join(",\n            ", jsonParts)}
        }}";
    }

    #endregion

    #region 🏋️ Workouts Module

    /// <summary>
    /// Workout simple pour utilisateurs - Format exact de WorkoutHttpIntegrationTests
    /// Endpoint: POST /api/v1/workouts/my-workouts
    /// Status attendu: 201 Created
    /// Structure: Juste les champs de base, pas de phases
    /// </summary>
    public static string CreateUserWorkout(
        string name,
        string description = "Custom workout created by user",
        string type = "UserCreated",
        string category = "Cardio",
        string difficulty = "Beginner",
        int estimatedDurationMinutes = 25) => 
        $$"""
        {
            "name": "{{name}}",
            "description": "{{description}}",
            "type": "{{type}}",
            "category": "{{category}}",
            "difficulty": "{{difficulty}}",
            "estimatedDurationMinutes": {{estimatedDurationMinutes}}
        }
        """;

    /// <summary>
    /// Template workout simple - Format exact pour cas basiques
    /// Endpoint: POST /api/v1/workouts/templates  
    /// Status attendu: 201 Created
    /// Structure: Champs de base uniquement
    /// </summary>
    public static string CreateSimpleWorkoutTemplate(
        string name,
        string description = "Template workout",
        string type = "Template",
        string category = "Strength", 
        string difficulty = "Beginner",
        int estimatedDurationMinutes = 30) => 
        $$"""
        {
            "name": "{{name}}",
            "description": "{{description}}",
            "type": "{{type}}",
            "category": "{{category}}",
            "difficulty": "{{difficulty}}",
            "estimatedDurationMinutes": {{estimatedDurationMinutes}}
        }
        """;

    /// <summary>
    /// Template workout complexe avec phases - Format exact de WorkoutHttpIntegrationTests
    /// Endpoint: POST /api/v1/workouts/templates
    /// Status attendu: 201 Created
    /// Structure: Avec phases complètes et exercices
    /// </summary>
    public static string CreateComplexWorkoutTemplate(
        string name,
        string description,
        string type = "Template",
        string category = "Strength",
        string difficulty = "Intermediate", 
        int estimatedDurationMinutes = 40) => 
        $$"""
        {
            "name": "{{name}}",
            "description": "{{description}}",
            "type": "{{type}}",
            "category": "{{category}}",
            "difficulty": "{{difficulty}}",
            "estimatedDurationMinutes": {{estimatedDurationMinutes}},
            "phases": [
                {
                    "type": "WarmUp",
                    "name": "Dynamic warm-up",
                    "description": "Muscle activation and light cardio",
                    "estimatedDurationMinutes": 8,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 60,
                            "sets": 2,
                            "restTimeSeconds": 30
                        }
                    ]
                },
                {
                    "type": "MainEffort",
                    "name": "Strength Circuit",
                    "description": "3 rounds of 4 exercises, focus on form",
                    "estimatedDurationMinutes": 27,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440202",
                            "reps": 12,
                            "sets": 3,
                            "restTimeSeconds": 60
                        }
                    ]
                }
            ]
        }
        """;

    /// <summary>
    /// Workout cardio avec structure complète - Format exact de WorkoutHttpIntegrationTests  
    /// Endpoint: POST /api/v1/workouts/templates
    /// Status attendu: 201 Created
    /// Structure: 3 phases (WarmUp, MainEffort, Recovery) avec exercices
    /// </summary>
    public static string CreateCardioWorkout(
        string name = "Express Cardio - 15 minutes",
        string description = "Intensive 15-minute cardio workout to burn maximum calories",
        string type = "Template",
        string category = "Cardio",
        string difficulty = "Advanced",
        int estimatedDurationMinutes = 15) => 
        $$"""
        {
            "name": "{{name}}",
            "description": "{{description}}",
            "type": "{{type}}",
            "category": "{{category}}",
            "difficulty": "{{difficulty}}",
            "estimatedDurationMinutes": {{estimatedDurationMinutes}},
            "phases": [
                {
                    "type": "WarmUp",
                    "name": "Quick activation",
                    "estimatedDurationMinutes": 2,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 60,
                            "sets": 2,
                            "restTimeSeconds": 0
                        }
                    ]
                },
                {
                    "type": "MainEffort",
                    "name": "Intense Cardio",
                    "description": "Tabata - 20s all-out / 10s rest, 4 rounds",
                    "estimatedDurationMinutes": 12,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440101",
                            "durationSeconds": 20,
                            "sets": 8,
                            "restTimeSeconds": 10,
                            "notes": "Maximum intensity"
                        }
                    ]
                },
                {
                    "type": "Recovery",
                    "name": "Cool down",
                    "estimatedDurationMinutes": 1,
                    "exercises": [
                        {
                            "exerciseId": "550e8400-e29b-41d4-a716-446655440100",
                            "durationSeconds": 30,
                            "sets": 1,
                            "notes": "Very slow tempo for recovery"
                        }
                    ]
                }
            ]
        }
        """;

    /// <summary>
    /// Mise à jour workout (Admin uniquement) - Format exact de WorkoutHttpIntegrationTests
    /// Endpoint: PUT /api/v1/workouts/admin/{id}
    /// Status attendu: 200 OK
    /// Structure: Seulement les champs à modifier
    /// </summary>
    public static string UpdateWorkout(
        string? name = null,
        string? description = null,
        string? difficulty = null,
        int? estimatedDurationMinutes = null)
    {
        var fields = new List<string>();
        
        if (!string.IsNullOrEmpty(name))
            fields.Add($@"""name"": ""{name}""");
        if (!string.IsNullOrEmpty(description))
            fields.Add($@"""description"": ""{description}""");
        if (!string.IsNullOrEmpty(difficulty))
            fields.Add($@"""difficulty"": ""{difficulty}""");
        if (estimatedDurationMinutes.HasValue)
            fields.Add($@"""estimatedDurationMinutes"": {estimatedDurationMinutes.Value}");
        
        return $$"""
        {
            {{string.Join(",\n            ", fields)}}
        }
        """;
    }

    /// <summary>
    /// Workout invalide pour tests d'erreur - Format exact de WorkoutHttpIntegrationTests
    /// Endpoint: POST /api/v1/workouts/templates
    /// Status attendu: 400 BadRequest
    /// Structure: Données invalides (nom vide, durée négative)
    /// </summary>
    public static string CreateInvalidWorkout() => 
        """
        {
            "name": "",
            "type": "Template",
            "category": "Strength",
            "difficulty": "Beginner",
            "estimatedDurationMinutes": -5
        }
        """;

    #endregion

    #region 📊 Tracking Module
    
    /// <summary>
    /// Crée un JSON pour enregistrer une métrique utilisateur générique
    /// </summary>
    public static string CreateUserMetric(string metricType, double value, string unit = "", string recordedAt = "", string notes = "")
    {
        var actualUnit = string.IsNullOrEmpty(unit) ? (metricType == "Height" ? "cm" : "kg") : unit;
        var actualRecordedAt = string.IsNullOrEmpty(recordedAt) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : recordedAt;
        return $$"""
        {
            "metricType": "{{metricType}}",
            "value": {{value.ToString(CultureInfo.InvariantCulture)}},
            "unit": "{{actualUnit}}",
            "recordedAt": "{{actualRecordedAt}}",
            "notes": "{{notes}}"
        }
        """;
    }

    /// <summary>
    /// Crée un JSON pour enregistrer une métrique de poids
    /// </summary>
    public static string CreateWeightMetric(double weight, string notes = "") =>
        CreateUserMetric("Weight", weight, notes: notes);

    /// <summary>
    /// Crée un JSON pour enregistrer une métrique de taille
    /// </summary>
    public static string CreateHeightMetric(double height, string notes = "") =>
        CreateUserMetric("Height", height, notes: notes);

    /// <summary>
    /// Met à jour une métrique utilisateur existante
    /// </summary>
    public static string UpdateUserMetric(string metricType, double value, string notes = "")
    {
        var unit = metricType == "Height" ? "cm" : "kg";
        return $$"""
        {
            "metricType": "{{metricType}}",
            "value": {{value.ToString(CultureInfo.InvariantCulture)}},
            "unit": "{{unit}}",
            "notes": "{{notes}}"
        }
        """;
    }


    #endregion

    #region 📝 Helper Classes

    /// <summary>
    /// Représentation d'une phase de workout
    /// </summary>
    public class WorkoutPhase
    {
        public string Name { get; }
        public string PhaseType { get; }
        public string? Description { get; }
        public int Order { get; }
        public int EstimatedDurationMinutes { get; }
        public WorkoutExercise[] Exercises { get; }

        public WorkoutPhase(string name, string phaseType, int order, int estimatedDurationMinutes, WorkoutExercise[] exercises, string? description = null)
        {
            Name = name;
            PhaseType = phaseType;
            Description = description;
            Order = order;
            EstimatedDurationMinutes = estimatedDurationMinutes;
            Exercises = exercises;
        }

        public string ToJson()
        {
            var exercisesJson = string.Join(",\n                        ", Exercises.Select(e => e.ToJson()));
            var descriptionJson = !string.IsNullOrEmpty(Description) ? $@",
                        ""description"": ""{Description}""" : "";
            
            return $$"""
            {
                        "type": "{{PhaseType}}",
                        "name": "{{Name}}"{{descriptionJson}},
                        "estimatedDurationMinutes": {{EstimatedDurationMinutes}},
                        "exercises": [
                            {{exercisesJson}}
                        ]
                    }
            """;
        }
    }

    /// <summary>
    /// Représentation d'un exercice dans un workout
    /// </summary>
    public class WorkoutExercise
    {
        public Guid ExerciseId { get; }
        public int Sets { get; }
        public int? Reps { get; }
        public int? DurationSeconds { get; }
        public double? Weight { get; }
        public int RestTimeSeconds { get; }
        public int Order { get; }

        // Constructeur pour exercices avec répétitions
        public WorkoutExercise(Guid exerciseId, int sets, int reps, int restTimeSeconds, double? weight = null, int order = 1)
        {
            ExerciseId = exerciseId;
            Sets = sets;
            Reps = reps;
            DurationSeconds = null;
            Weight = weight;
            RestTimeSeconds = restTimeSeconds;
            Order = order;
        }

        // Constructeur pour exercices avec durée
        public static WorkoutExercise WithDuration(Guid exerciseId, int sets, int durationSeconds, int restTimeSeconds, int order = 1)
        {
            return new WorkoutExercise(exerciseId, sets, null, durationSeconds, null, restTimeSeconds, order);
        }

        private WorkoutExercise(Guid exerciseId, int sets, int? reps, int? durationSeconds, double? weight, int restTimeSeconds, int order)
        {
            ExerciseId = exerciseId;
            Sets = sets;
            Reps = reps;
            DurationSeconds = durationSeconds;
            Weight = weight;
            RestTimeSeconds = restTimeSeconds;
            Order = order;
        }

        public string ToJson()
        {
            var fields = new List<string>
            {
                $@"""exerciseId"": ""{ExerciseId}""",
                $@"""sets"": {Sets}"
            };

            if (Reps.HasValue)
                fields.Add($@"""reps"": {Reps.Value}");
            
            if (DurationSeconds.HasValue)
                fields.Add($@"""durationSeconds"": {DurationSeconds.Value}");
            
            if (Weight.HasValue)
                fields.Add($@"""weight"": {Weight.Value.ToString(CultureInfo.InvariantCulture)}");
            
            fields.Add($@"""restTimeSeconds"": {RestTimeSeconds}");

            return $$"""
            {
                                {{string.Join(",\n                                ", fields)}}
                            }
            """;
        }
    }

    /// <summary>
    /// Représentation d'une série d'exercice
    /// </summary>
    public class ExerciseSet
    {
        public int Reps { get; }
        public double? Weight { get; }
        public int RestDuration { get; }
        public DateTime CompletedAt { get; }

        public ExerciseSet(int reps, double? weight, int restDuration, DateTime completedAt)
        {
            Reps = reps;
            Weight = weight;
            RestDuration = restDuration;
            CompletedAt = completedAt;
        }

        public string ToJson() => 
            $$"""
            {
                        "reps": {{Reps}}{{(Weight.HasValue ? $@",
                        ""weight"": {Weight.Value.ToString(CultureInfo.InvariantCulture)}" : "")}},
                        "restDuration": {{RestDuration}},
                        "completedAt": "{{CompletedAt:yyyy-MM-ddTHH:mm:ss.fffZ}}"
                    }
            """;
    }

    #endregion

    #region 🛠️ Utility Methods

    /// <summary>
    /// Convertit JSON en StringContent pour les requêtes HTTP
    /// </summary>
    public static StringContent ToStringContent(this string json) => 
        new(json, Encoding.UTF8, "application/json");

    /// <summary>
    /// Génère un email unique pour les tests
    /// </summary>
    public static string GenerateTestEmail(string prefix = "test") => 
        $"{prefix}-{Guid.NewGuid():N}@example.com";

    /// <summary>
    /// Génère un nom d'utilisateur unique pour les tests
    /// </summary>
    public static string GenerateTestUserName(string prefix = "user", int maxLength = 15) => 
        $"{prefix}{Guid.NewGuid():N}"[..Math.Min(maxLength, $"{prefix}{Guid.NewGuid():N}".Length)];

    /// <summary>
    /// Date de naissance par défaut pour les tests
    /// </summary>
    public static string DefaultBirthDate => "1990-05-15T00:00:00Z";

    /// <summary>
    /// Mot de passe sécurisé par défaut pour les tests
    /// </summary>
    public static string DefaultPassword => "SecureTest#2024!";

    #endregion
}
