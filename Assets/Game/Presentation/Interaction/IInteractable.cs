using LastHope.Core.Commands;

namespace LastHope.Presentation.Interaction
{
    /// <summary>
    /// Instant interact (E) — design docs specify no hold-to-interact durations for P1/P2.
    /// </summary>
    public interface IInteractable
    {
        string PromptText { get; }
        bool CanInteract(GameContext ctx);
        void Interact(GameContext ctx, CommandProcessor processor);
    }
}
