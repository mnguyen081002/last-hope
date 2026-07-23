namespace LastHope.Core.Random
{
    /// <summary>
    /// Mutable state cell for one named RNG stream. Lives inside WorldState so it survives
    /// serialization and the stream resumes bit-exactly after load (never re-rolled).
    /// </summary>
    public sealed class RngStreamState
    {
        public ulong State { get; set; }
    }

    /// <summary>
    /// xorshift64* generator over an explicit, serializable ulong state (technical-specification.md
    /// mục 9/§32 determinism). Wraps a shared RngStreamState reference — construction is cheap.
    /// </summary>
    public sealed class RngStream
    {
        private readonly RngStreamState _state;

        public RngStream(RngStreamState state)
        {
            _state = state;
            if (_state.State == 0) _state.State = 1; // xorshift64* is fixed-point at 0
        }

        private ulong NextRaw()
        {
            ulong x = _state.State;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            _state.State = x;
            return x * 0x2545F4914F6CDD1DUL;
        }

        /// <summary>Uniform integer in [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextRaw() % range);
        }

        /// <summary>Uniform double in [0, 1).</summary>
        public double NextDouble()
        {
            return (NextRaw() >> 11) * (1.0 / (1UL << 53));
        }
    }
}
