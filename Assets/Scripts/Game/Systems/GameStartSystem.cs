using Unity.Entities;
using System.Collections.Generic;
using Unity.Collections;
using Zoxel.Voxels;
using UnityEngine;
using Zoxel.UI;

namespace Zoxel
{

    [DisableAutoCreation]
    public class GameStartSystem : ComponentSystem
    {
        public Dictionary<int, GameDatam> meta = new Dictionary<int, GameDatam>();
        public WaveModeSystem waveSystem;
        public SystemsManager boss;
        public MapDatam mainMenuMap;
        public MapDatam startMap;
        public CameraDatam startCamera;
        public CharacterDatam startCharacter;
        //public System.Action<GameState> OnStateChanged;
        public Dictionary<int, Entity> games = new Dictionary<int, Entity>();
        public PlayerSpawnSystem playerSpawnSystem;
        public System.Action OnStarted;
        //public ClassDatam newCharacterClass;
        // connected
        public CharacterSpawnSystem characterSpawnSystem;
        public CameraSystem cameraSystem;
        public WorldSpawnSystem worldSpawnSystem;
        public MenuSpawnSystem menuSpawnSystem;
        public GameUISystem gameUISystem;
        public PlayerInputSystem playerControllerSystem;
        public CameraFirstPersonSystem cameraFirstPersonSystem;
        public PlayerSkillsSystem playerSkillsSystem;
        // disabled game state
        public MovementSystemGroup movementSystemGroup;
        public GameEndSystem gameEndSystem;

        public Entity CreateGame(GameDatam data)
        {
            if (meta.ContainsKey(data.id) == false)
            {
                Debug.Log("Game not part of meta [" + data.name + "]");
                return new Entity();
                //meta.Add(data.id, data);
            }
            Entity game = World.EntityManager.CreateEntity();
            int id = Bootstrap.GenerateUniqueID();
            World.EntityManager.AddComponentData(game, new Game
            {
                id = id,
                metaID = data.id
            });
            games.Add(id, game);
            Debug.Log("Created new game [" + data.name + "] as [" + id + "]");
            return game;
        }

