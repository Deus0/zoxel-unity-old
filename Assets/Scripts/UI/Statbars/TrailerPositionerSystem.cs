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
    /// Updates the Translation of the UI bar
    /// Remember to Test: StatBarFrontPositionerSystem - with > 2000 units, to check if it lags.
    /// Since it goes through list each time to find entity ids
    /// </summary>
    [DisableAutoCreation]
    public class TrailerPositionerSystem : JobComponentSystem
    {
        //private EntityQuery characterQuery;

        [BurstCompile]
        struct SystemJob : IJobForEach<UITrailer, Translation>
        {

            public void Execute(ref UITrailer trailer, ref Translation position)
            {
                // Add some lerp maybe
                position.Value = trailer.position + new float3(0, trailer.heightAddition, 0);
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SystemJob { }.Schedule(this, inputDeps);
		}
	}
}
