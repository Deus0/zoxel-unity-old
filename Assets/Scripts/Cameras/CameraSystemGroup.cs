using Unity.Entities;

namespace Zoxel
{
    [UpdateAfter(typeof(MovementSystemGroup))]
    public class CameraSystemGroup : ComponentSystemGroup
    {
        public CameraSystem cameraSystem;
        //public CameraProxySystem cameraProxySystem; // need to make seperate component for cameraID - maybe just use zoxID (seperate that into zoxID, and clanID
        // Third person systems
        private CameraInputSystem cameraInputSystem;
        private CameraMovementSystem cameraMovementSystem;
        private CameraFollowSystem cameraFollowSystem;
        // first person systems
        public CameraFirstPersonSystem cameraFirstPersonSystem;
        private CharacterToCameraSystem characterToCameraSystem;
        private CameraSynchSystem cameraSynchSystem;            // can be considered more utility
        // UI x Camera
        private CameraOrbitStartSystem cameraOrbitStartSystem;
        private CameraOrbitSystem cameraOrbitSystem;
        private CameraFacerSystem cameraFacerSystem;


        public void Clear()
        {
            cameraSystem.Clear();
        }

        public void Initialize(Unity.Entities.World space)
        {
            // stat bars
            cameraOrbitStartSystem = space.GetOrCreateSystem<CameraOrbitStartSystem>();
            AddSystemToUpdateList(cameraOrbitStartSystem);

            cameraSystem = space.GetOrCreateSystem<CameraSystem>();
            AddSystemToUpdateList(cameraSystem);

            cameraInputSystem = space.GetOrCreateSystem<CameraInputSystem>();
            AddSystemToUpdateList(cameraInputSystem);
            cameraMovementSystem = space.GetOrCreateSystem<CameraMovementSystem>();
            AddSystemToUpdateList(cameraMovementSystem);
            cameraFollowSystem = space.GetOrCreateSystem<CameraFollowSystem>();
            AddSystemToUpdateList(cameraFollowSystem);

            characterToCameraSystem = space.GetOrCreateSystem<CharacterToCameraSystem>();
            AddSystemToUpdateList(characterToCameraSystem);
            cameraFirstPersonSystem = space.GetOrCreateSystem<CameraFirstPersonSystem>();
            AddSystemToUpdateList(cameraFirstPersonSystem);
            cameraSynchSystem = space.GetOrCreateSystem<CameraSynchSystem>();
            AddSystemToUpdateList(cameraSynchSystem);

            //cameraProxySystem = space.GetOrCreateSystem<CameraProxySystem>();
            //AddSystemToUpdateList(cameraProxySystem);
            cameraOrbitSystem = space.GetOrCreateSystem<CameraOrbitSystem>();
            AddSystemToUpdateList(cameraOrbitSystem);
            cameraFacerSystem = space.GetOrCreateSystem<CameraFacerSystem>();
            AddSystemToUpdateList(cameraFacerSystem);
            SetLinks();
        }

        void SetLinks()
        {
            cameraFacerSystem.cameraSystem = cameraSystem;
        }

        public void CombineWithVoxels(ChunkMapCompleterSystem chunkMapComp)
        {

        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            cameraSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            //c/haracterToCameraSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }

        public void CombineWithPlayers(PlayerSystemGroup playerSYstemGroup)
        {
            cameraSystem.playerSystem = playerSYstemGroup.playerSpawnSystem;
        }

        public void SetMeta(GameDatam data)
        {
            cameraSystem.meta = data.GetCameras();
#if POST_PROCESSING
            CameraSystem.profile = data.profile;
            CameraSystem.resources = data.resources;
#endif
        }

    }
}
