using Unity.Entities;
using UnityEngine;

namespace Zoxel.WorldGeneration
{
    public class MonsterSpawnZoneComponent : ComponentDataProxy<MonsterSpawnZone> { }

    public struct MonsterBiome
    {
        public int monsterMetaID;
        public int spawnAmount;
        public int spawnMax;
        public float spawnCooldown;
    }

    [System.Serializable]
    public struct MonsterSpawnZone : IComponentData
    {
        [Header("Data")]
        public int spawnAmount;
        public float spawnCooldown; // calculalted based on average
        public BlitableArray<MonsterBiome> spawnDatas;
        [Header("Instanced")]
        public float lastTimeSpawned;
        public BlitableArray<int> spawnedIDs;
        public int clanID;

        public void Dispose()
        {
            spawnedIDs.Dispose();
            spawnDatas.Dispose();
        }
        
        public void CalculateValues()
        {
            spawnCooldown = 0;
            for (int i = 0; i < spawnDatas.Length; i++)
            {
                spawnCooldown += spawnDatas[i].spawnCooldown;
            }
            if (spawnDatas.Length != 0)
            {
                spawnCooldown /= spawnDatas.Length;
            }
            spawnAmount = 0;
            for (int i = 0; i < spawnDatas.Length; i++)
            {
                spawnAmount += spawnDatas[i].spawnAmount;
                ///Debug.Log("Spawn Data ["+ i + "] amount is: " + spawnDatas[i].spawnAmount);
            }
            if (spawnDatas.Length != 0)
            {
                spawnAmount /= spawnDatas.Length;
            }
            //Debug.Log("Set spawnAmount to " + spawnAmount + " with data length of: " + spawnDatas.Length);
        }
    }
}