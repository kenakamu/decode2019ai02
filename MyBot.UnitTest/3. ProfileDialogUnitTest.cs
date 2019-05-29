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
    public class C_ProfileDialogUnitTest
    {
        [TestMethod]
        public async Task ShouldAskNameAndPetPreference()
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
            dialogs.Add(new ProfileDialog());

            var testFlow = new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dialogContext = await dialogs.CreateContextAsync(
                    turnContext, cancellationToken);

                var results = await dialogContext.ContinueDialogAsync(
                    cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    results = await dialogContext.BeginDialogAsync(
                        nameof(ProfileDialog),
                        new Profile(), // Profile の初期値
                        cancellationToken);
                }

                if (results.Status == DialogTurnStatus.Complete)
                {
                    await turnContext.SendActivityAsync("Done");
                }
            });

            await testFlow
                .Test("開始", "名前は？")
                .Test("中村", "🐈派？🐕派？")
                .Send("🐈派")
                .AssertReply(activity =>
                {
                    Assert.AreEqual("Done", (activity as Activity).Text);
                })
                .StartTestAsync();
        }        
    }
}
