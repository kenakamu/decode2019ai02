using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyBot.Dialogs
{
    public class ScheduleDialog : ComponentDialog
    {
        private MSGraphService graphClient;
        public ScheduleDialog(MSGraphService graphClient) : base(nameof(ScheduleDialog))
        {
            this.graphClient = graphClient;
            // ウォーターフォールのステップを定義。処理順にメソッドを追加。
            var waterfallSteps = new WaterfallStep[]
            {
                LoginStepAsync,
                ReturnScheduleAsync,
            };

            // ウォーターフォールダイアログと各種プロンプトを追加
            AddDialog(new WaterfallDialog("schedule", waterfallSteps));
            AddDialog(new LoginDialog());
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(LoginDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> ReturnScheduleAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var accessToken = stepContext.Result.ToString();
            if (accessToken != null)
            {
                this.graphClient.Token = accessToken;
                var events = await graphClient.GetScheduleAsync();
                events.ForEach(async x =>
                {
                    await stepContext.Context.SendActivityAsync($"{System.DateTime.Parse(x.Start.DateTime).ToString("HH:mm")}-{System.DateTime.Parse(x.End.DateTime).ToString("HH:mm")} : {x.Subject}", cancellationToken: cancellationToken);
                });
            }
            else
                await stepContext.Context.SendActivityAsync($"サインインに失敗しました。", cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(true, cancellationToken);
        }
    }
}
