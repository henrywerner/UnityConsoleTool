using UnityEngine;

[CreateAssetMenu(fileName = "newConsoleCmd", menuName = "Console Commands/newConsoleCmd")]
public class newConsoleCmd : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        // Add code here...

        return true;
    }
}