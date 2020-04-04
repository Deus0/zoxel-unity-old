using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{

    /// <summary>
    /// Add force to bodies
    /// </summary>
    [DisableAutoCreation]
    public class LerpPositionSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ForceJob : IJobForEach<PositionLerper, Translation>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref PositionLerper lerper, ref Translation position)
            {
                var timePassed = time - lerper.createdTime;
                if (timePassed > lerper.lifeTime && lerper.loop == 1)
                {
                    var tempEnd = lerper.positionEnd;
                    lerper.positionEnd = lerper.positionBegin;
                    lerper.positionBegin = tempEnd;
                    lerper.createdTime = time;
                    timePassed = 0;
                }
                position.Value = math.lerp(lerper.positionBegin, lerper.positionEnd, timePassed / lerper.lifeTime);
            }
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ForceJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
    /// <summary>
    /// Add force to bodies
    /// </summary>
    [DisableAutoCreation]
    public class LerpPositionEntitySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<PositionEntityLerper, Translation>().ForEach((Entity e, ref PositionEntityLerper lerper, ref Translation position) =>
            {
                if (World.EntityManager.Exists(lerper.positionEnd) && World.EntityManager.HasComponent<Translation>(lerper.positionEnd))
                {
                    Translation characterTranslation = World.EntityManager.GetComponentData<Translation>(lerper.positionEnd);
                    position.Value = math.lerp(lerper.positionBegin, characterTranslation.Value, (UnityEngine.Time.time - lerper.createdTime) / lerper.lifeTime);
                }
            });
        }
    }
}