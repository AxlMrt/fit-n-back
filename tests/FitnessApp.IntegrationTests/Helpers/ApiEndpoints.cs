namespace FitnessApp.IntegrationTests.Helpers;

/// <summary>
/// Centralized API endpoints for FitnessApp integration tests.
/// Provides type-safe endpoint constants to prevent typos and maintain consistency across tests.
/// All endpoints are validated against working integration tests.
/// </summary>
public static class ApiEndpoints
{
    #region Authentication Module

    /// <summary>Authentication endpoints</summary>
    public static class Auth
    {
        private const string BaseUrl = "/api/v1/auth";
        
        public static string Register => $"{BaseUrl}/register";
        public static string Login => $"{BaseUrl}/login";
        public static string Refresh => $"{BaseUrl}/refresh";
        public static string Me => $"{BaseUrl}/me";
        public static string Logout => $"{BaseUrl}/logout";
        public static string ExistsEmail(string email) => $"{BaseUrl}/exists/email?email={email}";
    }

    #endregion

    #region Users Module

    /// <summary>User profile and management endpoints</summary>
    public static class Users
    {
        private const string BaseUrl = "/api/v1/users";
        
        public static string Profile => $"{BaseUrl}/profile";
        public static string ProfilePersonal => $"{BaseUrl}/profile/personal";
        public static string ProfileMeasurements => $"{BaseUrl}/profile/measurements";
        public static string ProfileFitness => $"{BaseUrl}/profile/fitness";
        public static string ProfilePreferences => $"{BaseUrl}/profile/preferences";
    }

    #endregion


    #region Exercises Module

    /// <summary>Exercise management endpoints</summary>
    public static class Exercises
    {
        private const string BaseUrl = "/api/v1/exercises";
        
        public static string List => BaseUrl;
        public static string Create => BaseUrl;
        public static string GetById(Guid id) => $"{BaseUrl}/{id}";
        public static string Update(Guid id) => $"{BaseUrl}/{id}";
        public static string Delete(Guid id) => $"{BaseUrl}/{id}";
        public static string Activate(Guid id) => $"{BaseUrl}/{id}/activate";
        public static string Deactivate(Guid id) => $"{BaseUrl}/{id}/deactivate";
        public static string Search(string term, int? limit = null) => 
            $"{BaseUrl}/search?term={term}" + (limit.HasValue ? $"&limit={limit}" : "");
        public static string ListWithFilters(string? type = null, string? difficulty = null, string? muscleGroup = null) =>
            BaseUrl + "?" + string.Join("&", 
                new[] { type is not null ? $"type={type}" : null,
                        difficulty is not null ? $"difficulty={difficulty}" : null,
                        muscleGroup is not null ? $"muscleGroup={muscleGroup}" : null }
                .Where(x => x != null));
        public static string GetAll(int? limit = null, int? offset = null) =>
            BaseUrl + "?" + string.Join("&", 
                new[] { limit?.ToString() is { } l ? $"limit={l}" : null, 
                        offset?.ToString() is { } o ? $"offset={o}" : null }
                .Where(x => x != null));
    }

    #endregion

    #region Workouts Module

    /// <summary>Workout management endpoints</summary>
    public static class Workouts
    {
        private const string BaseUrl = "/api/v1/workouts";
        
        /// <summary>Template management (Admin only)</summary>
        public static string Templates => $"{BaseUrl}/templates";
        public static string TemplatesWithLimit(int limit) => $"{BaseUrl}/templates?limit={limit}";
        public static string CreateTemplate => $"{BaseUrl}/templates";
        
        /// <summary>User workout management</summary>
        public static string MyWorkouts => $"{BaseUrl}/my-workouts";
        public static string CreateUserWorkout => $"{BaseUrl}/my-workouts";
        
        /// <summary>General workout operations</summary>
        public static string GetById(Guid id) => $"{BaseUrl}/{id}";
        public static string Search(string searchTerm) => $"{BaseUrl}/search?searchTerm={searchTerm}";
        public static string GetByCategory(string category) => $"{BaseUrl}/category/{category}";
        public static string GetActive => $"{BaseUrl}/active";
        
