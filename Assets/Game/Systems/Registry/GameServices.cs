using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Telemetry;

namespace LastHope.Systems.Registry
{
    /// <summary>
    /// Bộ service của một phiên chơi. Tập cố định, không phải DI container tổng quát —
    /// thêm service là sửa class này, để luôn nhìn thấy toàn bộ phụ thuộc ở một chỗ.
    /// </summary>
    public class GameServices
    {
        public DefinitionRegistry Definitions { get; }
        public WorldState World { get; private set; }
        public EventBus Events { get; }
        public RngService Rng { get; private set; }
        public SimulationClock Clock { get; }
        public TickScheduler Ticks { get; private set; }
        public CommandProcessor Commands { get; private set; }
        public SaveService Saves { get; }
        public GameContext Context { get; private set; }
        public TelemetryLogger Telemetry { get; }

        public GameServices(
            DefinitionRegistry definitions, WorldState world, string saveDirectory, string telemetryDirectory)
        {
            Definitions = definitions;
            Events = new EventBus();
            Clock = new SimulationClock();
            Saves = new SaveService(saveDirectory, definitions.DefinitionVersion);

            BindWorld(world);

            Telemetry = new TelemetryLogger(this, telemetryDirectory);
        }

        /// <summary>
        /// Trỏ toàn bộ service sang một WorldState khác (new game / load save) rồi báo cho
        /// view đọc lại. Không tạo lại GameServices để mọi tham chiếu đang giữ vẫn đúng.
        /// </summary>
        public void BindWorld(WorldState world)
        {
            World = world;
            Rng = new RngService(world.MasterSeed, world.RngStreams);
            Ticks = new TickScheduler(world, Events);
            Context = new GameContext
            {
                World = world,
                Definitions = Definitions,
                Events = Events,
                Rng = Rng,
                Ticks = Ticks,
            };
            Commands = new CommandProcessor(Context);
        }

        public void ReloadWorld(WorldState world)
        {
            BindWorld(world);
            Events.Publish(new WorldStateReloaded());
        }

        /// <summary>Đẩy state RNG về WorldState rồi ghi save — thiếu bước này chuỗi RNG sẽ lặp lại.</summary>
        public void SaveTo(string slotId)
        {
            Rng.FlushState();
            Saves.Save(World, slotId);
        }

        public string SaveAutosave()
        {
            Rng.FlushState();
            return Saves.SaveAutosave(World);
        }

        public void LoadFrom(string slotId) => ReloadWorld(Saves.Load(slotId));
    }
}
