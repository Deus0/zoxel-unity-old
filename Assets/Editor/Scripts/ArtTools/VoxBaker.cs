using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using AnimatorSystem;
using Zoxel.Voxels;
using Unity.Mathematics;
using Zoxel.WorldGeneration;
using Unity.Entities;

namespace Zoxel
{
    public class VoxBaker : EditorWindow
    {
        VoxDatam voxDatam;
        GameDatam gameDatam;
        Mesh mesh;
        Material voxMaterial;
        Unity.Entities.World space;
        List<System.Type> types;
        private VoxDatam bakedVoxDatam;
        public float spawnScale = 1;
        private int worldID = 0;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Zoxel/VoxBaker")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            VoxBaker window = (VoxBaker)EditorWindow.GetWindow(typeof(VoxBaker));
            window.titleContent = new GUIContent("Vox Baker");
            window.Show();
        }

        private void OnEnable()
        {
            if (GameObject.Find("Bootstrap"))
            {
                gameDatam = GameObject.Find("Bootstrap").GetComponent<Bootstrap>().data;
                voxMaterial = gameDatam.voxelColorMaterial;
            }
        }
        public void OnDisable()
        {
            //Debug.Log("Disabling Vox Baker.");
            worldID = 0;
            DestroyECS();
        }

        void OnGUI()
        {
            if (gameDatam == null)
            {
                GUILayout.Label("Re open.");
                return;
            }
            //gameDatam = EditorGUILayout.ObjectField(gameDatam, typeof(GameDatam), false) as GameDatam;
            //voxMaterial = EditorGUILayout.ObjectField(voxMaterial, typeof(Material), false) as Material;
            GUILayout.Label("Select a Vox Data.");
            Bootstrap.DebugMeshWeights = GUILayout.Toggle(Bootstrap.DebugMeshWeights, "Debug Mesh Weights");
            voxDatam = EditorGUILayout.ObjectField(voxDatam, typeof(VoxDatam), false) as VoxDatam;
            //if (voxDatam)
             //   voxDatam.skeleton = EditorGUILayout.ObjectField(voxDatam.skeleton, typeof(SkeletonDatam), false) as SkeletonDatam;
            // mesh = EditorGUILayout.ObjectField(mesh, typeof(Mesh), false) as Mesh;
            if (voxDatam)
            {
                if (space == null)
                {
                    if (GUILayout.Button("Spawn Mesh"))
                    {
                        worldID = 0;
                        InitECS();
                        BakeMesh();
                    }
                    /*if (GUILayout.Button("Spawn Oldschool Mesh"))
                    {
                        SkinnedMeshRenderer skinnyMesh = CreateSkinny().GetComponent<SkinnedMeshRenderer>();
                        voxDatam.skeleton.BakeMeshWeights(skinnyMesh);
                        VoxBaker.SaveMesh(skinnyMesh.sharedMesh);
                    }*/
                }
                else
                {
                    /*if (worldID == 0 && GUILayout.Button("Test World"))
                    {
                        worldID = space.GetOrCreateSystem<VoxelSystemGroup>().worldSpawnSystem.QueueWorld(Unity.Mathematics.float3.zero, gameDatam.startingMap);

                    }*/
                    if ((bakedVoxDatam != voxDatam || worldID == 0) && GUILayout.Button("ReBake Mesh"))
                    {
                        BakeMesh();
                        Repaint();
                    }
                    else if (worldID != 0 )
                    {
                        if (mesh == null)
                        {
                            GUILayout.Label("Fetching Mesh.");
                            FetchMesh();
                        }
                        if (mesh != null)
                        {
                            if (GUILayout.Button("Centre Mesh"))
                            {
                                CentreMesh();
                            }
                            if (GUILayout.Button("Rotate Mesh"))
                            {
                                RotateMesh();
                            }
                            if (GUILayout.Button("Save Mesh"))
                            {
                                SaveMesh();
                            }
                        }
                    }
                    if (GUILayout.Button("Despawn Mesh"))
                    {
                        mesh = null;
                        worldID = 0;
                        DestroyECS();
                    }
                }
            }
        }

        private GameObject CreateSkinny()
        {
            // spawn object
            GameObject newSkinny = new GameObject();
            //voxDatam.skeleton.InstantiateBones(newSkinny);
            newSkinny.name = voxDatam.name;
            //newSkinny.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            SkinnedMeshRenderer mesher = newSkinny.AddComponent<SkinnedMeshRenderer>();
            //mesher.bones = voxDatam.skeleton.GetBoneTransforms();
            //mesher.sharedMesh = voxDatam.bakedMesh;
            //mesher.sharedMaterial = voxDatam.bakedMaterial;
            mesher.rootBone = newSkinny.transform;
           /* if (isPlaceUpwards)
            {
                newSkinny.transform.position = new Vector3(0, mesher.bounds.extents.y, 0);
            }*/
            Animation animationComponent = newSkinny.AddComponent<Animation>();
            /*for (int i = 0; i < animations.Count; i++)
            {
                animationComponent.AddClip(animations[i], animations[i].name);
            }*/
            return newSkinny;
        }

