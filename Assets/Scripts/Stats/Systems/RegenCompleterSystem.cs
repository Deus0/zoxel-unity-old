using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// Push state changes to UIs
    /// </summary>
    [DisableAutoCreation]
    public class RegenCompleterSystem : ComponentSystem
    {
        public static void StartRegening(EntityManager EntityManager, Entity character)
        {
            if (EntityManager.HasComponent<Regening>(character) == false)
            {
                Stats stats = EntityManager.GetComponentData<Stats>(character);
                if (stats.states.Length > 0)
                {
                    Regening newRegen = new Regening { };
                    newRegen.Initialize(stats.states.Length);
                    EntityManager.AddComponentData(character, newRegen);
                }
            }
        }

        protected override void OnUpdate()
        {
            // Update Statbars
            Entities.WithAll<Regening, Stats>().ForEach((Entity e, ref Regening regening, ref Stats stats) =>
            {
                // generally this is bad practice. Needs to be done when adding stats/removign them from character, or just creating the regening
                if (stats.states.Length != regening.stateUpdated.Length)
                {
                    regening.Initialize(stats.states.Length);
                }

                for (int i = 0; i < regening.stateUpdated.Length; i++)
                {
                    if (regening.stateUpdated[i] == 1)
                    {
                        regening.stateUpdated[i] = 0;
                        StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, e, StatType.State, 0);
                    }
                }
                for (int i = 0; i < regening.stateMaxed.Length; i++)
                {
                    if (regening.stateMaxed[i] == 1)
                    {
                        regening.stateMaxed[i] = 0;
                        UpdateStatbar(stats, e, i);
                    }
                }
                if (regening.finished == 1)
                {
                    //regening.finished = 0;
                    // remove regening when all regening done
                    //Debug.LogError("Regening finished!");
                    World.EntityManager.RemoveComponent<Regening>(e);
                }
            });
        }

        protected void UpdateStatbar(Stats stats, Entity character, int stateIndex)
        {
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
            int characterID = zoxID.id;
            if (stateIndex != -1)
            {
                StateStaz targetStateStat = stats.states[stateIndex];
                if (targetStateStat.value == targetStateStat.maxValue)
                {
                    // new system needed - struct that contains multiple bars, for various state stats
                    if (StatbarSystem.frontBars.ContainsKey(characterID))
                    {
                        StatBarUI statBarUI = World.EntityManager.GetComponentData<StatBarUI>(StatbarSystem.frontBars[characterID]);
                        if (statBarUI.isTakingDamage == 1)
                        {
                            statBarUI.isTakingDamage = 0;
                            //statBarUI.timeStateChanged = UnityEngine.Time.time;
                            World.EntityManager.SetComponentData(StatbarSystem.frontBars[characterID], statBarUI);
                        }
                    }
                }
                //RegenStaz[] regens = stats.regens.ToArray();
                // later check for all regens, if the value increase to max
                //foreach (RegenStaz regen in regens)
                {
                //RegenStaz regen = stats.regens[regenIndex];
                // get index id
                /*int targetStatIndex = -1;
                for (int i = 0; i < stats.states.Length; i++)
                {
                    if (stats.states[i].id == targetStatIndex)
                    {
                        targetStatIndex = i;
                        break;
                    }
                }
                */
                }
            }
        }
    }
}