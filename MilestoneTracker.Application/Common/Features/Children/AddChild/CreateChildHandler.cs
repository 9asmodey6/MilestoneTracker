namespace MilestoneTracker.Application.Common.Features.Children.AddChild;

using Domain.Entities;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateChildHandler(
    IParentRepository parentRepository,
    ILogger<CreateChildHandler> logger)
    : IRequestHandler<CreateChildCommand, int>
{
    public async Task<int> Handle(CreateChildCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating child. Name: {Name}, BirthDate: {Date}, ParentId: {ParentId}",
            command.Name,
            command.Date,
            command.ParentId);

        try
        {
            var child = new Child
            {
                ParentId = command.ParentId,
                Name = command.Name,
                BirthDate = command.Date.ToUniversalTime(),
                PhotoFileId = command.PhotoId,
                CreatedAt = DateTime.UtcNow
            };

            var childId = await parentRepository.AddChildAsync(command.ParentId, child, cancellationToken);

            logger.LogInformation(
                "Child created successfully. Id: {ChildId}, Name: {Name}",
                childId,
                child.Name);

            return childId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating child for ParentId: {ParentId}", command.ParentId);
            throw;
        }
    }
}