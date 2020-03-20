using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    // contains voxelDatas, material?, VoxelTilemapData, spawn points, etc
    public struct MapData
    {

    }
    /// <summary>
    /// Each map has a model data
    /// Voxel Datas
    /// and a tilemmap data (baked)
    /// </summary>
    [CreateAssetMenu(fileName = "Map", menuName = "Zoxel/Map")]//, order = -3)]
    public class MapDatam : ScriptableObject
    {
        public int id;

        [Header("World Spawning")]
        public float3 newPlayerPosition;
        public float3 worldPosition;
        public float3 worldRotation;
        public float3 worldScale = new float3(1, 1, 1);
        public int3 voxelDimensions = new int3(16, 64, 16);

        [Header("World Generation")]
        public List<BiomeDatam> biomes;
        //public List<CharacterDatam> monsters;
        // should be towns - add towns to chunks that are within towns
        public List<Town> towns;
        public List<Building> buildings;

        [Header("Art")]
        public VoxelTilemapDatam tilemap;


        [ContextMenu("GenerateID")]
        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }
}