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
    [SerializeField] private CommandCollection _commandCollection;
    private ListView _cmdList, _scriptList;
    
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
        CreateRefreshButtons();
        CreateBuildCmdButton();
        //CreateCollectionListView();
        
    }

    private void UpdateCmdDictionary() // _Really unoptimized_
    {
        FindAllCommands(out ConsoleCommand[] cmdAssets);
        Dictionary<string, ConsoleCommand> tempCmds = new Dictionary<string, ConsoleCommand>();
        foreach (var c in cmdAssets)
            tempCmds.Add(c.CommandName, c);

        _commandCollection.cmds = tempCmds;
        
        Debug.Log("<DevConsoleTool> :: CommandCollection updated.");
    }

    private void CreateRefreshButtons()
    {
        var scr_refresh = rootVisualElement.Q<Button>("scr-refresh-btn");
        Action OnRefreshScripts = () => CreateScriptsListView();
        scr_refresh.RegisterCallback<MouseUpEvent>((evt) => OnRefreshScripts());
        
        var cmd_refresh = rootVisualElement.Q<Button>("cmd-refresh-btn");
        Action OnRefreshCmds = () =>
        {
            CreateCmdListView();
            UpdateCmdDictionary();
        };
        cmd_refresh.RegisterCallback<MouseUpEvent>((evt) => OnRefreshCmds());
    }

    private void CreateBuildCmdButton()
    {
        var btn = rootVisualElement.Q<Button>("create-cmd-btn");
        Action OnClick = () =>
        {
            CreateAssetFromScript();
            CreateCmdListView();
        };
        btn.RegisterCallback<MouseUpEvent>((evt) => OnClick());
    }

    private void CreateNewCmdButton()
    {
        var btn = rootVisualElement.Q<Button>("add-cmd-btn");
        var textEntry = rootVisualElement.Q<TextField>("add-cmd-entry");
        Action OnPress = () =>
        {
            CreateCmdFromTemplate(textEntry.text);
            CreateCmdListView(); // we have to redraw the whole section?
            textEntry.value = "";
        };
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
        
        _cmdList = rootVisualElement.Query<ListView>("cmd-list").First();
        _cmdList.makeItem = () => new Label();
        _cmdList.bindItem = (element, i) =>
            (element as Label).text =
            (cmds[i].CommandName == string.Empty)
                ? "[no name]"
                : cmds[i].CommandName;

        _cmdList.itemsSource = cmds;
        _cmdList.itemHeight = 16; // ?????
        _cmdList.selectionType = SelectionType.Single;

        _cmdList.onSelectionChange += enumerable =>
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
        
        _cmdList.Refresh();
    }

    private void CreateScriptsListView()
    {
        List<string> scripts = new List<string>();
        DirectoryInfo dir = new DirectoryInfo("Assets/DevConsole/Commands");
        FileInfo[] info = dir.GetFiles("*.cs");
        foreach (var file in info)
        {
            scripts.Add(file.Name.Substring(0, file.Name.Length - 3));
        }
        
        _scriptList = rootVisualElement.Query<ListView>("script-list").First();
        _scriptList.makeItem = () => new Label();
        _scriptList.bindItem = (element, i) => (element as Label).text = scripts[i];
        
        _scriptList.itemsSource = scripts;
        _scriptList.itemHeight = 16; // ?????
        _scriptList.selectionType = SelectionType.Single;
        
        _scriptList.Refresh();
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
    
    // Create new asset from script
    private void CreateAssetFromScript()
    {
        string className = _scriptList.selectedItem.ToString();
        ScriptableObject obj = ScriptableObject.CreateInstance(className);

        string suffix = "";
        int x = 0;
        while (System.IO.File.Exists("Assets/DevConsole/Assets/" + className + suffix + ".asset"))
        {
            x++;
            suffix = "_" + x;
        }
        
        AssetDatabase.CreateAsset(obj, "Assets/DevConsole/Assets/" + className + suffix + ".asset");
        AssetDatabase.SaveAssets();
    }
}
