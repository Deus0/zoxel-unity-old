using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zoxel
{
    [CustomEditor(typeof(VoxelDatam))]
    public class VoxelDatamEditor : Editor
    {

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            // Draw the legacy IMGUI base
            var imgui = new IMGUIContainer(OnInspectorGUI);
            container.Add(imgui);

            // Create property fields.
            // Add fields to the container.
            container.Add(new PropertyField(serializedObject.FindProperty("uvMap")));
            return container;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

    }

}
