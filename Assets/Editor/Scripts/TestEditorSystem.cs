using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel
{
    public struct TestEditorComponent : IComponentData
    {
        public byte enabled;
        public int count;
    }

    //[ExecuteInEditMode]
    //[ExecuteAlways]
    [DisableAutoCreation]
    public class TestEditorSystem : JobComponentSystem
    {
        private static EntityManager entityManager;
        public static Entity testEntity;

        [BurstCompile]
        struct TestJob : IJobForEach<TestEditorComponent>
        {
            public void Execute(ref TestEditorComponent tester)
            {
                if (tester.enabled == 1)
                {
                    tester.count++;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            TestJob job = new TestJob { };
            JobHandle handle = job.Schedule(this, inputDeps);
            //handle.Complete();
            //Debug.LogError("Called once!");
            return handle;
        }
    }
}