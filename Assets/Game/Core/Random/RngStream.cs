namespace LastHope.Core.Random
{
    /// <summary>
    /// xorshift64* — PRNG có state phơi ra được (<see cref="State"/>), nên serialize vào
    /// save là load xong chạy tiếp đúng bit. <c>System.Random</c> không làm được việc này.
    /// </summary>
    public class RngStream
    {
        public ulong State;

        public RngStream(ulong seed)
        {
            // State 0 làm xorshift đứng yên vĩnh viễn.
            State = seed == 0UL ? 0x9E3779B97F4A7C15UL : seed;
        }

        public ulong NextULong()
        {
            ulong x = State;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            State = x;
            return x * 0x2545F4914F6CDD1DUL;
        }

        /// <summary>Số thực trong [0,1).</summary>
        public double NextDouble() => (NextULong() >> 11) * (1.0 / 9007199254740992.0);

        /// <summary>Số nguyên trong [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            ulong range = (ulong)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextULong() % range);
        }

        /// <summary>Số nguyên trong [minInclusive, maxInclusive].</summary>
        public int NextIntInclusive(int minInclusive, int maxInclusive) =>
            NextInt(minInclusive, maxInclusive + 1);

        /// <summary><paramref name="percent"/> tính theo thang 0–100.</summary>
        public bool NextChance(float percent)
        {
            if (percent <= 0f) return false;
            if (percent >= 100f) return true;
            return NextDouble() * 100.0 < percent;
        }
    }
}
