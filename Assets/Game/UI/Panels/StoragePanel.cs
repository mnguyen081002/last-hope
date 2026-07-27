using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Inventory;
using UnityEngine;

namespace LastHope.UI.Panels
{
    /// <summary>Kho shelter — không giới hạn sức chứa, chuyển hai chiều với túi đồ.</summary>
    public class StoragePanel : MonoBehaviour
    {
        bool visible;
        string locationId;
        Vector2 playerScroll;
        Vector2 storageScroll;

        void OnEnable()
        {
            if (GameBootstrapper.IsReady) Subscribe();
            else GameBootstrapper.Ready += Subscribe;
        }

        void OnDisable()
        {
            GameBootstrapper.Ready -= Subscribe;
            if (GameBootstrapper.IsReady)
                GameBootstrapper.Services.Events.Unsubscribe<StorageOpened>(OnOpened);
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<StorageOpened>(OnOpened);
        }

        void OnOpened(StorageOpened e)
        {
            locationId = e.LocationId;
            visible = true;
        }

        void Close() => visible = false;

        void OnGUI()
        {
            if (!visible || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var storage = services.World.GetOrCreateLocation(locationId).StorageContainer;
            var playerInventory = services.World.Player.Inventory;

            const float width = 520f, height = 360f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            float columnWidth = width / 2f - 16f;

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Kho Shelter");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(columnWidth));
            GUILayout.Label("Túi đồ");
            if (!string.IsNullOrEmpty(playerInventory.CarriedObjectItemId))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"[ôm] {playerInventory.CarriedObjectItemId}", GUILayout.Width(columnWidth - 40f));
                if (GUILayout.Button(">>", GUILayout.Width(30f)))
                {
                    services.Commands.Submit(new TransferItemCommand(
                        InventoryOwner.Player, InventoryOwner.ShelterStorage(locationId),
                        playerInventory.CarriedObjectItemId, 1));
                }
                GUILayout.EndHorizontal();
            }
            playerScroll = GUILayout.BeginScrollView(playerScroll, GUILayout.Height(height - 130f));
            foreach (var slot in new List<ItemInstanceState>(playerInventory.Slots))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{slot.ItemId} ×{slot.Quantity}", GUILayout.Width(columnWidth - 40f));
                if (GUILayout.Button(">>", GUILayout.Width(30f)))
                {
                    services.Commands.Submit(new TransferItemCommand(
                        InventoryOwner.Player, InventoryOwner.ShelterStorage(locationId), slot.ItemId, 1));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(columnWidth));
            GUILayout.Label("Kho (không giới hạn)");
            storageScroll = GUILayout.BeginScrollView(storageScroll, GUILayout.Height(height - 100f));
            foreach (var slot in new List<ItemInstanceState>(storage))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<<", GUILayout.Width(30f)))
                {
                    services.Commands.Submit(new TransferItemCommand(
                        InventoryOwner.ShelterStorage(locationId), InventoryOwner.Player, slot.ItemId, 1));
                }
                GUILayout.Label($"{slot.ItemId} ×{slot.Quantity}", GUILayout.Width(columnWidth - 40f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Đóng")) Close();
            GUILayout.EndArea();
        }
    }
}
