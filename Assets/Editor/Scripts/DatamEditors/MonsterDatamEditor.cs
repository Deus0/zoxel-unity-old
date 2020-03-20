using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Zoxel
{
    [CustomEditor(typeof(CharacterDatam))]
    public class MonsterDatamEditor : Editor
    {

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            // Draw the legacy IMGUI base
            var imgui = new IMGUIContainer(OnInspectorGUI);

            // Create property fields.
            // Add fields to the container.
            container.Add(new PropertyField(serializedObject.FindProperty("stats")));

            container.Add(imgui);

            return container;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                DrawDefaultInspector();
            }
            catch (System.NullReferenceException e)
            {
                UnityEngine.Debug.Log("e: " + e.ToString());
            }
        }

    }

}
