using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

namespace Zoxel
{
    [DisableAutoCreation]
    public class RenderBoundsUpdateSystem : JobComponentSystem
    {
        EntityQuery m_WorldRenderBounds;
        
        [BurstCompile]
        struct BoundsJob : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkComponentType<RenderBounds> RendererBounds;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LocalToWorld;
            public ArchetypeChunkComponentType<WorldRenderBounds> WorldRenderBounds;
            public ArchetypeChunkComponentType<ChunkWorldRenderBounds> ChunkWorldRenderBounds;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var worldBounds = chunk.GetNativeArray(WorldRenderBounds);
                var localBounds = chunk.GetNativeArray(RendererBounds);
                var localToWorld = chunk.GetNativeArray(LocalToWorld);
                MinMaxAABB combined = MinMaxAABB.Empty;
                for (int i = 0; i != localBounds.Length; i++)
                {
                    var transformed = AABB.Transform(localToWorld[i].Value, localBounds[i].Value);

                    worldBounds[i] = new WorldRenderBounds { Value = transformed };
                    combined.Encapsulate(transformed);
                }
                
                chunk.SetChunkComponentData(ChunkWorldRenderBounds, new ChunkWorldRenderBounds { Value = combined });
            }
        }

        protected override void OnCreate()
        {
            m_WorldRenderBounds = GetEntityQuery
            (
                new EntityQueryDesc
                {
                    All = new[] { ComponentType.ChunkComponent<ChunkWorldRenderBounds>(), ComponentType.ReadWrite<WorldRenderBounds>(), ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>() },
                }
            );
            m_WorldRenderBounds.SetChangedVersionFilter(new [] { ComponentType.ReadOnly<RenderBounds>(), ComponentType.ReadOnly<LocalToWorld>()} );
        }

        protected override JobHandle OnUpdate(JobHandle dependency)
        {
            var boundsJob = new BoundsJob
            {
                RendererBounds = GetArchetypeChunkComponentType<RenderBounds>(true),
                LocalToWorld = GetArchetypeChunkComponentType<LocalToWorld>(true),
                WorldRenderBounds = GetArchetypeChunkComponentType<WorldRenderBounds>(),
                ChunkWorldRenderBounds = GetArchetypeChunkComponentType<ChunkWorldRenderBounds>(),
            };
            return boundsJob.ScheduleParallel(m_WorldRenderBounds, dependency);
        }
    }
}
