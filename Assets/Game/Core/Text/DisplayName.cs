namespace LastHope.Core.Text
{
    /// <summary>
    /// Turns a snake_case definition id into a readable display string
    /// (2026-07-24 feedback — every panel was showing raw ids like "module_barrier" verbatim).
    /// Lives in Core (not Presentation, where the original WorldLabel.Prettify was written) because
    /// LastHope.UI does not reference LastHope.Presentation — this is the lowest layer both share.
    /// No localization system exists yet (DefinitionBase.DisplayNameKey is declared but no content
    /// file has ever populated it), so this is the whole "display name" story for now.
    /// </summary>
    public static class DisplayName
    {
        /// <summary>"shelter_entrance" -> "Shelter Entrance".</summary>
        public static string Prettify(string snakeCaseId)
        {
            if (string.IsNullOrEmpty(snakeCaseId)) return snakeCaseId;

            string[] words = snakeCaseId.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
            }
            return string.Join(" ", words);
        }

        /// <summary>Strips a known category prefix before prettifying — used only where the
        /// prefix is purely a naming-scheme artifact with no meaning to the player (e.g. world
        /// label ids like "slot_shelter_entrance_1", already under a "Slot" heading line). Entity
        /// names shown standalone (module/item ids in Build/Shelter/Inventory panels) should use
        /// plain Prettify() instead, per 2026-07-24 feedback: "module_barrier" -> "Module Barrier".</summary>
        public static string PrettifyWithoutPrefix(string id, string prefix)
        {
            if (string.IsNullOrEmpty(id)) return id;
            string trimmed = id.StartsWith(prefix) ? id.Substring(prefix.Length) : id;
            return Prettify(trimmed);
        }
    }
}
