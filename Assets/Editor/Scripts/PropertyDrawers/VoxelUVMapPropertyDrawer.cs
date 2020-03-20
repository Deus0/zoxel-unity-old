using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    [CustomPropertyDrawer(typeof(VoxelUVMap))]
    public class VoxelUVMapPropretyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VoxelUVMap uvMap = (VoxelUVMap)property.GetValue();
            var container = new VisualElement();
            /*Label newLabel = new Label();
            newLabel.text = "Uvs [" + uvMap.uvs.Length + "]";
            container.Add(newLabel);*/
            container.styleSheets.Add(Resources.Load<StyleSheet>("VoxelUvMapEditor/VoxelUvMapEditor"));
            var quickToolVisualTree = Resources.Load<VisualTreeAsset>("VoxelUvMapEditor/VoxelUvMapEditor");
            quickToolVisualTree.CloneTree(container);
            Label uvsLengthLabel = container.Query("uvsLengthLabel").First() as Label;
            uvsLengthLabel.text = "Uvs [" + uvMap.uvs.Length + "]";
            return container;
        }
    }
}