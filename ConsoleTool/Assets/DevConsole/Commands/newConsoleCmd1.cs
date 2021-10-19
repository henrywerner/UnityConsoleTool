using UnityEngine;

[CreateAssetMenu(fileName = "newConsoleCmd1", menuName = "Console Commands/newConsoleCmd1")]
public class newConsoleCmd1 : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        // Add code here...

        return true;
    }
}