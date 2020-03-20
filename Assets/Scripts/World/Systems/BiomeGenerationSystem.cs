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
	[DisableAutoCreation, UpdateBefore(typeof(TerrainGenerationSystem))]
	public class BiomeGenerationSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkTerrainJob : IJobForEach<WorldGenerationChunk, Chunk, Biome, ChunkTerrain>
		{
			public void Execute(ref WorldGenerationChunk worldGenerationChunk, ref Chunk chunk, ref Biome biome, ref ChunkTerrain chunkTerrain)
			{
				if (worldGenerationChunk.state == 1)
				{
					worldGenerationChunk.state = 2;
					float3 position = new float3(0, 0, 0);
					float2 heightPosition = new float2(1, 1);
					float2 perlinOffset = new float2(chunk.Value.chunkPosition.x * chunk.Value.voxelDimensions.x, chunk.Value.chunkPosition.z * chunk.Value.voxelDimensions.z);
					float2 noisePosition;
					/*Unity.Mathematics.Random random = new Unity.Mathematics.Random();
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
					random.InitState(uniqueness);*/

					// need to generate a blend of all biome types ranging from 0 to 1, like a combination of the biome maps will equal 1
					// then in terrain generation - for heightmaps
					//		Add heights as a combination of blends
					//		Base types as a percentage chance between all the voxels based on weights
					//			if 0.6 value of grass lands, give like 3 types of grass 0.2 chance each out of 1
					//int voxelIndex = 0;
					for (heightPosition.x = 0; heightPosition.x < chunk.Value.voxelDimensions.x; heightPosition.x++)
					{
						for (heightPosition.y = 0; heightPosition.y < chunk.Value.voxelDimensions.z; heightPosition.y++)
						{
							noisePosition = (perlinOffset + heightPosition);
							//noisePosition.x *= chunkTerrain.biome.landScale;
							//noisePosition.y *= chunkTerrain.biome.landScale;
							//int biomeInt = (int)math.floor(0.99f * (chunkTerrain.biomes.Length - 1) *  (1 + noise.snoise(0.01f * noisePosition)) / 2);
							//biome.biomes[voxelIndex] = (byte)(biomeInt);
							//Debug.LogError("Biome spawned of: " + biome.biomes[voxelIndex] + "::" + biomeInt);
							// one or two?
							float noiseScale = 0.008f;
							float noiseValue = (1 + noise.snoise(noiseScale * noisePosition)) / 2f;// (1 + noise.snoise(0.003f * noisePosition)) / 2;
							//noiseValue += noise.snoise(noisePosition * noiseScale * 4) / 8;
							noiseValue += noise.snoise(noisePosition * noiseScale * 10) / 6;
							//noiseValue = (1 + math.clamp(noiseValue, 0, 1)) / 2f;
							noiseValue = math.clamp(noiseValue, 0, 1);
							int voxelIndex = (int)(heightPosition.y + heightPosition.x * chunk.Value.voxelDimensions.z);
							//float noiseValue = (noise.snoise(0.001f * noisePosition));
							biome.biomes[voxelIndex] = (byte) math.floor(noiseValue * chunkTerrain.biomes.Length * 0.99f);
							biome.blends[voxelIndex] = noiseValue;
							//math.clamp((0.99f * (chunkTerrain.biomes.Length) * noiseValue), 0, chunkTerrain.biomes.Length);
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