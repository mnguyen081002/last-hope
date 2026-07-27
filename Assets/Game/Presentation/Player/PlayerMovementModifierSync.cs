using LastHope.Systems.Boot;
using LastHope.Systems.Condition;
using LastHope.Systems.Inventory;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Tính lại các yếu tố ảnh hưởng tốc độ mỗi frame, đẩy kết quả vào
    /// <see cref="PlayerController.SpeedModifier"/>. Overload có hệ số tăng dần
    /// (<c>balance.json.inventory</c>); Collapsed là chặn nhị phân — balance.json không cho
    /// số modifier tăng dần cho Fatigue/Cold/Health nên chưa nhân thêm các yếu tố đó (xem
    /// docs/plans/2026-07-27-p2a-condition-core.md).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovementModifierSync : MonoBehaviour
    {
        PlayerController controller;

        void Awake() => controller = GetComponent<PlayerController>();

        void Update()
        {
            if (!GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var balance = services.Definitions.Balance;
            var player = services.World.Player;

            var tier = InventorySystem.ComputeLoadTier(player.Inventory, services.Definitions, balance.Inventory);
            float overloadModifier = InventorySystem.SpeedModifierFor(tier, balance.Inventory);

            bool collapsed = ConditionSystem.IsCollapsed(player, balance.Condition);

            controller.SpeedModifier = collapsed ? 0f : overloadModifier;
        }
    }
}