        private void OnStateChanged(int worldID, int metaID, GameState oldState, GameState newState)
        {
            Debug.Log("New Game state: " + newState);
            if (oldState == GameState.StartScreen && newState == GameState.MainMenu)
            {
                OnStarted.Invoke();
            }
            if (newState == GameState.MainMenu)
            {
                OnStarted.Invoke();
                //Debug.LogError("Game starting with: " + playerSpawnSystem.controllers.Count + " Players.");
                foreach (KeyValuePair<int, Entity> KVP in playerSpawnSystem.controllers)
                {
                    if (World.EntityManager.Exists(KVP.Value))
                    {
                        CameraSystem.SpawnCameraController(World.EntityManager, meta[metaID].startingCamera.Value,
                            EntityManager.GetComponentData<Controller>(KVP.Value), KVP.Key);
                    }
                }
            }


            //int cameraID = cameraSystem.SpawnCamera(meta[metaID].startingCamera.Value);
            //Entity camera = cameraSystem.cameras[cameraID];    // systemsManager.cameraSystem.cam
            //EntityManager.AddComponentData(camera, EntityManager.GetComponentData<Controller>(KVP.Value));
            //newEntites.Add(KVP.Key, camera);
            /*try
            {
                worldSpawnSystem.OnAddedStreamer(camera, worldID);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }*/
            /*else if (newState == GameState.NewGameScreen)
            {
                Debug.LogError("Spawn New Game UI for players (they should both be able to edit new game data)");
                //mainScreen.DeinitializeIt();
                // spawn a character
                // remove its controller
                // move camera to new character
                //newGameScreen.InitializeIt();
                // edit colours of character model
                // spawn a character to edit
            }*/
            else if (newState == GameState.PauseScreen)
            {
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    //Controller controller = EntityManager.GetComponentData<Controller>(entity);
                    MenuSpawnSystem.SpawnUI(World.EntityManager, entity, "PauseMenu");
                    //systemsManager.menuSpawnSystem.QueueSpawnUI(entity, "PauseMenu");
                }
            }
            else if (newState == GameState.GameUI)
            {
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    Controller controller = EntityManager.GetComponentData<Controller>(entity);
                    gameUISystem.AddGameUI(entity, controller.gameUIIndex);  // spawn the game ui
                }
            }
            else if (newState == GameState.InGame)
            {
                menuSpawnSystem.Clear();
                gameUISystem.Clear();
                //pauseScreen.DeinitializeIt();
            }
            else if (newState == GameState.RespawnScreen)
            {
                //Debug.LogError("TODO: Spawn Player Respawn UI");
                // detatch camera from character
                // add in dead post processing effects
                //systemsManager.menuSpawnSystem.QueueSpawnUI(entity, "Respawn");
                //endOfGameScreen.InitializeIt();
            }
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Game>().ForEach((Entity e, ref Game game) =>
            {
                if (game.newState != game.state)
                {
                    if (game.state == ((byte)GameState.PauseScreen)
                    || game.state == ((byte)GameState.GameUI))
                    {
                        UnityEngine.Time.timeScale = 1f;
                        playerControllerSystem.Enabled = true;
                        cameraFirstPersonSystem.Enabled = true;
                        playerSkillsSystem.Enabled = true;
                    }
                    else if (game.state == ((byte)GameState.LoadCharacter))
                    {
                        movementSystemGroup.Enabled = true;
                        gameEndSystem.Enabled = true;
                    }
                    GameState previousState = (GameState)game.state;
                    game.state = game.newState;
                    game.previousState = (byte)previousState;
                    game.timeChanged = UnityEngine.Time.time;
                    if (game.state == ((byte)GameState.PauseScreen)
                    || game.state == ((byte)GameState.GameUI))
                    {
                        UnityEngine.Time.timeScale = 0.2f;
                        // instead disable players controls
                        playerControllerSystem.Enabled = false;
                        cameraFirstPersonSystem.Enabled = false;
                        playerSkillsSystem.Enabled = false;
                    }
                    else if (game.state == ((byte)GameState.LoadCharacter))
                    {
                        movementSystemGroup.Enabled = false;
                        gameEndSystem.Enabled = false;
                        game.newState = ((byte)GameState.InGame);
                    }
                    OnStateChanged(game.mapID, game.metaID, previousState, (GameState)game.state);
                    //OnStateChanged.Invoke((GameState)game.state);
                }

                if (game.state == ((byte)GameState.LoadingStartScreen))
                {
                    game.newState = ((byte)GameState.StartScreen);
                    if (mainMenuMap != null)
                    {
                        game.mapID = worldSpawnSystem.SpawnMap(mainMenuMap);
                        LightManager.instance.SetLight("MainMenuSun");
                    }
                }
                else if (game.state == ((byte)GameState.LoadCharacter))
                {
                    if (UnityEngine.Time.time - game.timeChanged >= 2)
                    {
                        game.newState = game.previousState;
                    }
                }
                else if (game.state == ((byte)GameState.StartScreen))
                {
                    // wait for any new thing pressed
                    // set first playerDeviceID

                    // set UI to start UI
                    if (playerSpawnSystem.controllers.Count > 0)
                    {
                        Debug.Log("Setting new game state to Main Menu");
                        game.newState = ((byte)GameState.MainMenu);
                    }
                }
                else if (game.state == ((byte)GameState.MainMenu))
                {
                    // main menu
                    // new players can join now
                    // add in UI icons for players at bottom right
                }
                else if (game.state == ((byte)GameState.RespawnScreen))
                {
                    game.newState = ((byte)GameState.ClearingGame);
                }

                else if (game.state == ((byte)GameState.InGame))
                {

                }
                else if (game.state == ((byte)GameState.PauseScreen))
                {

                }
                else if (game.state == ((byte)GameState.ClearingGame))
                {
                    ClearGame(game.mapID);
                    game.mapID = 0;
                    game.newState = ((byte)GameState.LoadingStartScreen);
                }

                // ===== New Game =====

                // ===== Save Game =====
                else if (game.state == ((byte)GameState.SaveGamesScreen))
                {
                    // select of a level
                    // for now chose betewen grass lands, and dessert
                }
            });
        }

        private void ClearGame(int mapID)
        {
            UnityEngine.Time.timeScale = 1;
            //systemsManager.ClearGame();
            worldSpawnSystem.DestroyWorld(mapID);
            //worldSpawnSystem.Clear();       // should clear chunks of the spawned units
            characterSpawnSystem.Clear();   // just need to clear player characters though
            cameraSystem.Clear();      // need to remove all cameras from game
            boss.ClearGame();
            // items and bullets will despawn over time
        }
    }
}


