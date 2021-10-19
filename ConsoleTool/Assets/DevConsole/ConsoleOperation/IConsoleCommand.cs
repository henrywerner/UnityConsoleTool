public interface IConsoleCommand
{
    string CommandName { get; }
    string CommandDesc { get; }
    bool Process(string[] args);
}