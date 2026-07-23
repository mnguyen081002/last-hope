using LastHope.Core.State;

namespace LastHope.Core.Random
{
    /// <summary>
    /// Named deterministic RNG streams derived from the world's master seed (e.g. "loot", "events",
    /// "npc"). Extra rolls in one stream never shift another stream's sequence.
    /// </summary>
    public sealed class RngService
    {
        private readonly WorldState _world;

        public RngService(WorldState world)
        {
            _world = world;
        }

        public RngStream GetStream(string name)
        {
            if (!_world.RngStreams.TryGetValue(name, out var state))
            {
                state = new RngStreamState { State = SplitMix64(_world.RandomSeed ^ Fnv1a64(name)) };
                _world.RngStreams[name] = state;
            }
            return new RngStream(state);
        }

        private static ulong SplitMix64(ulong seed)
        {
            ulong z = seed + 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        private static ulong Fnv1a64(string text)
        {
            const ulong offsetBasis = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offsetBasis;
            foreach (char c in text)
            {
                hash ^= c;
                hash *= prime;
            }
            return hash;
        }
    }
}
