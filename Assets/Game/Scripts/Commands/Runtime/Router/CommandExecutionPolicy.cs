namespace Game.Commands
{
    public enum CommandExecutionPolicy
    {
        Concurrent = 0,
        Drop = 1,
        Sequential = 2,
        Switch = 3
    }
}