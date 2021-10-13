using UnityEngine;

[CreateAssetMenu(fileName = "demo", menuName = "Console Commands/demo")]
public class cmd_demo : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        Debug.Log("Hello World");
        return true;
    }
}