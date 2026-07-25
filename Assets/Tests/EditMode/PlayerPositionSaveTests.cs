using LastHope.Core.Save;
using LastHope.Core.State;
using NUnit.Framework;

namespace LastHope.Tests.EditMode
{
    public class PlayerPositionSaveTests
    {
        [Test]
        public void PlayerPosition_SurvivesCanonicalRoundtrip()
        {
            var world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            world.Player.PositionX = 1.5f;
            world.Player.PositionY = 0.1f;
            world.Player.PositionLocationId = "location_shelter";

            string json = WorldStateSerializer.SerializeCanonical(world);
            WorldState roundtripped = WorldStateSerializer.Deserialize(json);

            Assert.AreEqual(world.Player.PositionX, roundtripped.Player.PositionX);
            Assert.AreEqual(world.Player.PositionY, roundtripped.Player.PositionY);
            Assert.AreEqual(world.Player.PositionLocationId, roundtripped.Player.PositionLocationId);
        }
    }
}
