using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

namespace Zoxel
{
    [Serializable]
    public struct NativeString
    {
        public NativeArray<byte> Value;
    }

    // a list of animation and data
    [Serializable]
    public struct AnimationData
    {
        public string name;
        public float time;
        public int frames;
        public int framesPerSecond;
    }

    [Serializable]
    public struct AnimatorData
    {
        public AnimationData[] datas;

        public BlitableArray<AnimationData> GetBlittableArray()
        {
            BlitableArray<AnimationData> data = new BlitableArray<AnimationData>(datas.Length, Allocator.Persistent);
            int i = 0;
            foreach (AnimationData dat in datas)
            {
                data[i] = dat;
                i++;
            }
            return data;
        }
    }

    [CreateAssetMenu(fileName = "Animator", menuName = "ZoxelArt/Animator")]
    public class AnimatorDatam : ScriptableObject
    {
        public AnimatorData data;
        public Mesh mesh;
        public Material material;
        public float scale = 1;

        public AnimatorDatam()
        {
            data = new AnimatorData();
            data.datas = new AnimationData[3];
        }
    }

}