using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
//using Unity.Particles;

namespace Zoxel
{
    [DisableAutoCreation]
    public class LevelUpSystem : JobComponentSystem
    {

        [BurstCompile]
        struct LevelUpJob : IJobForEach<Stats>
        {
            [ReadOnly]
            public int levelID;
            [ReadOnly]
            public int experienceID;
            [ReadOnly]
            public int statPointID;

            public void Execute(ref Stats stats)
            {
                if (stats.leveledUp == 1)
                {
                    stats.leveledUp = 0;
                    /*
                    #region Indexes
                    int levelIndex = -1;
                    for (int i = 0; i < stats.stats.Length; i++)
                    {
                        if (stats.stats[i].id == levelID)
                        {
                            levelIndex = i;
                            break;
                        }
                    }
                    if (levelIndex == -1)
                    {
                        return;
                    }
                    int experienceIndex = -1;
                    for (int i = 0; i < stats.states.Length; i++)
                    {
                        if (stats.states[i].id == experienceID)
                        {
                            experienceIndex = i;
                            break;
                        }
                    }
                    if (experienceIndex == -1)
                    {
                        return;
                    }
                    #endregion
                    */
                    Level level = stats.levels[0];
                    //StateStaz experience = stats.states[experienceIndex];

                    int counter = 0;
                    int statPointsGained = 0;
                    while (level.experienceGained >= level.experienceRequired)
                    {
                        level.experienceGained -= level.experienceRequired;
                        level.experienceRequired *= 1.2f;
                        // increase SkillPoints (base stat)
                        level.value++;
                        statPointsGained += 3;
                        counter++;
                        if (counter >= 255)
                        {
                            break;
                        }
                    }
                    stats.levels[0] = level;


                    //stats.states[0] = health; // until i made attribute value, then need AI to spend points
                    // also give some skill points
                    // give some attribute points too
                    stats.leveledUpEffects = 1; // now add particles and sound
                    // restore states
                    for (int j = 0; j < stats.states.Length; j++)
                    {
                        StateStaz state = stats.states[j];
                        if (state.id != experienceID)
                        {
                            state.value = state.maxValue * 0.99f;
                            stats.states[j] = state;
                        }
                    }
                    // give stat points
                    int statPointIndex = -1;
                    for (int i = 0; i < stats.stats.Length; i++)
                    {
                        if (stats.stats[i].id == statPointID)
                        {
                            statPointIndex = i;
                            break;
                        }
                    }
                    if (statPointIndex == -1)
                    {
                        return;
                    }
                    Staz statPoint = stats.stats[statPointIndex];
                    statPoint.value += statPointsGained;
                    stats.stats[statPointIndex] = statPoint;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new LevelUpJob
            {
                levelID = -1151411696,
                experienceID = -681813160,
                statPointID = -510241704
            }.Schedule(this, inputDeps);
        }
    }
}