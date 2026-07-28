using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Đánh dấu một GameObject root (Ground/Boundary/Interactables của 1 tầng) thuộc tầng
    /// nào (0 = trệt, 1 = lầu 1...). <see cref="FloorRenderController"/> đọc field này để
    /// quyết định Full/Dimmed/Hidden. Root không có component này coi như tầng 0 mặc định —
    /// scene 1 tầng (mọi Location hiện có ngoài Shelter) không cần đổi gì.
    /// </summary>
    public class FloorLevel : MonoBehaviour
    {
        [SerializeField] int floor;

        public int Floor => floor;
    }
}
