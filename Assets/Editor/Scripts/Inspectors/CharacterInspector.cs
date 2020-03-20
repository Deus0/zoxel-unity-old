//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Zoxel.Voxels;
using System;

namespace Zoxel
{
    /// <summary>
    /// Used to show all the characters in map
    /// </summary>
    public class CharacterInspector : EditorWindow
    {
        private SystemsManager systemsManager;
        private Bootstrap bootstrap;

        [MenuItem("Zoxel/Inspectors/CharacterInspector")]
        static public void Init()
        {
            CharacterInspector window = (CharacterInspector)EditorWindow.GetWindow(typeof(CharacterInspector), false, "CharacterInspector");
            window.Show();
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        private void OnGUI()
        {
            if (bootstrap == null)
            {
                if (GameObject.Find("Bootstrap"))
                {
                    bootstrap = GameObject.Find("Bootstrap").GetComponent<Bootstrap>();
                }
            }
            if (bootstrap)
            {
                systemsManager = bootstrap.GetSystems();
            }
            if (systemsManager != null )
            {
                /*if (systemsManager.characterSpawnSystem != null)
                {
                    GUILayout.Label("Characters: " + systemsManager.characterSpawnSystem.characters.Count);
                }
                else
                {
                    GUILayout.Label("systemsManager.characterSpawnSystem is null.");
                }*/
            }
            else
            {
                GUILayout.Label("Systems Manager is null.");
            }
        }
    }
}