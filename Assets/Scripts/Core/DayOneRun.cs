using System;

namespace LastHope.Core
{
    public enum DayOneStep
    {
        HearWarning,
        InspectStorage,
        InspectFilter,
        LeaveShelter,
        FindFilter,
        DecideWhetherToContinue,
        ReturnHome,
        SpendEvening,
        Complete
    }

    public sealed class DayOneRun
    {
        public const float MaxExposure = 100f;

        public DayOneStep Step { get; private set; } = DayOneStep.HearWarning;
        public float Hour { get; private set; } = 6f;
        public float Exposure { get; private set; }
        public int Filters { get; private set; }
        public int Materials { get; private set; }
        public bool IsOutside { get; private set; }

        public bool Interact(string interactionId)
        {
            switch (interactionId)
            {
                case "radio" when Step == DayOneStep.HearWarning:
                    Step = DayOneStep.InspectStorage;
                    return true;
                case "storage" when Step == DayOneStep.InspectStorage:
                    Step = DayOneStep.InspectFilter;
                    return true;
                case "filter_unit" when Step == DayOneStep.InspectFilter:
                    Step = DayOneStep.LeaveShelter;
                    return true;
                case "door" when Step == DayOneStep.LeaveShelter:
                    IsOutside = true;
                    Step = DayOneStep.FindFilter;
                    return true;
                case "near_loot" when Step == DayOneStep.FindFilter:
                    Filters++;
                    Step = DayOneStep.DecideWhetherToContinue;
                    return true;
                case "far_loot" when Step == DayOneStep.DecideWhetherToContinue:
                    Materials += 2;
                    return true;
                case "door" when IsOutside && Filters > 0:
                    IsOutside = false;
                    Step = DayOneStep.SpendEvening;
                    return true;
                case "workbench" when Step == DayOneStep.SpendEvening:
                    Filters--;
                    Step = DayOneStep.Complete;
                    return true;
                default:
                    return false;
            }
        }

        public void AdvanceOutside(float hours, float exposurePerHour)
        {
            if (!IsOutside || hours <= 0f)
            {
                return;
            }

            Hour = Math.Min(24f, Hour + hours);
            Exposure = Math.Min(MaxExposure, Exposure + hours * Math.Max(0f, exposurePerHour));
        }
    }
}
