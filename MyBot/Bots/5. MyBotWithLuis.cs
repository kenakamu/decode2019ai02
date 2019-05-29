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
    public class MyBotWithLuis : ActivityHandler 
    {
        BotState conversationState;
        BotState userState;
        IRecognizer recognizer;
        DialogSet dialogSet;

        public MyBotWithLuis(ConversationState conversationState, 
            UserState userState, IRecognizer recognizer)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            this.recognizer = recognizer;
            dialogSet = new DialogSet(conversationState.CreateProperty<DialogState>(nameof(DialogState)));
            dialogSet.Add(new ProfileDialog());
            dialogSet.Add(new ProfileWithChoiceDialog());
            dialogSet.Add(new WeatherDialog());
        }
              
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {            
            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            var results = await dialogContext.ContinueDialogAsync(cancellationToken);
            if (results.Status == DialogTurnStatus.Empty)
            {
                var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);
                switch (recognizerResult.GetTopScoringIntent().intent)
                {
                    case "Weather":
                        string day = recognizerResult.Entities["day"]?.FirstOrDefault()?.FirstOrDefault()?.ToString();
                        await dialogContext.BeginDialogAsync(nameof(WeatherDialog), day, cancellationToken);
                        break;
                    case "Profile":
                        await dialogContext.BeginDialogAsync(nameof(ProfileWithChoiceDialog), new Profile(), cancellationToken);
                        break;
                    default:
                        break;
                }
            }
            else if (results.Status == DialogTurnStatus.Complete)
            {
                if (results.Result is Profile)
                {
                    var profile = (Profile)results.Result;
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"ようこそ{profile.PetPreference}の{profile.Name}さん"));
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"何をしますか？"));
                }
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
