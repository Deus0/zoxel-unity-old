using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Zoxel
{
    /// <summary>
    ///     If bullets collided, they are done for
    ///     If their life is up, also remove them
    /// </summary>
    [DisableAutoCreation]
    public class BulletDeathSystem : ComponentSystem
    {
        public BulletSpawnSystem bulletSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<Bullet>().ForEach((Entity e, ref Bullet bullet) => // , ref RenderMesh renderer
            {
                // lifetime is up
                if (UnityEngine.Time.time - bullet.timeStarted >= 2)
                {
                    UseBullet(e, ref bullet);
                }
            });
            Entities.WithAll<BulletShrink>().ForEach((Entity e, ref BulletShrink bullet) => // , ref RenderMesh renderer
            {
                // lifetime is up
                if (UnityEngine.Time.time - bullet.timeBorn >= bullet.lifetime)
                {
                    //UseBullet(bullet.id);
                    if (bulletSpawnSystem.bullets.ContainsKey(bullet.id))
                    {
                        bulletSpawnSystem.bullets.Remove(bullet.id);
                    }
                    World.EntityManager.DestroyEntity(e);
                }
            });
        }

        public void UseBullet(int bulletID)
        {
            if (bulletSpawnSystem.bullets.ContainsKey(bulletID))
            {
                Entity e = bulletSpawnSystem.bullets[bulletID];
                if (World.EntityManager.HasComponent<Bullet>(e))
                {
                    Bullet bullet = World.EntityManager.GetComponentData<Bullet>(e);
                    UseBullet(e, ref bullet);
                }
            }
        }
        public void UseBullet(Entity entity, ref Bullet bullet)
        {
            World.EntityManager.AddComponentData(entity, new BulletShrink { 
                id = World.EntityManager.GetComponentData<ZoxID>(entity).id,
                timeBorn =UnityEngine.Time.time,
                lifetime = UnityEngine.Random.Range(bullet.lifetime.x, bullet.lifetime.y)
            });
            World.EntityManager.RemoveComponent<Bullet>(entity);
            World.EntityManager.RemoveComponent<Body>(entity);
            World.EntityManager.RemoveComponent<BodyForce>(entity);
            BulletDatam meta = bulletSpawnSystem.meta[bullet.metaID];
            AudioManager.instance.PlaySound(meta.hitSound, World.EntityManager.GetComponentData<Translation>(entity).Value);
        }
    }
}
