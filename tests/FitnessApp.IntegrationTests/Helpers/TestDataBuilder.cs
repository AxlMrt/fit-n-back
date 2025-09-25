using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.IntegrationTests.Helpers;

/// <summary>
/// Builder pattern pour créer des données de test cohérentes
/// </summary>
public static class TestDataBuilder
{
    #region User Test Data

    public class UserProfileBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private string _firstName = "John";
        private string _lastName = "Doe";
        private DateOnly _dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));
        private Gender? _gender = Gender.Male;
        private FitnessLevel? _fitnessLevel = FitnessLevel.Beginner;
        private FitnessGoal? _primaryGoal = FitnessGoal.Weight_Loss;
        private decimal? _heightCm = 175m;
        private decimal? _weightKg = 75m;

        public UserProfileBuilder WithUserId(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public UserProfileBuilder WithName(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
            return this;
        }

        public UserProfileBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = dateOfBirth;
            return this;
        }

        public UserProfileBuilder WithDateOfBirth(DateTime dateOfBirth)
        {
            _dateOfBirth = DateOnly.FromDateTime(dateOfBirth);
            return this;
        }

        public UserProfileBuilder WithGender(Gender? gender)
        {
            _gender = gender;
            return this;
        }

        public UserProfileBuilder WithFitnessLevel(FitnessLevel? fitnessLevel)
        {
            _fitnessLevel = fitnessLevel;
            return this;
        }

        public UserProfileBuilder WithFitnessGoal(FitnessGoal? primaryGoal)
        {
            _primaryGoal = primaryGoal;
            return this;
        }

        public UserProfileBuilder WithPhysicalMeasurements(decimal? heightCm, decimal? weightKg)
        {
            _heightCm = heightCm;
            _weightKg = weightKg;
            return this;
        }

        public UserProfileBuilder WithFitnessProfile(FitnessLevel? fitnessLevel, FitnessGoal? primaryGoal)
        {
            _fitnessLevel = fitnessLevel;
            _primaryGoal = primaryGoal;
            return this;
        }

        public UserProfile Build()
        {
            var profile = new UserProfile(_userId);
            
            // Set personal information
            var fullName = FullName.Create(_firstName, _lastName);
            var dateOfBirth = DateOfBirth.Create(_dateOfBirth.ToDateTime(TimeOnly.MinValue));
            profile.UpdatePersonalInfo(fullName, dateOfBirth, _gender);

            // Set physical measurements if provided
            if (_heightCm.HasValue || _weightKg.HasValue)
            {
                var measurements = PhysicalMeasurements.Create(_heightCm, _weightKg);
                profile.UpdatePhysicalMeasurements(measurements);
            }

            // Set fitness profile
            profile.UpdateFitnessProfile(_fitnessLevel, _primaryGoal);

            return profile;
        }
    }

    public static UserProfileBuilder CreateUser() => new UserProfileBuilder();

    #endregion

    #region Tracking Test Data

    public class UserMetricBuilder
    {
        private Guid _userId = Guid.NewGuid();
        private UserMetricType _metricType = UserMetricType.Weight;
        private double _value = 70.5;
        private DateTime _recordedAt = DateTime.UtcNow;
        private string? _notes = null;
        private string? _unit = null;

        public UserMetricBuilder ForUser(Guid userId)
        {
            _userId = userId;
            return this;
        }

        public UserMetricBuilder WithMetricType(UserMetricType metricType)
        {
            _metricType = metricType;
            return this;
        }

        public UserMetricBuilder WithValue(double value, string? unit = null)
        {
            _value = value;
            _unit = unit;
            return this;
        }

        public UserMetricBuilder RecordedAt(DateTime recordedAt)
        {
            _recordedAt = recordedAt;
            return this;
        }

        public UserMetricBuilder WithNotes(string notes)
        {
            _notes = notes;
            return this;
        }

        public UserMetric Build()
        {
            return new UserMetric(
                _userId,
                _metricType,
                _value,
                _recordedAt,
                _notes,
                _unit
            );
        }
    }

    public static UserMetricBuilder CreateMetric() => new UserMetricBuilder();

    #endregion

    #region Common Test Scenarios

    /// <summary>
    /// Crée un utilisateur complet avec des métriques initiales
    /// </summary>
    public static class TestScenarios
    {
        public static (UserProfile user, List<UserMetric> metrics) CreateUserWithMetrics(
            string firstName = "John", 
            string lastName = "Doe")
        {
            var userId = Guid.NewGuid();
            var user = CreateUser()
                .WithUserId(userId)
                .WithName(firstName, lastName)
                .WithPhysicalMeasurements(175m, 75m)
                .Build();

            var metrics = new List<UserMetric>
            {
                CreateMetric()
                    .ForUser(userId)
                    .WithMetricType(UserMetricType.Weight)
                    .WithValue(75.0, "kg")
                    .RecordedAt(DateTime.UtcNow.AddDays(-30))
                    .Build(),

                CreateMetric()
                    .ForUser(userId)
                    .WithMetricType(UserMetricType.Height)
                    .WithValue(175.0, "cm")
                    .RecordedAt(DateTime.UtcNow.AddDays(-30))
                    .Build(),

                CreateMetric()
                    .ForUser(userId)
                    .WithMetricType(UserMetricType.Weight)
                    .WithValue(74.5, "kg")
                    .RecordedAt(DateTime.UtcNow.AddDays(-15))
                    .Build(),

                CreateMetric()
                    .ForUser(userId)
                    .WithMetricType(UserMetricType.Weight)
                    .WithValue(74.0, "kg")
                    .RecordedAt(DateTime.UtcNow)
                    .Build()
            };

            return (user, metrics);
        }

        /// <summary>
        /// Crée plusieurs utilisateurs pour les tests de performance
        /// </summary>
        public static List<UserProfile> CreateMultipleUsers(int count)
        {
            var users = new List<UserProfile>();
            
            for (int i = 0; i < count; i++)
            {
                var user = CreateUser()
                    .WithUserId(Guid.NewGuid())
                    .WithName($"User{i}", $"Test{i}")
                    .WithDateOfBirth(DateOnly.FromDateTime(DateTime.Today.AddYears(-20 - i % 40)))
                    .WithGender(i % 2 == 0 ? Gender.Male : Gender.Female)
                    .WithFitnessLevel((FitnessLevel)(i % Enum.GetValues<FitnessLevel>().Length))
                    .WithFitnessGoal((FitnessGoal)(i % Enum.GetValues<FitnessGoal>().Length))
                    .Build();
                
                users.Add(user);
            }
            
            return users;
        }

        /// <summary>
        /// Crée des données de test pour la conversion d'unités
        /// </summary>
        public static List<UserMetric> CreateMetricsForUnitConversion(Guid userId)
        {
            var baseDate = DateTime.UtcNow.AddDays(-60);
            
            return new List<UserMetric>
            {
                // Poids en différentes unités
                CreateMetric().ForUser(userId).WithMetricType(UserMetricType.Weight)
                    .WithValue(75.0, "kg").RecordedAt(baseDate).Build(),
                CreateMetric().ForUser(userId).WithMetricType(UserMetricType.Weight)
                    .WithValue(165.3, "lb").RecordedAt(baseDate.AddDays(15)).Build(),
                
                // Taille en différentes unités
                CreateMetric().ForUser(userId).WithMetricType(UserMetricType.Height)
                    .WithValue(175.0, "cm").RecordedAt(baseDate).Build(),
                CreateMetric().ForUser(userId).WithMetricType(UserMetricType.Height)
                    .WithValue(69.0, "in").RecordedAt(baseDate.AddDays(30)).Build(),
                
                // Performances personnelles
                CreateMetric().ForUser(userId).WithMetricType(UserMetricType.PersonalRecord)
                    .WithValue(100.0, "kg").RecordedAt(baseDate.AddDays(45))
                    .WithNotes("Deadlift PR").Build()
            };
        }
    }

    #endregion
}
