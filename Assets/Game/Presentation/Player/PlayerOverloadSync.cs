using LastHope.Systems.Boot;
using LastHope.Systems.Inventory;
using UnityEngine;

namespace LastHope.Presentation.Player
{
    /// <summary>Tính lại tải mỗi frame, đẩy speed modifier vào PlayerController.</summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerOverloadSync : MonoBehaviour
    {
        PlayerController controller;

        void Awake() => controller = GetComponent<PlayerController>();

        void Update()
        {
            if (!GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;
            var balance = services.Definitions.Balance.Inventory;
            var tier = InventorySystem.ComputeLoadTier(services.World.Player.Inventory, services.Definitions, balance);

            controller.SpeedModifier = InventorySystem.SpeedModifierFor(tier, balance);
        }
    }
}
