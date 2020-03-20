using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>
namespace Zoxel
{

    // Essentially trigger system

    /// <summary>
    /// Every x seconds, the turret finds a new character to target
    /// Every frame, it will update the target characters Translation, if Translation has updated, it will update target rotation
    /// </summary>
    [DisableAutoCreation]
    public class ShootSystem : JobComponentSystem
	{
		[BurstCompile]
		struct ShootJob : IJobForEach<Shooter>//, Stats>
		{
			[ReadOnly]
			public float time;

            public void Execute(ref Shooter shooter)//, ref Stats stats)
            {
				// turret.shootCooldown
				if (shooter.triggered == 1) // cooldown or speed?
                {
                    shooter.triggered = 0;
                    if (shooter.CanTrigger(time)) //time - shooter.lastShotTime >= 1f)//stats.stats[3].value)
                    {
                        shooter.lastShotTime = time;
                        shooter.isShoot = 1;
                    }
                    // use mana too
                }
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ShootJob job = new ShootJob
			{
				time = UnityEngine.Time.time,
            };
			JobHandle handle = job.Schedule(this, inputDeps);
            return handle;
        }
    }
    
    [DisableAutoCreation]
    public class ShootCompleterSystem : ComponentSystem
    {
        public BulletSpawnSystem bulletSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<Shooter, ZoxID, Stats>().ForEach((Entity e, ref Shooter shooter, ref ZoxID zoxID, ref Stats stats) =>
            {
                if (shooter.isShoot == 1)
                {
                    shooter.isShoot = 0;
                    //Debug.LogError("Pre Queueing Bullet in ShootCompleterSystem.");
                    //ZoxID stats = World.EntityManager.GetComponentData<Stats>(e);
                    BulletDatam bulletDatam = bulletSpawnSystem.meta[shooter.bulletMetaID];
                    float3 spawnPosition = shooter.shootPosition + math.mul(shooter.shootRotation, new float3(0, 0, 0.15f));
                    AudioManager.instance.PlaySound(bulletDatam.spawnSound, spawnPosition);

                    if (bulletDatam.Value.betweenSpread == 0)
                    {
                        bulletSpawnSystem.QueueBullet(
                                shooter.bulletMetaID,
                                spawnPosition,
                                shooter.shootRotation,
                                zoxID.id,
                                zoxID.clanID
                                //shooter.attackDamage,
                                //shooter.attackForce,
                                );
                    }
                    else
                    {
                        float betweenSpread = bulletDatam.Value.betweenSpread;
                        float2 spreadAmount = bulletDatam.Value.spread; // new float2(15, 30);
                        float2 max = spreadAmount / 2;
                        float2 min = -max;
                        float3 originalRotation = new Quaternion(
                            shooter.shootRotation.value.x, shooter.shootRotation.value.y,
                            shooter.shootRotation.value.z, shooter.shootRotation.value.w).eulerAngles;
                        float3 tempRotation = originalRotation;

                        for (float x = min.x; x <= max.x; x += betweenSpread)
                        {
                            tempRotation.x = (originalRotation.x + x) % 360;
                            for (float y = min.y; y <= max.y; y += betweenSpread)
                            {
                                tempRotation.y = (originalRotation.y + y) % 360;
                                bulletSpawnSystem.QueueBullet(
                                shooter.bulletMetaID,
                                shooter.shootPosition + math.mul(shooter.shootRotation, new float3(0, 0, 0.3f)),
                                Quaternion.Euler(tempRotation),
                                zoxID.id,
                                zoxID.clanID );
                            }
                        }
                    }
                }
            });
        }
    }
}
