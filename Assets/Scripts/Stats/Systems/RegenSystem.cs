using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    /// <summary>
    /// If 
    /// </summary>
    [DisableAutoCreation]
    public class RegenSystem : JobComponentSystem
    {

        [BurstCompile]
		struct StatsJob : IJobForEach<Regening, Stats>
		{
            [ReadOnly]
			public float time;

            public void Execute(ref Regening regening, ref Stats stats)
            {
                //stats.regened = 0;
                const float regenModifier = 20; // divide by
				float timeDifference;
				RegenStaz regenStat;
				StateStaz stater;
                int targetStatIndex;
                byte anythingUpdated = 0;
                for (int i = 0; i < stats.regens.Length; i++)
                {
                    regenStat = stats.regens[i];

                    #region IdToindex
                    targetStatIndex = -1;
                    for (int j = 0; j < stats.states.Length; j++)
                    {
                        if (stats.states[j].id == regenStat.targetID)
                        {
                            targetStatIndex = j;
                            break;
                        }
                    }
                    #endregion

                    if (targetStatIndex != -1)
                    {
                        stater = stats.states[targetStatIndex];
                        if (stater.value == stater.maxValue)
                        {
                            //Debug.LogError("Stat is maxxed");
                            continue;
                        }
                        anythingUpdated = 1;
                        // for regen rates, make sure to wait the time needed first
                        if (regenStat.rate != 0 && time - regenStat.lastUpdatedTime < regenStat.rate)
                        {
                            continue;   // if rated, then only update then
                        }
                        timeDifference = time - regenStat.lastUpdatedTime;
                        //stater.value += regenStat.value * timeDifference;   // based on time
                        if (regenStat.rate == 0)
                        {
                            stater.value += regenStat.value / regenModifier * timeDifference;   // based on time
                        }
                        else
                        {
                            stater.value += regenStat.value / regenModifier;    // based purely on rate
                        }
                        if (stater.value >= stater.maxValue)
                        {
                            stater.value = stater.maxValue;
                            //stats.regenCompleted = 1;
                            regening.stateMaxed[targetStatIndex] = 1;
                        }
                        stats.states[targetStatIndex] = stater;
                        if (targetStatIndex < regening.stateUpdated.Length)
                        {
                            regening.stateUpdated[targetStatIndex] = 1;  // is regening
                        }
                    }
                    regenStat.lastUpdatedTime = time;
                    stats.regens[i] = regenStat;
                }
                if (anythingUpdated == 0)
                {
                    // nothing updated!
                    regening.finished = 1;
                }
            }
        }
        /*else
        {
            // debugging any regen stats that dont work
            stater = stats.states[0];
            stater.value = 1;
            stats.states[0] = stater;
        }*/

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new StatsJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
		}
	}

}