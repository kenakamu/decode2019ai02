using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class G_GraphBotUnitTest
    {
        [TestMethod]
        public async Task ShouldReplySchedule()
        {
            // ステート管理
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);

            var adapter = new TestAdapter();
            adapter.AddUserToken("AADv2", "test", "user1", "dummyToken");

            // Microsoft Graph 系のモック
            var mockGraphSDK = new Mock<IGraphServiceClient>();
            // ダミーの予定を返す。
            DateTime datetime = DateTime.Now;
            mockGraphSDK.Setup(x => x.Me.CalendarView.Request(It.IsAny<List<QueryOption>>()).GetAsync())
                .ReturnsAsync(() =>
                {
                    var page = new UserCalendarViewCollectionPage();
                    page.Add(new Event()
                    {
                        Subject = "Dummy 1",
                        Start = new DateTimeTimeZone() { DateTime = datetime.ToString() },
                        End = new DateTimeTimeZone() { DateTime = datetime.AddMinutes(30).ToString() }
                    });
                    page.Add(new Event()
                    {
                        Subject = "Dummy 2",
                        Start = new DateTimeTimeZone() { DateTime = datetime.AddMinutes(60).ToString() },
                        End = new DateTimeTimeZone() { DateTime = datetime.AddMinutes(90).ToString() }
                    });
                    return page;
                });
            var msGraphService = new MSGraphService(mockGraphSDK.Object);

            var bot = new GraphBot(conversationState, userState, msGraphService);

            await new TestFlow(adapter, bot.OnTurnAsync)
                .Send("予定を知りたい")
                .AssertReply($"{datetime.ToString("HH:mm")}-{datetime.AddMinutes(30).ToString("HH:mm")} : Dummy 1")
                .AssertReply($"{datetime.AddMinutes(60).ToString("HH:mm")}-{datetime.AddMinutes(90).ToString("HH:mm")} : Dummy 2")
                .StartTestAsync();
        }
    }
}
