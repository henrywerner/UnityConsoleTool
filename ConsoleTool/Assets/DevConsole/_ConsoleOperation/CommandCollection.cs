using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCollection : ScriptableObject
{
    [SerializeField] public ConsoleCommand[] cmds;
}
