# Dev Console Creator Tool
✅ Verified as working in versions 2020.3.1f1 and 2020.3.18f1 of Unity

## Installation
Import `DevConsoleTool.unitypackage` into your Unity project.

## Here's how the whole thing works
**Scripts:** Contains the code that will be run whenever your command is run from the console.

**Commands:** Scriptable objects that store the command name and description. 

**CommandCollection:** A singular scriptable object that holds a list of all saved commands.

**ConsoleManager:** The script that handles input and output for your in-game console

## Setup
### Setting up your in-game console:
1. The whole console is based around two UI elements: one Text object and one InputField object. 
    - The Text object will be used for the console’s history log.
    - The InputField will be used as the entry field for inputting commands.
2. Once you’ve set up your UI elements, attach the ConsoleManager script to an object in your scene. This doesn’t have to be on a UI layer, it can even be on an empty GameObject.
3. Plug in your Text and InputField objects into the ConsoleManager script
4. Set History Length to the number of lines tall you want your history log to be
5. Set “Command Collection” to the CommandCollection scriptable object that came included with the DevConsole. (located at `DevConsole/_ConsoleOperation/CommandCollection.asset`)
6. That’s it. Now you can start creating your commands!

### Creating a new script:
1. Open the Console Tool window (Tools > Console Tool Window)
2. In the script column, look for the text entry field labeled “Create new script”.
3. Type a name for your script into the entry field. Then click the “+” button. (Note: a “cmd_” prefix will be added to your script name)
4. Unity will focus on the new file that is being created. To complete the creation process either press the enter key or click anywhere on the Console Tool window.
5. The console tool may take a second for the ui to show the newly created file.

### Adding functionality to your script:
1. Open your script in an IDE.
2. The Process function is called whenever your command is run via the in-game console. This is where you can code whatever functionality you want.
    - The “args” string array contains any strings that followed the initial command name when the command was called (args are acquired by splitting the player’s input string on spaces).

### Adding a new command:
1. Open the Console Tool window (Tools > Console Tool Window)
2. In the script column select the script that you want to be called whenever your command will be run.
3. Now click the “+ new command” button.
4. A new entry should appear in the Commands column with the label “[no name]”. This is your new command. Click on it.
5. After selecting your new command, edit its name and description using the Details panel.
6. After you’ve entered your name and description, click the save button in the bottom right.
