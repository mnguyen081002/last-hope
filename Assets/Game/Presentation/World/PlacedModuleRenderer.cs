using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Data.Definitions;
using LastHope.Systems.Boot;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Hiện sprite thật trong thế giới cho từng Module đã xây xong (<see cref="ShelterState.PlacedModules"/>)
    /// — trước đây Free Placement chỉ có ghost lúc đặt, xây xong không có gì hiện lại (user báo
    /// 2026-07-29). Đồng bộ toàn bộ danh sách (không chỉ thêm/bớt lẻ theo event) mỗi khi có thay
    /// đổi — cùng nguyên tắc "luôn tính lại từ state hiện tại, không suy/cache" đã áp dụng cho
    /// cầu thang (xem <see cref="StaircaseZone"/>).
    /// </summary>
    public class PlacedModuleRenderer : MonoBehaviour
    {
        [SerializeField] Sprite propSprite;

        readonly Dictionary<string, GameObject> views = new();

        void OnEnable()
        {
            if (GameBootstrapper.IsReady) Subscribe();
            else GameBootstrapper.Ready += Subscribe;
        }

        void OnDisable()
        {
            GameBootstrapper.Ready -= Subscribe;
            if (GameBootstrapper.IsReady)
            {
                GameBootstrapper.Services.Events.Unsubscribe<ConstructionCompleted>(OnChanged);
                GameBootstrapper.Services.Events.Unsubscribe<ModuleDismantled>(OnChanged);
                GameBootstrapper.Services.Events.Unsubscribe<ModuleRedeployed>(OnChanged);
            }
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<ConstructionCompleted>(OnChanged);
            GameBootstrapper.Services.Events.Subscribe<ModuleDismantled>(OnChanged);
            GameBootstrapper.Services.Events.Subscribe<ModuleRedeployed>(OnChanged);
            SyncAll();
        }

        void OnChanged(ConstructionCompleted e) => SyncAll();

        void OnChanged(ModuleDismantled e) => SyncAll();

        void OnChanged(ModuleRedeployed e) => SyncAll();

        void SyncAll()
        {
            var placed = GameBootstrapper.Services.World.Shelter.PlacedModules;

            var toRemove = new List<string>();
            foreach (var placementId in views.Keys)
            {
                if (!placed.ContainsKey(placementId)) toRemove.Add(placementId);
            }
            foreach (var placementId in toRemove)
            {
                Destroy(views[placementId]);
                views.Remove(placementId);
            }

            foreach (var pair in placed)
            {
                if (!views.ContainsKey(pair.Key)) views[pair.Key] = SpawnView(pair.Value);
            }
        }

        GameObject SpawnView(BuiltModuleState built)
        {
            var definitions = GameBootstrapper.Services.Definitions;
            int floor = definitions.TryGetShelterZone(built.ZoneId, out var zone) && zone.Floor == ShelterFloor.Upper
                ? 1
                : 0;

            var go = new GameObject($"Module_{built.ModuleId}");
            go.transform.SetParent(FindFloorRoot(floor), false);
            go.transform.position = new Vector3(built.PositionX, built.PositionY, 0f);

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(go.transform, false);
            var renderer = spriteGo.AddComponent<SpriteRenderer>();
            renderer.sprite = propSprite;
            renderer.color = new Color(0.5f, 0.55f, 0.6f);
            if (renderer.sprite != null)
            {
                spriteGo.transform.localPosition = new Vector3(0f, renderer.sprite.bounds.extents.y, 0f);
            }

            return go;
        }

        static Transform FindFloorRoot(int floor)
        {
            foreach (var level in FindObjectsByType<FloorLevel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (level.Floor == floor) return level.transform;
            }
            return null;
        }
    }
}
