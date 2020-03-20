using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [CreateAssetMenu(fileName = "Tilemap", menuName = "ZoxelArt/Tilemap")]
    public class TilemapDatam : ScriptableObject
    {
        public Texture2D VoxelTilemap;
        public List<TextureDatam> textures;
        public List<Texture2D> texturesRaw;
        public int horizontalCount = 4;
        public int verticalCount = 1;
        float2 largestSize = new float2();
        List<Texture2D> texturesInput;

        [ContextMenu("Generate")]
        public void Generate()
        {
            Debug.Log("Generating VoxelTilemap with [" + textures.Count + "] Voxels.");
            if (VoxelTilemap)
            {
#if UNITY_EDITOR
                // destroy
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(VoxelTilemap);
                // destroy?
#endif
            }
            texturesInput = new List<Texture2D>();
            largestSize = new float2();
            for (int i = 0; i < textures.Count; i++)
            {
                AddTexture(textures[i].texture as Texture2D);
            }
            for (int i = 0; i < texturesRaw.Count; i++)
            {
                AddTexture(texturesRaw[i]);
            }
            horizontalCount = textures.Count;   // get highest power of two (ceil)
            float textureWidth = 1 / (float)(horizontalCount);
            float textureHeight = 1 / (float)(verticalCount);
            // for all textures set horizontalCount, verticalCount
            TilemapGenerator generator = new TilemapGenerator(horizontalCount, verticalCount, (int)largestSize.x, (int)largestSize.y);
            VoxelTilemap = generator.CreateVoxelTilemap(texturesInput);
            VoxelTilemap.name = name + "_baked";
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(VoxelTilemap, this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            texturesInput.Clear();
        }

        private void AddTexture(Texture2D newTexture)
        {
            texturesInput.Add(newTexture);
            float2 newSize = new float2(newTexture.width, newTexture.height);
            if (newSize.x > largestSize.x)
            {
                largestSize.x = newSize.x;
            }
            if (newSize.y > largestSize.y)
            {
                largestSize.y = newSize.y;
            }
        }
    }
}