using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Zoxel;
using Unity.Rendering;
using System;
using UnityEngine;

namespace Zoxel.Voxels
{

    /// <summary>
    /// Check for update in chunk position (this can be used for all characters to record positions to chunks
    /// </summary>
    [DisableAutoCreation]
    public class ChunkStreamSystem : JobComponentSystem
    {
        [BurstCompile]
        struct MyJob : IJobForEach<ChunkStreamPoint, Translation>
        {
            public void Execute(ref ChunkStreamPoint streamer, ref Translation position)
            {
                if (streamer.didUpdate == 0) 
                {
                    // should do some transform stuff for the position
                    var newChunkPosition = VoxelRaycastSystem.GetChunkPosition(new int3(position.Value), streamer.voxelDimensions);
                    /*if (float.IsNaN(newChunkPosition.x) || (newChunkPosition.x >= -0.001f && newChunkPosition.x <= 0.001f))
                    {
                        Debug.LogError("New Chunk Position.x is NaN");
                        newChunkPosition.x = 0;
                    }
                    if (float.IsNaN(newChunkPosition.z) || (newChunkPosition.z >= -0.001f && newChunkPosition.z <= 0.001f))
                    {
                        Debug.LogError("New Chunk Position.z is NaN");
                        newChunkPosition.z = 0;
                    }*/
                    newChunkPosition.y = 0; // can limit bounds here, if out should teleport them back in (faling)
                    if (newChunkPosition.x != streamer.chunkPosition.x ||
                        newChunkPosition.y != streamer.chunkPosition.y ||
                        newChunkPosition.z != streamer.chunkPosition.z)
                    {
                        streamer.didUpdate = 1;
                        streamer.chunkPosition = newChunkPosition;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new MyJob { }.Schedule(this, inputDeps);
        }
    }

    /// <summary>
    /// when positions update, set the world position
    /// </summary>
    [DisableAutoCreation]
    public class ChunkStreamEndSystem : ComponentSystem
    {
        public WorldSpawnSystem worldSpawnSystem;

        protected override void OnUpdate()
        {
            if (Bootstrap.isStreamChunks == false)
            {
                return;
            }
            Entities.WithAll<ChunkStreamPoint, ZoxID>().ForEach((Entity e, ref ChunkStreamPoint streamer, ref ZoxID zoxID) => // , ref RenderMesh renderer
            {
                if (streamer.didUpdate == 1)
                {
                    streamer.didUpdate = 0;
                    // tell worldspawner to update
                    // move this stuff to here
                    worldSpawnSystem.SetWorldPosition(zoxID.id, streamer.worldID, streamer.chunkPosition);
                }
            });
        }

        public void SetWorldPosition(int streamerID, int worldID, float3 newCentralPosition)
        {

        }
    }
}