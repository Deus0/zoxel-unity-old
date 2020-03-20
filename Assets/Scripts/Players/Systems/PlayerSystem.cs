using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    [DisableAutoCreation]
    public class PlayerSpawnSystem : ComponentSystem
    {
        public Dictionary<int, Entity> controllers = new Dictionary<int, Entity>();
        public GameStartSystem gameStartSystem;

        protected override void OnUpdate()
        {
            // this is bad! 
            Entities.WithAll<Game>().ForEach((Entity gameEntity, ref Game game) =>
            {
                if (game.state == ((byte)GameState.StartScreen) || game.state == ((byte)GameState.MainMenu))
                {
                    Gamepad pad = GetConnectingGamepad();
                    if (pad != null)
                    {
                        //SpawnPlayer(pad, game.id);
                        SpawnPlayer(pad, game.id, DeviceType.Gamepad);
                        game.newState = ((byte)GameState.MainMenu);
                    }
                    else
                    {
                        Keyboard keyboard = GetConnectingKeyboard();
                        if (keyboard != null)
                        {
                            //SpawnPlayer(keyboard, game.id);
                            SpawnPlayer(keyboard, game.id, DeviceType.KeyboardMouse);
                            game.newState = ((byte)GameState.MainMenu);
                        }
                    }
                }
                // get all controllers and set state
                foreach (Entity e in controllers.Values)
                {
                    if (World.EntityManager.Exists(e))
                    {
                        Controller controller = World.EntityManager.GetComponentData<Controller>(e);
                        controller.gameState = game.state;
                        World.EntityManager.SetComponentData(e, controller);
                    }
                }
            });

            Entities.WithAll<Controller>().ForEach((Entity e, ref Controller controller) =>
            {
                if (controller.deviceID == 0)
                {
                    HandleNewConnections(ref controller);
                }
                else
                {
                    if (UnityEngine.Time.realtimeSinceStartup - controller.stateChangedTime >= 0.1f)
                    {
                        HandlePausing(ref controller);
                        HandleGameUI(ref controller);
                    }
                    HandleDisconnections(ref controller);
                }
            });
        }

        private void HandlePausing(ref Controller controller)
        {
            if (controller.gameState == ((byte)GameState.InGame))
            {
                if (controller.inputType == ((byte)DeviceType.Gamepad))
                {
                    foreach (Gamepad pad in Gamepad.all)
                    {
                        if (controller.deviceID == pad.deviceId)
                        {
                            if (pad.startButton.wasPressedThisFrame)
                            {
                                // get game
                                SetGameState(ref controller, GameState.PauseScreen);
                            }
                            break;
                        }
                    }
                }
                else if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
                {
                    Keyboard keyboard = GetKeyboard(controller.deviceID);
                    if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame
                        || keyboard.enterKey.wasPressedThisFrame))
                    {
                        SetGameState(ref controller, GameState.PauseScreen);
                    }
                }
            }
            else if (controller.gameState == ((byte)GameState.PauseScreen))
            {
                if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
                {
                    Keyboard keyboard = GetKeyboard(controller.deviceID);
                    if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame
                        || keyboard.enterKey.wasPressedThisFrame))
                    {
                        SetGameState(ref controller, GameState.InGame);
                    }
                }
                else if (controller.inputType == ((byte)DeviceType.Gamepad))
                {
                    foreach (Gamepad pad in Gamepad.all)
                    {
                        if (controller.deviceID == pad.deviceId)
                        {
                            if (pad.startButton.wasPressedThisFrame)
                            {
                                SetGameState(ref controller, GameState.InGame);
                                //game.newState = ((byte)GameState.UnPausing);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void HandleGameUI(ref Controller controller)
        {
            if (controller.gameState == ((byte)GameState.InGame))
            {
                if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
                {
                    Keyboard keyboard = GetKeyboard(controller.deviceID);
                    //if (Keyboard.current.backspaceKey.wasPressedThisFrame)
                    if (keyboard != null && keyboard.backspaceKey.wasPressedThisFrame)
                    {
                        //Debug.LogError("Entering GameUI");
                        //keyboard.backspaceKey.wasPressedThisFrame = false;
                        SetGameState(ref controller, GameState.GameUI);
                    }
                }
                else if(controller.inputType == ((byte)DeviceType.Gamepad))
                {
                    foreach (Gamepad pad in Gamepad.all)
                    {
                        if (controller.deviceID == pad.deviceId)
                        {
                            if (pad.selectButton.wasPressedThisFrame)
                            {
                                // get game
                                SetGameState(ref controller, GameState.GameUI);
                            }
                            break;
                        }
                    }
                }
            }
            else if (controller.gameState == ((byte)GameState.GameUI))
            {
                if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
                {
                    Keyboard keyboard = GetKeyboard(controller.deviceID);
                    if (keyboard != null && keyboard.backspaceKey.wasPressedThisFrame)
                    {
                        //Debug.LogError("Exiting GameUI");
                        SetGameState(ref controller, GameState.InGame);
                    }
                }
                else if (controller.inputType == ((byte)DeviceType.Gamepad))
                {
                    foreach (Gamepad pad in Gamepad.all)
                    {
                        if (controller.deviceID == pad.deviceId)
                        {
                            if (pad.selectButton.wasPressedThisFrame)
                            {
                                SetGameState(ref controller, GameState.InGame);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void SetPlayerCharacter(Entity characterEntity, int playerID)
        {
            //int i = 0;
            //foreach (Entity controllerEntity in controllers.Values)
            if (controllers.ContainsKey(playerID))
            {
                // first set character controller data
                // destroy controller entity, set it etc
                Controller controller = World.EntityManager.GetComponentData<Controller>(controllers[playerID]);
                int deviceID = controller.deviceID;
                World.EntityManager.SetComponentData(characterEntity, controller);
                controllers[deviceID] = characterEntity;
            }
            else
            {
                Debug.LogError("Could not find a controller for player: " + playerID + ".");
            }
        }

        public void RemoveControllerCharacter(Entity character)
        {
            foreach (Entity controllerEntity in controllers.Values)
            {
                if (character == controllerEntity)
                {
                    Entity newEntity = World.EntityManager.CreateEntity();
                    Controller controller = World.EntityManager.GetComponentData<Controller>(controllerEntity);
                    World.EntityManager.AddComponentData(newEntity, controller);
                    int deviceID = controller.deviceID;
                    controllers[deviceID] = newEntity;
                    World.EntityManager.RemoveComponent<Controller>(character);
                    return;
                }
            }
        }

        private void OnDisconnnect(ref Controller controller)
        {
            if (IsConnected(controller.deviceID))
            {
                Debug.LogError(((DeviceType)controller.inputType) + ":" + controller.deviceID + " is no longer connected.");
                //connectedControllerIDs.Remove(controller.deviceID);
                controller.deviceID = 0;
                controller.inputType = 0;
            }
        }

        private void OnConnect(ref Controller controller, int deviceID, DeviceType deviceType)
        {
            controller.deviceID = deviceID;
            controller.inputType = (byte)(deviceType);
            //connectedControllerIDs.Add(deviceID);
        }

        private bool IsConnected(int deviceID)
        {
            return controllers.ContainsKey(deviceID);
           // return connectedControllerIDs.Contains(deviceID);
        }

        private void SetGameState(ref Controller controller, GameState newState)
        {
            controller.stateChangedTime = UnityEngine.Time.realtimeSinceStartup;
            Entity game = gameStartSystem.games[controller.gameID];
            Game gameComponent = World.EntityManager.GetComponentData<Game>(game);
            gameComponent.newState = ((byte)newState);
            World.EntityManager.SetComponentData(game, gameComponent);
        }
        
        private void SpawnPlayer(InputDevice pad, int gameID, DeviceType type)
        {
            if (!controllers.ContainsKey(pad.deviceId))
            {
                Entity entity = World.EntityManager.CreateEntity();
                Controller controller = new Controller
                {
                    deviceID = pad.deviceId,
                    inputType = ((byte)type),
                    gameID = gameID
                };
                World.EntityManager.AddComponentData(entity, controller);
                controllers.Add(pad.deviceId, entity);
            }
            else
            {
                Debug.LogError("Trying to connect pad again.");
            }
        }
       /* private void SpawnPlayer(Gamepad pad, int gameID)
        {
            if (!controllers.ContainsKey(pad.deviceId))
            {
                Entity entity = World.EntityManager.CreateEntity();
                Controller controller = new Controller
                {
                    deviceID = pad.deviceId,
                    inputType = ((byte)DeviceType.Gamepad),
                    gameID = gameID
                };
                World.EntityManager.AddComponentData(entity, controller);
                controllers.Add(pad.deviceId, entity);
                //connectedControllerIDs.Add(pad.deviceId);
                Debug.Log("Connected pad: " + pad.deviceId);
                //Bootstrap.instance.playersToSpawn = controllers.Count;
            }
            else
            {
                Debug.LogError("Trying to connect pad again.");
            }
        }

        private void SpawnPlayer(Keyboard keyboard, int gameID)
        {
            if (!controllers.ContainsKey(keyboard.deviceId))
            {
                //Debug.LogError("Spawning New Controller");
                Entity entity = World.EntityManager.CreateEntity();
                Controller controller = new Controller
                {
                    deviceID = keyboard.deviceId,
                    inputType = ((byte)DeviceType.KeyboardMouse),
                    gameID = gameID
                };
                World.EntityManager.AddComponentData(entity, controller);
                controllers.Add(keyboard.deviceId, entity);
                //connectedControllerIDs.Add(keyboard.deviceId);
                Debug.Log("Connected keyboard: " + keyboard.deviceId);
                //Bootstrap.instance.playersToSpawn = controllers.Count;
            }
            else
            {
                Debug.LogError("Trying to connect keyboard again.");
            }
        }
        */

        public static Gamepad GetGamepad(int deviceID)
        {
            foreach (Gamepad pad in Gamepad.all)
            {
                if (pad.deviceId == deviceID)
                {
                    return pad;
                }
            }
            return null;
        }
        private void HandleDisconnections(ref Controller controller)
        {
            if (controller.inputType == ((byte)DeviceType.Gamepad))
            {
                if (GetGamepad(controller.deviceID) != null)
                {
                    return;
                }
                OnDisconnnect(ref controller);
            }
            else if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
            {
                if (GetKeyboard(controller.deviceID) != null)
                {
                    return;
                }
                OnDisconnnect(ref controller);
            }
        }

        private void HandleNewConnections(ref Controller controller)
        {
            // seek new gamepad
            Gamepad pad = GetConnectingGamepad();
            if (pad != null)
            {
                OnConnect(ref controller, pad.deviceId, DeviceType.Gamepad);
                return;
            }
            Keyboard keyboard = GetConnectingKeyboard();
            if (keyboard != null)
            {
                OnConnect(ref controller, keyboard.deviceId, DeviceType.KeyboardMouse);
            }
        }

        public static Keyboard GetKeyboard(int deviceID)
        {
            foreach (InputDevice inputDevice in Keyboard.all)
            {
                Keyboard keyboard = inputDevice as Keyboard;
                if (keyboard != null
                    && keyboard.deviceId == deviceID)
                {
                    return keyboard;
                }
            }
            return null;
        }

        private Keyboard GetConnectingKeyboard()
        {
            /*Keyboard currentKeyboard = Keyboard.current;
            if (currentKeyboard != null && !IsConnected(currentKeyboard.deviceId))
            {
                if (DidKeyboardPressAnyKey(currentKeyboard))
                {
                    return currentKeyboard;
                }
            }*/
            foreach (InputDevice inputDevice in Keyboard.all)
            {
                Keyboard keyboard = inputDevice as Keyboard;
                if (keyboard != null && !IsConnected(keyboard.deviceId))
                {
                    if (DidKeyboardPressAnyKey(keyboard))
                    {
                        Debug.Log("Keys were pressed.");
                        return keyboard;
                    }
                }
            }
            return null;
        }

        private bool DidKeyboardPressAnyKey(Keyboard keyboard)
        {
            return (//!isAnyKey && keyboard.enterKey.wasPressedThisFrame) ||
                    //(isAnyKey && (
                    keyboard.enterKey.wasPressedThisFrame ||
                    keyboard.wKey.wasPressedThisFrame ||
                    keyboard.aKey.wasPressedThisFrame ||
                    keyboard.sKey.wasPressedThisFrame ||
                    keyboard.dKey.wasPressedThisFrame ||
                    keyboard.qKey.wasPressedThisFrame ||
                    keyboard.eKey.wasPressedThisFrame ||
                    keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.backspaceKey.wasPressedThisFrame);
        }

        private Gamepad GetConnectingGamepad()
        {
            foreach (Gamepad pad in Gamepad.all)
            {
                if (!IsConnected(pad.deviceId))
                {
                    if (pad.startButton.wasPressedThisFrame ||
                            pad.selectButton.wasPressedThisFrame ||
                            pad.aButton.wasPressedThisFrame ||
                            pad.bButton.wasPressedThisFrame ||
                            pad.yButton.wasPressedThisFrame ||
                            pad.xButton.wasPressedThisFrame ||
                            pad.leftTrigger.wasPressedThisFrame ||
                            pad.rightTrigger.wasPressedThisFrame ||
                            pad.leftShoulder.wasPressedThisFrame ||
                            pad.rightShoulder.wasPressedThisFrame ||
                            pad.leftStickButton.wasPressedThisFrame ||
                            pad.rightStickButton.wasPressedThisFrame ||

                            pad.buttonEast.wasPressedThisFrame ||
                            pad.buttonNorth.wasPressedThisFrame ||
                            pad.buttonSouth.wasPressedThisFrame ||
                            pad.buttonWest.wasPressedThisFrame ||

                            pad.crossButton.wasPressedThisFrame ||
                            pad.circleButton.wasPressedThisFrame ||
                            pad.triangleButton.wasPressedThisFrame ||
                            pad.squareButton.wasPressedThisFrame)
                    {
                        Debug.Log("Buttons were pressed.");
                        return pad;
                    }
                    float2 leftStick = pad.leftStick.ReadValue();
                    float2 rightStick = pad.rightStick.ReadValue();
                    if (math.abs(leftStick.x) > 0.49f ||
                        math.abs(leftStick.y) > 0.49f ||
                        math.abs(rightStick.x) > 0.49f ||
                        math.abs(rightStick.y) > 0.49f)
                    {
                        Debug.Log("Sticks were pressed:" + leftStick.ToString() + ":" + rightStick.ToString());
                        return pad;
                    }
                }
            }
            return null;
        }

        /*private void CheckNewKeyboard(ref Controller controller, Keyboard keyboard)//, bool isAnyKey)
        {
            if (!IsConnected(keyboard.deviceId))
            {
                if ((!isAnyKey && keyboard.enterKey.wasPressedThisFrame) ||
                        (isAnyKey && keyboard.wasUpdatedThisFrame))
                {
                    // connect pad as new controller
                    OnConnect(ref controller, keyboard.deviceId, DeviceType.KeyboardMouse);
                }
            }
        }*/
    }
}