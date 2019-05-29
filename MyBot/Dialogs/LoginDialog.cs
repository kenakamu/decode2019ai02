using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace MyBot.Dialogs
{
    public class LoginDialog : ComponentDialog
    {
        public LoginDialog() : base(nameof(LoginDialog))
        {
            // ウォーターフォールのステップを定義。処理順にメソッドを追加。
            var waterfallSteps = new WaterfallStep[]
            {
                LoginStepAsync,
                ReturnTokenAsync,
            };

            // ウォーターフォールダイアログと各種プロンプトを追加
            AddDialog(new WaterfallDialog("login", waterfallSteps));
            AddDialog(new OAuthPrompt(nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = "AADv2",
                    Text = "ログインしてください",
                    Title = "ログイン",
                    Timeout = 300000,
                }));
        }

        private static async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private static async Task<DialogTurnResult> ReturnTokenAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                return await stepContext.EndDialogAsync(tokenResponse.Token, cancellationToken);

            }
            else
                await stepContext.Context.SendActivityAsync($"サインインに失敗しました。", cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
