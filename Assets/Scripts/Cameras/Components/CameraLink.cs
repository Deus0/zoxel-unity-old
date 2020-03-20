using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Zoxel
{

    public class CameraLinkComponent : ComponentDataProxy<CameraLink> { }
    /// <summary>
    /// Put this on character with a camera
    /// </summary>
    [System.Serializable]
    public struct CameraLink : IComponentData
    {
        public Entity camera;
        //public int cameraID;
        //public Entity camera;    // isnt this better?
        public float fov;
        public float aspectRatio;
    }

}