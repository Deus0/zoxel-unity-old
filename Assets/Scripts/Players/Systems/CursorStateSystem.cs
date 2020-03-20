using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using System;


namespace Zoxel
{


    [DisableAutoCreation]
    public class CursorStateSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            bool didUnlock = false;
            Entities.WithAll<Game>().ForEach((Entity e, ref Game game) =>
            {
                if (game.state == ((byte)GameState.PauseScreen)
                || game.state == ((byte)GameState.GameUI)
                || game.state == ((byte)GameState.MainMenu)
               // || game.state == ((byte)GameState.NewGameScreen)
                || game.state == ((byte)GameState.SaveGamesScreen)
                || game.state == ((byte)GameState.RespawnScreen))
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    didUnlock = true;
                }
            });
            if (didUnlock)
            {
                return;
            }
            Entities.WithAll<Controller>().ForEach((Entity e, ref Controller controller) =>
            {
                if (controller.inputType == (byte)(DeviceType.KeyboardMouse))
                {
                    // lock cursor if mouse?
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else if (controller.inputType == (byte)(DeviceType.Gamepad))
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                // hide normally if gamepad
            });
        }
    }
}