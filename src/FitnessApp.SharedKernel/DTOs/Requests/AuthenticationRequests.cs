using System.ComponentModel.DataAnnotations;

namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to register a new user.
/// Contains only authentication-related information.
/// Profile data should be created separately after registration.
/// </summary>
public record RegisterRequest(
    [Required]
    [EmailAddress]
    [StringLength(320, MinimumLength = 5)]
    string Email,
    
    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,30}$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    string UserName,
    
    [Required]
    [StringLength(128, MinimumLength = 8)]
    string Password,
    
    [Required]
    string ConfirmPassword
);

/// <summary>
/// Represents a request to log in a user.
/// </summary>
public record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,
    
    [Required]
    string Password
);

/// <summary>
/// Represents a request to refresh an authentication token.
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);

/// <summary>
/// Represents a request to initiate the forgot password process.
/// </summary>
public record ForgotPasswordRequest(
    [Required]
    [EmailAddress]
    string Email
);

/// <summary>
/// Represents a request to reset a user's password using a token.
/// </summary>
public record ResetPasswordRequest(
    [Required]
    [EmailAddress]
    string Email,
    
    [Required]
    string Token,
    
    [Required]
    [StringLength(128, MinimumLength = 8)]
    string NewPassword,

    [Required]
    string ConfirmNewPassword
);

/// <summary>
/// Request to change the current user's password.
/// Requires current password for security.
/// </summary>
public record ChangePasswordRequest(
    [Required]
    string CurrentPassword,

    [Required]
    [StringLength(128, MinimumLength = 8)]
    string NewPassword,

    [Required]
    string ConfirmNewPassword
);

/// <summary>
/// Request to update email address.
/// Will require email confirmation.
/// </summary>
public record UpdateEmailRequest(
    [Required]
    [EmailAddress]
    [StringLength(320, MinimumLength = 5)]
    string NewEmail
);

/// <summary>
/// Request to update username.
/// </summary>
public record UpdateUsernameRequest(
    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,30}$", ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    string NewUsername
);

/// <summary>
/// Request to confirm email with token.
/// </summary>
public record ConfirmEmailRequest(
    [Required]
    string Token
);

/// <summary>
/// Request to resend email confirmation.
/// </summary>
public record ResendEmailConfirmationRequest(
    [Required]
    [EmailAddress]
    string Email
);
