using Unity.Entities;

namespace Zoxel
{
    /// <summary>
    /// A group of systems that does AI
    /// </summary>
    public class AISystemGroup : ComponentSystemGroup
    {
        private TargetSeekSystem targetSystem;
        private AIStateSystem aiStateSystem;
        private WanderSystem wanderSystem;
        private MoveToSystem moveToSystem;
        private RotateToSystem rotateToSystem;
        private BrainSystem brainSystem;
        private AimSystem aimSystem;
        private FollowTargetSystem followTargetSystem;
        public NearbyCharactersSystem nearbyCharactersSystem;
        public TargetTrackerSystem targetTrackerSystem;

        public void Initialize(Unity.Entities.World space)
        {
            targetSystem = space.GetOrCreateSystem<TargetSeekSystem>();
            aiStateSystem = space.GetOrCreateSystem<AIStateSystem>();
            wanderSystem = space.GetOrCreateSystem<WanderSystem>();
            moveToSystem = space.GetOrCreateSystem<MoveToSystem>();
            rotateToSystem = space.GetOrCreateSystem<RotateToSystem>();
            brainSystem = space.GetOrCreateSystem<BrainSystem>();
            aimSystem = space.GetOrCreateSystem<AimSystem>();
            followTargetSystem = space.GetOrCreateSystem<FollowTargetSystem>();
            targetTrackerSystem = space.GetOrCreateSystem<TargetTrackerSystem>();
            AddSystemToUpdateList(targetTrackerSystem);

            AddSystemToUpdateList(targetSystem);
            AddSystemToUpdateList(aiStateSystem);
            AddSystemToUpdateList(wanderSystem);
            AddSystemToUpdateList(moveToSystem);
            AddSystemToUpdateList(rotateToSystem);
            AddSystemToUpdateList(brainSystem);
            AddSystemToUpdateList(aimSystem);
            AddSystemToUpdateList(followTargetSystem);

            nearbyCharactersSystem = space.GetOrCreateSystem<NearbyCharactersSystem>();
            AddSystemToUpdateList(nearbyCharactersSystem);
            SetLinks();
        }
        void SetLinks()
        {

        }

        public void Clear()
        {

        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            nearbyCharactersSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }

    }
}