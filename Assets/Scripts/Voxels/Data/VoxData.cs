using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel.Voxels
{
    [Serializable]
    public struct VoxData
    {
        // maybe keep a list of position as well as the byte data
        // so position data will be in here too
        public int id;
        public float3 scale;
        // i should change this to use int3
        public int3 size;
        public BlitableArray<byte> data;
        public BlitableArray<byte> colorsR;
        public BlitableArray<byte> colorsG;
        public BlitableArray<byte> colorsB;

        public void Merge(VoxData newPart, int3 offset)
        {
            var oldData = data.ToArray();
            var oldSize = size;
            // i should also change bytes depending on colours, using a colour lookup or something
            if (colorsR.Length == 0)
            {
                InitializeColors(newPart.colorsR.Length);
                for (int i = 0; i < newPart.colorsR.Length; i++)
                {
                    colorsR[i] = newPart.colorsR[i];
                    colorsG[i] = newPart.colorsG[i];
                    colorsB[i] = newPart.colorsB[i];
                }
            }
            // get new size first
            size = new int3(
                math.max(size.x, newPart.size.x + offset.x),
                math.max(size.y, newPart.size.y + offset.y),
                math.max(size.z, newPart.size.z + offset.z));
            scale = new float3(
                math.max(scale.x, newPart.scale.x),
                math.max(scale.y, newPart.scale.y),
                math.max(scale.z, newPart.scale.z));
            //UnityEngine.Debug.LogError("New size is set to: " + size + " from " + oldSize);
            // create new data
            InitializeData();
            int3 localPosition;
            for (localPosition.x = 0; localPosition.x < size.x; localPosition.x++)
            {
                for (localPosition.y = 0; localPosition.y < size.y; localPosition.y++)
                {
                    for (localPosition.z = 0; localPosition.z < size.z; localPosition.z++)
                    {
                        int newIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, size);
                        data[newIndex] = 0;
                    }
                }
            }

            // first add in old parts
            for (localPosition.x = 0; localPosition.x < oldSize.x; localPosition.x++)
            {
                for (localPosition.y = 0; localPosition.y < oldSize.y; localPosition.y++)
                {
                    for (localPosition.z = 0; localPosition.z < oldSize.z; localPosition.z++)
                    {
                        int oldIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, oldSize);
                        var newPosition = localPosition; // + position offset?
                        int newIndex = VoxelRaycastSystem.GetVoxelArrayIndex(newPosition, size);
                        if (oldData[oldIndex] != 0) 
                        {
                            data[newIndex] = oldData[oldIndex];
                        }
                    }
                }
            }

            // now add in new part
            for (localPosition.x = 0; localPosition.x < newPart.size.x; localPosition.x++)
            {
                for (localPosition.y = 0; localPosition.y < newPart.size.y; localPosition.y++)
                {
                    for (localPosition.z = 0; localPosition.z < newPart.size.z; localPosition.z++)
                    {
                        int partIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, newPart.size);
                        var newPosition = localPosition +  offset; // + position offset?
                        int newIndex = VoxelRaycastSystem.GetVoxelArrayIndex(newPosition, size);
                        if (newPart.data[partIndex] != 0) 
                        {
                            data[newIndex] = newPart.data[partIndex];
                        }
                    }
                }
            }
        }

        public float3 GetSize()
        {
            float3 newSize = scale * 0.5f * (size.ToFloat3() / 16f);    // scaled size
            newSize.x = math.max(newSize.x, 0.5f);
            newSize.y = math.max(newSize.y, 0.5f);
            newSize.z = math.max(newSize.z, 0.5f);
            return newSize;
        }

        public void InitializeData()
        {
            int count = (int) math.floor(size.x * size.y * size.z);
            if (data.Length > 0)
            {
                data.Dispose();
            }
            data = new BlitableArray<byte>(count, Allocator.Persistent);
        }

        public void InitializeColors(int count)
        {
            if (colorsR.Length > 0)
            {
                colorsR.Dispose();
                colorsG.Dispose();
                colorsB.Dispose();
            }
            colorsR = new BlitableArray<byte>(count, Allocator.Persistent);
            colorsG = new BlitableArray<byte>(count, Allocator.Persistent);
            colorsB = new BlitableArray<byte>(count, Allocator.Persistent);
        }

        public List<Color> GetColors()
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < colorsR.Length; i++)
            {
                colors.Add(new Color(
                    ((int)colorsR[i])/ 255f,
                    ((int)colorsG[i]) / 255f,
                    ((int)colorsB[i]) / 255f));
            }
            return colors;
        }

        public SerializeableVoxData GetSerializeableClone()
        {
            SerializeableVoxData clone = new SerializeableVoxData();
            clone.id = id;
            clone.scale = scale;
            clone.size = size;
            clone.data = new byte[data.Length];
            var dataArray = data.ToArray();
            for (int i = 0; i < dataArray.Length; i++)
            {
                clone.data[i] = dataArray[i];
            }
            clone.colorsR = new byte[colorsR.Length];
            clone.colorsG = new byte[colorsG.Length];
            clone.colorsB = new byte[colorsB.Length];
            var colorsRArray = colorsR.ToArray();
            var colorsGArray = colorsG.ToArray();
            var colorsBArray = colorsB.ToArray();
            for (int i = 0; i < colorsRArray.Length; i++)
            {
                clone.colorsR[i] = colorsRArray[i];
                clone.colorsG[i] = colorsGArray[i];
                clone.colorsB[i] = colorsBArray[i];
            }
            return clone;
        }

        [Serializable]
        public class SerializeableVoxData
        {
            public int id;
            public float3 scale;
            public int3 size; // size will be 1,1,1 for single chunks
            public byte[] data;
            public byte[] colorsR;
            public byte[] colorsG;
            public byte[] colorsB;

            public SerializeableVoxData()
            {
                data = new byte[0];
                colorsR = new byte[0];
                colorsG = new byte[0];
                colorsB = new byte[0];
            }
            
            public VoxData GetRealOne()
            {
                VoxData voxData = new VoxData();
                voxData.id = id;
                voxData.scale = scale;
                voxData.size = size;
                voxData.InitializeData(); // data.Length
                for (int i = 0; i < data.Length; i++)
                {
                    voxData.data[i] = data[i];
                }
                voxData.InitializeColors(colorsR.Length);
                for (int i = 0; i < colorsR.Length; i++)
                {
                    voxData.colorsR[i] = colorsR[i];
                    voxData.colorsG[i] = colorsG[i];
                    voxData.colorsB[i] = colorsB[i];
                }
                return voxData;
            }
        }
    }
}