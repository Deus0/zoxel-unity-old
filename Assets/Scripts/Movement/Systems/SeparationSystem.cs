using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>


namespace Zoxel
{
    /// <summary>
    /// Add force to bodies
    /// </summary>
    [DisableAutoCreation]
    public class SeparationSystem : JobComponentSystem
    {
        private EntityQuery characterQuery;
        public static float distanceToAvoid2 = 1f;//1f;
        public static float avoidanceForce2 = 0.5f;//0.2f;//0.14f;//0.06f;

        protected override void OnCreate()
        {
            base.OnCreate();
            characterQuery = GetEntityQuery(
                ComponentType.ReadOnly<Character>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<ZoxID>());
        }

        [BurstCompile]
        struct AvoidanceJob : IJobForEach<Character, ZoxID, BodyForce, Translation>
        {
            [ReadOnly]
            public NativeArray<Translation> positions; 
            [ReadOnly]
            public NativeArray<ZoxID> characterIDs;
            [ReadOnly]
            public float distanceToAvoid;
            [ReadOnly]
            public float avoidanceForce;

            public void Execute(ref Character character, ref ZoxID zoxID, ref BodyForce body, ref Translation position)
            {
                // important
                body.worldVelocity = float3.zero;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (characterIDs[i].id != zoxID.id)
                    {
                        AvoidPosition(ref body, position.Value, positions[i].Value);
                    }
                }
                body.worldVelocity.y = 0;
            }

            public void AvoidPosition(ref BodyForce body, float3 position, float3 otherPosition)
            {
                float distanceTo = math.distance(position, otherPosition);
                if (distanceTo < distanceToAvoid)
                {
                    // add force away from it
                    float3 newForce = avoidanceForce * (distanceToAvoid - distanceTo) * math.normalizesafe(position - otherPosition);
                    body.worldVelocity += newForce;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new JobHandle();
            /*
            // for stats, make another one called ZoxID -> give ID and clanID 
            NativeArray<Translation> translations = characterQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
            NativeArray<ZoxID> staters = characterQuery.ToComponentDataArray<ZoxID>(Allocator.TempJob);
            JobHandle handle = new AvoidanceJob
            {
                //time =UnityEngine.Time.time,
                distanceToAvoid = distanceToAvoid2,
                avoidanceForce = avoidanceForce2,
                positions = translations,//positions,
                characterIDs = staters//characterIDs
            }.Schedule(this, inputDeps);
            handle.Complete();
            staters.Dispose();
            translations.Dispose();
            return handle;
            */
        }
    }

}