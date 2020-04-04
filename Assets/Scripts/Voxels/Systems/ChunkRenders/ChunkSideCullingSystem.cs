using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel.Voxels
{

    /// <summary>
    /// Todo:
    /// add positions as a static that i can get to add to chunk
    /// do scaling after mesh is built
    /// fill in voxel data from current data - to test system
    /// build another system to place the voxel terrain
    /// 
    /// After this:
    /// Make Chunk get and set voxel data directly to the arrays?
    /// 
    /// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkToRendererSystem))]
    public class ChunkSideCullingSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ChunkSideCullingJob : IJobForEach<ChunkRendererBuilder, ChunkRenderer, ChunkSides>
		{
			public void Execute(ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer chunk, ref ChunkSides chunkSides)
			{
				if (chunkRendererBuilder.state == 1)
				{
					/*if (ChunkSpawnSystem.isDebugLog)
					{
						UnityEngine.Debug.LogError("Processing ChunkSideCullingSystem voxes.");
					}*/
					chunkRendererBuilder.state = 2;
					int voxelIndex;
					byte meta;
					int3 position; //  = new float3(0, 0, 0)
					int normalVoxelIndex = -1;
					for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
					{
						for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
						{
							for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                            {
                                normalVoxelIndex++;
								if (normalVoxelIndex >= chunkSides.sidesUp.Length)
								{
									//UnityEngine.Debug.LogError("normalVoxelIndex is too high at: " + normalVoxelIndex);
									return;
								}
                                //voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                meta = chunk.Value.voxels[normalVoxelIndex];
								if (meta == 0)
								{
									// if air don't draw anything!
									chunkSides.sidesUp[normalVoxelIndex] = 0;
									chunkSides.sidesDown[normalVoxelIndex] = 0;
									chunkSides.sidesLeft[normalVoxelIndex] = 0;
									chunkSides.sidesRight[normalVoxelIndex] = 0;
									chunkSides.sidesForward[normalVoxelIndex] = 0;
									chunkSides.sidesBack[normalVoxelIndex] = 0;
								}
								// for all solid voxels
								else
								{
									// Y AXIS - Up - Down
									if (position.y < chunk.Value.voxelDimensions.y - 1)
									{
                                        // x + HEIGHT * (y + WIDTH* z)
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Up(), chunk.Value.voxelDimensions);// (int)(sizeY * sizeX * (position.x) + sizeY * (position.y + 1) + (position.z));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesUp[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesUp[normalVoxelIndex] = 0;
										}
									}
									else
									{
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.z * (position.x) + (position.z));  // get the XZ Index
										if (chunk.Value.voxelsUp[voxelIndex] == 0)
										{
											chunkSides.sidesUp[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesUp[normalVoxelIndex] = 0;
										}
									}
									if (position.y > 0)
                                    {
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Down(), //new float3(0, -1, 0),
										 chunk.Value.voxelDimensions);
                                        //voxelIndexDown = (int)(sizeY * sizeX * (position.x) + sizeY * (position.y - 1) + (position.z));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesDown[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesDown[normalVoxelIndex] = 0;
										}
									}
									else
									{
                                        //chunk.sidesDown[voxelIndex] = 1;
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.z * (position.x) + (position.z));
										if (chunk.Value.voxelsDown[voxelIndex] == 0)
										{
											chunkSides.sidesDown[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesDown[normalVoxelIndex] = 0;
										}
									}
									 // Z AXIS - Forward - Back
									if (position.z < chunk.Value.voxelDimensions.z - 1)
                                    {
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Forward(),
										//new float3(0, 0, 1), 
										chunk.Value.voxelDimensions);
                                        //voxelIndexForward = (int)((position.z + 1) + sizeY * (position.y) + sizeY * sizeX * (position.x));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesForward[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesForward[normalVoxelIndex] = 0;
										}
									}
									else
									{
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.y * (position.x) + (position.y));
										if (chunk.Value.voxelsForward[voxelIndex] == 0)
										{
											chunkSides.sidesForward[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesForward[normalVoxelIndex] = 0;
										}
										//chunk.sidesForward[voxelIndex] = 1;
									}
									if (position.z > 0)
                                    {
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Back(),
										//new float3(0, 0, -1), 
										chunk.Value.voxelDimensions);
                                        //voxelIndexBack = (int)((position.z - 1) + sizeY * (position.y) + sizeY * sizeX * (position.x));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesBack[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesBack[normalVoxelIndex] = 0;
										}
									}
									else
									{
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.y * (position.x) + (position.y));
										if (chunk.Value.voxelsBack[voxelIndex] == 0)
										{
											chunkSides.sidesBack[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesBack[normalVoxelIndex] = 0;
										}
										//chunk.sidesBack[voxelIndex] = 1;
									}

									// X AXIS - Left - Right
									#region LeftRight
									if (position.x > 0)
                                    {
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Left(),
										//new float3(-1, 0, 0), 
										chunk.Value.voxelDimensions);
                                        //voxelIndexLeft = (int)(sizeY * sizeX * (position.x - 1) + sizeY * (position.y) +(position.z));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesLeft[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesLeft[normalVoxelIndex] = 0;
										}
									}
									else //if (position.x == 0)
									{
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.z * (position.y) + (position.z));
										if (chunk.Value.voxelsLeft[voxelIndex] == 0)
										{
											chunkSides.sidesLeft[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesLeft[normalVoxelIndex] = 0;
										}
									}
									if (position.x < chunk.Value.voxelDimensions.x - 1)
                                    {
                                        voxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position + int3.Right(),
										//new float3(1, 0, 0), 
										chunk.Value.voxelDimensions);
                                        //voxelIndexRight = (int)(sizeY * sizeX * (position.x + 1) + sizeY * (position.y) + (position.z));
										if (chunk.Value.voxels[voxelIndex] == 0)
										{
											chunkSides.sidesRight[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesRight[normalVoxelIndex] = 0;
										}
									}
									else// if (position.x == 15)
									{
                                        voxelIndex = (int)(chunk.Value.voxelDimensions.z * (position.y) + (position.z));
										if (chunk.Value.voxelsRight[voxelIndex] == 0)
										{
											chunkSides.sidesRight[normalVoxelIndex] = 1;
										}
										else
										{
											chunkSides.sidesRight[normalVoxelIndex] = 0;
										}
									}
									#endregion
								}
							}
						}
					}
					//chunk.isCullChunkSides = 1;
					//chunk.isCullSides = 0;
				}
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			return new ChunkSideCullingJob { }.Schedule(this, inputDeps);
		}
	}
}