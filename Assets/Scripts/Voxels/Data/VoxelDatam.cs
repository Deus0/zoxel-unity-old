using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public enum VoxelSide
    {
        Default,
        Forward,
        Back,
        Left,
        Right,
        Up,
        Down
    }

    [Serializable]
    public struct VoxelUVMap
    {
        public BlitableArray<float2> uvs;

        [Serializable]
        public struct SerializeableVoxelUVMap
        {
            public float2[] uvs;
        }

        public void Initialize()
        {
            uvs = new BlitableArray<float2>(24, Unity.Collections.Allocator.Persistent);    // 6 sides, 4 corner points of uvs
        }

        public void SetSide(float2 size, float2 position, VoxelSide side)
        {
            int uvPointer = 0;
            if (side == VoxelSide.Up)
            {
                uvPointer = 0;
            }
            else if (side == VoxelSide.Down)
            {
                uvPointer += 4;
            }
            // up
            else if (side == VoxelSide.Left)
            {
                uvPointer += 8;
            }
            // down
            else if (side == VoxelSide.Right)
            {
                uvPointer += 12;
            }
            if (side == VoxelSide.Forward)
            {
                uvPointer += 16;
            }
            if (side == VoxelSide.Back)
            {
                uvPointer += 20;
            }
            if (side == VoxelSide.Left || side == VoxelSide.Right)
            {
                // reverse
                uvs[uvPointer + 3] = new float2(position.x, position.y);
                uvs[uvPointer ] = new float2(position.x + size.x, position.y);
                uvs[uvPointer + 1] = new float2(position.x + size.x, position.y + size.y);
                uvs[uvPointer + 2] = new float2(position.x, position.y + size.y);
                /*
                uvs[uvPointer + 3] = float2.zero;
                uvs[uvPointer] = float2.zero;
                uvs[uvPointer + 1] = float2.zero;
                uvs[uvPointer + 2] = float2.zero;*/
            }
            else
            {
                uvs[uvPointer] = new float2(position.x, position.y);
                uvs[uvPointer + 1] = new float2(position.x + size.x, position.y);
                uvs[uvPointer + 2] = new float2(position.x + size.x, position.y + size.y);
                uvs[uvPointer + 3] = new float2(position.x, position.y + size.y);
            }
        }
    }

    [CreateAssetMenu(fileName = "Voxel", menuName = "Zoxel/Voxel")]//, order = -2)]
    public class VoxelDatam : ScriptableObject, ISerializationCallbackReceiver
    {
        public VoxelData Value;
        // link to material


        // link to texture
        //public TextureDatam texture;

        // if multiple textures
        public List<TextureDatam> textures;
        public List<VoxelSide> textureSides;

       // public MaterialDatam material;  // for things like water - give seperate material
        public MeshDatam mesh; // link to a mesh!
        // also store texture map here
        public VoxelUVMap uvMap;
        [HideInInspector]
        public VoxelUVMap.SerializeableVoxelUVMap uvMapClone;
        // link to item to drop when destroyed

        // link to stats! for things like walls

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }

        /*[ContextMenu("Generate UVs")]
        public void GenerateUVs()
        {
            // using mesh datam - 
            // if one texture
        }*/

        public void InitializeCubeMap()
        {
            uvMap = new VoxelUVMap();
            uvMap.Initialize(); // for cube!
        }

        public void OnBeforeSerialize()
        {
            uvMapClone = new VoxelUVMap.SerializeableVoxelUVMap { };
            uvMapClone.uvs = uvMap.uvs.ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (uvMapClone.uvs != null)
            {
                uvMap.uvs = new BlitableArray<float2>(uvMapClone.uvs.Length, Unity.Collections.Allocator.Persistent);
                for (int i = 0; i < uvMapClone.uvs.Length; i++)
                {
                    uvMap.uvs[i] = uvMapClone.uvs[i];
                }
            }
        }
    }

}