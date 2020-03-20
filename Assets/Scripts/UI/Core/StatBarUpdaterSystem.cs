using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    [DisableAutoCreation]
    public class StatBarUpdaterSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<StatBarUI, ZoxID>().ForEach((Entity littlebitchEntity, ref StatBarUI statbar, ref ZoxID zoxID) =>
            {
                if (characterSpawnSystem.characters.ContainsKey(zoxID.id))
                {
                    Stats characterStats = World.EntityManager.GetComponentData<Stats>(characterSpawnSystem.characters[zoxID.id]);
                    if (characterStats.states.Length > 0)
                    {
                        StateStaz staz = characterStats.states[0];
                        statbar.targetPercentage = staz.value / staz.maxValue;
                    }
                }
                else if (TurretSpawnerSystem.turrets.ContainsKey(zoxID.id))
                {
                    Entity turret = TurretSpawnerSystem.turrets[zoxID.id];
                    if (World.EntityManager.HasComponent<Stats>(turret))
                    {
                        Stats characterStats = World.EntityManager.GetComponentData<Stats>(turret);
                        if (characterStats.states.Length > 0)
                        {
                            StateStaz staz = characterStats.states[0];
                            statbar.targetPercentage = staz.value / staz.maxValue;
                        }
                    }
                    /*else
                    {
                        Debug.LogError("Turret does not have stats: " + turret.Index);
                    }*/
                }
            });
        }
    }
}