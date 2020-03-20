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
using Zoxel.WorldGeneration;

namespace Zoxel
{
    /// <summary>
    /// Used to edit map
    /// Todo:
    ///     Paint voxels / build
    ///     Move camera around in window itself
    ///     RenderTexture in window
    /// </summary>
    public class MapEditor : EditorWindow
    {
        private bool isDebug = false;
        private Editor gameObjectEditor;
        private MapDatam map;
        private MapDatam[] maps;
        private GameDatam gameData;
        public Camera editorCamera;
        //public SystemsManager systemsManager = new SystemsManager();
        private VisualElement buttonsParent;
        Unity.Entities.World space;
        List<Type> types;

        [MenuItem("Zoxel/Makers/MapMaker")]
        static public void Init()
        {
            MapEditor window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor), false, "MapMaker");
            window.Show();
        }

        public void OnEnable()
        {
            this.titleContent = new GUIContent("MapMaker"); // [" + System.DateTime.Now.ToString("HH-dd-MMMyy").Replace('.', '-') + "]");
            maps = Resources.FindObjectsOfTypeAll<MapDatam>();
            if (GameObject.Find("Bootstrap"))
            {
                gameData = GameObject.Find("Bootstrap").GetComponent<Bootstrap>().data;
                Debug.Log("Found Game Data: " + gameData.name);
            }
            InitECS();
            /*systemsManager.Initiate(gameData, "MapMaker");
            systemsManager.SetSystems(true);*/
            // init UI
            var root = rootVisualElement;
            root.styleSheets.Add(Resources.Load<StyleSheet>("Makers/MapMaker/MapMaker"));
            var quickToolVisualTree = Resources.Load<VisualTreeAsset>("Makers/MapMaker/MapMaker");
            quickToolVisualTree.CloneTree(root);
            var mapFieldPrefab = root.Query("MapSelectionPrefab").First();
            buttonsParent = mapFieldPrefab.parent;
            mapFieldPrefab.parent.Remove(mapFieldPrefab);
            SpawnMapButtons();
        }

        void InitECS()
        {
            types = new List<Type>();
            types.Add(typeof(VoxelSystemGroup));
            types.Add(typeof(WorldSystemGroup));
            types.AddRange(SystemsManager.GetUnityTypes());
            space = SystemsManager.CreateEditorWorld(types, "MapMaker");
            var voxelSystemGroup = space.GetOrCreateSystem<VoxelSystemGroup>();
            var worldSystemGroup = space.GetOrCreateSystem<WorldSystemGroup>();
            worldSystemGroup.Initialize(space);
            voxelSystemGroup.Initialize(space);
            worldSystemGroup.CombineWithVoxels(voxelSystemGroup);
            voxelSystemGroup.SetMeta(gameData);
            Debug.Log("Adding Update Method from MapMaker.");
            EditorApplication.update += UpdateEditorWindow;
        }

        public void OnDisable()
        {
            Debug.Log("Removing Update Method from MapMaker.");
            EditorApplication.update -= UpdateEditorWindow;
            //ClearMap();
            //systemsManager.Clear();
            if (space != null)
            {
                if (space.IsCreated)
                {
                    space.GetOrCreateSystem<VoxelSystemGroup>().Clear();
                    space.Dispose();
                }
                space = null;
                //EditorApplication.update -= UpdateEditorWindow;
                //Repaint();
            }
        }

        void ClearButtons()
        {
            List<VisualElement> buttons = new List<VisualElement>();
            foreach (var child in buttonsParent.Children())
            {
                buttons.Add(child);
            }
            foreach (var child in buttons)
            {
                buttonsParent.Remove(child);
            }
        }

        void SpawnMapButtons()
        {
            for (int i = 0; i < maps.Length; i++)
            {
                MapDatam selectedMap = maps[i];
                Button newButton = new Button();
                newButton.clicked += (() =>
                {
                    SetNewMap(selectedMap);
                });
                newButton.text = "[" + maps[i].name + "]";
                buttonsParent.Add(newButton);
            }
        }

        void SpawnMapSelectedUI()
        {
            Button newButton = new Button();
            newButton.clicked += (() =>
            {
                SetNewMap(null);
            });
            newButton.text = "Close Map";
            buttonsParent.Add(newButton);
        }

        void UpdateEditorWindow()
        {
            if (isDebug)
            {
                Debug.LogError("Updating");
            }
            if (space == null)
            {
                EditorApplication.update -= UpdateEditorWindow;
                return;
            }
            if (space.IsCreated == false)
            {
                EditorApplication.update -= UpdateEditorWindow;
                return;
            }
            foreach (System.Type theType in types)
            {
                if (space.GetOrCreateSystem(theType) != null)
                {
                    space.GetOrCreateSystem(theType).Update();
                }
            }
            /*if (systemsManager != null)
            {
                systemsManager.UpdateUnitySystems();
                if (systemsManager.voxelSystemGroup != null)
                {
                    systemsManager.voxelSystemGroup.Update();
                }
                else
                {
                    Debug.LogError("Voxel Systems were null.");
                    //CreateVoxelSystems();
                }
                if (systemsManager.worldSystemGroup != null)
                {
                    systemsManager.worldSystemGroup.Update();
                }
                else
                {
                    Debug.LogError("World Systems were null.");
                    //CreateWorldSystems();
                }
                Repaint();
            }
            else
            {
                Debug.LogError("SystemManager was null");
                systemsManager = new SystemsManager();
                systemsManager.Initiate(gameData, "MapMaker");
                systemsManager.SetSystems(true);
            }*/
        }

        private void ClearMap()
        {
            SetNewMap(null);
        }

        private void SetNewMap(MapDatam newMap)
        {
            if (map != newMap)
            {
                if (map != null)
                {
                    Debug.Log("Closing Old Map: " + map.name);
                    //systemsManager.voxelSystemGroup.worldSpawnSystem.Clear();
                    space.GetOrCreateSystem<VoxelSystemGroup>().Clear();
                }
                map = newMap;
                if (map != null)
                {
                    Debug.Log("New Map Set: " + newMap.name);
                    //systemsManager.voxelSystemGroup.worldSpawnSystem.QueueWorld(float3.zero, map);
                    space.GetOrCreateSystem<VoxelSystemGroup>().worldSpawnSystem.QueueMap(float3.zero, map);
                    this.titleContent = new GUIContent("MapMaker [" + map.name + "]");
                    // clear buttons
                    ClearButtons();
                    SpawnMapSelectedUI();
                }
                else
                {
                    this.titleContent = new GUIContent("MapMaker");
                    //this.titleContent = new GUIContent("MapMaker [" + System.DateTime.Now.ToString("HH-dd-MMMyy").Replace('.', '-') + "]");
                    // respawn buttons
                    ClearButtons();
                    SpawnMapButtons();
                }
            }
        }

        public void OnGUI() { }
    }
}

