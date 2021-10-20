using UnityEngine;

[CreateAssetMenu(fileName = "cmd_help", menuName = "Console Commands/cmd_help")]
public class cmd_help : ConsoleCommand
{
    public override bool Process(string[] args)
    {
        foreach (var x in ConsoleManager.current.Commands)
        {
            ConsoleManager.current.PrintToLog(
                (x.CommandName == string.Empty ? "[name missing]" : x.CommandName) 
                + " : " 
                + (x.CommandDesc == string.Empty ? "[description missing]" : x.CommandDesc));
        }        
        
        return true;
    }
}