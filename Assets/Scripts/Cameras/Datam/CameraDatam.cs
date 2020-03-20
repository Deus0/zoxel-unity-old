using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public enum CameraDataType
    {
        FPS,
        ThirdPerson,
        TopDown
    }
    // 0 for top down
    // 1 for third person
    // 2 for first person
    // 3 for platformer

    // other data
    // post processing
    // angle locked on
    // perspective/ortho
    // FOV

    [Serializable]
    public struct CameraData
    {
        public int id;
        [Header("Player")]
        public byte cameraType; // 0 for fps, 1 for thirdperson, 2 for rts
        public FirstPersonCameraData firstPersonCamera;
        public FollowCameraData followCameraData;
        [Header("Camera Spawning")]
        public byte isRenderTexture;
        public int textureDivision;
    }

    /// <summary>
    /// used for bullets
    /// Vox Model, or normal mesh?
    /// Needs particles (ongoing, on release, on destroyed)
    /// Needs animation (like oscillating the scale, etc)
    /// </summary>
    [CreateAssetMenu(fileName = "Camera", menuName = "Zoxel/Camera")]
    public class CameraDatam : ScriptableObject
    {
        public GameObject gameCameraPrefab;
        public CameraData Value = new CameraData
        {
            textureDivision = 1
        };

        [ContextMenu("GenerateID")]
        public void GenerateID()
        {
            Value.id = Bootstrap.GenerateUniqueID();
        }
    }
}