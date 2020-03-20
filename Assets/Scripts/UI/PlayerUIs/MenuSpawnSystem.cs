using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Transforms;

namespace Zoxel
{
    public struct MenuUI : IComponentData
    {
        public byte type;
    }

    [DisableAutoCreation]
    public class MenuSpawnSystem : PlayerUISpawnSystem
    {
        public SaveSystem saveSystem; // save system later on

        public override void OnClickedButton(Entity player, Entity ui, int arrayIndex)
        {
            /*if (uis.ContainsKey(characterID) == false)
            {
                Debug.LogError("Navigation button pressed when menu is gone.");
                return;
            }*/
            if (World.EntityManager.HasComponent<MenuUI>(ui) == false)
            {
                Debug.LogError("(Menu?) Doesn't have menu component.");
                return;
            }
            //Debug.LogError("Clicking MenuButton: " + arrayIndex);
            MenuUI menuUI = World.EntityManager.GetComponentData<MenuUI>(ui);
            byte menuType = menuUI.type;
            // check the type of UI is on the main panel before!
            if (menuType == 0)
            {
                if (arrayIndex == 0)
                {
                    OnClickedPlayButton();
                }
                else
                {
                    OnClickedExitButton();
                }
            }
            else if (menuType == 1)
            {
                if (arrayIndex == 0)
                {
                    OnClickedResumeGameButton();
                }
                else
                {
                    OnClickedExitGameButton();
                }
            }
            else if (menuType == 2)
            {
                OnClickedLoadGameButton(arrayIndex);
            }
            else if (menuType == 3)
            {
                OnClickedLoadCharacterButton(player, arrayIndex);
            }
            else if (menuType == 4)
            {
                OnClickedClassChoiceButton(player, arrayIndex);
            }
        }

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData) //, float2 panelSize)
        {
            SpawnMenuUI spawnMenuData = (SpawnMenuUI)spawnData;
            List<Entity> buttons = new List<Entity>();
            // main menu
            if (spawnMenuData.spawnType == 0)
            {
                buttons.Add(SpawnMenuButton(panelUI, "play"));
                buttons.Add(SpawnMenuButton(panelUI, "exit"));
            }
            // pause ui
            if (spawnMenuData.spawnType == 1)
            {
                buttons.Add(SpawnMenuButton(panelUI, "return"));
                buttons.Add(SpawnMenuButton(panelUI, "exit"));
            }
            // load games
            if (spawnMenuData.spawnType == 2)
            {
                var saveSlots = saveSystem.GetSaveSlots();
                buttons.Add(SpawnMenuButton(panelUI, "back"));
                foreach (var saveSlot in saveSlots)
                {
                    buttons.Add(SpawnMenuButton(panelUI, saveSlot));
                }
                buttons.Add(SpawnMenuButton(panelUI, "new game"));
            }
            // load characters
            if (spawnMenuData.spawnType == 3)
            {
                var saveSlots = saveSystem.GetPlayerSlots();
                buttons.Add(SpawnMenuButton(panelUI, "back"));
                foreach (var saveSlot in saveSlots)
                {
                    buttons.Add(SpawnMenuButton(panelUI, saveSlot));
                }
                buttons.Add(SpawnMenuButton(panelUI, "new character"));
            }
            // class choices
            if (spawnMenuData.spawnType == 4)
            {
                List<ClassDatam> classes = Bootstrap.instance.data.classes;
                buttons.Add(SpawnMenuButton(panelUI, "back"));
                foreach (var classer in classes)
                {
                    buttons.Add(SpawnMenuButton(panelUI, classer.name.ToLower()));
                }
            }
            /*for (int i = 0; i < spawnMenuData.rowsCount; i++)
            {
                string text = "";
                if (spawnMenuData.spawnType == 0)
                {
                    if (i == 0)
                    {
                        text = "play";
                    }
                    else
                    {
                        text = "exit";
                    }
                }
                else if (spawnMenuData.spawnType == 1)
                {
                    if (i == 0)
                    {
                        text = "return";
                    }
                    else
                    {
                        text = "exit";
                    }
                }
                else if (spawnMenuData.spawnType == 2)
                {
                    var saveSlots = saveSystem.GetSaveSlots();
                    if (i == 0)
                    {
                        text = "back";
                    }
                    else if (i < saveSlots.Length + 1)
                    {
                        text = saveSlots[i - 1];
                    }
                    else
                    {
                        text = "new game";
                    }
                }
                else if (spawnMenuData.spawnType == 3)
                {
                    var saveSlots = saveSystem.GetPlayerSlots();
                    if (i == 0)
                    {
                        text = "back";
                    }
                    else if (i < saveSlots.Length + 1)
                    {
                        text = saveSlots[i - 1];
                    }
                    else
                    {
                        text = "new";
                    }
                }
                // classes
                else if (spawnMenuData.spawnType == 4)
                {
                    List<ClassDatam> classes = Bootstrap.instance.data.classes;
                    if (i == 0)
                    {
                        text = "back";
                    }
                    else if (i < classes.Count + 1)
                    {
                        text = classes[i - 1].name;
                    }
                }
                // first spawn subpanel
                ///float3 position = GetVerticalListPosition(i, spawnMenuData.rowsCount);
                //demButtons.entities[i] = subpanel;
                buttons.Add(SpawnMenuButton(text));
            }*/
            //icons.Add(zoxID.id, demButtons);
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(buttons.Count, Allocator.Persistent);
            for (int i = 0; i < buttons.Count; i++)
            {
                children.children[i] = buttons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            float2 buttonSize = new float2(0.06f * 7, 0.06f);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                updated = 1,
                gridSize = new float2(1, buttons.Count),
                iconSize = buttonSize,
                margins = new float2(0.01f, 0.03f),
                padding = new float2(0.003f, 0.003f),
            });

