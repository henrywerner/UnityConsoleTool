using UnityEngine;

[CreateAssetMenu(fileName = "spawn", menuName = "Console Commands/spawn")]
public class spawn : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        // Add code here...

        return true;
    }
}