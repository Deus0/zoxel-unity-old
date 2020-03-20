using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// Updates the scale of the UI bar
    /// Updates Translation based on scale
    /// </summary>
    [DisableAutoCreation]
    [UpdateAfter(typeof(TrailerPositionerSystem))]
    public class StatbarPositionerSystem : JobComponentSystem
	{
		[BurstCompile]
		struct SystemJob : IJobForEach<StatBarUI, Translation, Rotation, NonUniformScale>
        {
            [ReadOnly]
            public float delta;

            public void Execute(ref StatBarUI statBar, ref Translation position, ref Rotation rotation, ref NonUniformScale scale)
			{
                statBar.percentage = math.lerp(statBar.percentage, statBar.targetPercentage, delta * 2);
				float3 newScale = scale.Value;
                newScale.x = statBar.percentage; // statBar.value / statBar.max;
                scale.Value = newScale;
                //float3 newPosition = position.Value;
                float3 positionOffset = new float3(-statBar.width/2f, 0, 0);
                //newPosition += math.mul(rotation.Value, positionOffset);
                //position.Value = newPosition;
                position.Value = positionOffset;
            }
        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			SystemJob job = new SystemJob {
                delta = UnityEngine.Time.deltaTime//,
                //statBarWidth = StatbarSystem.healthBarWidth / 2f
            };
			JobHandle handle = job.Schedule(this, inputDeps);
			return handle;
		}
	}

}