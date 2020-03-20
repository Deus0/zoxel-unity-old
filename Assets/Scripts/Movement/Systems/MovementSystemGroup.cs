using Unity.Entities;

namespace Zoxel
{

    [UpdateAfter(typeof(CharacterSystemGroup))]
    public class MovementSystemGroup : ComponentSystemGroup
    {
        private PositionalForceSystem positionalForceSystem;
        private RotationalForceSystem rotationalForceSystem;
        private SeparationSystem separationSystem;
        private WorldBoundSystem worldBoundSystem;
        public VoxelCollisionSystem voxelCollisionSystem;
        private FallSystem fallSystem;
        private DebugColliderSystem debugColliderSystem;
        private DebugBoneSystem debugBoneSystem;
        private CharacterVoxelPositionSystem characterVoxelPositionSystem;

        public void Initialize(Unity.Entities.World space)
        {
            positionalForceSystem = space.GetOrCreateSystem<PositionalForceSystem>();
            rotationalForceSystem = space.GetOrCreateSystem<RotationalForceSystem>();
            AddSystemToUpdateList(positionalForceSystem);
            AddSystemToUpdateList(rotationalForceSystem);
            separationSystem = space.GetOrCreateSystem<SeparationSystem>();
            AddSystemToUpdateList(separationSystem);
            voxelCollisionSystem = space.GetOrCreateSystem<VoxelCollisionSystem>();
            AddSystemToUpdateList(voxelCollisionSystem);
            worldBoundSystem = space.GetOrCreateSystem<WorldBoundSystem>();
            AddSystemToUpdateList(worldBoundSystem);
            fallSystem = space.GetOrCreateSystem<FallSystem>();
            AddSystemToUpdateList(fallSystem);
            characterVoxelPositionSystem = space.GetOrCreateSystem<CharacterVoxelPositionSystem>();
            AddSystemToUpdateList(characterVoxelPositionSystem);

            //if (Bootstrap.DebugColliders)
            if (Bootstrap.instance != null && Bootstrap.instance.DebugColliders)
            {
                debugColliderSystem = space.GetOrCreateSystem<DebugColliderSystem>();
                AddSystemToUpdateList(debugColliderSystem);
                debugBoneSystem = space.GetOrCreateSystem<DebugBoneSystem>();
                AddSystemToUpdateList(debugBoneSystem);
            }
        }

        public void CombineWithVoxels(Voxels.VoxelSystemGroup voxelSystemGroup)
        {
            characterVoxelPositionSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
            characterVoxelPositionSystem.chunkSpawnSystem = voxelSystemGroup.chunkSpawnSystem;
        }
    }
}