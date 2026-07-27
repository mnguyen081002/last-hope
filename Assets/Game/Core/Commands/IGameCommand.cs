using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Core.Commands
{
    public enum CommandErrorCode
    {
        None = 0,
        UnknownDefinition,
        NotEnoughCapacity,
        ItemNotFound,
        InvalidTarget,
        WrongLocation,
        NotAllowedNow,
    }

    public readonly struct CommandResult
    {
        public readonly bool Success;
        public readonly CommandErrorCode Error;

        /// <summary>Thông tin cho debug/log. UI hiển thị theo <see cref="Error"/>, không parse chuỗi này.</summary>
        public readonly string Message;

        CommandResult(bool success, CommandErrorCode error, string message)
        {
            Success = success;
            Error = error;
            Message = message;
        }

        public static CommandResult Ok() => new(true, CommandErrorCode.None, null);

        public static CommandResult Fail(CommandErrorCode error, string message = null) =>
            new(false, error, message);
    }

    /// <summary>Bundle phụ thuộc duy nhất truyền vào command — không dùng singleton rải rác.</summary>
    public class GameContext
    {
        public WorldState World;
        public DefinitionRegistry Definitions;
        public EventBus Events;
        public RngService Rng;
    }

    /// <summary>
    /// Mọi thay đổi state do người chơi gây ra đi qua command. Tách Validate/Execute để
    /// lệnh sai bị chặn **trước khi** chạm vào state — không có mutate nửa vời.
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>Thời điểm game khi lệnh được submit, do processor đóng dấu.</summary>
        long WorldTime { get; set; }

        CommandResult Validate(GameContext context);

        void Execute(GameContext context);
    }
}
