using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour
{
    public static ConsoleManager current;
    
    [Header("Console UI Elements")] 
    [SerializeField] private InputField entryField;
    [SerializeField] private Text consoleLog;
    
    [SerializeField] private int historyLength;
    private List<string> _cmdHistory;

    //private ConsoleCommand[] cmds;
    //private Dictionary<string, ConsoleCommand> cmds;
    [SerializeField] private CommandCollection _commandCollection;
    public ConsoleCommand[] Commands => _commandCollection.cmds;

    private void Awake()
    {
        current = this;
        _cmdHistory = new List<string>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PrintToLog("> " + entryField.text);
            ParseCommand(entryField.text);
            entryField.text = string.Empty;
            entryField.ActivateInputField();
        }
    }

    private void ParseCommand(string str)
    {
        string[] commandItems = str.Split(' ');
        string command = commandItems[0];
        string[] arguments = commandItems.Skip(1).ToArray();

        if (_commandCollection.cmds.Length == 0)
        {
            Debug.Log("<DevConsoleTool> :: List of commands is empty");
            return;
        }

        foreach (var x in _commandCollection.cmds)
        {
            // check if command key word matches any existing commands
            if (!command.Equals(x.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            // if so, call process with the commands arguments
            if (x.Process(arguments))
            {
                return;
            }
        }
        
        // else, no command was found
        PrintToLog("Command '" + command + "' not found.");
    }

    public void PrintToLog(string str)
    {
        _cmdHistory.Add(str);
        if (_cmdHistory.Count > historyLength)
            _cmdHistory.RemoveAt(0);

        string output = "";
        foreach (var line in _cmdHistory)
        {
            output += line + "\n";
        }

        consoleLog.text = output;
    }
}
