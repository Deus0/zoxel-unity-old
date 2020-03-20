using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Zoxel
{
    public class CameraSynchComponent : ComponentDataProxy<CameraSynch> { }

    [System.Serializable]
    public struct CameraSynch : IComponentData
    {
        public Entity Value;
        public float3 localPosition;
    }


    public class FirstPersonCameraComponent : ComponentDataProxy<FirstPersonCamera> { }

    [System.Serializable]
    public struct FirstPersonCamera : IComponentData
    {
        public FirstPersonCameraData Value;
        public float3 rotation;
        public float3 rotationVelocity;
        public float3 rotationAcceleration;
        public float2 rightStick;
        public byte enabled;
        // remove these
        public quaternion lastRotation;
        public float3 lastPosition;
    }

    /// <summary>
    /// Put this in scriptableobject
    /// </summary>
    [System.Serializable]
    public struct FirstPersonCameraData
    {
        public float2 sensitivity;
        public float2 rotationBoundsX;
        public float lerpRotationSpeed;
        public float3 cameraAddition;
    }
}