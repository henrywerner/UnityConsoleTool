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
    private string _rootPath = "Assets/DevConsole"; // Directory path for the DevConsole folder
    [SerializeField] private CommandCollection _commandCollection; // ScriptableObject for saving commands
    
    private ListView _cmdList, _scriptList;
    private string _scriptableObjectFolder, _commandScriptFolder, _toolFolder, _operationFolder;

    
    [MenuItem("Tools/Dev Console Tool Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConsoleToolWindow>();
        window.titleContent = new GUIContent("Dev Console Tool");
        window.minSize = new Vector2(500, 200);
    }

    private void OnEnable()
    {
        // Define paths
        _scriptableObjectFolder = _rootPath + "/Commands";
        _commandScriptFolder = _rootPath + "/Scripts";
        _toolFolder = _rootPath + "/_Tool";
        _operationFolder = _rootPath + "/_ConsoleOperation";
        
        // Load window layout
        VisualTreeAsset original =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_toolFolder + "/ConsoleToolLayout.uxml");
        TemplateContainer treeAsset = original.CloneTree();
        rootVisualElement.Add(treeAsset);
        
        // Load style sheet
        StyleSheet ss = AssetDatabase.LoadAssetAtPath<StyleSheet>(_toolFolder + "/ConsoleToolStyle.uss");
        rootVisualElement.styleSheets.Add(ss);

        CreateNewCmdButton();
        CreateCmdListView();
        CreateScriptsListView();
        CreateRefreshButtons();
        CreateBuildCmdButton();
        CreateDeleteButton();
        CreateSaveButton();
    }

    /* Build the list view element for displaying commands */
    private void CreateCmdListView()
    {
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

    /* Build the list view element for displaying scripts */
    private void CreateScriptsListView()
    {
        List<string> scripts = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(_commandScriptFolder);
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

    /* Build save button */
    private void CreateSaveButton()
    {
        var btn = rootVisualElement.Q<Button>("save-btn");
        Action OnPress = () =>
        {
            UpdateSavedCommands();
            _cmdList.Refresh();
        };
        btn.RegisterCallback<MouseUpEvent>((evt) => OnPress());
    }

    /* Build delete button */
    private void CreateDeleteButton()
    {
        var btn = rootVisualElement.Q<Button>("delete-btn");
        Action OnPress = () =>
        {
            var i = _cmdList.selectedIndex;
            _cmdList.SetSelection(i != 0 ? 0 : 1);
            DeleteSelectedCmd(i);
        };
        btn.RegisterCallback<MouseUpEvent>((evt) => OnPress());
    }

    /* Build list refresh buttons */
    private void CreateRefreshButtons()
    {
        // refresh button for scripts list
        var scr_refresh = rootVisualElement.Q<Button>("scr-refresh-btn");
        Action OnRefreshScripts = () => CreateScriptsListView();
        scr_refresh.RegisterCallback<MouseUpEvent>((evt) => OnRefreshScripts());
        
        // refresh button for commands list
        var cmd_refresh = rootVisualElement.Q<Button>("cmd-refresh-btn");
        Action OnRefreshCmds = () =>
        {
            CreateCmdListView();
            UpdateSavedCommands();
        };
        cmd_refresh.RegisterCallback<MouseUpEvent>((evt) => OnRefreshCmds());
    } 
    
    /* Set up button for creating new scriptable objects based on command scripts */
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

    
    /* Find all console command files */
    private void FindAllCommands(out ConsoleCommand[] cmds)
    {
        var guids = AssetDatabase.FindAssets("t:ConsoleCommand", new []{_scriptableObjectFolder});

        cmds = new ConsoleCommand[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            cmds[i] = AssetDatabase.LoadAssetAtPath<ConsoleCommand>(path);
        }
    }
    
    /* Update the list of commands stored in the CommandCollection scriptable object */
    private void UpdateSavedCommands() // _Really unoptimized_
    {
        FindAllCommands(out ConsoleCommand[] cmdAssets);
        _commandCollection.cmds = cmdAssets;
        Debug.Log("<DevConsoleTool> :: CommandCollection updated.");
    }
    
    /* Delete the cmd currently selected in the cmdListView */
    private void DeleteSelectedCmd(int index)
    {
        ConsoleCommand selectedCmd = _cmdList.itemsSource[index] as ConsoleCommand;
        var path = AssetDatabase.GetAssetPath(selectedCmd);
        _cmdList.RemoveAt(index);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        
        //Update Saved Commands
        UpdateSavedCommands();
    }
    
    /* Generate new script from template */
    private void CreateCmdFromTemplate(string cmdName)
    {
        string templatePath = _operationFolder + "/consoleCmdTemplate.cs.txt";
        string destName = _commandScriptFolder + "/cmd_" + cmdName + ".cs";
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, destName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    /* Create new asset from script */
    private void CreateAssetFromScript()
    {
        string className = _scriptList.selectedItem.ToString();
        ScriptableObject obj = ScriptableObject.CreateInstance(className);

        string suffix = "";
        int x = 0;
        while (System.IO.File.Exists(_scriptableObjectFolder + "/" + className + suffix + ".asset"))
        {
            x++;
            suffix = "_" + x;
        }
        
        AssetDatabase.CreateAsset(obj, _scriptableObjectFolder + "/" + className + suffix + ".asset");
        AssetDatabase.SaveAssets();
    }
}
