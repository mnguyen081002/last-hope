using System;
using LastHope.Core.Logging;

namespace LastHope.Core.Commands
{
    /// <summary>Single mutation gateway: stamp world time, Validate, then Execute.</summary>
    public sealed class CommandProcessor
    {
        private readonly GameContext _ctx;

        public CommandProcessor(GameContext ctx)
        {
            _ctx = ctx;
        }

        public CommandResult Submit(IGameCommand cmd)
        {
            cmd.WorldTime = _ctx.World.WorldTimeMinutes;

            CommandResult validation = cmd.Validate(_ctx);
            if (!validation.Success)
            {
                GameLog.Warn(LogCategory.World, $"{cmd.GetType().Name} rejected: {validation.Code} {validation.DebugMessage}");
                return validation;
            }

            try
            {
                cmd.Execute(_ctx);
            }
            catch (Exception e)
            {
                GameLog.Error(LogCategory.World, $"{cmd.GetType().Name} threw during Execute: {e}");
                return CommandResult.Fail(CommandErrorCode.InternalError, e.Message);
            }

            return validation;
        }
    }
}
