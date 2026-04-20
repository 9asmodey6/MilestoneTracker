namespace MilestoneTracker.Application.Common.Features.Children.GetChildren;

using Domain.Entities.ValueObjects;
using Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Bot.Keyboards;

public class GetChildrenHandler(
    IParentRepository repository,
    ITelegramMessageService messageService,
    ILogger<GetChildrenHandler> logger) : IRequestHandler<GetChildrenQuery, List<GetChildrenResponse>>
{
    public async Task<List<GetChildrenResponse>> Handle(GetChildrenQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching children for ChatId: {ChatId}", request.ChatId);

        try
        {
            var children = await repository.GetChildrenAsync(request.ChatId, cancellationToken);
            if (!children.Any())
            {
                logger.LogInformation("No children found for ChatId: {ChatId}", request.ChatId);

                await messageService.SendMessageWithInlineKeyboardAsync(
                    request.ChatId,
                    "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                    BotKeyboards.AddChildKeyboard,
                    cancellationToken);

                return new List<GetChildrenResponse>();
            }

            var response = children.Select(child => new GetChildrenResponse(
                child.Id,
                child.Name,
                AgeInfo.Calculate(child.BirthDate, DateTime.UtcNow).ToString(),
                child.PhotoFileId
            )).ToList();

            foreach (var child in response)
            {
                var caption = 
                    $"<b>👶 Ребенок:</b> {child.Name}\n" +
                    $"<b>🎂 Возраст:</b> {child.Age}\n" +
                    $"──────────────────\n" +
                    $"<i>Выберите действие ниже, чтобы просмотреть вехи или изменить данные.</i>";
                
                if (!string.IsNullOrEmpty(child.PhotoFileId))
                {
                    await messageService.SendPhotoAsync(request.ChatId, child.PhotoFileId, caption, cancellationToken);
                }
                else
                {
                    await messageService.SendTextMessageAsync(request.ChatId, caption, ct: cancellationToken);
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching children for {ChatId}", request.ChatId);
            return new List<GetChildrenResponse>();
        }
    }
}