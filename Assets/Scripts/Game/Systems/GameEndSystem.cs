using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Zoxel
{
    [System.Serializable]
    public enum GameType
    {
        TowerDefence,   // Spawn towers to defeat waves of enemy
        Adventure,       // top down view, rotate character around, hit things!
        God,            // fly everywhere, use tools to edit levels, switch level easily, can't die
        Survival        // enemies spawn infinitely in waves (like TD), but have to use a character to survive, spawns a shop and a heart tower to defend
    }

    [System.Serializable]
    public enum EndGameReason
    {
        None,
        Victory,
        Defeat,
        Desertion
    }

    [DisableAutoCreation]
    public class GameEndSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;
        //private float lastCheckedEndOfGame;
        //private float lastCheckedWonGame;
        public EndGameReason endGameResult;
        //private bool isCheckingPlayer;

        // Game Modes
        public WaveModeSystem waveSystem;

        protected override void OnUpdate()
        {
            // check for end game
            Entities.WithAll<Game>().ForEach((Entity e, ref Game game) =>
            {
                if (game.state == ((byte)GameState.InGame) && 
                    UnityEngine.Time.time - game.lastCheckedEndOfGame >= 1)
                {
                    game.lastCheckedEndOfGame = UnityEngine.Time.time;
                    bool anyPlayersLeft = false;
                    for (int i = 0; i < game.spawnedPlayerIDs.Length; i++)
                    {
                        if (IsPlayerAlive(game.spawnedPlayerIDs[i]))
                        {
                            anyPlayersLeft = true;
                            break;
                        }
                    }
                    if (!anyPlayersLeft)
                    {
                        //EndGame(EndGameReason.Defeat);
                        game.newState = ((byte)GameState.RespawnScreen);
                    }
                }
                //CheckForNoMoreHorde();
                /*Entities.WithAll<Controller, ZoxID>().ForEach((Entity e, ref Controller controller, ref ZoxID zoxID) =>
                {
                    CheckForDeadPlayer(zoxID.id);
                });*/
            });
        }

        /*public void EndGame(EndGameReason reason)
        {
            Bootstrap.instance.EndGame(reason);
            endGameResult = reason;
            Debug.Log("Ending Game for: " + endGameResult.ToString());
            // turn on different UI for different reasons
        }*/

        // basic mode just starts with players
        bool IsPlayerAlive(int characterID)
        {
            return characterSpawnSystem.characters.ContainsKey(characterID);
        }

        private void CheckForNoMoreHorde()
        {
            /*if (Bootstrap.instance.debugDisableEndGames)
            {
                return;
            }*/
            /*if (waveSystem.hasWavesEnded == 1 &&UnityEngine.Time.time - lastCheckedWonGame >= 5f)
            {
                lastCheckedWonGame =UnityEngine.Time.time;
                // check if game over every 3 seconds or so
                for (int i = waveSystem.spawnedIDs.Count - 1; i >= 0; i--)
                {
                    if (!characterSpawnSystem.characters.ContainsKey(waveSystem.spawnedIDs[i]))
                    {
                        // remove when they die
                        waveSystem.spawnedIDs.RemoveAt(i);
                    }
                }
                if (waveSystem.spawnedIDs.Count == 0)
                {
                    EndGame(EndGameReason.Victory); // finished playing, show score!
                }
            }*/
        }
    }
}