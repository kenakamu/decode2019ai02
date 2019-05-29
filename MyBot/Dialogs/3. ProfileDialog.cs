using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MyBot.Models;

namespace MyBot.Dialogs
{
    public class ProfileDialog : ComponentDialog
    {
        public ProfileDialog() : base(nameof(ProfileDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskNameStepAsync,
                AskPetPreferenceStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var profile = (Profile)stepContext.Options;
            return await stepContext.PromptAsync(nameof(TextPrompt), 
                new PromptOptions { Prompt = MessageFactory.Text("名前は？") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskPetPreferenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var profile = (Profile)stepContext.Options;
            var name = (string)stepContext.Result;
            profile.Name = name;
            return await stepContext.PromptAsync(nameof(TextPrompt), 
                new PromptOptions {
                    Prompt = MessageFactory.Text("🐈派？🐕派？"),
                    RetryPrompt = MessageFactory.Text("🐈か🐕で答えてください。")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var profile = (Profile)stepContext.Options;
            var petPreference = (string)stepContext.Result;
            profile.PetPreference = petPreference;

            return await stepContext.EndDialogAsync(profile, cancellationToken);
        }
    }
}
