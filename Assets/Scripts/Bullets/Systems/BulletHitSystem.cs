using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

// bullet hit system relies on cached bullet data to be passed to each character
// it outputs the hits that happen
// a component system should take care of actual hits separately

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

namespace Zoxel
{
    public struct BulletHitTaker : IComponentData
    {
        public byte wasHit;
        //public float damage;
        public int bulletID;
    }
    /// <summary>
    /// Check if bullet hits a character body
    /// This causes lag!
    /// </summary>
    [DisableAutoCreation]
	public class BulletHitSystem : JobComponentSystem
    {
        public BulletSpawnSystem bulletSpawnSystem;

        [BurstCompile]
		struct BulletJob : IJobForEach<BulletHitTaker, Body, ZoxID, Translation> // WithEntit
        {
            [ReadOnly]
            public float time;
            [ReadOnly]
            public NativeArray<ZoxID> bullets;
            [ReadOnly]
            public NativeArray<Translation> bulletPositions;

            public void Execute(//Entity entity, int index,
                ref BulletHitTaker bulletHitTaker,
                ref Body body,
                ref ZoxID zoxID,
                ref Translation position) //  ref Targeter clan, 
            {
				//if (stats.isDead == 0)
				{
                    //float hitRadius = 1f;
                    //float hitRadius = body.size.x;
                    //loat distance;
                    // For all bullets, do a distance check using radius of minion
                    for (int bulletIndex = 0; bulletIndex < bullets.Length; bulletIndex++)
					{
						if (bullets[bulletIndex].clanID != zoxID.clanID && 
                            bullets[bulletIndex].creatorID != zoxID.id)
                        // && bulletsUsed[i] == 0)
                        {
                            //distance = math.distance(bullets[bulletIndex].position, position.Value);
                            //if (distance < hitRadius)
                            float3 difference = bulletPositions[bulletIndex].Value - position.Value;
                            if (math.abs(difference.x) <= body.size.x && math.abs(difference.y) <= body.size.y && math.abs(difference.z) <= body.size.z)
                            {
                                bulletHitTaker.bulletID = bullets[bulletIndex].id;
                                bulletHitTaker.wasHit = 1;
                                //characterIDs[index] = zoxID.id;
                                //bulletIDs[index] = bulletIndex;// bullets[bulletIndex].id;
                                break;  // can't hit multiple things?
							}
						}
					}
				}
			}
		}

        public BulletDeathSystem bulletDeathSystem;
        private EntityQuery bulletsQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            bulletsQuery = GetEntityQuery(ComponentType.ReadOnly<Bullet>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<ZoxID>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //return new JobHandle();
            ///*
            if (bulletsQuery.CalculateEntityCount() == 0)
            {
                return new JobHandle();
            }
			BulletJob job = new BulletJob
			{
				time = UnityEngine.Time.time,
                bullets = bulletsQuery.ToComponentDataArray<ZoxID>(Allocator.TempJob), //bullets.AsDeferredJobArray(),
                bulletPositions = bulletsQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
            };
			JobHandle handle = job.Schedule(this, inputDeps);
			handle.Complete();
            
            job.bullets.Dispose();
            job.bulletPositions.Dispose();
            return handle;
            //*/
		}
    }
}