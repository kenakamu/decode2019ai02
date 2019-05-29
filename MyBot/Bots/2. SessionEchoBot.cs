// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace MyBot
{
    public class SessionEchoBot : ActivityHandler
    {
        BotState conversationState;
        BotState userState;
        public SessionEchoBot(ConversationState conversationState, UserState userState)
        {
            this.conversationState = conversationState;
            this.userState = userState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var countProp = conversationState.CreateProperty<int>("count");
            var count = await countProp.GetAsync(turnContext,()=> { return 1; });
            await turnContext.SendActivityAsync(MessageFactory.Text($"{turnContext.Activity.Text} {count++}"));
            await countProp.SetAsync(turnContext, count);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            // ƒZƒbƒVƒ‡ƒ“‚Ì•Û‘¶
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
