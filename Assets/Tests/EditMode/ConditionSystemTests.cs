using LastHope.Core.State;
using LastHope.Data.Definitions;
using LastHope.Systems.Condition;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class ConditionSystemTests
    {
        PlayerState player;
        ConditionBalance balance;

        [SetUp]
        public void SetUp()
        {
            player = new PlayerState();
            balance = new ConditionBalance();
        }

        [Test]
        public void ShortTick_IncreasesThirstAndHunger_ByPerMinuteRate()
        {
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(balance.ThirstPerHour / 60f, player.Thirst, 0.0001f);
            Assert.AreEqual(balance.HungerPerHour / 60f, player.Hunger, 0.0001f);
        }

        [Test]
        public void ShortTick_ClampsThirstAndHungerAt100()
        {
            player.Thirst = 99.99f;
            player.Hunger = 99.99f;

            for (int i = 0; i < 1000; i++) ConditionSystem.ApplyShortTick(player, balance, false);

            Assert.AreEqual(100f, player.Thirst);
            Assert.AreEqual(100f, player.Hunger);
        }

        [Test]
        public void LongTick_IncreasesFatigue()
        {
            ConditionSystem.ApplyLongTick(player, balance);

            Assert.AreEqual(balance.FatiguePerLongTick, player.Fatigue, 0.0001f);
        }

        [Test]
        public void Wet_DriesAtShelter_NotOutside()
        {
            player.Wet = 50f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);
            Assert.AreEqual(50f - balance.WetDryPerMinuteAtShelter, player.Wet, 0.0001f);

            float wetOutside = player.Wet;
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);
            Assert.AreEqual(wetOutside, player.Wet, 0.0001f, "Ngoài shelter Wet không tự khô.");
        }

        [Test]
        public void Wet_NeverGoesNegative()
        {
            player.Wet = 1f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);

            Assert.AreEqual(0f, player.Wet);
        }

        [Test]
        public void Wet_GainsFromRain_WhenRainingAndNotAtShelter()
        {
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false, isRaining: true);

            Assert.AreEqual(balance.WetGainPerMinuteInRain, player.Wet, 0.0001f);
        }

        [Test]
        public void Wet_RainGain_ScaledByJacketMultiplier()
        {
            ConditionSystem.ApplyShortTick(
                player, balance, isAtShelter: false, isRaining: true, wetMultiplier: 0.3f);

            Assert.AreEqual(balance.WetGainPerMinuteInRain * 0.3f, player.Wet, 0.0001f);
        }

        [Test]
        public void Wet_NoRainGain_WhenNotRaining()
        {
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false, isRaining: false);

            Assert.AreEqual(0f, player.Wet);
        }

        [Test]
        public void BodyTemperature_DriftsDown_WhenWetAboveThreshold()
        {
            player.Wet = balance.WetThresholdForTempDrift;
            float before = player.BodyTemperature;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(before - balance.BodyTempDriftDownPerMinute, player.BodyTemperature, 0.0001f);
        }

        [Test]
        public void BodyTemperature_StaysPut_WhenDryAndNotAtShelter()
        {
            player.Wet = 0f;
            float before = player.BodyTemperature;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(before, player.BodyTemperature, 0.0001f);
        }

        [Test]
        public void BodyTemperature_RegensAtShelter_WhenDry()
        {
            player.Wet = 0f;
            player.BodyTemperature = 36f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);

            Assert.AreEqual(36f + balance.BodyTempRegenAtShelterPerMinute, player.BodyTemperature, 0.0001f);
        }

        [Test]
        public void BodyTemperature_RegenCapsAt37()
        {
            player.Wet = 0f;
            player.BodyTemperature = 36.99f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);

            Assert.AreEqual(37f, player.BodyTemperature);
        }

        [Test]
        public void ColdFlag_HasHysteresis_DoesNotFlickerBetweenThresholds()
        {
            player.BodyTemperature = 34f; // <= 35 (enter threshold)
            ConditionSystem.ApplyShortTick(player, balance, false);
            Assert.IsTrue(player.IsCold);

            // Ấm lên nhưng chưa vượt ngưỡng tắt (36) -> vẫn phải Cold.
            player.Wet = 0f;
            player.BodyTemperature = 35.5f;
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);
            Assert.IsTrue(player.IsCold, "Giữa hai ngưỡng phải giữ nguyên trạng thái (hysteresis).");

            player.BodyTemperature = 36f; // đủ ngưỡng tắt
            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: true);
            Assert.IsFalse(player.IsCold);
        }

        [Test]
        public void Stamina_RegensHalved_WhenFatigueHigh()
        {
            player.Stamina = 0f;
            player.Fatigue = 60f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(
                balance.StaminaRegenPerMinute * balance.StaminaRegenHalvedMultiplier,
                player.Stamina, 0.0001f);
        }

        [Test]
        public void Stamina_RegensFull_WhenConditionsGood()
        {
            player.Stamina = 0f;
            player.Fatigue = 0f;
            player.Thirst = 0f;
            player.IsCold = false;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(balance.StaminaRegenPerMinute, player.Stamina, 0.0001f);
        }

        [Test]
        public void Starvation_DecaysHealth_WhenHungerMaxed_StopsAtFloor()
        {
            player.Hunger = 100f;
            player.Health = balance.StarvationHealthFloor + balance.StarvationHealthDecayPerLongTick * 0.5f;

            ConditionSystem.ApplyLongTick(player, balance);

            Assert.AreEqual(balance.StarvationHealthFloor, player.Health,
                "Starvation không được giết chết player — chỉ dừng ở floor.");
        }

        [Test]
        public void Starvation_DoesNotHeal_IfHealthAlreadyBelowFloorFromOtherSource()
        {
            // Sick không có floor, có thể đưa Health xuống dưới StarvationHealthFloor.
            // Starvation tick sau đó không được "hồi" nó lên floor.
            player.Hunger = 100f;
            player.Health = balance.StarvationHealthFloor - 0.5f;

            ConditionSystem.ApplyLongTick(player, balance);

            Assert.AreEqual(balance.StarvationHealthFloor - 0.5f, player.Health,
                "Floor chỉ chặn CHÍNH starvation kéo xuống, không hồi máu từ nguồn khác.");
        }

        [Test]
        public void Starvation_TriggersOnThirstToo()
        {
            player.Thirst = 100f;
            float before = player.Health;

            ConditionSystem.ApplyLongTick(player, balance);

            Assert.Less(player.Health, before);
        }

        [Test]
        public void Sick_TriggersAtExposureThreshold_AndDecaysHealthWithNoFloor()
        {
            player.BlackWaterExposure = balance.SickExposureThreshold;
            player.Health = balance.SickDecayPerMinute * 0.5f;
            player.Wet = 0f; // tránh body temp drift làm Cold bật, không liên quan test này

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.IsTrue(player.IsSick);
            Assert.AreEqual(0f, player.Health, "Sick không có floor — có thể xuống 0.");
        }

        [Test]
        public void Sick_AlsoAcceleratesThirstAndHunger_SameRateAsHealthDecay()
        {
            player.BlackWaterExposure = balance.SickExposureThreshold;
            player.Thirst = 0f;
            player.Hunger = 0f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            float expectedThirst = balance.ThirstPerHour / 60f + balance.SickDecayPerMinute;
            float expectedHunger = balance.HungerPerHour / 60f + balance.SickDecayPerMinute;
            Assert.AreEqual(expectedThirst, player.Thirst, 0.0001f);
            Assert.AreEqual(expectedHunger, player.Hunger, 0.0001f);
        }

        [Test]
        public void Sick_HealthDecay_AppliesEveryMinute_NotJustLongTick()
        {
            player.BlackWaterExposure = balance.SickExposureThreshold;
            player.Health = 100f;

            ConditionSystem.ApplyShortTick(player, balance, isAtShelter: false);

            Assert.AreEqual(100f - balance.SickDecayPerMinute, player.Health, 0.0001f);
        }

        [Test]
        public void IsCollapsed_TrueAtOrBelowThreshold()
        {
            player.Health = balance.CollapsedHealthThreshold;
            Assert.IsTrue(ConditionSystem.IsCollapsed(player, balance));

            player.Health = balance.CollapsedHealthThreshold + 0.01f;
            Assert.IsFalse(ConditionSystem.IsCollapsed(player, balance));
        }
    }
}
