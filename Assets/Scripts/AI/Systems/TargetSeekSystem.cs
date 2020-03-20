using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Rendering;

// Problem:
//      Target seeking relies on cached character data on nearby targets of each unit

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>


// maintain character map inside each chunk
// containing list of their ids
// containing their positions? - a quadrant index (like 4x4x4, if a character in there or a surrounding quad, then its nearby)
// for targetting system use these lookups
// when character moving and reaches new voxel position - update in lookup

namespace Zoxel
{
    /// <summary>
    /// To do:
    /// Use maps to store the characters in chunks
    /// Only check self and surrounding chunks for targets
    /// Do a vision check on target using a vision radius
    /// </summary>
    [DisableAutoCreation]
    public class TargetSeekSystem : JobComponentSystem
    {
        public static bool debugCharacterPositions = false;


        [BurstCompile]
        struct SeekJob : IJobForEach<NearbyCharacters, ZoxID, Targeter>
        {
            [ReadOnly]
            public float time;

            public void Execute(ref NearbyCharacters nearbyCharacters, ref ZoxID zoxID, ref Targeter targeter)
            {
                //InterFloat targetFound;
                if (time - targeter.lastSeeked >= targeter.Value.seekCooldown)
                {
                    // targeter.hasTarget == 0 && 

                    // if has to seek new target, seek new
                    SeekTarget(nearbyCharacters, ref targeter, zoxID);
                }
            }

            /// <summary>
            /// Main seek algorithm
            ///     finds closest one
            ///     checks if clans equal
            ///     should also work for fellow clan members so they can flock together
            ///     flock needs a list of nearby ones to flock with - instead of wander - flocksystem
            /// </summary>
            private void SeekTarget(NearbyCharacters nearbyCharacters, ref Targeter targeter, ZoxID zoxID)
            {
                targeter.lastSeeked = time;
                float closestDistance = targeter.Value.seekRange;
                int targetIndex = -1;
                for (int i = 0; i < nearbyCharacters.characters.Length; i++)
                {
                    // if not this one, and not dead, then can attack! (And not from same clan)
                    if (zoxID.clanID == 0 || nearbyCharacters.characters[i].clan == 0 ||
                        nearbyCharacters.characters[i].clan != zoxID.clanID)
                    {
                        //float newDistance = math.distance(positionValue, character.position);
                        if (nearbyCharacters.characters[i].distance < closestDistance)
                        {
                            closestDistance = nearbyCharacters.characters[i].distance;
                            targetIndex = i;
                        }
                    }
                }
                //Debug.LogError("Seeking new target (from nearby), cd of: " + zoxID.id);
                if (targetIndex != -1)
                {
                    //Debug.LogError(zoxID.id + " has set a new target: " + nearbyCharacters.characters[targetIndex].Index + ", At Index: " + targetIndex);
                    targeter.nearbyCharacter = nearbyCharacters.characters[targetIndex];
                    //targeter.targetDistance = nearbyCharacters.distances[targetIndex];
                    //targeter.targetPosition = nearbyCharacters.positions[targetIndex];
                    targeter.hasTarget = 1;
                    //Debug.LogError(zoxID.id + " has new target:" + targetIndex + " out of " + nearbyCharacters.characters.Length);
                }
                else
                {
                    targeter.hasTarget = 0;
                    //Debug.LogError(zoxID.id + " has no target," + " out of " + nearbyCharacters.characters.Length);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SeekJob{ time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }

}
//else
//{
// otherwise?
//targetFound = new InterFloat(0, 0);
//}
//Seek a new target every x seconds!
// Todo: Add optimization buckets

/*if (targeter.targetID == 0)
{
    SetNewTarget(ref targeter, targetFound);
}
else
{*/
// if current target is ally
// check if target hostile exists
// set new target as that if it is hostile
/*if (targeter.targetClanID == zoxID.clanID)
{
    if (targetFound.x != 0)
    {
        //short newTargetID = (short)targetFound.x;
        int newTargetIndex = GetCharacterIndex(targetFound);
        int newClanID = characters[newTargetIndex].clanID;
        if (newClanID != zoxID.clanID)
        {
            SetNewTarget(ref targeter, targetFound);
        }
    }
}*/

//int listIndex = //GetListIndex(targeter.targetID);
// if removed from list, then stop targeting
/*if (indexes.ContainsKey(targeter.targetID) == false)
{
    targeter.targetID = 0;
    targeter.lastSeeked = time - targeter.Value.seekCooldown;
}
else
{
    MaintainTarget(ref targeter, position.Value, rotation.Value);
}*/
//}
//float3 positionValue = position.Value;
/*if (targetID != 0)
{
    return new InterFloat(targetID, closestDistance);
}
else
{
    return new InterFloat(0, 0);
}*/

/*private void SetNewTarget(ref Targeter targeter, InterFloat targetFound)
{
    if (targetFound.x != 0)
    {
        //short newTargetIndex = (short)targetFound.x;
        int newTargetIndex = GetCharacterIndex(targetFound);
        targeter.targetID = characters[newTargetIndex].id;
        targeter.targetPosition = characters[newTargetIndex].position;
        targeter.targetClanID = characters[newTargetIndex].clanID;
        targeter.targetDistance = targetFound.y;
    }
}

private int GetCharacterIndex(InterFloat targetFound)
{
    return indexes[((int)targetFound.x)];
}*/

/*private void MaintainTarget(ref Targeter targeter, float3 position, quaternion rotation)
{
    // if entity died, then no longer target it!
    int targetIndex = indexes[targeter.targetID];
    float3 targetPosition = characters[targetIndex].position;
    float distance = math.distance(position, targetPosition);
    if (distance > targeter.Value.maintainRange)
    {
        targeter.targetID = 0;
        // should flee after this if they are getting attacked..!
    }
    else
    {
        //targetPosition.y = position.Value.y;
        targeter.targetPosition = targetPosition;
        targeter.targetDistance = math.distance(position, targetPosition);
        targeter.targetAngle = quaternion.LookRotationSafe(math.normalizesafe(targetPosition - position), math.up());
        targeter.currentAngle = rotation;
    }
}*/

/*public struct CharacterInput
{
    public float3 position;
    public int id;
    public int clanID;
}


public struct InterFloat
{
    public int x;
    public float y;

    public InterFloat(int newX, float newY)
    {
        x = newX; y = newY;
    }
}*/

/*NativeArray<ZoxID> ids = characterQuery.ToComponentDataArray<ZoxID>(Allocator.TempJob);
NativeArray<Translation> translations = characterQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
NativeArray<CharacterInput> characters = new NativeArray<CharacterInput>(ids.Length, Allocator.TempJob);
NativeHashMap<int, int> indexes = new NativeHashMap<int, int>(ids.Length, Allocator.TempJob);*/

//int i = 0;
/*oreach (ZoxID id in ids)
 {
     CharacterInput character = new CharacterInput
     {
         id = id.id,
         clanID = id.clanID,
         position = translations[i].Value
     };
     characters[i] = character;
     indexes.Add(id.id, i);
     //Debug.LogError("Added: " + (id.id) + " to hashmap.");
     i++;
 }*/
//if (debugCharacterPositions)
//{
// Debug.DrawLine(characters[i].position, characters[i].position + new float3(0, 1, 0), Color.red);
//}