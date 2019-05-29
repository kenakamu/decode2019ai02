using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyBot.UnitTest
{
    [TestClass]
    public class F_WelcomeBotUnitTest
    {
        [TestMethod]
        public async Task ShouldReplyWelcomeCardOnMemberAdd()
        {
            var bot = new WelcomeBot();

            // システムメッセージを作成
            var conversationUpdateActivity = new Activity(ActivityTypes.ConversationUpdate)
            {
                Id = "test",
                From = new ChannelAccount("TestUser", "Test User"),
                ChannelId = "UnitTest",
                ServiceUrl = "https://example.org",
                MembersAdded = new List<ChannelAccount>() { new ChannelAccount("TestUser", "Test User") }
            };

            await new TestFlow(new TestAdapter(), bot.OnTurnAsync)
              .Send(conversationUpdateActivity)
              .AssertReply(activity =>
              {
                var attachment = (activity as Activity).Attachments.FirstOrDefault();
                Assert.IsTrue(attachment != null);
                Assert.AreEqual(
                JObject.Parse(attachment.Content.ToString()).ToString(),
                JObject.Parse(File.ReadAllText($"./AdaptiveCards/Welcome.json")).ToString());
              })
              .StartTestAsync();
        }
    }
}
