namespace Game.Commands
{
    public interface ICommandExecutionGroup
    {
        CommandExecutionPolicy ExecutionPolicy { get; }
    }
}