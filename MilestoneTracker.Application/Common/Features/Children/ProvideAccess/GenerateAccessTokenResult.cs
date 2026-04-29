namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess;

public record GenerateAccessTokenResult(
    bool IsSuccess,
    string? Token = null,
    DateTime? ExpiresAt = null,
    string? ErrorMessage = null)
{
    public static GenerateAccessTokenResult Success(string token, DateTime expiresAt) 
        => new(true, token, expiresAt);
        
    public static GenerateAccessTokenResult Failure(string errorMessage) 
        => new(false, ErrorMessage: errorMessage);
}