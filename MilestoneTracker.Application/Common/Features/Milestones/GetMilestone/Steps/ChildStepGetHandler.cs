namespace MilestoneTracker.Application.Common.Features.Milestones.GetMilestone.Steps;

using Constants;
using Domain.Enums;
using Infrastructure.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Bot.Keyboards;
using Shared.Models;
using Shared.State;

public class ChildStepGetHandler(
    IParentRepository parentRepository,
    ITelegramMessageService messageService,
    IUserStateService userStateService,
    ILogger<ChildStepGetHandler> logger) : IStepHandler<GetMilestoneData>
{ 
    public UserStateType Step => UserStateType.GetMilestoneSelectingChild;
    
    public async Task<StepResult<GetMilestoneData>> HandleAsync(BotContext context, GetMilestoneData data,
        CancellationToken ct)
    {
        logger.LogInformation("Processing GetMilestone step was started for chat {ChatId}.",
            context.ChatId);

        if (context.IsCallback)
        {
            if (!int.TryParse(context.CallbackData, out var childId))
            {
                logger.LogWarning("Unable to parse child id from callback data: {CallbackData}", context.CallbackData);
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "Пожалуйста, выберите ребенка, используя кнопки ниже.",
                    BotKeyboards.ChildSelectionKeyboard(await parentRepository.GetChildrenAsync(context.ChatId, ct)),
                    ct);
                return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingChild, data);
            }
            
            var child = await parentRepository.GetChildByIdAsync(childId, ct);

            if (child == null)
            {
                logger.LogWarning("Unable to find child with id {ChildId} for Chat {ChatId}", childId, context.ChatId);
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId,
                    "Ребенок не найден. Пожалуйста, выберите ребенка, используя кнопки ниже.",
                    BotKeyboards.ChildSelectionKeyboard(await parentRepository.GetChildrenAsync(context.ChatId, ct)),
                    ct);
                return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingChild, data);
            }

            var updatedData = data with { ChildId = child.Id, ChildName = child.Name };
            
            string messageText = 
                $"✨ <b>Отлично!</b> Открываем воспоминания для <b>{child.Name}</b> 🍼\n\n" +
                $"Выберите, как именно вы хотите просмотреть записи:\n" +
                $"• <b>По хронологии</b> — лента от новых к старым.\n" +
                $"• <b>По категориям</b> — первые слова, достижения, смешное.\n" +
                $"• <b>Конкретная дата</b> — вспомнить определенный день.\n\n" +
                $"<i>Нажмите одну из кнопок ниже:</i> 👇";

            if (context.MessageId.HasValue)
            {
                await messageService.EditMessageTextAsync(
                    context.ChatId, 
                    context.MessageId.Value,
                    messageText,
                    BotKeyboards.ViewMilestonesModeKeyboard(),
                    ct);
            }
            else
            {
                await messageService.SendMessageWithInlineKeyboardAsync(
                    context.ChatId, 
                    messageText,
                    BotKeyboards.ViewMilestonesModeKeyboard(),
                    ct);
            }
            
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingMode, updatedData);
        }
        
        var children = await parentRepository.GetChildrenAsync(context.ChatId, ct);
        
        if (children.Count == 0)
        {
            logger.LogWarning("Children not found for chat {ChatId}", context.ChatId);
            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId,
                "<b>В базе пока нет ваших детей.</b>\n\nНажмите кнопку ниже, чтобы добавить первого ребенка и начать отслеживать его достижения! 👇",
                BotKeyboards.AddChildKeyboard,
                ct);
            await userStateService.ResetAsync(context.ChatId, ct);
            return new StepResult<GetMilestoneData>(UserStateType.Idle, null);
        }

        if (children.Count == 1)
        {
            var child = children[0];
            var updatedData = data with { ChildId = child.Id, ChildName = child.Name };
            
            string messageText = 
                $"✨ <b>Отлично!</b> Открываем воспоминания для <b>{child.Name}</b> 🍼\n\n" +
                $"Выберите, как именно вы хотите просмотреть записи:\n" +
                $"• <b>По хронологии</b> — лента от новых к старым.\n" +
                $"• <b>По категориям</b> — первые слова, достижения, смешное.\n" +
                $"• <b>Конкретная дата</b> — вспомнить определенный день.\n\n" +
                $"<i>Нажмите одну из кнопок ниже:</i> 👇";

            await messageService.SendMessageWithInlineKeyboardAsync(
                context.ChatId, 
                messageText,
                BotKeyboards.ViewMilestonesModeKeyboard(),
                ct);
                
            return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingMode, updatedData);
        }
        
        await messageService.SendMessageWithInlineKeyboardAsync(
            context.ChatId,
            "<b>Чьи воспоминания вы хотите посмотреть?</b> 👶👧\n\nВыберите ребенка из списка ниже: 👇",
            BotKeyboards.ChildSelectionKeyboard(children),
            ct);
            
        return new StepResult<GetMilestoneData>(UserStateType.GetMilestoneSelectingChild, data);
    }
}