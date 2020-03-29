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
                math.max(size.x, newPart.size.x + math.abs(offset.x)),
                math.max(size.y, newPart.size.y + math.abs(offset.y)),
                math.max(size.z, newPart.size.z + math.abs(offset.z)));
            scale = new float3(
                math.max(scale.x, newPart.scale.x),
                math.max(scale.y, newPart.scale.y),
                math.max(scale.z, newPart.scale.z));
            UnityEngine.Debug.LogError("New size is set to: " + size + " from " + oldSize);
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
                        if (offset.x < 0) {
                            newPosition.x += -offset.x;
                        }
                        if (offset.y < 0) {
                            newPosition.y += -offset.y;
                        }
                        if (offset.z < 0) {
                            newPosition.z += -offset.z;
                        }
                        int newIndex = VoxelRaycastSystem.GetVoxelArrayIndex(newPosition, size);
                        if (oldData[oldIndex] != 0) 
                        {
                            data[newIndex] = oldData[oldIndex];
                        }
                    }
                }
            }
            if (offset.x < 0) {
                offset.x = 0;
            }
            if (offset.y < 0) {
                offset.y = 0;
            }
            if (offset.z < 0) {
                offset.z = 0;
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

        public static int3 GetSize(List<VoxData> voxes, List<int3> positions)
        {
            int3 min = new int3(666,666,666);
            int3 max = new int3(-666,-666,-666);
            for (int i = 0; i < voxes.Count; i++) {
                int3 lhs = positions[i];
                int3 rhs = lhs + voxes[i].size;
                if (lhs.x < min.x) min.x = lhs.x;
                if (lhs.y < min.y) min.y = lhs.y;
                if (lhs.z < min.z) min.z = lhs.z;
                if (rhs.x > max.x) max.x = rhs.x;
                if (rhs.y > max.y) max.y = rhs.y;
                if (rhs.z > max.z) max.z = rhs.z;
            }
            if (min.x < 0) {
                var addX = math.abs(min.x);
                min.x += addX;
                max.x += addX;
            }
            if (min.y < 0) {
                var addX = math.abs(min.y);
                min.y += addX;
                max.y += addX;
            }
            if (min.z < 0) {
                var addX = math.abs(min.z);
                min.z += addX;
                max.z += addX;
            }
            return max;
        }
        public static List<int3> FixPositions(List<VoxData> voxes, List<int3> positions)
        {
            int3 min = new int3(666,666,666);
            int3 max = new int3(-666,-666,-666);
            for (int i = 0; i < voxes.Count; i++) {
                int3 lhs = positions[i];
                int3 rhs = lhs + voxes[i].size;
                if (lhs.x < min.x) min.x = lhs.x;
                if (lhs.y < min.y) min.y = lhs.y;
                if (lhs.z < min.z) min.z = lhs.z;
                if (rhs.x > max.x) max.x = rhs.x;
                if (rhs.y > max.y) max.y = rhs.y;
                if (rhs.z > max.z) max.z = rhs.z;
            }
            int3 addition = new int3(0,0,0);
            if (min.x < 0) {
                addition.x = math.abs(min.x);
            }
            if (min.y < 0) {
                addition.y = math.abs(min.y);
            }
            if (min.z < 0) {
                addition.z = math.abs(min.z);
            }
            for (int i = 0; i < positions.Count; i++) {
                positions[i] = positions[i] + addition;
            }
            return positions;
        }

        // need to use vox layers instead, each layer has a list of voxes and positions
        public void Build(List<VoxData> voxes, List<int3> positions, int3 newSize)
        {
            if (voxes.Count == 0) {
                Debug.LogError("0 Voxes..Cannot build vox. Remember to have core on item.");
                return;
            }
            //Debug.LogError("Building Vox with: " + voxes.Count + " voxes. Size is: " + newSize);
            var oldData = data.ToArray();
            var oldSize = size;
            // i should also change bytes depending on colours, using a colour lookup or something
            if (colorsR.Length == 0)
            {
                var newPart = voxes[0];
                InitializeColors(newPart.colorsR.Length);
                for (int i = 0; i < newPart.colorsR.Length; i++)
                {
                    colorsR[i] = newPart.colorsR[i];
                    colorsG[i] = newPart.colorsG[i];
                    colorsB[i] = newPart.colorsB[i];
                }
            }
            // get new size first
            size = newSize;
            scale = new float3(0.5f, 0.5f, 0.5f);
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
            for (int i = 0; i < voxes.Count; i++)
            {
                var vox = voxes[i];
                var position = positions[i];
                //Debug.LogError("Building Vox: " + vox.size + " at position " + position);
                for (localPosition.x = 0; localPosition.x < vox.size.x; localPosition.x++)
                {
                    for (localPosition.y = 0; localPosition.y < vox.size.y; localPosition.y++)
                    {
                        for (localPosition.z = 0; localPosition.z < vox.size.z; localPosition.z++)
                        {
                            int partIndex = VoxelRaycastSystem.GetVoxelArrayIndex(localPosition, vox.size);
                            var newPosition = localPosition + position;
                            int newIndex = VoxelRaycastSystem.GetVoxelArrayIndex(newPosition, size);
                            if (vox.data[partIndex] != 0) 
                            {
                                data[newIndex] = vox.data[partIndex];
                            }
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
                voxData.InitializeColors(colorsR.Length);
                if (voxData.colorsR.Length != colorsR.Length)
                {
                    return voxData;
                }
                for (int i = 0; i < colorsR.Length; i++)
                {
                    voxData.colorsR[i] = colorsR[i];
                    voxData.colorsG[i] = colorsG[i];
                    voxData.colorsB[i] = colorsB[i];
                }
                voxData.InitializeData(); // data.Length
                if (voxData.data.Length != data.Length)
                {
                    return voxData;
                }
                for (int i = 0; i < data.Length; i++)
                {
                    voxData.data[i] = data[i];
                }
                return voxData;
            }
        }
    }
}