            World.EntityManager.AddComponentData(panelUI, new MenuUI { type = spawnMenuData.spawnType });
            // problem no controller on camera - its a seperate entity from controllerStateSsytem 
            //      - should spawn controllers on camera instead of there
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
            byte uiIndex = ((byte)((int)PlayerUIType.Menu));//((byte)((int)PlayerUIType.MainMenu + spawnMenuData.spawnType));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID.id,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
        }

        private Entity SpawnMenuButton(Entity panelUI, string text)
        {
            float2 iconSize = uiDatam.defaultIconSize;
            Entity button = UIUtilities.SpawnButton(
                    World.EntityManager, 
                    panelUI, 
                    new float3(0,0,-0.01f),
                    new float2(iconSize.x * text.Length * 0.6f, iconSize.y),
                    null, uiDatam.menuButton);
            RenderText renderText = new RenderText { };
            renderText.fontSize = 0.03f;
            renderText.SetText(text);
            World.EntityManager.AddComponentData(button, renderText);
            RenderTextSystem.SetLetterColor(World.EntityManager, button, Color.green);
            return button;
        }

        public override void OnSelectedButton(int characterID, int uiIndex, int arrayIndex)
        {
            //PlayerUIType uiType = (PlayerUIType)uiIndex;
            //Entity character = Bootstrap.instance.systemsManager.characterSpawnSystem.characters[characterID];
            // kinda want to know what UI is open?
            /*if (uiType == PlayerUIType.MainMenu)
            {
                if (arrayIndex == 0)
                {
                    //SetTooltipText(characterID, "enter the world");
                }
                else
                {
                    //SetTooltipText(characterID, "leave zoxel");
                }
            }
            else if (uiType == PlayerUIType.PauseMenu)
            {
                if (arrayIndex == 0)
                {
                    //SetTooltipText(characterID, "keep playing");
                }
                else
                {
                    //SetTooltipText(characterID, "quit playing");
                }
            }
            else if (uiType == PlayerUIType.LoadGameMenu)
            {
                if (arrayIndex == 0)
                {
                    //SetTooltipText(characterID, "return to last menu");
                }
                else if (saveSystem.GetSaveSlots().Length + 1 == arrayIndex)
                {
                    //SetTooltipText(characterID, "start a new game");
                }
                else
                {
                    //SetTooltipText(characterID, "play this game");
                }
            }
            else if (uiType == PlayerUIType.LoadPlayerMenu)
            {
                if (arrayIndex == 0)
                {
                   // //SetTooltipText(characterID, "return to last menu");
                }
                else if (saveSystem.GetPlayerSlots().Length + 1 == arrayIndex)
                {
                    ////SetTooltipText(characterID, "create a new character");
                }
                else
                {
                  ///  //SetTooltipText(characterID, "play as this character");
                }
            }
            else if (uiType == PlayerUIType.ClassChoiceMenu)
            {
                if (arrayIndex == 0)
                {
                   // //SetTooltipText(characterID, "return to last menu");
                }
                else
                {
                    //ClassDatam classer = Bootstrap.instance.data.classes[arrayIndex - 1];
                    ////SetTooltipText(characterID, classer.description);
                }
            }*/
        }


        #region Spawning-Removing
        public static void SpawnUI(EntityManager EntityManager, Entity character, string spawnType) //, int rowsCount)
        {
            if (spawnType == "MainMenu")
            {
                SpawnUI(EntityManager, character, 0);//, 2);
            }
            else if (spawnType == "PauseMenu")
            {
                SpawnUI(EntityManager, character, 1);//, 2);
            }
            else if (spawnType == "LoadGame")
            {
                SpawnUI(EntityManager, character, 2);//, Mathf.Min(4, saveSystem.GetSaveSlots().Length + 2));
            }
            else if (spawnType == "LoadCharacter")
            {
                SpawnUI(EntityManager, character, 3);//, Mathf.Min(4, saveSystem.GetPlayerSlots().Length + 2));
            }
            else if (spawnType == "ClassChoice")
            {
                SpawnUI(EntityManager, character, 4);//, Bootstrap.instance.data.classes.Count + 1);
            }
        }

        public struct SpawnMenuUI : IComponentData
        {
            public Entity controller;
            public byte spawnType;
            //public byte rowsCount;
        }

        public struct RemoveMenuUI : IComponentData
        {
            public Entity controller;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnMenuUI>().ForEach((Entity e, ref SpawnMenuUI command) =>
            {
                //float2 panelSize = GetVerticalListPanelSize(command.rowsCount);
                SpawnUI(command.controller, command); //, panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveMenuUI>().ForEach((Entity e, ref RemoveMenuUI command) =>
            {
                RemoveUI(command.controller);
                World.EntityManager.DestroyEntity(e);
            });
        }

        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveMenuUI { controller = character });
        }

        private static void SpawnUI(EntityManager EntityManager, Entity character, byte spawnType) // int rowsCount)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnMenuUI
            {
                controller = character,
                spawnType = spawnType//,
                //rowsCount = (byte)rowsCount
            });
        }
        #endregion

        #region FromBooty
        public List<ClassDatam> classes;
        public Voxels.WorldSpawnSystem worldSpawnSystem;
        public Voxels.ChunkSpawnSystem chunkSpawnSystem;
        public MapDatam startingMap;
        public CharacterDatam startingCharacter;
        public PlayerSpawnSystem playerSpawnSystem;
        public CameraSystem cameraSystem;
        public Entity game;

        public void OnClickedLoadGameButton(int arrayIndex)
        {
            if (arrayIndex == 0)
            {
                //Debug.LogError("Returning to main menu.");
                Clear();
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    SpawnUI(EntityManager, entity, "MainMenu");
                }
            }
            else if (arrayIndex < saveSystem.GetSaveSlots().Length + 1)
            {
                string loadName = saveSystem.GetSaveSlots()[arrayIndex - 1];
                //Debug.LogError("Loading Game: " + loadName);
                // clear menu
                // open player menu
                Clear();
                saveSystem.saveGameName = loadName;
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    SpawnUI(EntityManager, entity, "LoadCharacter");
                }
            }
            else if (saveSystem.GetSaveSlots().Length + 1 == arrayIndex)
            {
                // start new game
                ///Debug.LogError("Starting New Game.");
                // gamestartsystem
                NewGame();
            }
            else {
                Debug.LogError("ArrayIndex Clicked: " + arrayIndex);
            }
        }

        // should take in playerID as well so only player spawns a character
        public void OnClickedLoadCharacterButton(Entity camera, int arrayIndex)
        {
            if (arrayIndex == 0)
            {
                //Debug.LogError("Returning to LoadGame.");
                Clear();
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    SpawnUI(EntityManager, entity, "LoadGame");
                }
            }
            else if (saveSystem.GetPlayerSlots().Length + 1 == arrayIndex)
            {
                // start new game
                //string characterName = saveManager.GetPlayerSlots()[arrayIndex - 1];
                //Debug.LogError("Starting New Character!");
                // spawn a character randomly in main menu map
                // zoom into it with camera
                Clear();
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    // spawn new character
                    // camera to zoom next to character
                    SpawnUI(EntityManager, entity, "ClassChoice");
                }
            }
            else if (arrayIndex < saveSystem.GetSaveSlots().Length + 1)
            {
                var playerToLoad = int.Parse(saveSystem.GetPlayerSlots()[arrayIndex - 1]);
                //Debug.LogError("Loading Character: " + playerToLoad);
                // clear menu
                // open player menu
                Clear();
                // game start
                LoadPlayer(camera, playerToLoad, 0);
            }
        }

        public void OnClickedClassChoiceButton(Entity camera, int arrayIndex)
        {
            if (arrayIndex == 0)
            {
                //Debug.LogError("Returning to LoadGame.");
                Clear();
                foreach (Entity entity in playerSpawnSystem.controllers.Values)
                {
                    MenuSpawnSystem.SpawnUI(EntityManager, entity, "LoadCharacter");
                }
            }
            else if (arrayIndex < classes.Count + 1)
            {
                Clear();
                //Debug.LogError("Loading Class [" + data.classes[arrayIndex - 1].name + "].");
                // load race choice now or whatever
                // start game as that character
                //systemsManager.gameStartSystem.newCharacterClass = data.classes[arrayIndex - 1];
                //SetGameState(GameState.LoadNewWorld);
                //int cameraID = characterID; // World.EntityManager.GetComponentData<ZoxID>(CameraSystem.cameras[characterID]).id;
                LoadPlayer(camera, 0, classes[arrayIndex - 1].Value.id);
            }
        }

        public void OnClickedExitGameButton()
        {
            saveSystem.SaveGame();
            Clear();
            SetGameState(GameState.ClearingGame);
        }
        public void OnClickedResumeGameButton()
        {
            SetGameState(GameState.InGame);
        }
        public void OnClickedPlayButton()
        {
            Debug.Log("Loading Game.");
            Clear();
            foreach (Entity entity in playerSpawnSystem.controllers.Values)
            {
                MenuSpawnSystem.SpawnUI(EntityManager, entity, "LoadGame");
            }
        }

        public void OnClickedExitButton()
        {
            Debug.Log("Exiting Game.");
            Application.Quit();
        }


        private void NewGame()
        {
            // new game UI? with sliders for terrain etc
            // includes generation and use of items/blocks/biomes etc

            saveSystem.CreateNewSaveGame("Game" + saveSystem.GetSaveSlots().Length);
            Clear();
            foreach (Entity entity in playerSpawnSystem.controllers.Values)
            {
                // spawn new character
                // camera to zoom next to character
                // map generation UI
                // game generation UI
                SpawnUI(EntityManager, entity, "LoadCharacter");
            }
        }

        private void LoadPlayer(Entity camera, int characterID, int classID)
        {
            Game gameComponent = EntityManager.GetComponentData<Game>(game);
            worldSpawnSystem.RemoveWorld(gameComponent.mapID);
            LightManager.instance.SetLight("GameSun");
            gameComponent.mapID = worldSpawnSystem.SpawnMap(startingMap);
            EntityManager.SetComponentData(game, gameComponent);
            int3 newPosition = int3.Zero();
            foreach (Entity e in chunkSpawnSystem.chunks.Values)
            {
                Voxels.Chunk c = EntityManager.GetComponentData<Voxels.Chunk>(e);
                if (c.Value.chunkPosition.x == 0 && c.Value.chunkPosition.z == 0)
                {
                    newPosition = FindNewPosition(c);
                    break;
                }
            }
            int playerID = World.EntityManager.GetComponentData<Controller>(camera).deviceID;
            SetGameState(GameState.LoadCharacter);
            CharacterSpawnSystem.SpawnPlayer(World.EntityManager,
                gameComponent.id, playerID, camera, gameComponent.mapID,
                startingCharacter.Value.id, classID, characterID, newPosition.ToFloat3());
        }

        //Bootstrap.instance.playerClanID,
        //yield return new WaitForSeconds(1f);
        //gameComponent.timeChanged = UnityEngine.Time.time;
        // load player character
        // spawn character
        // load manager - load the character with ID
        //int characterID2 = int.Parse(characterID);
        /*gameComponent.spawnedPlayerIDs = new BlitableArray<int>(1, Unity.Collections.Allocator.Persistent);
        if (isLoad)
        {
            gameComponent.spawnedPlayerIDs[0] = characterID;
        }
        else
        {
            gameComponent.spawnedPlayerIDs[0] = characterID;
        }*/

        //private void SpawnPlayerCharacter(int gameID, int controllerIndex, int cameraID, int metaID, int characterID, int worldID, int classID, bool isLoad)
        //{
        //CameraData cameraData = data.startingCamera.Value;
        /*if (characterID == 0)
        {
            characterID = Bootstrap.GenerateUniqueID();
        }*/
        // find chunk position 000
        //SetGameState(GameState.LoadCharacter);
        //SetGameState(GameState.InGame);
        //}/
        /*cameraSystem.ConnectCameraToCharacter(cameraID, characterID);
        Entity characterEntity = characterSpawnSystem.characters[characterID];
        playerSpawnSystem.SetControllerCharacter(characterEntity, controllerIndex);
        if (isLoad)
        {
            saveSystem.LoadPlayer(characterEntity);
        }
        worldSpawnSystem.OnAddedStreamer(characterEntity, worldID);*/

        private int3 FindNewPosition(Voxels.Chunk chunk)
        {
            var voxelDimensions = chunk.Value.voxelDimensions;
            //int worldID = chunk.worldID;
            int voxelIndex;
            int3 checkVoxelPosition;
            int randomPositionX = (int)math.floor(UnityEngine.Random.Range(0, voxelDimensions.x - 1));
            int randomPositionZ = (int)math.floor(UnityEngine.Random.Range(0, voxelDimensions.z - 1));
            for (int j = (int)chunk.Value.voxelDimensions.y - 1; j >= 0; j--)
            {
                checkVoxelPosition = new int3(randomPositionX, j, randomPositionZ);
                voxelIndex = Voxels.VoxelRaycastSystem.GetVoxelArrayIndex(checkVoxelPosition, voxelDimensions);
                if (chunk.Value.voxels[voxelIndex] != 0)
                {
                    var newPosition = new int3(checkVoxelPosition.x, j + 1, checkVoxelPosition.z);
                    return newPosition;
                }

            }
            Debug.LogError("Could not find position for player.");
            return int3.Zero();
        }

        private void SetGameState(GameState newState)
        {
            Game gamer = World.EntityManager.GetComponentData<Game>(game);
            gamer.newState = ((byte)newState);
            World.EntityManager.SetComponentData(game, gamer);
        }
        #endregion
    }
}


// redo as a game start - disabled ( re enables to previous state after time runs out)
/*private IEnumerator DisableSticky()
{
    movementSystemGroup.Enabled = false;
    gameSystemGroup.Enabled = false;
    //yield return new WaitForSeconds(1.5f);
    SetGameState(GameState.InGame);
    movementSystemGroup.Enabled = true;
    gameSystemGroup.Enabled = true;
}*/
