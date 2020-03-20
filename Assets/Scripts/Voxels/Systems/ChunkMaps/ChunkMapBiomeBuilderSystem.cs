using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Zoxel.Voxels;
using UnityEngine;
namespace Zoxel.WorldGeneration
{

	[DisableAutoCreation]
	public class ChunkMapBiomeBuilderSystem : JobComponentSystem
	{
		
		[BurstCompile]
		struct ChunkMapBiomeJob : IJobForEach<ChunkMap, Chunk, Biome> //IJobForEach<ChunkRenderer>
		{
			public void Execute(ref ChunkMap chunkMap, ref Chunk chunk, ref Biome biome)   //Entity entity, int index,  
			{
				if (chunkMap.isBiome == 1 && chunkMap.dirty == 1)
				{
					chunkMap.dirty = 2;
					//Debug.LogError("Built ChunkBiomeMap for " + chunk.Value.chunkPosition);
					// get all the top voxels
					CreateFloatBiomeMap(ref chunkMap, ref biome, chunk.Value.voxelDimensions);
				}
			}

			public void CreateIntBiomeMap(ref ChunkMap chunkMap, ref Biome biome, int3 voxelDimensions)
			{
				int xzIndex2 = 0;
				for (int i = 0; i < voxelDimensions.x; i++)
				{
					for (int j = 0; j < voxelDimensions.z; j++)
					{
						int xzIndex = i + j * chunkMap.height;
						float multiplyerA = biome.biomes[xzIndex2];
						int biomeIndex = (int)math.floor(multiplyerA);
						//multiplyerA -= (int)math.floor(multiplyerA);
						chunkMap.topVoxels[xzIndex] = (byte)(biomeIndex + 1);
						chunkMap.heights[xzIndex] = (byte)((int)(1 * voxelDimensions.y));
						xzIndex2++;
					}
				}
			}

			public void CreateFloatBiomeMap(ref ChunkMap chunkMap,ref Biome biome, int3 voxelDimensions)
			{
				int xzIndex2 = 0;
				for (int i = 0; i < voxelDimensions.x; i++)
				{
					for (int j = 0; j < voxelDimensions.z; j++)
					{
						int xzIndex = i + j * chunkMap.height;
						float multiplyerA = biome.biomes[xzIndex2];
						/*if (biome.biomes[xzIndex2] < 0)
						{
							Debug.LogError("BIOME NEG");
						}*/
						int biomeIndex = (int)math.floor(multiplyerA);
						multiplyerA -= biomeIndex;
						chunkMap.topVoxels[xzIndex] = (byte)(biomeIndex + 1);
						if (biomeIndex != 1)
						{
							multiplyerA = 1 - multiplyerA;
						}
						chunkMap.heights[xzIndex] = (byte)((int)(multiplyerA * voxelDimensions.y));
						xzIndex2++;
					}
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkMapBiomeJob { }.Schedule(this, inputDeps);
		}
	}
}