/*
   if (map == null)
    {
        if (GUILayout.Button("Refresh"))
        {
            maps = Resources.FindObjectsOfTypeAll<MapDatam>();
        }
    }
    if (map != null)
    {
        DrawMapUI();
    }
    else
    {
        DrawLoadingUI();
    }
void DrawLoadingUI()
{
   GUILayout.Label("Select a map to load");
   GameDatam newGameData = EditorGUILayout.ObjectField(gameData, typeof(GameDatam), false) as GameDatam;
   if (newGameData != gameData)
   {
       gameData = newGameData;
       systemsManager.SetData(gameData);
   }
   MapDatam newMap = EditorGUILayout.ObjectField(map, typeof(MapDatam), false) as MapDatam;
   for (int i = 0; i < maps.Length; i++)
   {
       if (GUILayout.Button("Select [" + maps[i].name + "]"))
       {
           newMap = maps[i];
       }
   }
   SetNewMap(newMap);
}

void DrawMapUI()
{
   //GUI.enabled = false;
   MapDatam newMap = EditorGUILayout.ObjectField(map, typeof(MapDatam), false) as MapDatam;
   GUILayout.Label("Map [" + map.name + "] is loaded. It has no vox.");
   if (GUILayout.Button("Close"))
   {
       ClearMap();
   }
}*/
