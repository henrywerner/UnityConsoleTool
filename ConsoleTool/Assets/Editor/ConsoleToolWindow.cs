using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ConsoleToolWindow : EditorWindow
{
    [MenuItem("Tools/Console Tool Window")]
    public static void ShowWindow()
    {
        //ConsoleToolWindow window = (ConsoleToolWindow)GetWindow(typeof(ConsoleToolWindow));
        var window = GetWindow<ConsoleToolWindow>();
        window.titleContent = new GUIContent("Console Tool");
        window.minSize = new Vector2(600, 400);
    }

    private void OnEnable()
    {
        // Load window layout
        VisualTreeAsset original =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ConsoleToolLayout.uxml");
        TemplateContainer treeAsset = original.CloneTree();
        rootVisualElement.Add(treeAsset);
        
        // Load style sheet
        StyleSheet ss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ConsoleToolStyle.uss");
        rootVisualElement.styleSheets.Add(ss);

        CreateNewCmdButton();
        CreateCmdListView();
    }

    private void CreateNewCmdButton()
    {
        Action OnPress = () =>
        {
            // Do action
        };
        var btn = rootVisualElement.Q<Button>("add-cmd-btn");
        btn.RegisterCallback<MouseUpEvent>((evt) => OnPress());
    }

    private void CreateCmdListView()
    {
        //string[] tempCmds = {"one", "two", "three", "four"};
        
        const int itemCount = 1000;
        var tempCmds = new List<string>(itemCount);
        for (int i = 1; i <= itemCount; i++)
            tempCmds.Add(i.ToString());
        
        FindAllCommands(out ConsoleCommand[] cmds);
        
        ListView cmdList = rootVisualElement.Query<ListView>("cmd-list").First();
        cmdList.makeItem = () => new Label();
        cmdList.bindItem = (element, i) => (element as Label).text = cmds[i].CommandName; // for each loop through the ListView???

        cmdList.itemsSource = cmds;
        cmdList.itemHeight = 16; // ?????
        cmdList.selectionType = SelectionType.Single;

        cmdList.onSelectionChange += enumerable =>
        {
            foreach (var item in enumerable)
            {
                Box cmdDetails = rootVisualElement.Query<Box>("details").First();
                cmdDetails.Clear();
                
                ConsoleCommand cmd = item as ConsoleCommand;
                
                // Cast each unity object as a ConsoleCommand type
                SerializedObject serializedCmd = new SerializedObject(cmd);
                SerializedProperty cmdProperty = serializedCmd.GetIterator();
                cmdProperty.Next(true);

                while (cmdProperty.NextVisible(false))
                {
                    PropertyField prop = new PropertyField(cmdProperty);
                    prop.SetEnabled(cmdProperty.name != "m_Script");
                    prop.Bind(serializedCmd);
                    cmdDetails.Add(prop);
                }
            }
        };

        // cmdList.onItemsChosen += obj => Debug.Log(obj);
        // cmdList.onSelectionChange += objects => Debug.Log(objects);
        
        cmdList.Refresh();
    }
    
    // Find all console command files 
    private void FindAllCommands(out ConsoleCommand[] cmds)
    {
        var guids = AssetDatabase.FindAssets("t:ConsoleCommand", new []{"Assets/DevConsole/Commands"});

        cmds = new ConsoleCommand[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            cmds[i] = AssetDatabase.LoadAssetAtPath<ConsoleCommand>(path);
        }
    }
    
    // 
}
