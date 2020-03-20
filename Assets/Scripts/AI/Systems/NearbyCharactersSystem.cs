using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;

namespace Zoxel
{
    /// <summary>
    /// gets nearby characters
    ///     N x N atm, use chunks and nearby chunks later on for checking
    ///     Worldbound should have chunk references
    /// </summary>
        [DisableAutoCreation]
    public class NearbyCharactersSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;

        protected override void OnUpdate()
        {
            float time = UnityEngine.Time.time;
            Entities.WithAll<Translation, NearbyCharacters>().ForEach((Entity littlebitchEntity, 
                ref Translation position, ref NearbyCharacters nearbyCharacters) =>
            {
                if (time - nearbyCharacters.lastUpdatedTime >= 1)
                {
                    nearbyCharacters.lastUpdatedTime = time;
                    //Debug.LogError(littlebitchEntity.Index + " is getting nearby characters " + time + " with characters: " + characterSpawnSystem.characters.Count);
                    // now update all nearby characters
                    
                    List<Entity> nearbyCharacterEntities = new List<Entity>();
                    List<int> nearbyCharacterClans = new List<int>();
                    List<float> nearbyCharacterDistances = new List<float>();
                    List<float3> nearbyPositions = new List<float3>();
                    foreach (KeyValuePair<int, Entity> KVP in characterSpawnSystem.characters)
                    {
                        if (KVP.Value != littlebitchEntity)
                        {
                            // i should check their chunk position and compare it first - its quicker then calculating distances?
                            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(KVP.Value);
                            Translation otherPosition = World.EntityManager.GetComponentData<Translation>(KVP.Value);
                            float distanceTo = math.distance(position.Value, otherPosition.Value);
                            if (distanceTo < 10)
                            {
                                nearbyCharacterEntities.Add(KVP.Value);
                                nearbyCharacterClans.Add(zoxID.clanID);
                                nearbyCharacterDistances.Add(distanceTo);
                                nearbyPositions.Add(otherPosition.Value);
                            }
                            /*else
                            {
                                Debug.LogError("Creature is too far away");
                            }*/
                        }
                    }
                    // now convert them all to nearbyCharacters arrays
                    nearbyCharacters.SetData(nearbyCharacterEntities, nearbyCharacterClans, nearbyCharacterDistances, nearbyPositions);
                    
                }
            });
        }
    }

}