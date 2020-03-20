using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemMaker : EditorWindow
{
    [MenuItem("Zoxel/Makers/ItemMaker")]
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<ItemMaker>();
        // Adds a title to the window.
        window.titleContent = new GUIContent("ItemMaker");
       // window.resi
    }

    private void OnEnable()
    {
        Debug.Log("Initiated ItemMaker");
        // Reference to the root of the window.
        var root = rootVisualElement;

        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        root.styleSheets.Add(Resources.Load<StyleSheet>("Makers/ItemMaker/ItemMaker"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("Makers/ItemMaker/ItemMaker");
        quickToolVisualTree.CloneTree(root);
        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        var toolButtons = root.Query<Button>();
        toolButtons.ForEach(SetupButton);
    }

    private void SetupButton(Button button)
    {
        // Instantiates our primitive object on a left click.
        button.clickable.clicked += () => OnClicked(button.name);
    }

    private void OnClicked(string buttonName)
    {
        Debug.Log("Clicked Button " + buttonName);
    }
}