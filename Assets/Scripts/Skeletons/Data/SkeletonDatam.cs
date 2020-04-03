using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// Bone data
    /// Exported from a maya import with bones
    /// Used to spawn bone entities and to animate in game
    /// </summary>
    [CreateAssetMenu(fileName = "Skeleton", menuName = "ZoxelArt/Skeleton")]//, order = 7)]
    public class SkeletonDatam : ScriptableObject//or monobehaviour
    {
        public SkeletonData data;

        [ContextMenu("Weird Adjustment")]
        public void WeirdAdjustment()
        {
            for (int i = 0; i < data.datas.Count; i++)
            {
                BoneData boneData = data.datas[i];
                boneData.position = (new float3(boneData.position.x, boneData.position.y, boneData.position.z + 0.15f)
                    + new float3(0.5f, 1f, 0.5f));
                boneData.rotation = quaternion.identity;
                data.datas[i] = boneData;
            }
        }
        [ContextMenu("UnWeird Adjustment")]
        public void UnWeirdAdjustment()
        {
            for (int i = 0; i < data.datas.Count; i++)
            {
                BoneData boneData = data.datas[i];
                boneData.position = (new float3(boneData.position.x, boneData.position.y, boneData.position.z - 0.15f)
                    - new float3(0.5f, 1f, 0.5f));
                boneData.rotation = quaternion.identity;
                data.datas[i] = boneData;
            }
        }

        [ContextMenu("GenerateID")]
        public void GenerateID()
        {
            data.id = Bootstrap.GenerateUniqueID();
        }

        public BoneData GetBone(string name)
        {
            for (int i = 0; i < data.datas.Count; i++)
            {
                if (data.datas[i].name == name)
                    return data.datas[i];
            }
            return new BoneData();
        }

        List<Transform> boneTransforms;
        public Transform[] GetBoneTransforms()
        {
            return boneTransforms.ToArray();
        }

        public Dictionary<string, GameObject> InstantiateBones(GameObject skeleton)
        {
            skeleton.name = name;
            // spawn all the bones onto the gameobject
            Dictionary<string, bool> hasSpawned = new Dictionary<string, bool>();
            Dictionary<string, GameObject> spawns = new Dictionary<string, GameObject>();
            boneTransforms = new List<Transform>();
            for (int i = 0; i < data.datas.Count; i++)
            {
                hasSpawned.Add(data.datas[i].name, false);
            }
            int tryCounts = 0;
            bool hasSpawnedAll = false;
            while (hasSpawnedAll == false)
            {
                //foreach (KeyValuePair<string, bool> KVP in hasSpawned)
                foreach (BoneData bone in data.datas)
                {
                    if (hasSpawned[bone.name] == false)
                    {
                        //BoneData bone = GetBone(KVP.Key);
                        if (bone.parentName == "" || bone.parentName == "Root")
                        {
                            // spawn first bone hip
                            GameObject boneChild = new GameObject(bone.name);
                            boneChild.transform.parent = skeleton.transform;
                            //boneChild.transform.position = skeleton.transform.TransformPoint(bone.position);
                            boneChild.transform.localPosition = (bone.position);
                            boneTransforms.Add(boneChild.transform);
                            hasSpawned[bone.name] = true;
                            spawns[bone.name] = boneChild;
                        }
                        else
                        {
                            if (hasSpawned.ContainsKey(bone.parentName) == false)
                            {
                                Debug.LogError("No " + bone.parentName + " in bones.");
                            }
                            else
                            {
                                if (hasSpawned[bone.parentName])
                                {
                                    // spawn a new bone underneath that parent
                                    GameObject boneChild = new GameObject(bone.name);
                                    boneChild.transform.parent = skeleton.transform;
                                    //boneChild.transform.position = bone.position;
                                    boneChild.transform.position = (bone.position);
                                    boneChild.transform.parent = spawns[bone.parentName].transform;
                                    boneTransforms.Add(boneChild.transform);
                                    hasSpawned[bone.name] = true;
                                    spawns[bone.name] = boneChild;
                                }
                            }
                        }
                    }
                }
                bool checkAll = true;
                foreach (KeyValuePair<string, bool> KVP in hasSpawned)
                {
                    if (KVP.Value == false)
                    {
                        checkAll = false;
                        break;
                    }
                }
                if (checkAll)
                {
                    hasSpawnedAll = true;
                }
                tryCounts++;
                if (tryCounts >= 1000)
                {
                    Debug.LogError("Failed to spawn skeleton");
                    return spawns;
                }
            }

            return spawns;
        }

        [ContextMenu("Generate IDs")]
        public void GenerateIDs()
        {
            for (int i = 0; i < data.datas.Count; i++)
            {
                BoneData thisBone = data.datas[i];
                thisBone.id = Bootstrap.GenerateUniqueID();
                data.datas[i] = thisBone;
            }
            FixParentIDs();
        }

        [ContextMenu("Fix ParentIDs")]
        public void FixParentIDs()
        {
            for (int i = 0; i < data.datas.Count; i++)
            {
                if (data.datas[i].parentID == 0)
                {
                    // get bone data of parent
                    BoneData parentBone = GetBone(data.datas[i].parentName);
                    BoneData thisBone = data.datas[i];
                    thisBone.parentID = parentBone.id;
                    data.datas[i] = thisBone;
                }
            }
        }
        // bullet data
        // for other stuff?
        // list of bones
        // 
        [ContextMenu("Generate Humanoid")]
        public void GenerateHumanoid()
        {
            data = new SkeletonData();
            data.datas = new List<BoneData>();
            // first get our list of bones
            List<Vector3> bonePositions = new List<Vector3>();
            List<string> boneNames = new List<string>();
            List<string> boneParents = new List<string>();
            List<float> boneInfluences = new List<float>();
            // hips
            AddBone("Hip", "Root", new float3(0, -0.175f, 0), 0.4f);
            // chest
            AddBone("Chest", "Hip", new float3(0, 0.4f, -0.1f), 0.4f);
            // head
            AddBone("Head", "Chest", new float3(0, 0.75f, -0.1f), 0.3f);

            // Right Arm
            AddBone("RightShoulder", "Chest", new float3(0.22f, 0.4f, -0.1f), 0.19f);
            AddBone("RightElbow", "RightShoulder", new float3(0.325f, 0.175f, -0.1f), 0.16f);
            AddBone("RightHand", "RightElbow", new float3(0.325f, 0, -0.1f), 0.12f);

            // Left Arm
            AddBone("LeftShoulder", "Chest", new float3(-0.22f, 0.4f, -0.1f), 0.19f);
            AddBone("LeftElbow", "LeftShoulder", new float3(-0.325f, 0.175f, -0.1f), 0.16f);
            AddBone("LeftHand", "LeftElbow", new float3(-0.325f, 0, -0.1f), 0.12f);

            // Right Leg
            AddBone("LeftKnee", "Hip", new float3(0.2f, -0.575f, -0.1f), 0.44f);
            AddBone("LeftFoot", "LeftKnee", new float3(0.2f, -0.8f, -0.1f), 0.5f);

            // Left Leg
            AddBone("RightKnee", "Hip", new float3(-0.2f, -0.575f, -0.1f), 0.44f);
            AddBone("RightFoot", "RightKnee", new float3(-0.2f, -0.8f, -0.1f), 0.5f);
        }

        private void AddBone(string boneName, string parentName, float3 position, float influence)
        {
            BoneData newData = new BoneData();
            newData.id = Bootstrap.GenerateUniqueID();
            newData.name = boneName;
            newData.parentName = parentName;
            newData.position = position;
            newData.influence = influence;
            data.datas.Add(newData);
        }

        public void BakeMeshWeights(SkinnedMeshRenderer skinnyMesh)
        {
            int bonesCount = data.datas.Count;
            Mesh mesh = skinnyMesh.sharedMesh;
            if (skinnyMesh == null)
            {
                Debug.LogError("SkinnyMesh is null");
                return;
            }
            if (mesh == null)
            {
                Debug.LogError("SkinnyMesh is null");
                return;
            }
            List<Vector3> bonePositions = new List<Vector3>();
            List<string> boneNames = new List<string>();
            List<string> boneParents = new List<string>();
            List<float> boneInfluences = new List<float>();
            for (int i = 0; i < bonesCount; i++)
            {
                boneNames.Add(data.datas[i].name);
                boneParents.Add(data.datas[i].parentName);
                bonePositions.Add(data.datas[i].position);
                boneInfluences.Add(data.datas[i].influence);
            }
            BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
            Transform[] boneTransforms = new Transform[bonesCount];
            Matrix4x4[] bindPoses = new Matrix4x4[bonesCount];
            float[] boneInfluenceRadius = new float[bonesCount];
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                boneTransforms[j] = new GameObject(boneNames[j]).transform;
                int boneParentIndex = boneNames.IndexOf(boneParents[j]);
                if (boneParentIndex != -1)
                {
                    boneTransforms[j].parent = boneTransforms[boneParentIndex];
                }
                else
                {
                    boneTransforms[j].parent = skinnyMesh.transform;
                }
                boneInfluenceRadius[j] = boneInfluences[j];// 0.1f;
                boneTransforms[j].position = bonePositions[j];
                bindPoses[j] = boneTransforms[j].worldToLocalMatrix;// * skinnyMesh.transform.localToWorldMatrix;
            }
            for (int j = 0; j < weights.Length; j++)
            {
                // get vertex point
                Vector3 vertex = mesh.vertices[j];
                float[] boneDistances;
                int closestIndex;
                CalculateBoneDistances(vertex, ref boneTransforms, ref boneInfluenceRadius, out boneDistances, out closestIndex);
                weights[j].boneIndex0 = closestIndex;
                weights[j].weight0 = 1;
            }
            mesh.boneWeights = weights;
            mesh.bindposes = bindPoses;
            skinnyMesh.rootBone = skinnyMesh.transform;
            skinnyMesh.bones = boneTransforms;
            skinnyMesh.sharedMesh = mesh;
        }

        void CalculateBoneDistances(Vector3 vertexPosition, ref Transform[] bones, ref float[] boneInfluenceRadius, out float[] boneDistances, out int closestIndex)
        {
            boneDistances = new float[bones.Length];
            float closestDistance = 10000;
            closestIndex = 0; // default first bone
            for (int i = 0; i < bones.Length; i++)
            {
                boneDistances[i] = Vector3.Distance(bones[i].position, vertexPosition);
                // check if distance is less then others, check if is in bones influence radius
                if (boneDistances[i] < closestDistance && boneDistances[i] < boneInfluenceRadius[i])
                {
                    closestDistance = boneDistances[i];
                    closestIndex = i;
                }
            }
        }

        void FlipNormals(ref Mesh mesh)
        {
            int[] tris = mesh.triangles;
            for (int i = 0; i < tris.Length / 3; i++)
            {
                int a = tris[i * 3 + 0];
                int b = tris[i * 3 + 1];
                int c = tris[i * 3 + 2];
                tris[i * 3 + 0] = c;
                tris[i * 3 + 1] = b;
                tris[i * 3 + 2] = a;
            }
            mesh.triangles = tris;
        }
    }
}
/*float distanceToTop = Vector3.Distance(vertex, topBone);
float distanceToBottom = Vector3.Distance(vertex, bottomBone);
// check if point closer to top or bottom
if (distanceToTop < distanceToBottom)
{
    // Bottom bone is closer
    weights[j].boneIndex0 = 0;
    weights[j].boneIndex1 = 1;
    weights[j].weight0 = 1;
    weights[j].weight1 = 0;
}
else
{
    // top bone is closer
    weights[j].boneIndex0 = 0;
    weights[j].boneIndex1 = 1;
    weights[j].weight0 = 0;
    weights[j].weight1 = 1;
    //weights[j].weight0 = distanceToTop;//1;
    //weights[j].weight1 = distanceToBottom;// 0;
}
if (distanceToTop - 0.1f >= distanceToBottom && distanceToTop + 0.1f <= distanceToBottom)
{
    // if distances are close!
    weights[j].weight0 = distanceToTop;
    weights[j].weight1 = distanceToBottom;
}*/
