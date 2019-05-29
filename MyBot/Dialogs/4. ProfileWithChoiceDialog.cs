using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using MyBot.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyBot.Dialogs
{
    public class ProfileWithChoiceDialog : ComponentDialog
    {
        public ProfileWithChoiceDialog() : base(nameof(ProfileWithChoiceDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
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
            return await stepContext.PromptAsync(nameof(ChoicePrompt), 
                new PromptOptions {
                    Prompt = MessageFactory.Text("🐈派？🐕派？"),
                    Choices = ChoiceFactory.ToChoices(new List<string>() { "🐈派", "🐕派" }),
                    RetryPrompt = MessageFactory.Text("🐈か🐕で答えてください。")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var profile = (Profile)stepContext.Options;
            var petPreference = (FoundChoice)stepContext.Result;
            profile.PetPreference = petPreference.Value;

            return await stepContext.EndDialogAsync(profile, cancellationToken);
        }
    }
}