        /// <summary>Admin operations</summary>
        public static string AdminUpdate(Guid id) => $"{BaseUrl}/admin/{id}";
        public static string AdminDelete(Guid id) => $"{BaseUrl}/admin/{id}";
        
        /// <summary>HEAD requests for metadata</summary>
        public static string Head(Guid id) => $"{BaseUrl}/{id}";
    }

    #endregion

    #region Tracking Module

    /// <summary>Workout tracking and metrics endpoints</summary>
    public static class Tracking
    {
        private const string BaseUrl = "/api/v1/tracking";
        
        /// <summary>Session management</summary>
        public static class Sessions
        {
            private const string SessionsUrl = $"{BaseUrl}/sessions";
            
            public static string Start => $"{SessionsUrl}/start";
            public static string Complete(Guid sessionId) => $"{SessionsUrl}/{sessionId}/complete";
            public static string RecordExercise(Guid sessionId) => $"{SessionsUrl}/{sessionId}/exercises";
            public static string History(int? limit = null) => 
                $"{SessionsUrl}/history" + (limit.HasValue ? $"?limit={limit}" : "");
            public static string GetById(Guid sessionId) => $"{SessionsUrl}/{sessionId}";
        }
        
        /// <summary>User metrics management</summary>
        public static string CreateMetric => $"{BaseUrl}/metrics";
        public static string GetMetrics => $"{BaseUrl}/metrics";
        public static string UpdateMetric(Guid id) => $"{BaseUrl}/metrics/{id}";
        public static string DeleteMetric(Guid id) => $"{BaseUrl}/metrics/{id}";
        public static string GetMetricsByType(string metricType) => $"{BaseUrl}/metrics/{metricType}";
        public static string GetLatestMetric(string metricType) => $"{BaseUrl}/metrics/{metricType}/latest";
        
        /// <summary>Metrics and analytics - Legacy</summary>
        public static string Metrics => $"{BaseUrl}/metrics";
        public static string MetricsSummary => $"{BaseUrl}/metrics/summary";
    }

    #endregion

    #region Content Module

    /// <summary>Media and content management endpoints</summary>
    public static class Content
    {
        private const string BaseUrl = "/api/v1/content";
        
        public static string Upload => $"{BaseUrl}/upload";
        public static string GetById(Guid id) => $"{BaseUrl}/{id}";
        public static string Delete(Guid id) => $"{BaseUrl}/{id}";
        public static string GetByExercise(Guid exerciseId) => $"{BaseUrl}/exercise/{exerciseId}";
    }

    #endregion

    #region Health and Infrastructure

    /// <summary>Health check and system endpoints</summary>
    public static class Health
    {
        public static string Check => "/health";
        public static string Ready => "/health/ready";
        public static string Live => "/health/live";
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Build query string from parameters
    /// </summary>
    public static string BuildQuery(params (string key, object? value)[] parameters)
    {
        var validParams = parameters
            .Where(p => p.value != null)
            .Select(p => $"{p.key}={Uri.EscapeDataString(p.value!.ToString()!)}")
            .ToArray();
            
        return validParams.Length > 0 ? "?" + string.Join("&", validParams) : "";
    }

    /// <summary>
    /// Combine base URL with query parameters
    /// </summary>
    public static string WithQuery(this string baseUrl, params (string key, object? value)[] parameters)
    {
        return baseUrl + BuildQuery(parameters);
    }

    #endregion
}

/// <summary>
/// Extension methods for easier endpoint usage
/// </summary>
public static class ApiEndpointExtensions
{
    /// <summary>
    /// Add limit parameter to any endpoint
    /// </summary>
    public static string WithLimit(this string endpoint, int limit)
    {
        var separator = endpoint.Contains('?') ? "&" : "?";
        return $"{endpoint}{separator}limit={limit}";
    }

    /// <summary>
    /// Add offset parameter to any endpoint
    /// </summary>
    public static string WithOffset(this string endpoint, int offset)
    {
        var separator = endpoint.Contains('?') ? "&" : "?";
        return $"{endpoint}{separator}offset={offset}";
    }

    /// <summary>
    /// Add pagination parameters to any endpoint
    /// </summary>
    public static string WithPagination(this string endpoint, int limit, int offset = 0)
    {
        return endpoint.WithLimit(limit).WithOffset(offset);
    }
}
