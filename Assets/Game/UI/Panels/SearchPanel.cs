using System.Collections.Generic;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.UI;
using LastHope.Systems.Boot;
using LastHope.Systems.Commands;
using LastHope.Systems.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.UI.Panels
{
    /// <summary>
    /// Hiện toàn bộ nội dung search point đã mở — không progress bar, không reveal dần
    /// (thiết kế khóa). Tự mở khi nghe <see cref="SearchPointOpened"/>, không cần
    /// Presentation biết tới panel này (tránh Presentation phụ thuộc UI). Tương tác lại
    /// đúng search point đang mở, hoặc nhấn ESC (action <c>Close</c>), đều đóng panel.
    /// </summary>
    public class SearchPanel : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;

        InputAction closeAction;
        bool visible;
        string openSearchPointId;
        int quantityAtOpen;
        Vector2 scroll;
        string statusMessage = "";

        void Awake()
        {
            if (controls != null)
            {
                closeAction = controls.FindActionMap("Gameplay", true).FindAction("Close", true);
            }
        }

        void OnEnable()
        {
            closeAction?.Enable();
            if (GameBootstrapper.IsReady) Subscribe();
            else GameBootstrapper.Ready += Subscribe;
        }

        void OnDisable()
        {
            closeAction?.Disable();
            GameBootstrapper.Ready -= Subscribe;
            if (GameBootstrapper.IsReady)
                GameBootstrapper.Services.Events.Unsubscribe<SearchPointOpened>(OnSearchPointOpened);
        }

        void Update()
        {
            if (visible && closeAction != null && closeAction.WasPressedThisFrame()) Close();
        }

        void Subscribe()
        {
            GameBootstrapper.Ready -= Subscribe;
            GameBootstrapper.Services.Events.Subscribe<SearchPointOpened>(OnSearchPointOpened);
        }

        void OnSearchPointOpened(SearchPointOpened e) => Open(e.SearchPointId);

        /// <summary>Tương tác lại đúng search point đang mở = đóng (toggle), không mở lại.</summary>
        public void Open(string searchPointId)
        {
            if (visible && openSearchPointId == searchPointId)
            {
                Close();
                return;
            }

            openSearchPointId = searchPointId;
            quantityAtOpen = TotalRemainingQuantity(searchPointId);
            statusMessage = "";
            visible = true;
        }

        void Close()
        {
            if (openSearchPointId != null)
            {
                int remaining = TotalRemainingQuantity(openSearchPointId);
                int taken = Mathf.Max(0, quantityAtOpen - remaining);
                GameBootstrapper.Services?.Telemetry.LogSearchClosed(openSearchPointId, taken, remaining);
            }

            visible = false;
            openSearchPointId = null;
        }

        static int TotalRemainingQuantity(string searchPointId)
        {
            var services = GameBootstrapper.Services;
            if (!services.Definitions.TryGetSearchPoint(searchPointId, out var definition)) return 0;

            var location = services.World.GetOrCreateLocation(definition.LocationId);
            if (!location.SearchPoints.TryGetValue(searchPointId, out var state)) return 0;

            int total = 0;
            foreach (var slot in state.RemainingItems) total += slot.Quantity;
            return total;
        }

        void OnGUI()
        {
            if (!visible || openSearchPointId == null || !GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            if (!services.Definitions.TryGetSearchPoint(openSearchPointId, out var definition)) return;

            var location = services.World.GetOrCreateLocation(definition.LocationId);
            if (!location.SearchPoints.TryGetValue(openSearchPointId, out var state)) return;

            const float width = 360f, height = 320f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
            PointerOverUI.MarkHover(rect.Contains(Event.current.mousePosition));

            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label(openSearchPointId);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height - 90f));
            if (state.RemainingItems.Count == 0)
            {
                GUILayout.Label("(trống)");
            }
            foreach (var slot in new List<ItemInstanceState>(state.RemainingItems))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{slot.ItemId} ×{slot.Quantity}", GUILayout.Width(240f));
                if (GUILayout.Button("Lấy", GUILayout.Width(60f)))
                {
                    var result = services.Commands.Submit(new TransferItemCommand(
                        InventoryOwner.SearchPoint(openSearchPointId), InventoryOwner.Player, slot.ItemId, 1));
                    statusMessage = result.Success ? "" : $"Không lấy được: {result.Error}";
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Take All"))
            {
                var takeAll = new TakeAllFromSearchPointCommand(openSearchPointId);
                services.Commands.Submit(takeAll);
                statusMessage = takeAll.TookEverything ? "Đã lấy hết." : "Không đủ chỗ — còn sót lại đồ.";
            }
            if (GUILayout.Button("Đóng")) Close();
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(statusMessage)) GUILayout.Label(statusMessage);

            GUILayout.EndArea();
        }
    }
}
