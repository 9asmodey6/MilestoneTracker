namespace MilestoneTracker.Application.Common.Features.Children.ProvideAccess;

using MediatR;

public record GenerateAccessTokenCommand(
    int CreatorId,
    int ChildId,
    int ValidityHours = 24,
    int MaxUses = 1) : IRequest<GenerateAccessTokenResult>;