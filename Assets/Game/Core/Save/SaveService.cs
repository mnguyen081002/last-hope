using System;
using System.IO;
using System.Linq;
using LastHope.Core.Diagnostics;
using LastHope.Core.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastHope.Core.Save
{
    /// <summary>
    /// Ghi/đọc save. Ghi theo kiểu atomic: serialize → tmp → đọc lại verify → đẩy bản cũ
    /// thành .bak → rename. Mất điện giữa chừng thì cùng lắm mất bản mới, không mất bản cũ.
    /// </summary>
    public class SaveService
    {
        public const int AutosaveSlotCount = 3;
        public const string ManualSlotId = "manual_0";

        readonly string directory;
        readonly string definitionVersion;

        public SaveService(string directory, string definitionVersion)
        {
            this.directory = directory;
            this.definitionVersion = definitionVersion;
        }

        public string PathForSlot(string slotId) => Path.Combine(directory, slotId + ".json");

        public static string AutosaveSlotId(int index) => $"autosave_{index}";

        // ---------- Write ----------

        public void Save(WorldState world, string slotId)
        {
            Directory.CreateDirectory(directory);

            string worldJson = WorldStateSerializer.Serialize(world);
            var file = new SaveFile
            {
                DefinitionVersion = definitionVersion,
                SavedAtUtc = DateTime.UtcNow.ToString("o"),
                Checksum = SaveFile.ComputeChecksum(worldJson),
                SlotId = slotId,
                World = new JRaw(worldJson),
            };

            string payload = JsonConvert.SerializeObject(file, Formatting.Indented);
            string finalPath = PathForSlot(slotId);
            string tempPath = finalPath + ".tmp";
            string backupPath = finalPath + ".bak";

            File.WriteAllText(tempPath, payload);

            // Đọc lại ngay: đĩa đầy hoặc ghi hỏng thì phát hiện trước khi đụng bản cũ.
            VerifyReadable(tempPath);

            if (File.Exists(finalPath))
            {
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(finalPath, backupPath);
            }
            File.Move(tempPath, finalPath);

            GameLog.Info(LogCategory.Save, $"Đã ghi {slotId} ({payload.Length} bytes).");
        }

        /// <summary>Autosave xoay vòng, luôn đè lên slot có thời điểm ghi cũ nhất.</summary>
        public string SaveAutosave(WorldState world)
        {
            string slotId = OldestAutosaveSlotId();
            Save(world, slotId);
            return slotId;
        }

        string OldestAutosaveSlotId()
        {
            string oldestSlot = AutosaveSlotId(0);
            DateTime oldestTime = DateTime.MaxValue;

            for (int i = 0; i < AutosaveSlotCount; i++)
            {
                string slotId = AutosaveSlotId(i);
                string path = PathForSlot(slotId);

                if (!File.Exists(path)) return slotId;

                DateTime savedAt = ReadSavedAtUtc(path);
                if (savedAt < oldestTime)
                {
                    oldestTime = savedAt;
                    oldestSlot = slotId;
                }
            }

            return oldestSlot;
        }

        DateTime ReadSavedAtUtc(string path)
        {
            try
            {
                var file = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(path));
                return DateTime.TryParse(file?.SavedAtUtc, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
                    ? parsed
                    : DateTime.MinValue;
            }
            catch
            {
                // Slot hỏng là ứng viên tốt nhất để ghi đè.
                return DateTime.MinValue;
            }
        }

        // ---------- Read ----------

        public WorldState Load(string slotId)
        {
            string path = PathForSlot(slotId);
            if (!File.Exists(path))
                throw new SaveLoadException(SaveLoadError.FileNotFound, $"Không thấy save '{slotId}'.");

            return LoadFromPath(path);
        }

        public bool HasSlot(string slotId) => File.Exists(PathForSlot(slotId));

        public string[] ExistingSlotIds()
        {
            if (!Directory.Exists(directory)) return Array.Empty<string>();

            return Directory.GetFiles(directory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(id => id)
                .ToArray();
        }

        WorldState LoadFromPath(string path)
        {
            SaveFile file;
            try
            {
                file = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                throw new SaveLoadException(SaveLoadError.Corrupt, $"Save hỏng: {e.Message}");
            }

            if (file?.World == null)
                throw new SaveLoadException(SaveLoadError.Corrupt, "Save thiếu payload 'world'.");

            if (file.SaveVersion != SaveFile.CurrentSaveVersion)
                throw new SaveLoadException(SaveLoadError.SaveVersionMismatch,
                    $"save_version {file.SaveVersion} ≠ {SaveFile.CurrentSaveVersion}.");

            if (file.DefinitionVersion != definitionVersion)
                throw new SaveLoadException(SaveLoadError.DefinitionVersionMismatch,
                    $"definition_version '{file.DefinitionVersion}' ≠ '{definitionVersion}'.");

            string worldJson = file.World.ToString();
            if (SaveFile.ComputeChecksum(worldJson) != file.Checksum)
                throw new SaveLoadException(SaveLoadError.ChecksumMismatch,
                    "Checksum không khớp — file đã bị sửa hoặc ghi dở.");

            var world = WorldStateSerializer.Deserialize(worldJson);
            if (world == null)
                throw new SaveLoadException(SaveLoadError.Corrupt, "Không dựng lại được WorldState.");

            return world;
        }

        void VerifyReadable(string path)
        {
            try
            {
                LoadFromPath(path);
            }
            catch (SaveLoadException e)
            {
                File.Delete(path);
                throw new SaveLoadException(e.Error, $"Ghi save hỏng, đã huỷ: {e.Message}");
            }
        }
    }
}
