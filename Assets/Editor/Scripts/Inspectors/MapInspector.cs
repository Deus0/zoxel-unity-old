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
    public class MapInspector : EditorWindow
    {
        private SystemsManager systemsManager;
        private Bootstrap bootstrap;

        [MenuItem("Zoxel/Inspectors/MapInspector")]
        static public void Init()
        {
            MapInspector window = (MapInspector)EditorWindow.GetWindow(typeof(MapInspector), false, "MapInspector");
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
                if (systemsManager.voxelSystemGroup != null)
                {
                    GUILayout.Label("Worlds: " + systemsManager.voxelSystemGroup.worldSpawnSystem.worlds.Count);
                    if (systemsManager.voxelSystemGroup.worldSpawnSystem.worlds.Count > 0)
                    {
                        // now get map for this
                        // add to UI
                        GUILayout.Label("Chunks: " + systemsManager.voxelSystemGroup.chunkSpawnSystem.chunks.Count);
                    }

                }
                else
                {
                    GUILayout.Label("systemsManager.voxelSystemGroup is null.");
                }
            }
            else
            {
                GUILayout.Label("Systems Manager is null.");
            }
        }
    }
}