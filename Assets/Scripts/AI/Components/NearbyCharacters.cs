using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Zoxel
{
    public struct NearbyCharacter {
        public Entity character;
        public int clan;
        public float distance;
        public float3 position;
    }

    public struct NearbyCharacters : IComponentData
    {
        public float lastUpdatedTime;
        public BlitableArray<NearbyCharacter> characters;

        /// <summary>
        /// call before Destroying Character Entity
        /// </summary>
        public void Dispose()
        {
            if (characters.Length > 0)
            {
                characters.Dispose();
            }
        }

        public void SetData(List<Entity> newCharacters, List<int> newClans, List<float> newDistances, List<float3> newPositions)
        {
            Dispose();
            characters = new BlitableArray<NearbyCharacter>(newCharacters.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = new NearbyCharacter
                {
                    character = newCharacters[i],
                    clan = newClans[i],
                    distance = newDistances[i],
                    position = newPositions[i]
                };
            }
            //UnityEngine.Debug.LogError("Characters found: " + characters.Length);
        }
        
    }
}