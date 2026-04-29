namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone;

using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.Services;

public class GetMilestoneByIdHandler(
    IMilestoneRepository milestoneRepository,
    IMilestoneViewService milestoneViewService,
    ITelegramMessageService messageService,
    ILogger<GetMilestoneByIdHandler> logger) : IRequestHandler<GetMilestoneByIdQuery, Unit>
{
    public async Task<Unit> Handle(GetMilestoneByIdQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching milestone {MilestoneId} for chat {ChatId}",
            query.MilestoneId,
            query.ChatId);

        var milestone = await milestoneRepository.GetByIdAsync(query.MilestoneId, cancellationToken);

        if (milestone == null)
        {
            logger.LogWarning(
                "Milestone {MilestoneId} not found for chat {ChatId}",
                query.MilestoneId,
                query.ChatId);

            await messageService.SendTextMessageAsync(
                query.ChatId,
                "❌ Воспоминание не найдено. Возможно, оно было удалено.",
                ct: cancellationToken);

            return Unit.Value;
        }

        await milestoneViewService.SendMilestoneCardAsync(
            query.ChatId,
            milestone,
            query.ChildName,
            cancellationToken);

        return Unit.Value;
    }
}
