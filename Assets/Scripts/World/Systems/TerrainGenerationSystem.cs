using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Zoxel.Voxels;
using UnityEngine;

namespace Zoxel.WorldGeneration
{

    /// <summary>
    /// Todo:
    /// Add VoxTerrainComponent onto terrain parts
    /// When edited terrain (using gameplay) mark terrain as edited - and thus don't run through generation code
    ///		but instead grab the bytes from a file
    /// 
    /// add positions as a static that i can get to add to chunk
    /// do scaling after mesh is built
    /// fill in voxel data from current data - to test system
    /// build another system to place the voxel terrain
    /// 
    /// After this:
    /// Make Chunk get and set voxel data directly to the arrays?
    /// 
    /// </summary>
    [DisableAutoCreation]
    public class TerrainGenerationSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkTerrainJob : IJobForEach<WorldGenerationChunk, Chunk, Biome, ChunkTerrain>
		{
			public void Execute(ref WorldGenerationChunk worldGenerationChunk, ref Chunk chunk, ref Biome biome, ref ChunkTerrain chunkTerrain)
			{
				//if (chunk.isBuildTerrain != 0)
				if (worldGenerationChunk.state == 3)
				{
					worldGenerationChunk.state = 4;
					/*if (ChunkSpawnSystem.isDebugLog)
					{
						Debug.LogError("Finished building terrain for chunk at: " + chunk.Value.chunkPosition);
					}*/
					float3 position = new float3(0, 0, 0);
					int voxelIndex = 0;
					//float2 persistence = new float2(0, 0);
					float2 heightPosition = new float2(1, 1);
					float2 perlinOffset = new float2(chunk.Value.chunkPosition.x * chunk.Value.voxelDimensions.x, chunk.Value.chunkPosition.z * chunk.Value.voxelDimensions.z);
					//.loat2 noisePosition;
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
					/*for (heightPosition.x = 0; heightPosition.x < chunk.Value.voxelDimensions.x; heightPosition.x++)
					{
						for (heightPosition.y = 0; heightPosition.y < chunk.Value.voxelDimensions.z; heightPosition.y++)
						{
							noisePosition = (perlinOffset + heightPosition);
							var biomeData = chunkTerrain.biomes[math.min(chunkTerrain.biomes.Length - 1, (int) biome.biomes[voxelIndex])];
							noisePosition.x *= biomeData.landScale;
							noisePosition.y *= biomeData.landScale;
							chunkTerrain.heights[voxelIndex] = (int)(biomeData.landBase
								+ biomeData.landAmplitude * noise.snoise(noisePosition)
								+ biomeData.landAmplitude * noise.snoise(noisePosition * 10) / 4);
							voxelIndex++;
						}
					}*/
					voxelIndex = 0;
					int positionXZ;
					int heightOffset = (int) (chunk.Value.chunkPosition.y * chunk.Value.voxelDimensions.y);
					Unity.Mathematics.Random random2 = new Unity.Mathematics.Random();
					random2.InitState();
					//int newType = 0;
					for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
					{
						for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
						{
							for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
							{
								positionXZ = (int)(position.x * chunk.Value.voxelDimensions.z + position.z);
								var biomeData = chunkTerrain.biomes[math.min(chunkTerrain.biomes.Length - 1, (int)biome.biomes[positionXZ])];
								int soilRange = 1 + random2.NextInt(3);
								int grassChance = random2.NextInt(100);
								// bedrock
								if (position.y == 0)
								{
									chunk.Value.voxels[voxelIndex] = (byte)(biomeData.sandID);
								}
								// grass
								else if (position.y + heightOffset == chunkTerrain.heights[positionXZ] &&
									grassChance < biomeData.grassChance)
								{
									if (position.y + heightOffset < 15)
									{
										chunk.Value.voxels[voxelIndex] = (byte)(biomeData.sandID);
									}
									else
									{
										chunk.Value.voxels[voxelIndex] = (byte)(biomeData.grassID);
									}
								}
								else if (position.y + heightOffset <= chunkTerrain.heights[positionXZ])
								{
									if (position.y + heightOffset >= chunkTerrain.heights[positionXZ] - soilRange) //UnityEngine.Random.Range(0,3))
									{
										if (position.y + heightOffset < 15)
										{
											chunk.Value.voxels[voxelIndex] = (byte)(biomeData.sandID);
										}
										else
										{
											chunk.Value.voxels[voxelIndex] = (byte)(biomeData.dirtID);
										}
									}
									else
									{
										chunk.Value.voxels[voxelIndex] = (byte)(biomeData.stoneID);
									}
									// = 1;// + (int)(3 * ((1 + noise.snoise(position)) / 2f));
								}
								else
								{
									chunk.Value.voxels[voxelIndex] = 0;
								}
								// add some 3 dimensional noise here for caves and cliffs
								voxelIndex++;
							}
						}
					}
					//chunk.isGenerating = 0;
					//chunk.isDirty = 1;
                }

				/*UnityEngine.Debug.DrawLine(chunk.GetVoxelPosition(),
					chunk.GetVoxelPosition() + new float3(8.5f, 66, 8.5f),
					UnityEngine.Color.blue, 4);*/
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkTerrainJob { }.Schedule(this, inputDeps);
		}
	}
}