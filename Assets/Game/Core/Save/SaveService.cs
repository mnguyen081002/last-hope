using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LastHope.Core.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastHope.Core.Save
{
    public sealed class SaveResult
    {
        public bool Success { get; private set; }
        public string Error { get; private set; }
        public string SlotId { get; private set; }

        public static SaveResult Ok(string slotId) => new SaveResult { Success = true, SlotId = slotId };
        public static SaveResult Fail(string error) => new SaveResult { Success = false, Error = error };
    }

    public sealed class LoadResult
    {
        public bool Success { get; private set; }
        public string Error { get; private set; }
        public WorldState World { get; private set; }

        public static LoadResult Ok(WorldState world) => new LoadResult { Success = true, World = world };
        public static LoadResult Fail(string error) => new LoadResult { Success = false, Error = error };
    }

    /// <summary>
    /// Versioned JSON save/load with atomic writes and rotating autosave slots
    /// (technical-specification.md mục 9/§29-32). Never silently resets world state on a bad
    /// load — every failure path returns a clear error instead.
    /// </summary>
    public sealed class SaveService
    {
        private const int CurrentSaveVersion = 3; // S8: RouteState restructured (Flood/Current/Contamination/Closed)
        private const int AutosaveSlotCount = 3;

        private readonly string _saveDirectory;
        private readonly string _definitionVersion;
        private int _nextAutosaveIndex;

        public SaveService(string saveDirectory, string definitionVersion)
        {
            _saveDirectory = saveDirectory;
            _definitionVersion = definitionVersion;
            Directory.CreateDirectory(_saveDirectory);
        }

        /// <summary>Round-robin across autosave_0/1/2 (technical-specification.md mục 9/§30).</summary>
        public SaveResult Autosave(WorldState world)
        {
            string slotId = $"autosave_{_nextAutosaveIndex}";
            _nextAutosaveIndex = (_nextAutosaveIndex + 1) % AutosaveSlotCount;
            return SaveToSlot(world, slotId);
        }

        public SaveResult SaveToSlot(WorldState world, string slotId)
        {
            try
            {
                string worldJson = WorldStateSerializer.SerializeCanonical(world);
                string checksum = ComputeChecksum(worldJson);

                var file = new SaveFile
                {
                    SaveVersion = CurrentSaveVersion,
                    DefinitionVersion = _definitionVersion,
                    SavedAtUtc = DateTime.UtcNow.ToString("O"),
                    Checksum = checksum,
                    SlotId = slotId,
                    World = new JRaw(worldJson),
                };
                string fileJson = JsonConvert.SerializeObject(file, Formatting.Indented);

                string finalPath = SlotPath(slotId);
                string tempPath = finalPath + ".tmp";
                string backupPath = finalPath + ".bak";

                File.WriteAllText(tempPath, fileJson);

                // Verify before touching the existing save: re-read, recheck checksum, deserialize.
                WorldState verified = ReadAndValidate(tempPath, out string verifyError);
                if (verified == null)
                {
                    File.Delete(tempPath);
                    return SaveResult.Fail($"Save verification failed: {verifyError}");
                }

                if (File.Exists(finalPath))
                    File.Copy(finalPath, backupPath, overwrite: true);

                File.Copy(tempPath, finalPath, overwrite: true);
                File.Delete(tempPath);

                return SaveResult.Ok(slotId);
            }
            catch (Exception e)
            {
                return SaveResult.Fail(e.Message);
            }
        }

        public LoadResult Load(string slotId)
        {
            string path = SlotPath(slotId);
            if (!File.Exists(path)) return LoadResult.Fail($"Save slot '{slotId}' not found.");

            WorldState world = ReadAndValidate(path, out string error);
            return world != null ? LoadResult.Ok(world) : LoadResult.Fail(error);
        }

        public IReadOnlyList<SaveSlotInfo> ListSlots()
        {
            var infos = new List<SaveSlotInfo>();
            foreach (string path in Directory.GetFiles(_saveDirectory, "*.json"))
            {
                try
                {
                    var file = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(path));
                    if (file == null) continue;
                    infos.Add(new SaveSlotInfo
                    {
                        SlotId = file.SlotId,
                        SavedAtUtc = file.SavedAtUtc,
                        DefinitionVersion = file.DefinitionVersion,
                    });
                }
                catch
                {
                    // Corrupt/unreadable files are simply omitted from the listing.
                }
            }
            return infos;
        }

        private WorldState ReadAndValidate(string path, out string error)
        {
            error = null;
            SaveFile file;
            try
            {
                file = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                error = $"Save file unreadable: {e.Message}";
                return null;
            }

            if (file == null)
            {
                error = "Save file is empty.";
                return null;
            }

            if (file.SaveVersion > CurrentSaveVersion)
            {
                error = $"Save version {file.SaveVersion} is newer than supported version {CurrentSaveVersion}.";
                return null;
            }

            if (file.DefinitionVersion != _definitionVersion)
            {
                error = $"Save definition_version '{file.DefinitionVersion}' does not match current '{_definitionVersion}'.";
                return null;
            }

            string worldJson = file.World?.ToString();
            if (string.IsNullOrEmpty(worldJson))
            {
                error = "Save file has no world payload.";
                return null;
            }

            string actualChecksum = ComputeChecksum(worldJson);
            if (actualChecksum != file.Checksum)
            {
                error = "Save checksum mismatch (file may be corrupted).";
                return null;
            }

            try
            {
                return WorldStateSerializer.Deserialize(worldJson);
            }
            catch (Exception e)
            {
                error = $"World state failed to deserialize: {e.Message}";
                return null;
            }
        }

        private string SlotPath(string slotId) => Path.Combine(_saveDirectory, slotId + ".json");

        private static string ComputeChecksum(string payload)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
