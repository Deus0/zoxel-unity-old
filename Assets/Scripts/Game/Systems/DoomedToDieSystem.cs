using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    public struct DoomedToDie : IComponentData
    {
        public float beginTime;
        public float lifeTime;
    }

    [DisableAutoCreation]
    public class DoomedToDieSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<DoomedToDie>().ForEach((Entity e, ref DoomedToDie doomedOne) =>
            {
                if (UnityEngine.Time.time - doomedOne.beginTime >= doomedOne.lifeTime)
                {
                    World.EntityManager.DestroyEntity(e);
                }
            });
        }

        public void MarkForDeath(Entity e, float lifeTime)
        {
            if (World.EntityManager.HasComponent<DoomedToDie>(e) == false)
            {
                World.EntityManager.AddComponentData(e, new DoomedToDie { beginTime = UnityEngine.Time.time, lifeTime = lifeTime });
            }
        }
    }
}