        public void CentreMesh()
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i] -= mesh.bounds.min;
            }
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i] -= mesh.bounds.extents;
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public void RotateMesh()
        {
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                //verts[i].z = -verts[i].z;
                verts[i] = math.rotate(Quaternion.Euler(180, 0, 180), verts[i]);
            }
            mesh.vertices = verts;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        void FetchMesh()
        {
            Unity.Entities.EntityManager entityManager = space.EntityManager;
            ChunkSpawnSystem chunkSpawnSystem = space.GetOrCreateSystem<VoxelSystemGroup>().chunkSpawnSystem;
            foreach (KeyValuePair<int, Unity.Entities.Entity> KVP in chunkSpawnSystem.chunks)
            {
                Chunk chunk = entityManager.GetComponentData<Chunk>(KVP.Value);
                if (chunk.worldID == worldID)
                {
                    if (chunk.chunkRenders.Length > 0)
                    {
                        mesh = entityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(
                            chunkSpawnSystem.chunkRenders[chunk.chunkRenders[0]]).mesh;
                    }
                    else
                    {
                        Debug.LogError("No chunk renders in chunk: " + chunk.Value.chunkPosition);
                    }
                    break;
                }
            }
        }

        void SaveMesh()
        {
            SaveMesh(mesh, voxDatam); //, "", true, true);
            //voxDatam.bakedMesh = mesh;
            //voxDatam.bakedMaterial = voxMaterial;
            EditorUtility.SetDirty(voxDatam);
        }

        public static void SaveMesh(Mesh mesh, VoxDatam voxDatam)//, string name, bool makeNewInstance, bool optimizeMesh)
        {
            string name = voxDatam.name;
            if (name.Contains("Vox"))
                name = name.Replace("Vox", "Model");
           // bool makeNewInstance = true;
            //Debug.LogError("Saving with bones: " + mesh.bindposes.Length);
            BoneWeight[] newWeights = new BoneWeight[mesh.boneWeights.Length];
            for (int i = 0; i < mesh.boneWeights.Length; i++)
            {
                BoneWeight weight = mesh.boneWeights[i];
                if (mesh.boneWeights[i].boneIndex0 < 0
                    || mesh.boneWeights[i].boneIndex0 >= mesh.bindposes.Length)
                {
                    weight.boneIndex0 = 0;
                    Debug.LogError("Bone Index out of range: " + mesh.boneWeights[i].boneIndex0);
                }
                newWeights[i] = weight;
            }
            mesh.boneWeights = newWeights;
            string startFolder = AssetDatabase.GetAssetPath(voxDatam);
            startFolder = System.IO.Path.GetDirectoryName(startFolder);
            Debug.Log("Asset path of vox: " + startFolder);
            ///string startFolder = FileUtil.GetProjectRelativePath(assetPath);
            //Debug.Log("GetProjectRelativePath: " + startFolder);
            string path = EditorUtility.SaveFilePanel("Save Mesh Asset", startFolder, name, "asset");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Decided not to save.");
                return;
            }
            path = FileUtil.GetProjectRelativePath(path);

          //  Mesh meshToSave = (makeNewInstance) ? UnityEngine.Object.Instantiate(mesh) as Mesh : mesh;

            //if (optimizeMesh)
            //    MeshUtility.Optimize(meshToSave);
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
        }

        void BakeMesh()
        {
            VoxelSystemGroup voxelSystemGroup = space.GetOrCreateSystem<VoxelSystemGroup>();
            if (worldID != 0)
            {
                voxelSystemGroup.worldSpawnSystem.RemoveWorld(worldID);
            }
            //voxDatam.bakedMaterial = voxMaterial;
            worldID = voxelSystemGroup.worldSpawnSystem.SpawnModel(float3.zero, voxDatam);
            bakedVoxDatam = voxDatam;
            space.EntityManager.SetComponentData(voxelSystemGroup.worldSpawnSystem.worlds[worldID],
                new Unity.Transforms.NonUniformScale { Value = new float3(spawnScale, spawnScale, spawnScale) });
        }

        private void InitECS()
        {
            if (space != null)
            {
                return;
            }
            types = new List<Type>();
            types.Add(typeof(VoxelSystemGroup));
            types.Add(typeof(WorldSystemGroup));
            types.AddRange(SystemsManager.GetUnityTypes());
            space = SystemsManager.CreateWorld(types, "Zaker");
            VoxelSystemGroup voxelSystemGroup = space.GetOrCreateSystem<VoxelSystemGroup>();
            WorldSystemGroup worldSystemGroup = space.GetOrCreateSystem<WorldSystemGroup>();
            worldSystemGroup.Initialize(space);
            voxelSystemGroup.Initialize(space);
            worldSystemGroup.CombineWithVoxels(voxelSystemGroup);
            voxelSystemGroup.SetMeta(gameDatam);
            EditorApplication.update += UpdateEditorWindow;
            // add camera systems
            // zoom into mesh
            // Add render texture of camera to this UI
        }

        void DestroyECS()
        {
            if (space != null)
            {
                space.GetOrCreateSystem<VoxelSystemGroup>().Clear();
                space.Dispose();
                space = null;
                EditorApplication.update -= UpdateEditorWindow;
                Repaint();
            }
        }

        void UpdateEditorWindow()
        {
            //Debug.LogError("Updating Systems [" + types.Count + "]");
            foreach (System.Type theType in types)
            {
                space.GetOrCreateSystem(theType).Update();
            }
            //systemsManager.UpdateSystems();
            Repaint();
        }
    }

}