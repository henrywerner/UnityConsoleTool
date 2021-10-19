using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ConsoleToolWindow : EditorWindow
{
    //[SerializeField] private CommandCollection _commandCollection;
    
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
        
        // Load CommandCollection
        /*
        //_commandCollection = AssetDatabase.LoadAssetAtPath<CommandCollection>("Assets/DevConsole/ConsoleOperation/CommandCollection.asset");
        //_commandCollection = Resources.Load<CommandCollection>("Assets/DevConsole/ConsoleOperation/CommandCollection");
        if (_commandCollection == null)
        {
            CommandCollection collection = CreateInstance<CommandCollection>();
            AssetDatabase.CreateAsset(collection, "Assets/DevConsole/ConsoleOperation/CommandCollection.asset");
            AssetDatabase.SaveAssets();
            _commandCollection = Resources.Load<CommandCollection>("Assets/DevConsole/ConsoleOperation/CommandCollection");
        }
        */

        CreateNewCmdButton();
        CreateCmdListView();
        CreateScriptsListView();
        //CreateCollectionListView();
    }

    private void CreateNewCmdButton()
    {
        var btn = rootVisualElement.Q<Button>("add-cmd-btn");
        var textEntry = rootVisualElement.Q<TextField>("add-cmd-entry");
        Action OnPress = () => CreateCmdFromTemplate(textEntry.text);
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
        cmdList.bindItem = (element, i) =>
            (element as Label).text =
            (cmds[i].CommandName == string.Empty)
                ? "[no name]"
                : cmds[i].CommandName;

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

    /*
    private void CreateCollectionListView()
    {
        List<string> keys = new List<string>();
        List<ConsoleCommand> commands = new List<ConsoleCommand>();
        foreach (var kvp in _commandCollection.cmds)
        {
            keys.Add(kvp.Key);
            commands.Add(kvp.Value);
        }
        
        ListView collectionList = rootVisualElement.Query<ListView>("collection-list").First();
        collectionList.makeItem = () => new Label();
        collectionList.bindItem = (element, i) => (element as Label).text = (keys[i] == string.Empty) ? "[no name]": keys[i];

        collectionList.itemsSource = commands;
        collectionList.itemHeight = 16; // ?????
        collectionList.selectionType = SelectionType.Single;
        
        collectionList.Refresh();
    }
    */

    private void CreateScriptsListView()
    {
        List<string> scripts = new List<string>();
        DirectoryInfo dir = new DirectoryInfo("Assets/DevConsole/Commands");
        FileInfo[] info = dir.GetFiles("*.cs");
        foreach (var file in info)
        {
            scripts.Add(file.Name);
        }
        
        ListView scriptList = rootVisualElement.Query<ListView>("script-list").First();
        scriptList.makeItem = () => new Label();
        scriptList.bindItem = (element, i) => (element as Label).text = scripts[i];
        
        scriptList.itemsSource = scripts;
        scriptList.itemHeight = 16; // ?????
        scriptList.selectionType = SelectionType.Single;
        
        scriptList.Refresh();
    }
    
    // Find all console command files 
    private void FindAllCommands(out ConsoleCommand[] cmds)
    {
        var guids = AssetDatabase.FindAssets("t:ConsoleCommand", new []{"Assets/DevConsole/Assets"});

        cmds = new ConsoleCommand[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            cmds[i] = AssetDatabase.LoadAssetAtPath<ConsoleCommand>(path);
        }
    }
    
    // Generate new script from template
    private void CreateCmdFromTemplate(string cmdName)
    {
        string path = "Assets/DevConsole/";
        string templatePath = path + "consoleCmdTemplate.cs.txt";
        string destName = path + "Commands/cmd_" + cmdName + ".cs";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, destName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Make new scriptable object asset from newly generated script
        
        // AssetDatabase.CreateAsset(newCmd, path + "/Assets/newConsoleCmd.asset");
        // AssetDatabase.SaveAssets();
        // EditorUtility.FocusProjectWindow();
        // Selection.activeObject = newCmd;
    }
}
