using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Zoxel.Voxels;

namespace Zoxel
{

	[DisableAutoCreation]
	public class ChunkMapBuilderSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkMapJob : IJobForEach<ChunkMap, Chunk> //IJobForEach<ChunkRenderer>
		{
			public void Execute(ref ChunkMap chunkMap, ref Chunk chunk)   //Entity entity, int index,  
			{
				if (chunkMap.isBiome == 0 && chunkMap.dirty == 1)
				{
					chunkMap.dirty = 2;
					// get all the top voxels
					//int xzIndex = 0;
					for (int i = 0; i < chunk.Value.voxelDimensions.x; i++)
					{
						for (int j = 0; j < chunk.Value.voxelDimensions.z; j++)
						{
							byte topMostVoxel = 0;
							byte highestHeight = 0;
							int xzIndex = i + j * chunkMap.height;
							for (int k = (int)chunk.Value.voxelDimensions.y - 1; k >= 0 ; k--)
							{
								int xyzIndex = VoxelRaycastSystem.GetVoxelArrayIndex(new int3(i, k, j), chunk.Value.voxelDimensions);
								if (chunk.Value.voxels[xyzIndex] != 0)
								{
									topMostVoxel = chunk.Value.voxels[xyzIndex];
									highestHeight = (byte)k;
									break;
								}
							}
							chunkMap.topVoxels[xzIndex] = topMostVoxel;
							chunkMap.heights[xzIndex] = highestHeight;
							//xzIndex++;
						}
					}
				}
			}
		}
		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkMapJob { }.Schedule(this, inputDeps);
		}
	}
}