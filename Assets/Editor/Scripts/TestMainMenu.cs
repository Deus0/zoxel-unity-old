using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TestMainMenu : EditorWindow
{
    [MenuItem("Zoxel/TestMainMenu")]
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<TestMainMenu>();
        // Adds a title to the window.
        window.titleContent = new GUIContent("TestMainMenu");
    }

    private void OnEnable()
    {
        Debug.Log("Initiated Main Menu Test");
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        root.styleSheets.Add(Resources.Load<StyleSheet>("MainMenu/ZoxelMainMenu"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("MainMenu/ZoxelMainMenu");
        quickToolVisualTree.CloneTree(root);
        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        var toolButtons = root.Query<Button>();
        toolButtons.ForEach(SetupButton);
    }

    private void SetupButton(Button button)
    {
        button.clickable.clicked += () => CreateObject(button.name);
    }

    private void CreateObject(string primitiveTypeName)
    {
        Debug.Log("Clicked Button " + primitiveTypeName);
    }
}