/*private void LoadWorld(ref Game game, MapDatam map)
{
    worldSpawnSystem.DestroyWorld(game.mapID);
    LightManager.instance.SetLight("GameSun");
    game.mapID = worldSpawnSystem.SpawnMap(map);
    game.timeChanged = UnityEngine.Time.time;
}*/

// new characters
/*private void SpawnNewPlayerCharacter(ref Game game)
{
    int characterMetaID = startCharacter.Value.id;
    game.spawnedPlayerIDs = new BlitableArray<int>(CameraSystem.cameras.Count, Allocator.Persistent);
    int i = 0;
    foreach (KeyValuePair<int, Entity> KVP in CameraSystem.cameras)
    {
        int characterID = characterSpawnSystem.SpawnPlayer(characterMetaID, startMap.newPlayerPosition, Bootstrap.instance.playerClanID, game.mapID);
        cameraSystem.ConnectCameraToCharacter(KVP.Key, characterID);
        Entity characterEntity = characterSpawnSystem.characters[characterID];
        Skills skills = World.EntityManager.GetComponentData<Skills>(characterEntity);
        skills.SetSkillsWithMeta(newCharacterClass);
        World.EntityManager.SetComponentData(characterEntity, skills);
        CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(characterEntity);
        cameraLink.cameraID = KVP.Key;
        World.EntityManager.SetComponentData(characterEntity, cameraLink);
        Controller controller = World.EntityManager.GetComponentData<Controller>(characterEntity);
        playerSpawnSystem.SetControllerCharacter(characterEntity, i);
        game.spawnedPlayerIDs[i] = characterID;
        i++;
    }
}*/
/*else if (game.state == ((byte)GameState.LoadSaveWorld))
{
    // get player last in world
    LoadWorld(ref game, startMap);
    game.newState = ((byte)GameState.LoadingSaveWorld);
}*/

/*else if (game.state == ((byte)GameState.LoadingSaveWorld))
{
    if (UnityEngine.Time.time - game.timeChanged >= 0.1f)
    {
        game.newState = ((byte)GameState.SpawnSaveCharacters);
        //OnStateChanged.Invoke((GameState)game.state);
    }
}*/

/*else if (game.state == ((byte)GameState.SpawnSaveCharacters))
{
    SpawnPlayers(ref game, true);
    game.newState = ((byte)GameState.InGame);
}*/

/*else if (game.state == ((byte)GameState.NewGameScreen))
{
    // select of a level
    // for now chose betewen grass lands, and dessert
}

else if (game.state == ((byte)GameState.LoadNewWorld))
{
    LoadWorld(ref game, startMap);
    game.newState = ((byte)GameState.LoadingNewWorld);
}

else if (game.state == ((byte)GameState.LoadingNewWorld))
{
    if (UnityEngine.Time.time - game.timeChanged >= 0.1f)
    {
        game.newState = ((byte)GameState.SpawnNewCharacters);
    }
}

else if (game.state == ((byte)GameState.SpawnNewCharacters))
{
    //SpawnPlayers(ref game);
    Bootstrap.instance.SpawnPlayerCharacter(
        0,
        CameraSystem.GetMainCameraIndex(),
        startCharacter.Value.id, 
        0,
        game.mapID,
        false);
    game.newState = ((byte)GameState.SpawningCharacters);
}*/
