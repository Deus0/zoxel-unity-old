using Unity.Entities;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.Voxels;

namespace Zoxel
{
    /// <summary>
    /// Needs to take in meta data for a character before spawning
    /// </summary>
    [DisableAutoCreation]
    public class WaveModeSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;
        public WorldSpawnSystem worldSpawnSystem;
        // Data
        public static List<WaveDatam> waveData = new List<WaveDatam>();
        // waves
        public byte enabled;
        private int index;
        private float spawnLast;
        public float timeUntilNextWave;
        public int selectedWaveData = 0;
        public List<int> spawnedIDs = new List<int>();
        public byte hasWavesEnded;
        private int wavesClanID;

        protected override void OnStartRunning()
        {
            Debug.Log("Not yet fixed WaveModeSystem.");
            wavesClanID = Bootstrap.GenerateUniqueID();
            base.OnStartRunning();
            BeginGameMode();
        }

        public void BeginGameMode()
        {
            hasWavesEnded = 0;
            spawnLast = UnityEngine.Time.time;
            enabled = 1;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            Clear();
        }

        public void Clear()
        {
            index = 0;
            enabled = 0;
            hasWavesEnded = 0;
            spawnedIDs.Clear();
        }

        protected override void OnUpdate()
        {
            if (waveData.Count == 0)
            {
                return;
            }
            if (enabled == 1)
            {
                if (selectedWaveData < 0 && selectedWaveData >= waveData.Count)
                {
                    Debug.LogError("Selected wave data is null.");
                    return;
                }
                if (waveData[selectedWaveData] == null)
                {
                    Debug.LogError("Wave data is null.");
                    return;
                }
                if (index >= 0 && index < waveData[selectedWaveData].Values.Count)
                {
                    WaveData data = waveData[selectedWaveData].Values[index];
                    float time =UnityEngine.Time.time;
                    timeUntilNextWave = data.spawnCooldown - (time - spawnLast);
                    if (time - spawnLast >= data.spawnCooldown)
                    {
                        spawnLast = time;
                        int worldID = 0;
                        /*if (worldSpawnSystem != null)
                        {
                            worldID = worldSpawnSystem.GetFirstWorldID();
                        }*/
                        spawnedIDs.AddRange(CharacterSpawnSystem.SpawnNPCs(
                                World.EntityManager, worldID, data.spawnedOne.Value.id, wavesClanID, data.spawnPosition, data.spawnAmount));
                        index++;
                    }
                }
                else
                { 
                    hasWavesEnded = 1;
                }
                // move this check to an end game system
                // CheckForNoMoreHorde();
            }
        }
    }
}