using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Zoxel
{
    [System.Serializable]
    public struct Game : IComponentData
    {
        public int id;
        public int metaID;
        public byte state;  // current state of game
        public byte newState;  // current state of game
        public byte previousState;  // current state of game
        public float timeChanged;
        public Entity map;   // current map loaded
        public BlitableArray<int> spawnedPlayerIDs; // current loaded player characters
        public float lastCheckedEndOfGame;

        public void AddPlayerCharacter(int characterID)
        {
            int[] previousPlayers = spawnedPlayerIDs.ToArray();
            if (spawnedPlayerIDs.Length > 0)
            {
                spawnedPlayerIDs.Dispose();
            }
            spawnedPlayerIDs = new BlitableArray<int>(previousPlayers.Length + 1, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < previousPlayers.Length; i++)
            {
                spawnedPlayerIDs[i] = previousPlayers[i];
            }
            spawnedPlayerIDs[spawnedPlayerIDs.Length - 1] = characterID;

        }
    }
}