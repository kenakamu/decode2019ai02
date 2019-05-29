using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyBot.Dialogs;
using MyBot.Models;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class D_ProfileWithChoiceDialogUnitTest
    {
        [TestMethod]
        public async Task ShouldAskPetPreferenceWithInvalidInput()
        {
            // ステート管理
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);

            // アダプタ作成とステート保存ミドルウェア登録
            var adapter = new TestAdapter();
            adapter.Use(new AutoSaveStateMiddleware(conversationState));

            // ダイアログの管理
            var dialogState =
                conversationState.CreateProperty<DialogState>("DialogState");
            var dialogs = new DialogSet(dialogState);
            // ダイアログの追加
            dialogs.Add(new ProfileWithChoiceDialog());

            var testFlow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dialogContext = await dialogs.CreateContextAsync(
                    turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(
                    cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    results = await dialogContext.BeginDialogAsync(
                        nameof(ProfileWithChoiceDialog),
                        new Profile(),
                        cancellationToken);
                }

                if (results.Status == DialogTurnStatus.Complete)
                {
                    await turnContext.SendActivityAsync("Done");
                }
            });

            await testFlow
                .Test("開始", "名前は？")
                .Test("中村", "🐈派？🐕派？ (1) 🐈派 or (2) 🐕派")
                .Test("🐇派",
                    "🐈か🐕で答えてください。 (1) 🐈派 or (2) 🐕派")
                .Send("🐈派")
                .AssertReply(activity =>
                {
                    Assert.AreEqual("Done", (activity as Activity).Text);
                })
                .StartTestAsync();
        }
    }
}
