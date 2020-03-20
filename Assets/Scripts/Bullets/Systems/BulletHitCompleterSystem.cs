using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    [DisableAutoCreation]
    public class BulletHitCompleterSystem : ComponentSystem
    {
        public BulletSpawnSystem bulletSpawnSystem;
        public BulletDeathSystem bulletDeathSystem;
        public CharacterSpawnSystem characterSpawnSystem;
        public DamageSystem DamageSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<BulletHitTaker>().ForEach((Entity littlebitchEntity, ref BulletHitTaker littlebitch) =>
            {
                if (littlebitch.wasHit == 1)
                {
                    littlebitch.wasHit = 0;
                    Entity bulletEntity = bulletSpawnSystem.bullets[littlebitch.bulletID];
                    if (World.EntityManager.HasComponent<Bullet>(bulletEntity))
                    {
                        Bullet bullet = World.EntityManager.GetComponentData<Bullet>(bulletEntity);
                        ZoxID bulletID = World.EntityManager.GetComponentData<ZoxID>(bulletEntity);
                        if (characterSpawnSystem.characters.ContainsKey(bulletID.creatorID))
                        {
                            Entity attackingCharacter = characterSpawnSystem.characters[bulletID.creatorID];
                            DamageSystem.AddDamage(World.EntityManager,
                                attackingCharacter, littlebitchEntity, 1, bullet.damage);
                        }
                        else if (TurretSpawnerSystem.turrets.ContainsKey(bulletID.creatorID))
                        {
                            DamageSystem.AddDamage(World.EntityManager, 
                                TurretSpawnerSystem.turrets[bulletID.creatorID], littlebitchEntity, 1, bullet.damage);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Character does not exist: " + bulletID.id + " out of " + characterSpawnSystem.characters.Count + " characters.");
                        }
                        // get angle of hit
                        float3 littlebitchPosition = World.EntityManager.GetComponentData<Translation>(littlebitchEntity).Value;
                        float3 bulletPosition = World.EntityManager.GetComponentData<Translation>(bulletEntity).Value;
                        float3 difference = math.normalize(littlebitchPosition - bulletPosition);
#if UNITY_EDITOR
                        UnityEngine.Debug.DrawLine(bulletPosition, bulletPosition + difference, UnityEngine.Color.red, 5);
#endif
                        // add force to character
                        BodyForce force = World.EntityManager.GetComponentData<BodyForce>(littlebitchEntity);
                        force.acceleration += difference * 1.5f;
                        World.EntityManager.SetComponentData(littlebitchEntity, force);

                        bulletDeathSystem.UseBullet(littlebitch.bulletID);
                    }
                    // else bullet has already been removed
                }
            });
        }
    }

}