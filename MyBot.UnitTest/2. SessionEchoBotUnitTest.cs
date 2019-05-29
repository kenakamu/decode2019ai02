using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class B_SessionEchoBotUnitTest
    {
        [TestMethod]
        public async Task ShouldReplyEchoWithCount()
        {
            // ステート管理
            IStorage dataStore = new MemoryStorage();
            var conversationState = new ConversationState(dataStore);
            var userState = new UserState(dataStore);

            var bot = new SessionEchoBot(conversationState, userState);
            await new TestFlow(new TestAdapter(), bot.OnTurnAsync)
                .Test("Hi", "Hi 1")
                .Test("Yo", "Yo 2")
                .StartTestAsync();
        }
    }
}
