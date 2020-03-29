using Unity.Entities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Zoxel
{
    public class PortalCameraSystem : ComponentSystem
    {
        public Dictionary<int, Camera> cameras = new Dictionary<int, Camera>();

        protected override void OnUpdate()
        {
            Entities.WithAll<PortalCamera>().ForEach((Entity e, ref PortalCamera camera) =>
            {
                if (World.EntityManager.Exists(camera.portal))
                {
                    // position camera near portal
                    // Force camera to render it!
                    // thats all!
                }
            });
        }
    }
}
