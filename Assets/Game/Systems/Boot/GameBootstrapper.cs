using System;
using System.IO;
using LastHope.Core.Diagnostics;
using LastHope.Data;
using LastHope.Systems.Registry;
using UnityEngine;

namespace LastHope.Systems.Boot
{
    /// <summary>
    /// Composition root, sống trong <c>10_GamePersistent</c>. Load definition, dựng service,
    /// tạo ván mới. Lỗi definition là fail-fast: chơi tiếp với content hỏng chỉ đẻ ra bug
    /// khó lần hơn về sau.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        public static GameServices Services { get; private set; }
        public static bool IsReady => Services != null;

        /// <summary>Bắn sau khi service sẵn sàng, cho view đăng ký muộn.</summary>
        public static event Action Ready;

        [SerializeField] ulong masterSeed = 20260727UL;

        bool ownsServices;

        void Awake()
        {
            if (Services != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            ownsServices = true;

            string definitionsPath = Path.Combine(Application.streamingAssetsPath, "Definitions");
            string savePath = Path.Combine(Application.persistentDataPath, "Saves");

            DefinitionRegistry definitions;
            try
            {
                definitions = DefinitionLoader.LoadFromDirectory(definitionsPath);
            }
            catch (DefinitionLoadException e)
            {
                GameLog.Error(LogCategory.Boot, e.Message);
                enabled = false;
                return;
            }

            var world = NewGameFactory.Create(definitions, masterSeed);
            Services = new GameServices(definitions, world, savePath);

            GameLog.Info(LogCategory.Boot,
                $"Definitions v{definitions.DefinitionVersion} — {definitions.Items.Count} item, " +
                $"{definitions.Locations.Count} location, {definitions.SearchPoints.Count} search point.");

            Ready?.Invoke();
        }

        void OnDestroy()
        {
            if (ownsServices) Services = null;
        }
    }
}
