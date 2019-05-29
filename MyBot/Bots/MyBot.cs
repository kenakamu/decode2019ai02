// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MyBot.Dialogs;
using MyBot.Models;

namespace MyBot
{
    public class MyBot : ActivityHandler
    {
        BotState conversationState;
        BotState userState;
        DialogSet dialogSet;

        public MyBot(ConversationState conversationState, UserState userState)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            dialogSet.Add(new ProfileDialog());
            dialogSet.Add(new ProfileWithChoiceDialog());
        }
              
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            if (results.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(nameof(ProfileWithChoiceDialog), new Profile(), cancellationToken);
            }
            else if (results.Status == DialogTurnStatus.Complete)
            {
                var profile = (Profile)results.Result;
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"ようこそ{profile.PetPreference}の{profile.Name}さん"));
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            // ステートの保存
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
