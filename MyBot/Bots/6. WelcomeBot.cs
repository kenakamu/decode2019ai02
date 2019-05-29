// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyBot
{
    public class WelcomeBot : ActivityHandler
    {
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                var adaptiveCard = File.ReadAllText(Path.Combine("AdaptiveCards", "Welcome.json"));
                var welcomeCard = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCard),
                };

                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var response = ((Activity)turnContext.Activity).CreateReply();
                    response.Attachments = new List<Attachment>() { welcomeCard };
                    await turnContext.SendActivityAsync(response, cancellationToken);
                }
            }
        }
    }
}
