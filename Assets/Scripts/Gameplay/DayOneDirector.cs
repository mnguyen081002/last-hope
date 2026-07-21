using LastHope.Core;
using UnityEngine;

namespace LastHope.Gameplay
{
    public sealed class DayOneDirector : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField, Min(0.5f)] private float interactionRange = 1.5f;
        [SerializeField, Min(0f)] private float gameHoursPerSecond = 0.08f;
        [SerializeField, Min(0f)] private float exposurePerHour = 5f;

        private DayOneRun run;
        private DayOneInteractable focus;
        private string feedback = "Radio đang phát tín hiệu cảnh báo...";

        public DayOneRun Run => run;

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void Awake()
        {
            run = new DayOneRun();
        }

        private void Update()
        {
            if (run.IsOutside)
            {
                run.AdvanceOutside(Time.deltaTime * gameHoursPerSecond, exposurePerHour);
            }

            focus = FindClosestInteraction();
            if (focus != null && Input.GetKeyDown(KeyCode.E))
            {
                feedback = run.Interact(focus.InteractionId)
                    ? FeedbackFor(run.Step)
                    : "Chưa cần tương tác với " + focus.DisplayName + ".";
            }
        }

        private DayOneInteractable FindClosestInteraction()
        {
            if (player == null)
            {
                return null;
            }

            DayOneInteractable closest = null;
            float bestDistance = interactionRange;
            foreach (DayOneInteractable candidate in FindObjectsByType<DayOneInteractable>(FindObjectsSortMode.None))
            {
                float distance = Vector2.Distance(player.position, candidate.transform.position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    closest = candidate;
                }
            }

            return closest;
        }

        private static string FeedbackFor(DayOneStep step)
        {
            switch (step)
            {
                case DayOneStep.InspectStorage: return "Bão có thể đến trong 6 ngày. Kiểm tra kho dự trữ.";
                case DayOneStep.InspectFilter: return "Kho gần cạn. Kiểm tra hệ thống lọc khí.";
                case DayOneStep.LeaveShelter: return "Thiếu bộ lọc. Hãy ra ngoài tìm một bộ.";
                case DayOneStep.FindFilter: return "Đồng hồ đã chạy. Điểm gần an toàn hơn; điểm xa có thêm vật liệu.";
                case DayOneStep.DecideWhetherToContinue: return "Đã có bộ lọc. Đi tiếp lấy vật liệu hay quay về?";
                case DayOneStep.SpendEvening: return "Đã về shelter. Lắp bộ lọc tại bàn thao tác.";
                case DayOneStep.Complete: return "Ngày 1 hoàn tất. Shelter đã có khả năng lọc cơ bản.";
                default: return string.Empty;
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16f, 16f, 470f, 122f), "LAST HOPE — DAY 1");
            GUI.Label(new Rect(32f, 44f, 430f, 22f), $"06:00 + {run.Hour - 6f:0.0}h   Liều: {run.Exposure:0}/{DayOneRun.MaxExposure:0}");
            GUI.Label(new Rect(32f, 68f, 430f, 22f), $"Bộ lọc: {run.Filters}   Vật liệu: {run.Materials}");
            GUI.Label(new Rect(32f, 92f, 430f, 38f), feedback);

            if (focus != null)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 130f, Screen.height - 72f, 260f, 38f), "E — " + focus.DisplayName);
            }

            GUI.Label(new Rect(16f, Screen.height - 28f, 420f, 22f), "WASD / phím mũi tên: di chuyển   E: tương tác");
        }
    }
}
