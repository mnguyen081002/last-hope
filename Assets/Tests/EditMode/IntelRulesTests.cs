using LastHope.Core.Rules;
using LastHope.Core.State;
using LastHope.Data;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class IntelRulesTests
    {
        private static IntelBalance Cfg() => new IntelBalance
        {
            ConfirmedToReliableMinutes = 60,
            ReliableToUncertainMinutes = 180,
        };

        private static IntelRecord Record(IntelConfidence confidence, long observedAt) => new IntelRecord
        {
            SubjectId = "route_a",
            Kind = "route",
            Confidence = confidence,
            ObservedAtMinute = observedAt,
        };

        [Test]
        public void EffectiveConfidence_FreshRecord_Unchanged()
        {
            var record = Record(IntelConfidence.Confirmed, 100);
            Assert.AreEqual(IntelConfidence.Confirmed, IntelRules.EffectiveConfidence(record, 100, Cfg()));
        }

        [Test]
        public void EffectiveConfidence_PastFirstThreshold_DropsOneStep()
        {
            var record = Record(IntelConfidence.Confirmed, 0);
            Assert.AreEqual(IntelConfidence.Reliable, IntelRules.EffectiveConfidence(record, 60, Cfg()));
        }

        [Test]
        public void EffectiveConfidence_PastSecondThreshold_DropsTwoSteps()
        {
            var record = Record(IntelConfidence.Confirmed, 0);
            // Confirmed(3) - 2 steps = Uncertain(1), not Unverified — 2 steps of decay from the
            // top of the 4-value scale doesn't reach the floor.
            Assert.AreEqual(IntelConfidence.Uncertain, IntelRules.EffectiveConfidence(record, 180, Cfg()));
        }

        [Test]
        public void EffectiveConfidence_FloorsAtUnverified_NeverGoesNegative()
        {
            var record = Record(IntelConfidence.Uncertain, 0); // already low; 2-step decay would go negative
            Assert.AreEqual(IntelConfidence.Unverified, IntelRules.EffectiveConfidence(record, 500, Cfg()));
        }

        [Test]
        public void EffectiveConfidence_ReliableRecord_AlsoDecays()
        {
            var record = Record(IntelConfidence.Reliable, 0);
            Assert.AreEqual(IntelConfidence.Uncertain, IntelRules.EffectiveConfidence(record, 60, Cfg()));
        }

        [Test]
        public void ShouldReplace_NullExisting_AlwaysTrue()
        {
            Assert.IsTrue(IntelRules.ShouldReplace(null, Record(IntelConfidence.Uncertain, 10)));
        }

        [Test]
        public void ShouldReplace_NewerObservation_True()
        {
            var existing = Record(IntelConfidence.Confirmed, 10);
            var incoming = Record(IntelConfidence.Uncertain, 20);
            Assert.IsTrue(IntelRules.ShouldReplace(existing, incoming));
        }

        [Test]
        public void ShouldReplace_OlderObservation_False()
        {
            var existing = Record(IntelConfidence.Confirmed, 20);
            var incoming = Record(IntelConfidence.Uncertain, 10);
            Assert.IsFalse(IntelRules.ShouldReplace(existing, incoming));
        }

        [Test]
        public void ShouldReplace_SameMinute_True()
        {
            var existing = Record(IntelConfidence.Uncertain, 10);
            var incoming = Record(IntelConfidence.Confirmed, 10);
            Assert.IsTrue(IntelRules.ShouldReplace(existing, incoming));
        }
    }
}
