using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [Serializable]
    public struct TextureData
    {
        public int width;
        public int height;
        public byte[] data;
    }

    [CreateAssetMenu(fileName = "Texture", menuName = "ZoxelArt/Texture")]//, order = 3)]
    public class TextureDatam : ScriptableObject
    {
        public TextureData Value;
        // baked texture
        public Texture2D texture;
        // generation

        [ContextMenu("Generate Noise")]
        public void GenerateNoise()
        {
            Debug.Log("Generating noise texture for " + name);
            // just create simple noise texture
            // For each pixel in the texture...
            float y = 0.0F;
            texture = new Texture2D(32, 32);
            texture.filterMode = FilterMode.Point;
            Color[] pixels = new Color[32 * 32];
            while (y < texture.height)
            {
                float x = 0.0F;
                while (x < texture.width)
                {
                    float xCoord = 0 + x / texture.width * 1;
                    float yCoord = 0 + y / texture.height * 1;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    pixels[(int)y * texture.width + (int)x] = new Color(sample, sample, sample);
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = name + "_baked";
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(texture, this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }

}