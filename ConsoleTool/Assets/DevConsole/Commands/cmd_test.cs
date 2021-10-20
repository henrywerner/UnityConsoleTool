using UnityEngine;

[CreateAssetMenu(fileName = "cmd_test", menuName = "Console Commands/cmd_test")]
public class cmd_test : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        // Add code here...
        Debug.Log("Hello World");

        return true;
    }
}