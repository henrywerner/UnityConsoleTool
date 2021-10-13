using UnityEngine;

public abstract class ConsoleCommand : ScriptableObject
{
    [SerializeField] private string _commandName = string.Empty;
    [SerializeField] [TextArea] private string _commandDesc = string.Empty;
    public string CommandName => _commandName;
    public string CommandDesc => _commandDesc;
    public abstract bool Process(string[] args);
}
