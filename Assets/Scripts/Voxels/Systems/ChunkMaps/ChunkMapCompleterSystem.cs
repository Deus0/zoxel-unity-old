using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;

namespace Zoxel
{

	[DisableAutoCreation]
	public class ChunkMapCompleterSystem : ComponentSystem
	{
		// should be per world..!
		// shared component attached to worlds?
		public Dictionary<float2, UnityEngine.Texture2D> maps = new Dictionary<float2, UnityEngine.Texture2D>();

		protected override void OnUpdate()
		{

			Entities.WithAll<ChunkMap>().ForEach((Entity e, ref ChunkMap chunkMap) =>
			{
				if (chunkMap.dirty == 2)
				{
					// create texture
					UnityEngine.Texture2D mapTexture = new UnityEngine.Texture2D(
						chunkMap.width, chunkMap.height,
						UnityEngine.Experimental.Rendering.DefaultFormat.LDR,
						//UnityEngine.Experimental.Rendering.DefaultFormat.HDR, 
						UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
					if (mapTexture == null)
					{
						World.EntityManager.RemoveComponent<ChunkMap>(e);
						//UnityEngine.Debug.LogError("Map Texture [" + chunkMap.chunkPosition +  "] created for chunk was null.");
						return;
					}
					mapTexture.filterMode = UnityEngine.FilterMode.Point;
					UnityEngine.Color[] pixels = new UnityEngine.Color[chunkMap.width * chunkMap.height];
					mapTexture.name = "ChunkMap_" + chunkMap.chunkPosition.x + "_" + chunkMap.chunkPosition.y + "_" + chunkMap.chunkPosition.z;
					int xzIndex = 0;
					for (int i = 0; i < chunkMap.width; i++)
					{
						for (int j = 0; j < chunkMap.height; j++)
						{
							//int xzIndex = i + j * chunkMap.height;
							byte voxel = chunkMap.topVoxels[xzIndex];
							if (voxel == 0)
							{
								pixels[xzIndex] = UnityEngine.Color.black;
							}
							else if (voxel == 1)
							{
								pixels[xzIndex] = UnityEngine.Color.green;
							}
							else if (voxel == 2)
							{
								pixels[xzIndex] = UnityEngine.Color.red;
							}
							else if (voxel == 3)
							{
								pixels[xzIndex] = UnityEngine.Color.blue;
							}
							else if (voxel == 4)
							{
								pixels[xzIndex] = UnityEngine.Color.yellow;
							}
							else
							{
								pixels[xzIndex] = UnityEngine.Color.magenta;
							}
							int height = (int)chunkMap.heights[xzIndex];
							//float colorBrightness = 3 * ((float)height / (float)chunkMap.highestHeight);
							pixels[xzIndex] *= ((float)height / 64); //math.min(1, colorBrightness);
							xzIndex++;
						}
					}
					mapTexture.SetPixels(pixels);
					mapTexture.Apply();
					//Bootstrap.instance.debugMaps.Add(mapTexture);
					float2 mapPosition = new float2(chunkMap.chunkPosition.x, chunkMap.chunkPosition.z);
					if (maps.ContainsKey(mapPosition))
					{
						if (maps[mapPosition] != null)
						{
							UnityEngine.GameObject.Destroy(maps[mapPosition]);
						}
						maps.Remove(mapPosition);
					}
					maps.Add(mapPosition, mapTexture);
					World.EntityManager.RemoveComponent<ChunkMap>(e);
				}
			});
		}
	}
}