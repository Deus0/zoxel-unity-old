using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Zoxel.Voxels;

namespace Zoxel
{
    [DisableAutoCreation]
    public class BulletSpawnSystem : ComponentSystem
    {
        private Entity bulletPrefab;
        private EntityArchetype bulletArchtype;
        public Dictionary<int, Entity> bullets = new Dictionary<int, Entity>();
        public Dictionary<int, BulletDatam> meta = new Dictionary<int, BulletDatam>();

        protected override void OnCreate()
        {
            bulletArchtype = World.EntityManager.CreateArchetype(
                // game
                typeof(ZoxID),
                typeof(Bullet),
                // movement
                typeof(Body),
                typeof(BodyForce),
                // Transform and Rendering
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );
            bulletPrefab = World.EntityManager.CreateEntity(bulletArchtype);
            World.EntityManager.AddComponentData(bulletPrefab, new Prefab { });
        }

        public void Clear()
        {
            foreach (Entity e in bullets.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            bullets.Clear();
        }


        #region Spawning-Removing

        public void QueueBullet(int metaID, float3 spawnPosition, quaternion spawnRotation, int creatorID, int clanID) /*float damage, float bulletSpeed, BulletData data*/
        {
            Entity e = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponentData(e, new SpawnBulletCommand
            {
                metaID = metaID,
                creatorID = creatorID,
                clanID = clanID,
                spawnPosition = spawnPosition,
                spawnRotation = spawnRotation
            });
        }

        public struct SpawnBulletCommand : IComponentData
        {
            public int metaID;
            public int creatorID;
            public int clanID;
            public float3 spawnPosition;
            public quaternion spawnRotation;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnBulletCommand>().ForEach((Entity e, ref SpawnBulletCommand command) =>
            {
                SpawnBullet(command);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion

        private void SpawnBullet(SpawnBulletCommand command)
        {
            SpawnBullets(
                new int[] { command.metaID },
                new float3[] { command.spawnPosition },
                new quaternion[] { command.spawnRotation },
                new int[] { command.creatorID },
                new int[] { command.clanID }
                );
        }

        private void SpawnBullets(int[] metaIDs,
            float3[] positions, quaternion[] rotations, 
            int[] creatorIDs, int[] clanIDs)/*, float[] damages, float[] bulletSpeeds, )*/
        {
            NativeArray<Entity> bulletEntities = new NativeArray<Entity>(positions.Length, Allocator.Temp);
            World.EntityManager.Instantiate(bulletPrefab, bulletEntities);
            // for all bullets, set custom data using indexes entity
            float timeBegun = UnityEngine.Time.time;
            for (int i = 0; i < bulletEntities.Length; i++)
            {
                int metaID = metaIDs[i];
                // increase by character stats and skill level!
                float damage = UnityEngine.Random.Range(meta[metaID].Value.damage.x, meta[metaID].Value.damage.y);
                float lifetime = meta[metaID].Value.lifetime; // increase by character stats
                float speed = meta[metaID].Value.speed; // increase by character stats
                float scale = meta[metaID].Value.scale; // increase by character stats

                Entity bulletEntity = bulletEntities[i];
                int id = Bootstrap.GenerateUniqueID();
                bullets.Add(id, bulletEntity);
                World.EntityManager.SetComponentData(bulletEntity, new ZoxID {
                    id = id,
                    creatorID = creatorIDs[i],
                    clanID = clanIDs[i]
                });
                // stats
                World.EntityManager.SetComponentData(bulletEntity,new Bullet {
                    damage = damage,
                    timeStarted = timeBegun,
                    lifetime = lifetime,
                    metaID = metaID
                });

                // Transforms
                World.EntityManager.SetComponentData(bulletEntity, new Translation {
                    Value = positions[i]
                });
                World.EntityManager.SetComponentData(bulletEntity, new Scale { Value = scale });//new float3(0.33f, 0.33f, 0.33f) });
                World.EntityManager.SetComponentData(bulletEntity, new Rotation { Value = rotations[i] });
                // rendering
                var vox = meta[metaID].model;
                World.EntityManager.SetSharedComponentData(bulletEntity,
                    new RenderMesh {
                        material = Bootstrap.GetVoxelMaterial(),
                        mesh = new Mesh()//meta[metaID].model.bakedMesh
                    });
                WorldSpawnSystem.QueueUpdateModel(World.EntityManager, bulletEntity, id, vox.data);
                
                // movement
                // divide it by mass? or just set acceleration, and mass as parameters
                if (World.EntityManager.HasComponent<BodyForce>(bulletEntity))
                {
                    World.EntityManager.SetComponentData(bulletEntity, new BodyForce { velocity = new float3(0, 0, speed) });
                }

                // Add particle emitter here
                if (meta[metaID].Value.particlesName != "")
                {
                    ParticlesManager.instance.PlayParticles(
                        new ParticleSpawnCommand
                        {
                            name = meta[metaID].Value.particlesName,
                            deathParticleName = meta[metaID].Value.deathParticlesName,
                            deathParticleLife = meta[metaID].Value.deathParticleLife
                        },
                        bulletEntity, World.EntityManager);
                }
            }
            bulletEntities.Dispose();
        }

       /* private void SpawnBullet(float3 spawnPosition, quaternion rotation, int creatorID, int clanID, float damage, float bulletSpeed, BulletData bullet)
        {
            Entity bulletEntity = World.EntityManager.CreateEntity(bulletArchtype);
            int id = Bootstrap.GenerateUniqueID();
            World.EntityManager.SetComponentData(bulletEntity, 
                new Bullet
                {
                    damage = damage,
                    timeStarted =UnityEngine.Time.time
                });
            World.EntityManager.SetComponentData(bulletEntity,
                new ZoxID
                {
                    id = id,
                    creatorID = creatorID,
                    clanID = clanID
                });
            World.EntityManager.SetComponentData(bulletEntity, new Translation {
                Value = spawnPosition
            });
            World.EntityManager.SetComponentData(bulletEntity, new Scale { Value = bullet.scale });//new float3(0.33f, 0.33f, 0.33f) });
            World.EntityManager.SetComponentData(bulletEntity, new Rotation { Value = rotation });
            World.EntityManager.SetComponentData(bulletEntity, new Body { velocity = new float3(0, 0, bulletSpeed) }); // divide it by mass? or just set acceleration, and mass as parameters
            World.EntityManager.SetSharedComponentData(bulletEntity, new RenderMesh
            {
                material = meta[bullet.id].model.bakedMaterial,
                mesh = meta[bullet.id].model.bakedMesh
            });
            bullets.Add(id, bulletEntity);
            AudioManager.instance.PlayBulletSpawnSound(spawnPosition);
            //Bootstrap.instance.StartCoroutine(DestroyBulletInTime(bulletEntity, bulletSpeed * 20f));
        }*/
        /*IEnumerator DestroyBulletInTime(Entity entity, float time)
        {
            yield return new WaitForSeconds(time);
            if (World.EntityManager.Exists(entity))
            {
                bullets.Remove(entity);
                World.EntityManager.DestroyEntity(entity);
            }
        }*/

        /*public void UseBullet(int bulletIndex)
        {
            if (bulletIndex < bullets.Count)
            {
                Entity entity = bullets[bulletIndex];
                UseBullet(entity);
            }
        }*/
    }
}
