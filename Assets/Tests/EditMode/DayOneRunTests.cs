using LastHope.Core;
using NUnit.Framework;

namespace LastHope.Tests
{
    public sealed class DayOneRunTests
    {
        [Test]
        public void ColdOpenRequiresShelterInteractionsInOrder()
        {
            var run = new DayOneRun();

            Assert.That(run.Interact("door"), Is.False);
            Assert.That(run.Interact("radio"), Is.True);
            Assert.That(run.Interact("storage"), Is.True);
            Assert.That(run.Interact("filter_unit"), Is.True);
            Assert.That(run.Step, Is.EqualTo(DayOneStep.LeaveShelter));
        }

        [Test]
        public void OutsideTimeAndExposureOnlyAdvanceOutside()
        {
            var run = ReadyToLeave();
            run.AdvanceOutside(2f, 5f);
            Assert.That(run.Hour, Is.EqualTo(6f));

            run.Interact("door");
            run.AdvanceOutside(2f, 5f);

            Assert.That(run.Hour, Is.EqualTo(8f));
            Assert.That(run.Exposure, Is.EqualTo(10f));
        }

        [Test]
        public void PlayerCanReturnAfterFilterAndCompleteDay()
        {
            var run = ReadyToLeave();
            run.Interact("door");
            run.Interact("near_loot");

            Assert.That(run.Interact("door"), Is.True);
            Assert.That(run.Step, Is.EqualTo(DayOneStep.SpendEvening));
            Assert.That(run.Interact("workbench"), Is.True);
            Assert.That(run.Step, Is.EqualTo(DayOneStep.Complete));
            Assert.That(run.Filters, Is.Zero);
        }

        private static DayOneRun ReadyToLeave()
        {
            var run = new DayOneRun();
            run.Interact("radio");
            run.Interact("storage");
            run.Interact("filter_unit");
            return run;
        }
    }
}
