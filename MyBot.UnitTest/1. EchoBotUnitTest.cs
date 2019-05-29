using Microsoft.Bot.Builder.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class A_EchoBotUnitTest
    {
        [TestMethod]
        public async Task ShouldReplyEcho()
        {
            var bot = new EchoBot();
            await new TestFlow(new TestAdapter(), bot.OnTurnAsync)
                .Test("Hi", "Hi")
                .StartTestAsync();
        }
    }
}
