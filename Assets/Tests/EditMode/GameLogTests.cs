using LastHope.Core.Diagnostics;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class GameLogTests
    {
        LogCategory saved;

        [SetUp]
        public void SetUp() => saved = GameLog.Enabled;

        [TearDown]
        public void TearDown() => GameLog.Enabled = saved;

        [Test]
        public void AllCategories_EnabledByDefault()
        {
            GameLog.Enabled = LogCategory.All;

            Assert.IsTrue(GameLog.IsEnabled(LogCategory.Boot));
            Assert.IsTrue(GameLog.IsEnabled(LogCategory.Save));
        }

        [Test]
        public void DisablingOneCategory_LeavesOthersEnabled()
        {
            GameLog.Enabled = LogCategory.All & ~LogCategory.Time;

            Assert.IsFalse(GameLog.IsEnabled(LogCategory.Time));
            Assert.IsTrue(GameLog.IsEnabled(LogCategory.Boot));
        }

        [Test]
        public void NoneCategory_DisablesEverything()
        {
            GameLog.Enabled = LogCategory.None;

            Assert.IsFalse(GameLog.IsEnabled(LogCategory.Boot));
            Assert.IsFalse(GameLog.IsEnabled(LogCategory.Camera));
        }
    }
}
