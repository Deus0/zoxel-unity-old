using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct BiomeData
    {
        public int id;

        [Header("HeightMap")]
        public float landBase;// = 6;
        public float landAmplitude;// = 1;
        public float landScale;// = 0.015f;

        [Header("Blocks")]
        public byte dirtID;
        public byte grassID;
        public byte sandID;
        public byte stoneID;
        public byte waterID;
        public byte floorID;
        public byte wallID;
        public byte roofID;

        [Header("BlocksChance")]
        public byte grassChance;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }

    [CreateAssetMenu(fileName = "Biome", menuName = "Zoxel/Biome")]
    public class BiomeDatam : ScriptableObject
    {
        public BiomeData Value;
        public VoxelDatam dirt;
        public VoxelDatam grass;
        public VoxelDatam sand;
        public VoxelDatam stone;
        public VoxelDatam water;
        [Header("Buildings")]
        public VoxelDatam floor;
        public VoxelDatam wall;
        public VoxelDatam roof;

        public int monsterSpawnAmount;
        public float monsterSpawnCooldown;
        public List<CharacterDatam> monsters;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }

        public void InitializeIDs(VoxelTilemapDatam tilemap)
        {
            for (int i = 0; i < tilemap.voxels.Count;i++)
            {
                if (dirt == tilemap.voxels[i])
                {
                    Value.dirtID = (byte)(i + 1);
                }
                if (grass == tilemap.voxels[i])
                {
                    Value.grassID = (byte)(i + 1);
                }
                if (sand == tilemap.voxels[i])
                {
                    Value.sandID = (byte)(i + 1);
                }
                if (stone == tilemap.voxels[i])
                {
                    Value.stoneID = (byte)(i + 1);
                }
                if (water == tilemap.voxels[i])
                {
                    Value.waterID = (byte)(i + 1);
                }
                //
                if (floor == tilemap.voxels[i])
                {
                    Value.floorID = (byte)(i + 1);
                }
                if (wall == tilemap.voxels[i])
                {
                    Value.wallID = (byte)(i + 1);
                }
                if (roof == tilemap.voxels[i])
                {
                    Value.roofID = (byte)(i + 1);
                }
            }
        }
    }
}