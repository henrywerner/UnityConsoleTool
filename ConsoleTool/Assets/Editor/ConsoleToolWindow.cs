using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConsoleToolWindow : EditorWindow
{
    [MenuItem("Window/Console Tool")]
    static void OpenWindow()
    {
        ConsoleToolWindow window = (ConsoleToolWindow)GetWindow(typeof(ConsoleToolWindow));
        window.minSize = new Vector2(80, 180);
        window.Show();
    }
}
