using System.Collections.Generic;

namespace LastHope.Core.Random
{
    /// <summary>
    /// Quản lý các stream RNG đặt tên. Mỗi stream có seed riêng derive từ master seed nên
    /// rút số ở "loot" không làm xê dịch chuỗi của "events" — đổi thứ tự gameplay không
    /// phá tính tái lập.
    /// </summary>
    public class RngService
    {
        public const string Loot = "loot";
        public const string Events = "events";
        public const string Npc = "npc";

        readonly Dictionary<string, RngStream> streams = new();
        readonly Dictionary<string, ulong> backing;

        public ulong MasterSeed { get; }

        /// <param name="backing">
        /// Dictionary state nằm trong WorldState. Service ghi thẳng vào đây mỗi lần rút số
        /// để save luôn bắt được state mới nhất.
        /// </param>
        public RngService(ulong masterSeed, Dictionary<string, ulong> backing)
        {
            MasterSeed = masterSeed;
            this.backing = backing;
        }

        public RngStream Stream(string name)
        {
            if (streams.TryGetValue(name, out var existing)) return existing;

            ulong seed = backing != null && backing.TryGetValue(name, out ulong saved)
                ? saved
                : DeriveSeed(MasterSeed, name);

            var stream = new RngStream(seed);
            streams[name] = stream;
            return stream;
        }

        /// <summary>Đẩy state hiện tại của mọi stream về WorldState. Gọi trước khi save.</summary>
        public void FlushState()
        {
            if (backing == null) return;
            foreach (var pair in streams) backing[pair.Key] = pair.Value.State;
        }

        public static ulong DeriveSeed(ulong masterSeed, string name) =>
            masterSeed ^ Fnv1A(name);

        static ulong Fnv1A(string value)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            ulong hash = offset;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }
            return hash;
        }
    }
}
