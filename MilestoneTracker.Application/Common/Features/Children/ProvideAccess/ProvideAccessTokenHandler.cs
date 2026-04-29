namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess;

using MediatR;
using Shared.Interfaces.Services;

public class ProvideAccessTokenHandler(IChildAccessTokenService accessTokenService) 
    : IRequestHandler<GenerateAccessTokenCommand, GenerateAccessTokenResult>
{
    public async Task<GenerateAccessTokenResult> Handle(GenerateAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await accessTokenService.GenerateTokenAsync(
            request.ChildId, 
            request.CreatorId, 
            request.ValidityHours,
            cancellationToken);

        if (result.IsFailure)
        {
            return GenerateAccessTokenResult.Failure(result.ErrorMessage!);
        }

        return GenerateAccessTokenResult.Success(result.Value!.Token, result.Value.ExpiresAt);
    }
}