using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoxel
{
    public class SkeletonTester : MonoBehaviour
    {
        public SkeletonDatam skeleton;
        private Dictionary<string, GameObject> bones = new Dictionary<string, GameObject>();

        [ContextMenu("Flippity FLoop")]
        public void Flippity()
        {
            int index = 0;
            SkinnedMeshRenderer skinnyMesh = GetComponent<SkinnedMeshRenderer>();
            foreach (GameObject bone in bones.Values)
            {
                bone.transform.position = skeleton.data.datas[index].position;
                bone.transform.position +=skinnyMesh.bounds.extents; // new float3(0.5f, 0.5f, 0.5f);
                index++;
            }
        }
        [ContextMenu("Spawn")]
        public void Spawn()
        {
            bones = skeleton.InstantiateBones(gameObject);
            SkinnedMeshRenderer skinnyMesh = GetComponent<SkinnedMeshRenderer>();
            skinnyMesh.rootBone = skinnyMesh.transform;
            List<Transform> transforms = new List<Transform>();
            foreach (GameObject bone in bones.Values)
            {
                transforms.Add(bone.transform);
            }
            skinnyMesh.bones = transforms.ToArray();
        }

        [ContextMenu("Despawn")]
        public void Despawn()
        {
            foreach (GameObject bone in bones.Values) 
            {
                DestroyImmediate(bone);
            }
            bones.Clear();
        }
    }

}