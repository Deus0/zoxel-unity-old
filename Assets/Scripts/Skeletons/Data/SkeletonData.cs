using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Zoxel
{

    [Serializable]
    public struct SkeletonData
    {
        public int id;
        public List<BoneData> datas;

        public BlitableArray<BoneData> GetBlittableArray()
        {
            BlitableArray<BoneData> data = new BlitableArray<BoneData>(datas.Count, Allocator.Persistent);
            int i = 0;
            foreach (BoneData dat in datas)
            {
                data[i] = dat;
                i++;
            }
            return data;
        }
    }

}