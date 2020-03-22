using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Zoxel
{
    /// <summary>
    /// extracts input, from unity, and gives it to my systems
    /// </summary>
    [DisableAutoCreation]
    public class ControllerSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            Entities.WithAll<Controller>().ForEach((Entity e, ref Controller controller) =>
            {
                if (controller.deviceID != 0)
                {
                    if (controller.inputType == ((byte)DeviceType.Gamepad))
                    {
                        foreach (Gamepad pad in Gamepad.all)
                        {
                            if (controller.deviceID == pad.deviceId)
                            {
                                controller.Value = ExtractGamepad(pad);
                            }
                        }
                    }
                    else if (controller.inputType == ((byte)DeviceType.KeyboardMouse))
                    {
                        //foreach (Keyboard keyboard in Keyboard.all)
                        CheckKeyboard(ref controller, PlayerSpawnSystem.GetKeyboard(controller.deviceID));
                    }
                }
            });
        }

        private void CheckKeyboard(ref Controller controller, Keyboard keyboard)
        {
            if (controller.deviceID == keyboard.deviceId)
            {
                controller.Value = ExtractKeyboard(keyboard);
            }
        }

        private ControllerData ExtractGamepad(Gamepad pad)
        {
            ControllerData newClone = new ControllerData();
            if (pad == null)
            {
                return newClone;
            }
            newClone.leftStick = pad.leftStick.ReadValue();
            newClone.rightStick = pad.rightStick.ReadValue();
            //UnityEngine.Debug.LogError("Right stick: " + newClone.rightStick.ToString());
            if (pad.aButton.wasPressedThisFrame)
            {
                newClone.buttonA = 1;
            }
            if (pad.bButton.wasPressedThisFrame)
            {
                newClone.buttonB = 1;
            }
            if (pad.xButton.wasPressedThisFrame)
            {
                newClone.buttonX = 1;
            }
            if (pad.yButton.wasPressedThisFrame)
            {
                newClone.buttonY = 1;
            }
            if (pad.startButton.isPressed && pad.startButton.wasPressedThisFrame)
            {
                newClone.startButton = 1;
            }
            if (pad.selectButton.isPressed && pad.selectButton.wasPressedThisFrame)
            {
                newClone.selectButton = 1;
            }
            if (pad.rightShoulder.isPressed && pad.rightShoulder.wasPressedThisFrame)
            {
                newClone.buttonRB = 1;
            }
            if (pad.leftShoulder.isPressed && pad.leftShoulder.wasPressedThisFrame)
            {
                newClone.buttonLB = 1;
            }
            if (pad.rightTrigger.isPressed && pad.rightTrigger.wasPressedThisFrame)
            {
                newClone.buttonRT = 1;
            }
            if (pad.leftTrigger.isPressed && pad.leftTrigger.wasPressedThisFrame)
            {
                newClone.buttonLT = 1;
            }
            return newClone;
        }

        private ControllerData ExtractKeyboard(Keyboard keyboard)
        {
            ControllerData newClone = new ControllerData();
            if (keyboard == null)
            {
                return newClone;
            }
            newClone.leftStick = float2.zero;
            newClone.rightStick = float2.zero;
            if (keyboard.aKey.ReadValue() != 0)
            {
                newClone.leftStick.x = -1;
            }
            if (keyboard.dKey.ReadValue() != 0)
            {
                newClone.leftStick.x = 1;
            }
            if (keyboard.sKey.ReadValue() != 0)
            {
                newClone.leftStick.y = -1;
            }
            if (keyboard.wKey.ReadValue() != 0)
            {
                newClone.leftStick.y = 1;
            }
            if (keyboard.fKey.wasPressedThisFrame)
            {
                newClone.buttonA = 1;
            }
            if (keyboard.rKey.wasPressedThisFrame)
            {
                newClone.buttonB = 1;
            }
            if (keyboard.enterKey.wasPressedThisFrame)
            {
                newClone.startButton = 1;
            }
            if (keyboard.backspaceKey.wasPressedThisFrame)
            {
                newClone.selectButton = 1;
            }
            if (keyboard.eKey.wasPressedThisFrame)
            {
                newClone.buttonRB = 1;
            }
            if (keyboard.qKey.wasPressedThisFrame)
            {
                newClone.buttonLB = 1;
            }
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    newClone.buttonRT = 1;
                    newClone.buttonRTHeld = 1;
                }
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    newClone.buttonRTHeld = 0;
                }
                // this scheme is for RTS, scroll for zoom etc

                float2 mouseDelta = Mouse.current.delta.ReadValue();
                float mouseDeltaMax = 10;
                if (mouseDelta.x > 0)
                {
                    mouseDelta.x = math.min(mouseDelta.x, mouseDeltaMax);
                    newClone.rightStick = mouseDelta.x / mouseDeltaMax;
                }
                if (mouseDelta.x < 0)
                {
                    mouseDelta.x = math.max(mouseDelta.x, -mouseDeltaMax);
                    newClone.rightStick = mouseDelta.x / mouseDeltaMax;
                }
                float2 mouseScroll = Mouse.current.scroll.ReadValue();
                newClone.rightStick.y = mouseScroll.y * 0.05f;

                // for fps
                newClone.rightStick = mouseDelta / mouseDeltaMax;

            }
            return newClone;
        }
    }
}