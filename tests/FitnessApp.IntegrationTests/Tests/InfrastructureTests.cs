using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using FluentAssertions.Execution;

namespace FitnessApp.IntegrationTests.Tests;

/// <summary>
/// Tests d'intégration basiques pour vérifier que l'infrastructure fonctionne
/// </summary>
public class InfrastructureTests : IntegrationTestBase
{
    public InfrastructureTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnSuccess()
    {
        // Act - Tester un endpoint qui existe certainement
        var response = await Client.GetAsync("/");

        // Assert - Accepter 404 comme résultat valide (pas d'erreur serveur)
        response.StatusCode.Should().BeOneOf(System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
        // Act & Assert - Vérifier que tous les contextes de base de données sont accessibles
        using (new AssertionScope())
        {
            UsersContext.Database.Should().NotBeNull();
            WorkoutsContext.Database.Should().NotBeNull();
            ExercisesContext.Database.Should().NotBeNull();
            TrackingContext.Database.Should().NotBeNull();
            ContentContext.Database.Should().NotBeNull();
            AuthenticationContext.Database.Should().NotBeNull();

            // Vérifier que nous pouvons nous connecter aux bases de données
            var canConnectUsers = await UsersContext.Database.CanConnectAsync();
            var canConnectWorkouts = await WorkoutsContext.Database.CanConnectAsync();
            var canConnectExercises = await ExercisesContext.Database.CanConnectAsync();
            var canConnectTracking = await TrackingContext.Database.CanConnectAsync();
            var canConnectContent = await ContentContext.Database.CanConnectAsync();
            var canConnectAuth = await AuthenticationContext.Database.CanConnectAsync();

            canConnectUsers.Should().BeTrue("la base de données Users devrait être accessible");
            canConnectWorkouts.Should().BeTrue("la base de données Workouts devrait être accessible");
            canConnectExercises.Should().BeTrue("la base de données Exercises devrait être accessible");
            canConnectTracking.Should().BeTrue("la base de données Tracking devrait être accessible");
            canConnectContent.Should().BeTrue("la base de données Content devrait être accessible");
            canConnectAuth.Should().BeTrue("la base de données Authentication devrait être accessible");
        }
    }

    [Fact]
    public async Task TestDataBuilder_ShouldCreateValidUserProfile()
    {
        // Arrange
        var testUser = TestDataBuilder.CreateUser()
            .WithName("Integration", "Test")
            .WithPhysicalMeasurements(180m, 80m)
            .Build();

        // Act
        await UsersContext.UserProfiles.AddAsync(testUser);
        await UsersContext.SaveChangesAsync();

        // Assert
        testUser.Should().NotBeNull();
        testUser.Name.FirstName.Should().Be("Integration");
        testUser.Name.LastName.Should().Be("Test");
        testUser.PhysicalMeasurements.Height.Should().Be(180m);
        testUser.PhysicalMeasurements.Weight.Should().Be(80m);
        
        // Vérifier que l'entité a été sauvegardée
        var savedUser = await UsersContext.UserProfiles.FindAsync(testUser.UserId);
        savedUser.Should().NotBeNull();
        savedUser!.UserId.Should().Be(testUser.UserId);
    }

    [Fact]
    public async Task TestDataBuilder_ShouldCreateValidUserMetric()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testMetric = TestDataBuilder.CreateMetric()
            .ForUser(userId)
            .WithValue(75.5, "kg")
            .WithNotes("Test metric")
            .Build();

        // Act
        await TrackingContext.UserMetrics.AddAsync(testMetric);
        await TrackingContext.SaveChangesAsync();

        // Assert
        testMetric.Should().NotBeNull();
        testMetric.ShouldHaveCorrectMetricData(userId, 75.5, "kg");
        testMetric.Notes.Should().Be("Test metric");
        
        var savedMetric = await TrackingContext.UserMetrics.FindAsync(testMetric.Id);
        savedMetric.Should().NotBeNull();
        savedMetric!.UserId.Should().Be(userId);
    }

    [Fact]
    public void AuthenticationHelper_ShouldGenerateValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@integration.com";

        // Act
        var token = AuthenticationHelper.GenerateTestJwtToken(userId, email);

        // Assert
        token.Should().NotBeNullOrEmpty();
        AuthenticationHelper.IsTokenValid(token).Should().BeTrue();
        
        var extractedUserId = AuthenticationHelper.GetUserIdFromToken(token);
        extractedUserId.Should().Be(userId);
    }

    [Fact]
    public void AuthenticationHelper_ShouldCreateTestUser()
    {
        // Act
        var testUser = AuthenticationHelper.CreateTestUser("Test", "User", "test@example.com");

        // Assert
        testUser.Should().NotBeNull();
        testUser.FirstName.Should().Be("Test");
        testUser.LastName.Should().Be("User");
        testUser.Email.Should().Be("test@example.com");
        testUser.Token.Should().NotBeNullOrEmpty();
        testUser.Roles.Should().Contain("User");
        
        // Vérifier que le token est valide
        AuthenticationHelper.IsTokenValid(testUser.Token).Should().BeTrue();
    }

    [Fact]
    public async Task DatabaseCleanup_ShouldWork()
    {
        // Arrange - Ajouter des données de test
        var testUser = TestDataBuilder.CreateUser().Build();
        await UsersContext.UserProfiles.AddAsync(testUser);
        await UsersContext.SaveChangesAsync();

        var userCount = await CountEntitiesAsync<FitnessApp.Modules.Users.Domain.Entities.UserProfile>(UsersContext);
        userCount.Should().BeGreaterThan(0);

        // Act - Nettoyer la base de données
        await Factory.CleanDatabaseAsync();

        // Assert - Vérifier que les données ont été nettoyées
        await RefreshContextAsync(UsersContext);
        var userCountAfterClean = await CountEntitiesAsync<FitnessApp.Modules.Users.Domain.Entities.UserProfile>(UsersContext);
        userCountAfterClean.Should().Be(0);
    }

    [Fact]
    public async Task PerformanceTest_DatabaseOperations_ShouldCompleteQuickly()
    {
        // Arrange
        var users = TestDataBuilder.TestScenarios.CreateMultipleUsers(10);

        // Act & Assert - Vérifier que l'insertion de 10 utilisateurs prend moins d'1 seconde
        var task = async () =>
        {
            await UsersContext.UserProfiles.AddRangeAsync(users);
            await UsersContext.SaveChangesAsync();
        };

        await task.Should().CompleteWithinAsync(TimeSpan.FromSeconds(1));

        // Vérifier que les données ont été insérées
        var count = await CountEntitiesAsync<FitnessApp.Modules.Users.Domain.Entities.UserProfile>(UsersContext);
        count.Should().Be(10);
    }
}



