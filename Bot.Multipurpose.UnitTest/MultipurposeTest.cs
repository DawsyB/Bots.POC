using System;
//using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bots.POC.UnitTest.Infrastructure;
using Bot.Multipurpose.Dialogs;
using System.Collections.Generic;
using Bot.Multipurpose;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bots.POC.UnitTest
{
    [TestClass]
    public class MultipurposeTest : LuisTestBase
    {
        [TestMethod]
        public async Task RootLuisDialogFlow()
        {
            try
            {
                var luis = new Mock<ILuisService>();

                //arrange
                var now = DateTime.UtcNow;
                var entity_LuisQuery = EntityFor("CardDesigns::AdaptiveCard", "adaptive");

                Func<IDialog<object>> MakeRoot = () => new RootLuisDialog(luis.Object);
                var toBot = MakeTestMessage();
                toBot.Text = string.Empty;

                using (new FiberTestBase.ResolveMoqAssembly(luis.Object))
                using (var container = Build(Options.ScopedQueue, luis.Object))
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        // arrange
                        SetupLuis<RootLuisDialog>(luis, a => a.CardDesigns(null, null, null), 1.0, entity_LuisQuery);

                        // act
                        await task.PostAsync(toBot, CancellationToken.None);

                        // assert
                        luis.VerifyAll();
                        AssertMentions("Inside CardDesigns", scope);
                    }
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeRoot);

                        var task = scope.Resolve<IPostToBot>();

                        // arrange
                        LuisResult luisResult = new LuisResult();
                        luisResult.Query = "What comes before B";
                        //luisResult.Intents = "None";
                        SetupLuis<RootLuisDialog>(luis, a => a.None(null, null, null), 1.0);

                        // act
                        await task.PostAsync(toBot, CancellationToken.None);

                        // assert
                        luis.VerifyAll();
                        AssertMentions("Hooray!! I don't know what to reply. My developer has not designed me to handle this intent?", scope);
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception thrown: " + ex.Message);
            }

        }
    }
}
