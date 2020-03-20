using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{
    public struct ItemHitTaker : IComponentData
    {
        public byte wasHit;
        public int itemID;
        public float radius;
    }

    [DisableAutoCreation]
    public class ItemHitSystem : JobComponentSystem
    {
        private EntityQuery itemQuery;
        public ItemSpawnerSystem itemSpawnSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            itemQuery = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<WorldItem>());
        }


        [BurstCompile]
        struct ItemHitJob : IJobForEach<ItemHitTaker, Body, Translation>
        {
            [ReadOnly]
            public float time;
            [ReadOnly]
            public NativeArray<Translation> translations;
            [ReadOnly]
            public NativeArray<WorldItem> items;

            public void Execute(ref ItemHitTaker itemHitTaker, ref Body body, ref Translation position)
            {
                //float hitRadius = 0.5f;
                // 0.5f is radius of item!
                // times aura of item pickup range!
                float hitRadius = (body.size.x + 0.5f) + itemHitTaker.radius;
                float distance;
                // For all bullets, do a distance check using radius of minion
                for (int i = 0; i < translations.Length; i++)
                {
                    distance = math.distance(translations[i].Value, position.Value);
                    if (distance < hitRadius)
                    {
                        itemHitTaker.wasHit = 1;
                        itemHitTaker.itemID = items[i].id;
                        break;  // just one hit per frame
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //return new JobHandle();
            ///*
            ItemHitJob job = new ItemHitJob
            {
                time = UnityEngine.Time.time,
                translations = itemQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
                items = itemQuery.ToComponentDataArray<WorldItem>(Allocator.TempJob)
            };
            JobHandle handle = job.Schedule(this, inputDeps);
            handle.Complete();
            job.translations.Dispose();
            job.items.Dispose();
            return handle;
            //*/
        }
    }
}