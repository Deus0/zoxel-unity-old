using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
namespace Zoxel
{
    /// <summary>
    /// If attributes are updated, re apply that attribute to the corresponding stat
    /// </summary>
    [DisableAutoCreation]
    public class AttributesSystem : JobComponentSystem
    {

        [BurstCompile]
        struct StatsJob : IJobForEach<Stats>
        {

            public void Execute(ref Stats stats)// ref ZoxID zoxID) // Entity e, int index, 
            {
                if (stats.attributesApplied == 0)
                {
                    stats.attributesApplied = 1;
                    // do this when leveling up and gained alot of stats
                    // or did a big quest chain and got free stats as quest rewards

                    //Debug.LogError("Applying Attributes");
                    // first set all stats back to original

                    // now apply new stats
                    for (int j = 0; j < stats.attributes.Length; j++)
                    {
                        AttributeStaz attribute = stats.attributes[j];
                        bool didFind = false;
                        for (int i = 0; i < stats.states.Length; i++)
                        {
                            if (stats.states[i].id == attribute.targetID)
                            {
                                didFind = true;
                                if (attribute.previousAdded != 0)
                                {
                                    //Debug.LogError("Previous stat value was: " + attribute.previousAdded);
                                    float originalValue = stats.states[i].maxValue - attribute.previousAdded;
                                    stats.SetStateMaxValue(i, originalValue);
                                }
                                float bonusValue = attribute.value * attribute.multiplier;
                                float newValue = stats.states[i].maxValue + bonusValue;
                                stats.SetStateMaxValue(i, newValue);
                                attribute.previousAdded = bonusValue;
                                stats.attributes[j] = attribute;
                                //Debug.LogError("Adding bonus value to stat: " + bonusValue);
                                break;
                            }
                        }
                        if (!didFind)
                        {
                            //Debug.LogError("Could not find attribute target stat.");
                        }
                    }
                }

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new StatsJob {  }.Schedule(this, inputDeps);
        }
    }
}