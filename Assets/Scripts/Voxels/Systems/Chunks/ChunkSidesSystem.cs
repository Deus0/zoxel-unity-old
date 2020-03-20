using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel.Voxels
{

    /// <summary>
    ///     Copies sides of other chunks onto this one
    /// 
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
    [DisableAutoCreation]// UpdateAfter(typeof(ChunkBuildStarterSystem))]
    public class ChunkSidesSystem : JobComponentSystem   // unsafe
	{

        [BurstCompile]
		struct ChunkSidesJob : IJobForEach<ChunkBuilder, Chunk>
		{
			[ReadOnly]
			public NativeArray<Chunk> chunks;
            [ReadOnly]
            public NativeArray<float4> chunkMap;

            private Chunk GetChunk(ref Chunk chunkInput, int3 chunkAddition, out bool failed)
            {
                int worldID = chunkInput.worldID;
                int3 chunkPosition = chunkInput.Value.chunkPosition + chunkAddition;
                float4 chunkPointer = new float4(chunkPosition.x, chunkPosition.y, chunkPosition.z, worldID);
                int indexOf = chunkMap.IndexOf(chunkPointer);
                if (indexOf != -1)
                {
					failed = false;
					return chunks[indexOf];
                }
                else
				{
					failed = true;
					return new Chunk();
                }
            }

            public void Execute(ref ChunkBuilder chunkBuilder, ref Chunk chunk)
			{
				if (chunkBuilder.state == 0)
                {
                    byte edgeVoxelType = 0; // for making edges not show

                    // for all chunk sides, get side chunk, set side voxels
                    // if chunk has all sides updated, and is also dirty, it can cull sides
                    int voxelIndex = 0;
                    int otherChunkVoxelIndex = 0;
					int3 position;
					bool failed;

					#region UpDown
					if (chunk.hasUpdatedUp == 0)
					{
						voxelIndex = 0;
						if (chunk.indexUp != -1)
                        {
                            position.y = 0;
							Chunk otherChunk = GetChunk(ref chunk, int3.Up(), out failed);
							if (failed)
							{
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
									{
										chunk.Value.voxelsUp[voxelIndex] = edgeVoxelType;
										voxelIndex++;
									}
								}
								chunk.hasUpdatedUp = 1;
							}
							else if (otherChunk.isGenerating == 0)
							{
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
									{
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsUp[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
										voxelIndex++;
									}
								}
								chunk.hasUpdatedUp = 1;
							}
						}
						else
						{
							for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
							{
								for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
								{
									chunk.Value.voxelsUp[voxelIndex] = edgeVoxelType;
									voxelIndex++;
								}
							}
							chunk.hasUpdatedUp = 1;
						}
					}
					if (chunk.hasUpdatedDown == 0)
					{
						voxelIndex = 0;
						if (chunk.indexDown != -1)
						{
							Chunk otherChunk = GetChunk(ref chunk, int3.Down(), out failed);
							if (failed)
							{
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
									{
										chunk.Value.voxelsDown[voxelIndex] = edgeVoxelType;   // 0
										voxelIndex++;
									}
								}
								chunk.hasUpdatedDown = 1;
							}
							else if (otherChunk.isGenerating == 0)
							{
                                // For All Top Voxels of Other Chunk
                                position.y = (int) chunk.Value.voxelDimensions.y - 1;
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                    {
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsDown[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
                                        voxelIndex++;
									}
								}
								chunk.hasUpdatedDown = 1;
							}
						}
						else
                        {
                            for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                            {
                                for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                {
									if (chunk.isWeights == 1)
									{
										chunk.Value.voxelsDown[voxelIndex] = edgeVoxelType;   // 0
									}
									else
									{
										chunk.Value.voxelsDown[voxelIndex] = 1;// edgeVoxelType;   // 0
									}
									voxelIndex++;
								}
							}
							chunk.hasUpdatedDown = 1;
						}
					}
					#endregion

					#region LeftRight
					if (chunk.hasUpdatedLeft == 0)
					{
						voxelIndex = 0;
						if (chunk.indexLeft != -1)
						{
							Chunk otherChunk = GetChunk(ref chunk, int3.Left(), out failed);
							if (failed)
							{
								for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
									{
										chunk.Value.voxelsLeft[voxelIndex] = edgeVoxelType;
										voxelIndex++;
									}
								}
								chunk.hasUpdatedLeft = 1;
							}
							else if (otherChunk.isGenerating == 0)
                            {
                                // For All Right Voxels of Other Chunk
                                position.x = (int) chunk.Value.voxelDimensions.x - 1;
								for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                    {
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsLeft[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
                                        voxelIndex++;
									}
								}
								chunk.hasUpdatedLeft = 1;
							}
						}
						else
                        {
                            for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                            {
                                for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                {
									chunk.Value.voxelsLeft[voxelIndex] = edgeVoxelType;
									voxelIndex++;
								}
							}
							chunk.hasUpdatedLeft = 1;
						}
					}
					if (chunk.hasUpdatedRight == 0)
					{
						voxelIndex = 0;
						if (chunk.indexRight != -1)
						{
							Chunk otherChunk = GetChunk(ref chunk, int3.Right(), out failed);
							if (failed)
							{
								for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
								{
									for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
									{
										chunk.Value.voxelsRight[voxelIndex] = edgeVoxelType;   // 0
										voxelIndex++;
									}
								}
								chunk.hasUpdatedRight = 1;
							}
							else if (otherChunk.isGenerating == 0)
                            {
                                // For All Left Voxels of Other Chunk
                                position.x = 0;
                                for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                                {
                                    for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                    {
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsRight[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
                                        voxelIndex++;
									}
								}
								chunk.hasUpdatedRight = 1;
							}
						}
						else
                        {
                            for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                            {
                                for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                                {
									chunk.Value.voxelsRight[voxelIndex] = edgeVoxelType;   // 0
									voxelIndex++;
								}
							}
							chunk.hasUpdatedRight = 1;
						}
					}
					#endregion

					#region ForwardBack
					if (chunk.hasUpdatedForward == 0)
					{
						voxelIndex = 0;
						if (chunk.indexForward != -1)
						{
							Chunk otherChunk = GetChunk(ref chunk, int3.Forward(), out failed);
							if (failed)
							{
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
									{
										chunk.Value.voxelsForward[voxelIndex] = edgeVoxelType;
										voxelIndex++;
									}
								}
								chunk.hasUpdatedForward = 1;
							}
							else if (otherChunk.isGenerating == 0)
                            {
                                // For All Back Voxels of Other Chunk
                                position.z = 0;
                                for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                                    {
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsForward[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
                                        voxelIndex++;
									}
								}
								chunk.hasUpdatedForward = 1;
							}
						}
						else
                        {
                            for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                            {
                                for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                                {
									chunk.Value.voxelsForward[voxelIndex] = edgeVoxelType;
									voxelIndex++;
								}
							}
							chunk.hasUpdatedForward = 1;
						}
					}
					if (chunk.hasUpdatedBack == 0)
					{
						voxelIndex = 0;
						if (chunk.indexBack != -1)
						{
							Chunk otherChunk = GetChunk(ref chunk, int3.Back(), out failed);
							if (failed)
							{
								for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
								{
									for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
									{
										chunk.Value.voxelsBack[voxelIndex] = edgeVoxelType;   // 0
										voxelIndex++;
									}
								}
								chunk.hasUpdatedBack = 1;
							}
							else if (otherChunk.isGenerating == 0)
                            {
                                // For All Front Voxels of Other Chunk
                                position.z = (int) chunk.Value.voxelDimensions.z - 1;
                                for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                                {
                                    for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                                    {
                                        otherChunkVoxelIndex = VoxelRaycastSystem.GetVoxelArrayIndex(position, chunk.Value.voxelDimensions);
                                        chunk.Value.voxelsBack[voxelIndex] = otherChunk.Value.voxels[otherChunkVoxelIndex];
                                        voxelIndex++;
									}
								}
								chunk.hasUpdatedBack = 1;
							}
						}
						else
                        {
                            for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                            {
                                for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                                {
									chunk.Value.voxelsBack[voxelIndex] = edgeVoxelType;   // 0
									voxelIndex++;
								}
							}
							chunk.hasUpdatedBack = 1;
						}
					}
                    #endregion
					if (chunk.hasUpdatedLeft == 1 && chunk.hasUpdatedRight == 1
						&& chunk.hasUpdatedForward == 1 && chunk.hasUpdatedBack == 1)
					{
						chunkBuilder.state = 1;
					}
				}
			}
		}

        public static int3 upPosition = new int3(0, 1, 0);
		public static int3 downPosition = new int3(0, -1, 0);
		public static int3 leftPosition = new int3(-1, 0, 0);
		public static int3 rightPosition = new int3(1, 0, 0);
		public static int3 forwardPosition = new int3(0, 0, 1);
		public static int3 backPosition = new int3(0, 0, -1);

		private EntityQuery chunkQuery;
		private EntityQuery chunkQuery2;

		protected override void OnCreate()
        {
            base.OnCreate();
			chunkQuery = GetEntityQuery(ComponentType.ReadOnly<Chunk>());
			chunkQuery2 = GetEntityQuery(ComponentType.ReadOnly<ChunkBuilder>());
		}

        protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (chunkQuery2.CalculateEntityCount() == 0)
			{
				return new JobHandle();
			}
            var chunks = chunkQuery.ToComponentDataArray<Chunk>(Allocator.TempJob);
            NativeArray<float4> chunkMap = new NativeArray<float4>(chunks.Length, Allocator.TempJob);
            int i = 0;
            foreach (Chunk chunk in chunks)
            {
                // do a check, add to nativeArrayList if it needs to update the chunks or the surrounding one needs it
                chunkMap[i] = new float4(chunk.Value.chunkPosition.x, chunk.Value.chunkPosition.y, chunk.Value.chunkPosition.z, chunk.worldID);
                i++;
            }
            ChunkSidesJob job = new ChunkSidesJob
			{
				chunks = chunks,
                chunkMap = chunkMap
            };
			JobHandle handle = job.Schedule(this, inputDeps);
			handle.Complete();
			job.chunks.Dispose();
            chunkMap.Dispose();
			return handle;
		}
	}
}
