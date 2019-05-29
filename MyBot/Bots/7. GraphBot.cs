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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MyBot
{
    public class GraphBot : ActivityHandler 
    {
        BotState conversationState;
        BotState userState;
        DialogSet dialogSet;

        public GraphBot(ConversationState conversationState, 
            UserState userState, MSGraphService graphClient)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            dialogSet.Add(new ScheduleDialog(graphClient));
        }
              
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {            
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            if (results.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(nameof(ScheduleDialog), null, cancellationToken);
            }
            else if (results.Status == DialogTurnStatus.Complete)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"何をしますか？"));
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            await dialogContext.ContinueDialogAsync(cancellationToken);
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
