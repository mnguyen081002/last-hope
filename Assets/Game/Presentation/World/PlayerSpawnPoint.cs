using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>
    /// Đánh dấu vị trí player xuất hiện khi scene này được load (travel tới đây). Scene có
    /// nhiều <see cref="TravelPointView"/> (nhiều lối vào) nên có nhiều spawn point — mỗi
    /// cái gắn <see cref="RouteId"/> khớp route vừa đi qua để xuất hiện đúng gần cổng đó,
    /// không phải luôn về một chỗ cố định.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [Tooltip("Route dẫn tới cổng này. Để trống = spawn mặc định (dùng khi boot lần đầu, không có route nào vừa đi qua).")]
        [SerializeField] string routeId = "";

        public string RouteId => routeId;
    }
}
