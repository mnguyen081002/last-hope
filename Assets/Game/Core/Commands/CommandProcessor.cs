using LastHope.Core.Diagnostics;

namespace LastHope.Core.Commands
{
    /// <summary>
    /// Chạy command đồng bộ: đóng dấu thời gian → Validate → Execute. Validate fail thì
    /// không có gì bị đổi.
    /// </summary>
    public class CommandProcessor
    {
        readonly GameContext context;

        public CommandProcessor(GameContext context) => this.context = context;

        public CommandResult Submit(IGameCommand command)
        {
            if (command == null) return CommandResult.Fail(CommandErrorCode.InvalidTarget, "command null");

            command.WorldTime = context.World.WorldTimeMinutes;

            CommandResult result = command.Validate(context);
            if (!result.Success)
            {
                GameLog.Info(LogCategory.State,
                    $"{command.GetType().Name} bị từ chối: {result.Error} {result.Message}");
                return result;
            }

            command.Execute(context);
            return result;
        }
    }
}
