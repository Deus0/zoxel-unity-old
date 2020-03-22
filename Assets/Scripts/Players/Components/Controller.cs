using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    public enum DeviceType
    {
        None,
        KeyboardMouse,
        Gamepad,
        Touch
    }

    public struct ControllerData
    {
        public float2 leftStick;
        public float2 rightStick;
        public byte startButton;
        public byte selectButton;
        public byte buttonA;
        public byte buttonB;
        public byte buttonX;
        public byte buttonY;
        public byte buttonLB;
        public byte buttonRB;
        public byte buttonLT;
        public byte buttonRT;
        public byte buttonRTHeld;
    }

    [System.Serializable]
	public struct Controller : IComponentData
    {
        public int deviceID;
        public byte mappingType;
        public int gameID;
        //public int cameraID;
        public byte inputType;
        //public byte autoWalkForward;
        public ControllerData Value;
        public byte gameState;
        public byte gameUIIndex;    // later put this somewhere else
        public float stateChangedTime;
    }

    public class ControllerComponent : ComponentDataProxy<Controller> { }
}