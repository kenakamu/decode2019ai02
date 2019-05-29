using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MyBot.Dialogs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class E_MyBotWithLuisUnitTest
    {
        private (TestFlow testFlow, TestAdapter adapter, 
            ConversationState conversationState) Arrange()
        {
            // ステート管理
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);

            // アダプタ作成とステート保存ミドルウェア登録
            var adapter = new TestAdapter();
            adapter.Use(new AutoSaveStateMiddleware(conversationState));

            // IRecognizer のモック
            var mockRecognizer = new Mock<IRecognizer>();
            mockRecognizer.Setup(x => x.RecognizeAsync(
                It.IsAny<ITurnContext<IMessageActivity>>(),
                It.IsAny<CancellationToken>()))
                .Returns((ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken) =>
                {
                    // RecognizerResult の作成
                    var recognizerResult = new RecognizerResult()
                    {
                        Intents = new Dictionary<string, IntentScore>(),
                        Entities = new JObject()
                    };

                    switch (turnContext.Activity.Text)
                    {
                        case "天気を確認":
                            recognizerResult.Intents.Add("Weather", new IntentScore() { Score = 1 });
                            break;
                        case "今日の天気を確認":
                            recognizerResult.Intents.Add("Weather", new IntentScore() { Score = 1 });
                            recognizerResult.Entities.Add("day", JArray.Parse("[['今日']]"));
                            break;
                        case "プロファイルの変更":
                            recognizerResult.Intents.Add("Profile", new IntentScore() { Score = 1 });
                            break;
                        default:
                            recognizerResult.Intents.Add("None", new IntentScore() { Score = 1 });
                            break;
                    }
                    return Task.FromResult(recognizerResult);
                });

            var bot = new MyBotWithLuis(conversationState, userState, mockRecognizer.Object);
            
            // Tuple で要素を返す
            return (new TestFlow(adapter, bot.OnTurnAsync), adapter, conversationState);
        }

        [TestMethod]
        [DataRow("今日")]
        [DataRow("明日")]
        [DataRow("明後日")]
        public async Task ShouldDispatchToWeatherDialog(string day)
        {
            var testFlow = Arrange().testFlow;
            await testFlow.Test("天気を確認", 
                "いつの天気を知りたいですか？ (1) 今日, (2) 明日, or (3) 明後日")
                .Test(day, $"{day}の天気は晴れです")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldDispatchToWeatherDialogWithEntity()
        {
            var testFlow = Arrange().testFlow;
            await testFlow.Test("今日の天気を確認", "今日の天気は晴れです")
                .StartTestAsync();
        }

        [TestMethod]
        public async Task ShouldDispatchToProfile()
        {
            var arrange = Arrange();
            var dialogs = new DialogSet(arrange.conversationState.CreateProperty<DialogState>("DialogState"));
            await arrange.testFlow.Send("プロファイルの変更")
                .AssertReply(activity =>
                {
                    // Activity とアダプターからコンテキストを作成
                    var turnContext = new TurnContext(arrange.adapter, activity as Activity);
                    // ダイアログコンテキストを取得
                    var dc = dialogs.CreateContextAsync(turnContext).Result;
                    Assert.AreEqual(nameof(ProfileWithChoiceDialog), dc.ActiveDialog.Id);
                })
                .StartTestAsync();
        }

    }
}
