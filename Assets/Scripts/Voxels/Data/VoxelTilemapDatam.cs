using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    [CreateAssetMenu(fileName = "VoxelTilemap", menuName = "ZoxelArt/VoxelTilemap")]//, order = -2)]
    public class VoxelTilemapDatam : ScriptableObject
    {
        public List<VoxelDatam> voxels;

        // should generate a square map based on amount of textures
        // sometimes i may want to manually set them though
        [HideInInspector]
        public List<Texture2D> tilemaps;
        public List<Material> materials;
        private float2 largestSize;
        private List<Texture2D> textures;
        private List<float2> texturePositions;
        private int horizontalCount = 4;
        private int verticalCount = 1;
        // needs to generate materials as well from voxels
        // create multiple materials, ie for water voxel

        // however i want to generate normal maps and stuff for better lighting

        [ContextMenu("Generate")]
        public void Generate()
        {
            Debug.Log("Generating VoxelTilemap with [" + voxels.Count + "] Voxels.");
            for (int i = 0; i < tilemaps.Count; i++)
            {
#if UNITY_EDITOR
                // destroy
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(tilemaps[i]);
                // destroy?
#endif
            }
            tilemaps.Clear();
            textures = new List<Texture2D>();
            largestSize = new float2();
            texturePositions = new List<float2>();
            for (int i = 0; i < voxels.Count; i++)
            {
                //AddTexture(voxels[i].texture.texture as Texture2D);
                for (int j = 0; j < voxels[i].textures.Count; j++)
                {
                    AddTexture(voxels[i].textures[j].texture as Texture2D);
                }
            }
            horizontalCount = textures.Count;   // get highest power of two (ceil)
            float textureWidth = 1 / (float)(horizontalCount);
            float textureHeight = 1 / (float)(verticalCount);
            float2 textureSize = new float2(textureWidth, textureHeight);
            // for all textures, get size
            for (int i = 0; i < textures.Count; i++)
            {
                int positionX = i % horizontalCount;
                int positionY = i / horizontalCount;
                float2 position = new float2(((float)positionX) * textureWidth, ((float)positionY) * textureHeight);
                texturePositions.Add(position);
            }
            //int sideCount = 0;
            for (int i = 0; i < voxels.Count; i++)
            {
                //  intiate new UV Map
                voxels[i].InitializeCubeMap();
                for (int j = 0; j < voxels[i].textures.Count; j++)
                {
                    int textureIndex = textures.IndexOf((voxels[i].textures[j].texture as Texture2D));
                    float2 position = texturePositions[textureIndex];
                    if (voxels[i].textureSides[j] == VoxelSide.Default)
                    {
                        for (int k = 1; k <= 6; k++)
                        {
                            voxels[i].uvMap.SetSide(textureSize, position, ((VoxelSide)k));
                        }
                    }
                    else
                    {
                        voxels[i].uvMap.SetSide(textureSize, position, voxels[i].textureSides[j]);
                    }
                }
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(voxels[i]);
#endif
            }
            // for all textures set horizontalCount, verticalCount
            TilemapGenerator generator = new TilemapGenerator(horizontalCount, verticalCount, (int)largestSize.x, (int)largestSize.y);
            // this should be done per material!
            tilemaps.Add(generator.CreateVoxelTilemap(textures));
            for (int i = 0; i < tilemaps.Count; i++)
            {
                tilemaps[i].name = name + "_baked";
                materials[i].SetTexture("_BaseMap", tilemaps[i]);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.AddObjectToAsset(tilemaps[i], this);
                UnityEditor.AssetDatabase.SaveAssets();
#endif
            }
            textures.Clear();
            texturePositions.Clear();
        }

        private void AddTexture(Texture2D newTexture)
        {
            if (textures.Contains(newTexture) == false)
            {
                textures.Add(newTexture);
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

        /// <summary>
        /// One of these is generated for each VoxelTilemap! Each chunk can have its own tile map as well!
        /// </summary>
        public class TilemapGenerator
        {
            public Vector2 Size = new Vector2(32 * 8f, 32 * 8f);
            public List<string> TextureNames = new List<string>();
            public List<Vector2> TexturePositions = new List<Vector2>();

            public Texture2D VoxelTilemapTexture;
            public List<Texture2D> Tiles = new List<Texture2D>();

            // The default variables for our tile map
            public int TilesLengthX = 8;
            public int TilesLengthY = 8;
            public int TileSizeX = 16;
            public int TileSizeY = 16;
            public bool IsMipMaps;
            public bool HasAlpha;

            public TilemapGenerator() { }

            public TilemapGenerator(int TileLengthX_, int TileLengthY_, int TileSizeX_, int TileSizeY_)
            {
                Set(TileLengthX_, TileLengthY_, TileSizeX_, TileSizeY_);
            }

            public void Set(int TileLengthX_, int TileLengthY_, int TileSizeX_, int TileSizeY_)
            {
                TilesLengthX = TileLengthX_;
                TilesLengthY = TileLengthY_;
                TileSizeX = TileSizeX_;
                TileSizeY = TileSizeY_;
            }

            public Vector2 GetTilePosition(int TileIndex)
            {
                //float TotalTiles = TilesLengthX * TilesLengthY;
                float PosX = 0;
                float PosY = 0;
                if (TilesLengthX != 1)
                {
                    PosX = (TileIndex % (TilesLengthX));  // - 1
                }
                if (TilesLengthY != 1)
                {
                    PosY = (TileIndex / (TilesLengthY)); //- 1
                }
                // get float point Translation from Tile index position
                PosX /= ((float)TilesLengthX);
                PosY /= ((float)TilesLengthY);
                return new Vector2(PosX, PosY);
            }

            public Texture2D CreateVoxelTilemap(List<Texture2D> TiledTextures, int OriginalTileCountX)
            {
                if (TiledTextures.Count == 0)
                {
                    Debug.LogError("No Textures for VoxelTilemap.");
                    return null;
                }
                Set(OriginalTileCountX, OriginalTileCountX, TiledTextures[0].width, TiledTextures[0].width);
                return CreateVoxelTilemap(TiledTextures);
            }

            /// <summary>
            /// Grabs all the textures and chucks them into a tile map
            /// ToDO:
            /// 
            /// </summary>
            public Texture2D CreateVoxelTilemap(List<Texture2D> TiledTextures)
            {
                int BufferLength = 0;
                if (TiledTextures.Count == 0
                    || TiledTextures[0] == null)
                {
                    //Debug.LogError("TiledTextures count is 0. Cannot create a tile map.");
                    return null;
                }
                //int PixelResolution = TiledTextures[0].width;
                int TileCountX = TiledTextures.Count;
                //int MaxX = MyVoxelTilemapInfo.TileSizeX;
                //int MaxY = OriginalTileCountX;
                /*if (MyVoxelTilemapInfo.TilesLengthX != -1)
                {
                }*/
                TileCountX = TilesLengthX;

                Texture2D NewVoxelTilemap = new Texture2D(
                    TileSizeX * TilesLengthX,    // 8 x 16 = 128
                    TileSizeY * TilesLengthY,
                    TextureFormat.RGBA32,
                    false);

                NewVoxelTilemap.filterMode = FilterMode.Point;
                NewVoxelTilemap.wrapMode = TextureWrapMode.Clamp;

                int MaxTextures = (TilesLengthX - BufferLength) * (TilesLengthY - BufferLength);
                Color32[] TileColors = NewVoxelTilemap.GetPixels32(0);
                // Start with blank Colors!
                /*for (int i = 0; i < TileColors.Length; i++)
                {
                    TileColors[i] = new Color32(255, 255, 255, 255);
                }*/
                int TileIndex = -1;  // Our real index !
                for (int i = 0; i < TiledTextures.Count; i++)
                {
                    TileIndex++;
                    if (TiledTextures[i] != null && i < MaxTextures)
                    {
                        /*int DatDivisionDoe = ((TileIndex + 1) % ((int)(TileCountX)));
                        if (DatDivisionDoe == 0 && i != 0)
                        {
                            TileIndex += BufferLength;
                        }*/
                        int TilePositionX = (TileIndex / TileCountX);
                        int TilePositionY = (TileIndex % TileCountX);
                        Color32[] BlockColors = TiledTextures[i].GetPixels32(0);
                        TileColors = PlaceTile(
                            TileColors, BlockColors,
                            TileSizeX, BufferLength,
                            TilePositionX, TilePositionY,
                            TileCountX);
                    }
                }
                NewVoxelTilemap.SetPixels32(TileColors, 0);
                if (HasAlpha)
                {
                    //NewVoxelTilemap.alphaIsTransparency = true;
                }
                NewVoxelTilemap.Apply(false);    // don't automate mipmaps
                return NewVoxelTilemap;
            }

            /// <summary>
            ///  uses buffering and finds what the tile index is
            /// </summary>
            public static int GetTileIndex(int PixelIndex, Texture2D MyTexture)
            {
                int BufferLength = 0;
                //int BufferLength = 1;
                int OriginalTileCountX = 8;
                int MaxTextures = (OriginalTileCountX - BufferLength) * (OriginalTileCountX - BufferLength);
                int TileWidth = 16 + 2 * BufferLength;  // 18
                int TileCountX = MyTexture.width / TileWidth;
                // Translation in texture
                int PosX = (PixelIndex / TileWidth);
                int PosY = (PixelIndex % TileWidth);
                return (PosX * TileCountX + PosY);
            }

            /// <summary>
            /// Places a tile on our tile map
            /// </summary>
            private static Color32[] PlaceTile(
                Color32[] TileColors,
                Color32[] BlockColors,
                int PixelResolution,
                int BufferLength,
                int TilePositionX,
                int TilePositionY,
                int TileCountX)
            {
                for (int i = 0; i < PixelResolution; i++)
                {
                    for (int j = 0; j < PixelResolution; j++)
                    {
                        int TileIndex = Mathf.FloorToInt(i * PixelResolution + j);
                        // Get x and y index of our entire pixel position
                        int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + PixelResolution);
                        int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + PixelResolution);
                        int VoxelTilemapIndex = Mathf.FloorToInt(i2 * TileCountX * PixelResolution + j2);
                        if (TileIndex < BlockColors.Length && VoxelTilemapIndex < TileColors.Length)
                        {
                            TileColors[VoxelTilemapIndex] = BlockColors[TileIndex];
                        }
                    }
                }
                return TileColors;
            }

            /// <summary>
            /// Updates the surrounding pixels of a tile with a buffer layer
            /// </summary>
            private static Color32[] UpdateTileEdge(
                Color32[] TileColors,
                Color32[] BlockColors,
                int PixelResolution,
                int BufferLength,
                int TilePositionX,
                int TilePositionY,
                int TileCountX)
            {

                // buffers texture edges 
                // also update the edge
                for (int i = -1; i <= PixelResolution; i++)
                    for (int j = -1; j <= PixelResolution; j++)
                    {
                        if (i == -1 || j == -1 || i == PixelResolution || j == PixelResolution)
                        {
                            int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));   // the size fot the VoxelTilemap
                            int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));   // the size fot the VoxelTilemap

                            //int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + j);
                            int PixelIndex2 = Mathf.RoundToInt(i2 * TileCountX * PixelResolution + j2);

                            if (PixelIndex2 >= 0 && PixelIndex2 < TileColors.Length)
                            {
                                if (i == -1 && (j > -1 && j < PixelResolution)) // bottom line
                                {
                                    int PixelIndex1 = Mathf.RoundToInt(0 * PixelResolution + j);
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                // top line
                                else if (i == PixelResolution && (j > -1 && j < PixelResolution))
                                {
                                    int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + j);
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                else if (j == PixelResolution && (i > -1 && i < PixelResolution))
                                {
                                    int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + (PixelResolution - 1));
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                else if (j == -1 && (i > -1 && i < PixelResolution))
                                {
                                    int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + (0));
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                // corners
                                else if (i == -1 && j == -1)
                                {
                                    int PixelIndex1 = Mathf.RoundToInt(0 * PixelResolution + 0);
                                    if (PixelIndex1 >= 0 && PixelIndex1 < BlockColors.Length)
                                    {
                                        TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                    }
                                }
                                else if (i == PixelResolution && j == -1)
                                {
                                    int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + 0);
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                else if (i == -1 && j == PixelResolution)
                                {
                                    int PixelIndex1 = Mathf.RoundToInt((0) * PixelResolution + (PixelResolution - 1));
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                else if (i == PixelResolution && j == PixelResolution)
                                {
                                    int PixelIndex1 = Mathf.RoundToInt((PixelResolution - 1) * PixelResolution + (PixelResolution - 1));
                                    TileColors[PixelIndex2] = BlockColors[PixelIndex1];
                                }
                                else
                                {
                                    TileColors[PixelIndex2] = new Color32(0, 0, 0, 255);
                                }
                            }
                        }
                    }
                return TileColors;
            }

            /// <summary>
            /// Grabs all the textures and chucks them into a tile map
            /// </summary>
            public static Texture2D CreateVoxelTilemapMeta(List<Texture2D> TiledTextures, int OriginalTileCountX) // Texture2D NewVoxelTilemap, 
            {
                int BufferLength = 0;

                int PixelResolution = TiledTextures[0].width;
                int TileCountX = TiledTextures.Count;
                int MaxX = OriginalTileCountX;
                int MaxY = OriginalTileCountX;
                if (MaxX != -1)
                {
                    TileCountX = MaxX;
                }
                int TileLengthY = MaxY;

                Texture2D NewVoxelTilemap = new Texture2D(
                    PixelResolution * TileCountX,    // 8 x 16 = 128
                    PixelResolution * TileLengthY,
                    TextureFormat.RGBA32,
                    false);
                NewVoxelTilemap.filterMode = FilterMode.Point;
                NewVoxelTilemap.wrapMode = TextureWrapMode.Clamp;

                int MaxTextures = (OriginalTileCountX - BufferLength) * (OriginalTileCountX - BufferLength);
                Color32[] TileColors = NewVoxelTilemap.GetPixels32(0);

                int TileIndex = -1;  // Our real index !
                for (int i = 0; i < TiledTextures.Count; i++)
                {
                    TileIndex++;
                    if (TiledTextures[i] && i < MaxTextures)
                    {
                        int DatDivisionDoe = ((TileIndex + 1) % ((int)(TileCountX)));
                        if (DatDivisionDoe == 0 && i != 0)
                        {
                            TileIndex += BufferLength;
                        }
                        int TilePositionX = (TileIndex / TileCountX);
                        int TilePositionY = (TileIndex % TileCountX);
                        Texture2D BlockTexture = TiledTextures[i];
                        Color32[] BlockColors = BlockTexture.GetPixels32(0);
                        TileColors = PlaceTileMeta(
                            TileColors, BlockColors,
                            PixelResolution, BufferLength,
                            TilePositionX, TilePositionY,
                            TileCountX,
                            TileIndex);
                    }
                }
                NewVoxelTilemap.SetPixels32(TileColors, 0);
                NewVoxelTilemap.Apply(false);    // don't automate mipmaps
                return NewVoxelTilemap;
            }

            private static Color32[] PlaceTileMeta(
               Color32[] TileColors,
               Color32[] BlockColors,
               int PixelResolution,
               int BufferLength,
               int TilePositionX,
               int TilePositionY,
               int TileCountX,
               int TileIndex)
            {
                for (int i = 0; i < PixelResolution; i++)
                {
                    for (int j = 0; j < PixelResolution; j++)
                    {
                        // Get x and y index of our entire pixel position
                        int i2 = i + BufferLength + TilePositionX * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));
                        int j2 = j + BufferLength + TilePositionY * (BufferLength * 2 + Mathf.RoundToInt(PixelResolution));

                        int PixelIndex1 = Mathf.RoundToInt(i * PixelResolution + j);
                        int PixelIndex2 = Mathf.RoundToInt(i2 * TileCountX * PixelResolution + j2);
                        if (PixelIndex1 < BlockColors.Length && PixelIndex2 < TileColors.Length)
                        {
                            TileColors[PixelIndex2] = new Color32((byte)TileIndex, (byte)TileIndex, (byte)TileIndex, (byte)TileIndex);
                        }
                    }
                }
                return TileColors;
            }
        }
}