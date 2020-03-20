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
	public class HeightMapGenerationSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkTerrainJob : IJobForEach<WorldGenerationChunk, Chunk, Biome, ChunkTerrain>//, ChunkTown>
		{
			private int MergeBiomeHeights(ref ChunkTerrain chunkTerrain, ref Biome biome, int biomeIndexA, int biomeIndexB, float2 noisePosition, float blend, float amplitudeModifier = 1)
			{
				var biomeDataA = chunkTerrain.biomes[biomeIndexA];
				var biomeDataB = chunkTerrain.biomes[biomeIndexB];
				//float calculatedHeight = 0;
				float2 biomeNoisePositionA = noisePosition * biomeDataA.landScale;
				float noiseValueA = (1 + noise.snoise(biomeNoisePositionA)) / 2f;
				noiseValueA += noise.snoise(biomeNoisePositionA * 10) / 4;
				noiseValueA *= biomeDataA.landAmplitude * amplitudeModifier;
				float2 biomeNoisePositionB = noisePosition * biomeDataB.landScale;
				float noiseValueB = (1 + noise.snoise(biomeNoisePositionB)) / 2f;
				noiseValueB += noise.snoise(biomeNoisePositionB * 10) / 4;
				noiseValueB *= biomeDataB.landAmplitude * amplitudeModifier;
				float landHeight = (blend * biomeDataA.landBase + (1 - blend) * biomeDataB.landBase);
				return (int)(landHeight + noiseValueA * blend
								+ noiseValueB * (1 - blend));

			}

			public void Execute(ref WorldGenerationChunk worldGenerationChunk, ref Chunk chunk, ref Biome biome, ref ChunkTerrain chunkTerrain)//, ref ChunkTown chunkTown)
			{
				//if (chunk.isBuildTerrain != 0)
				if (worldGenerationChunk.state == 2)
				{
					worldGenerationChunk.state = 3; //.hasBuiltTerrain = 2;
					//float3 position = new float3(0, 0, 0);
					int voxelIndex = 0;
					//float2 persistence = new float2(0, 0);
					float2 heightPosition = new float2(1, 1);
					float2 perlinOffset = new float2(chunk.Value.chunkPosition.x * chunk.Value.voxelDimensions.x, chunk.Value.chunkPosition.z * chunk.Value.voxelDimensions.z);
					float2 noisePosition;
					Unity.Mathematics.Random random = new Unity.Mathematics.Random();
					int thingoX = (int)chunk.Value.chunkPosition.x;
					if (thingoX < 0)
					{
						thingoX *= -64;
					}
					else
					{
						thingoX++;
					}
					int thingoZ = (int)chunk.Value.chunkPosition.z * 128;
					if (thingoZ < 0)
					{
						thingoZ *= -256;
					}
					uint uniqueness = (uint)(thingoX + thingoZ);
					random.InitState(uniqueness);
					for (heightPosition.x = 0; heightPosition.x < chunk.Value.voxelDimensions.x; heightPosition.x++)
					{
						for (heightPosition.y = 0; heightPosition.y < chunk.Value.voxelDimensions.z; heightPosition.y++)
						{
							noisePosition = (perlinOffset + heightPosition);
							float blend = biome.blends[voxelIndex];
							int biomeIndex = (int)biome.biomes[voxelIndex];	//math.floor(multiplyerA);
							//multiplyerA -= (int)math.floor(multiplyerA);	// gets a value of between 0 and 1
							int otherBiome;
							if (biomeIndex == 1)
							{
								otherBiome = 0;
							} 
							else if (biomeIndex == 0)
							{
								otherBiome = 1;
							}
							else
							{
								otherBiome = 0;
							}
							otherBiome = math.clamp(otherBiome, 0, chunkTerrain.biomes.Length - 1);
							
							int positionXZ2 = (int)(heightPosition.x * chunk.Value.voxelDimensions.z + heightPosition.y);
							/*if (chunkTown.IsPointInsideOfWalls(chunk.GetVoxelPosition() + new float3(heightPosition.x, 0, heightPosition.y)))
							{
								//chunkTerrain.heights[positionXZ2] /= 2; // blend with nearby positions later on
								chunkTerrain.heights[positionXZ2] = MergeBiomeHeights(ref chunkTerrain, ref biome, biomeIndex, otherBiome, noisePosition, blend, 0.5f);//
							}
							else if (chunkTown.IsPointInsideOf(chunk.GetVoxelPosition() + new float3(heightPosition.x, 0, heightPosition.y)))
							{
								//chunkTerrain.heights[positionXZ2] /= 2; // blend with nearby positions later on
								chunkTerrain.heights[positionXZ2] = MergeBiomeHeights(ref chunkTerrain, ref biome, biomeIndex, otherBiome, noisePosition, blend, 0.75f);//
							}
							else*/
							{
								chunkTerrain.heights[positionXZ2] = MergeBiomeHeights(ref chunkTerrain, ref biome, biomeIndex, otherBiome, noisePosition, blend);
							}
							voxelIndex++;
							/*if (noiseValueB < 0 || noiseValueB > 1)
							{
								Debug.LogError("noiseValueB: " + noiseValueB);
							}*/
							/*if (biomeIndex < 0 || biomeIndex > 1)
							{
								Debug.LogError("Bimoe out of bounds: " + biomeIndex);
							}
							if (multiplyerA < 0 || multiplyerA > 1)
							{
								Debug.LogError("multiplyerA: " + multiplyerA);
							}
							if (multiplyerB < 0 || multiplyerB > 1)
							{
								Debug.LogError("multiplyerB: " + multiplyerB);
							}*/
						}
					}
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkTerrainJob { }.Schedule(this, inputDeps);
		}
	}
}