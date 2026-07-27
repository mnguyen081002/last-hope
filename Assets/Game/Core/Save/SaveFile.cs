using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace LastHope.Core.Save
{
    /// <summary>
    /// Bọc payload world kèm metadata kiểm tra tính hợp lệ. <c>world</c> giữ nguyên dạng
    /// JSON thô (<see cref="JRaw"/> qua string) để checksum tính trên đúng chuỗi đã ghi,
    /// không bị lệch do serialize lại.
    /// </summary>
    public class SaveFile
    {
        public const int CurrentSaveVersion = 1;

        [JsonProperty("save_version")] public int SaveVersion = CurrentSaveVersion;
        [JsonProperty("definition_version")] public string DefinitionVersion;
        [JsonProperty("saved_at_utc")] public string SavedAtUtc;
        [JsonProperty("checksum")] public string Checksum;
        [JsonProperty("slot_id")] public string SlotId;

        [JsonProperty("world")] public Newtonsoft.Json.Linq.JRaw World;

        public static string ComputeChecksum(string worldJson)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(worldJson));
            return Convert.ToBase64String(hash);
        }
    }

    public enum SaveLoadError
    {
        None = 0,
        FileNotFound,
        Corrupt,
        ChecksumMismatch,
        SaveVersionMismatch,
        DefinitionVersionMismatch,
    }

    public class SaveLoadException : Exception
    {
        public SaveLoadError Error { get; }

        public SaveLoadException(SaveLoadError error, string message) : base(message) =>
            Error = error;
    }
}
