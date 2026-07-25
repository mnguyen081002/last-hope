namespace LastHope.Data.Definitions
{
    /// <summary>
    /// A recruitable NPC's static profile (npc-framework §2-3, S15 foundation — S16 adds the
    /// commands/system that actually use SkillMultiplier/StartingTrust; S15 only needs the
    /// definition to exist so NpcState has something to seed from).
    /// </summary>
    public sealed class NpcDefinition : DefinitionBase
    {
        public string DisplayName { get; set; }
        public string Skill { get; set; } // e.g. "electric" — read by task-assignment in S16

        /// <summary>Task duration multiplier for tasks matching Skill (e.g. 1.5 = 50% faster).</summary>
        public float SkillMultiplier { get; set; } = 1f;

        public int StartingTrust { get; set; }
        public string StartingLocationId { get; set; }
    }
}
