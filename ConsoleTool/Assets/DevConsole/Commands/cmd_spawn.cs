using UnityEngine;

[CreateAssetMenu(fileName = "cmd_spawn", menuName = "Console Commands/cmd_spawn")]
public class cmd_spawn : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        // Add code here...

        return true;
    }
}