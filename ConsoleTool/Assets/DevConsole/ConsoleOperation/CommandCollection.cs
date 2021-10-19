using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCollection : ScriptableObject
{
    public Dictionary<string, ConsoleCommand> cmds = new Dictionary<string, ConsoleCommand>();
}
