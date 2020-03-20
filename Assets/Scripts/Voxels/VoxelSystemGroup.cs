using Unity.Entities;
using System.Collections.Generic;
using Zoxel.WorldGeneration;

namespace Zoxel.Voxels
{

    /// <summary>
    /// Systems include all things voxels
    /// </summary>
    public class VoxelSystemGroup : ComponentSystemGroup
    {
        public WorldSpawnSystem worldSpawnSystem;
        public ChunkSpawnSystem chunkSpawnSystem;
        public VoxelSpawnSystem voxelSpawnSystem;
        ChunkMeshBuilderSystem chunkMeshBuildSystem;
        ChunkMeshEndingSystem chunkMeshEndSystem;
        ChunkSideCullingSystem chunkSideCullSystem;
        ChunkToRendererSystem chunkUpdateSystem;
        ChunkWeightBuilder chunkWeightBuilder;
        ChunkSidesSystem chunkSideSystem;
        VoxelRaycastSystem voxelRaycastSystem;
        ChunkStreamSystem chunkStreamSystem;
        ChunkStreamEndSystem chunkStreamEndSystem;
        ChunkMapStarterSystem chunkMapStarterSystem;
        WorldStreamSystem worldStreamSystem;
        public VoxelPreviewSystem voxelPreviewSystem;
        public CharacterRaycastSystem characterRaycastSystem;
        private ChunkMapBuilderSystem chunkMapBuilderSystem;
        public ChunkMapCompleterSystem chunkMapCompleterSystem;

        public void Clear()
        {
            worldSpawnSystem.Clear();
            //chunkSpawnSystem.Clear();
        }

        public void SetMeta(GameDatam game)
        {
            List<VoxelDatam> voxels = game.voxels;
            List<MapDatam> maps = game.maps;
            List<VoxDatam> models = game.models;
            voxelSpawnSystem.meta = new Dictionary<int, VoxelDatam>();
            voxelSpawnSystem.voxelIDs = new List<int>();
            foreach (VoxelDatam voxel in voxels)
            {
                if (voxel.Value.id == 0)
                {
                    voxel.GenerateID();
                }
                voxelSpawnSystem.meta.Add(voxel.Value.id, voxel);
                voxelSpawnSystem.voxelIDs.Add(voxel.Value.id);
            }
            chunkUpdateSystem.meta = voxelSpawnSystem.meta;
            chunkUpdateSystem.voxelIDs = voxelSpawnSystem.voxelIDs;

            worldSpawnSystem.mapsMeta = new Dictionary<int, MapDatam>();
            foreach (MapDatam map in maps)
            {
                if (map.id == 0)
                {
                    map.GenerateID();
                }
                worldSpawnSystem.mapsMeta.Add(map.id, map);
            }
            worldSpawnSystem.skeletonsMeta = new Dictionary<int, SkeletonDatam>();
            List<SkeletonDatam> skeletons = game.skeletons;
            foreach (SkeletonDatam map in skeletons)
            {
                if (map.data.id == 0)
                {
                    map.GenerateID();
                }
                worldSpawnSystem.skeletonsMeta.Add(map.data.id, map);
            }

            worldSpawnSystem.modelsMeta = new Dictionary<int, VoxData>();
            foreach (VoxDatam vox in models)
            {
                if (vox.data.id == 0)
                {
                    vox.GenerateID();
                }
                worldSpawnSystem.modelsMeta.Add(vox.data.id, vox.data);
            }
        }

        public void Initialize(Unity.Entities.World space)
        {
            // entity spawn
            worldSpawnSystem = space.GetOrCreateSystem<WorldSpawnSystem>();
            chunkSpawnSystem = space.GetOrCreateSystem<ChunkSpawnSystem>();
            voxelSpawnSystem = space.GetOrCreateSystem<VoxelSpawnSystem>();
            AddSystemToUpdateList(worldSpawnSystem);
            AddSystemToUpdateList(chunkSpawnSystem);
            AddSystemToUpdateList(voxelSpawnSystem);
            
            // mesh gen
            chunkMeshBuildSystem = space.GetOrCreateSystem<ChunkMeshBuilderSystem>();
            chunkMeshEndSystem = space.GetOrCreateSystem<ChunkMeshEndingSystem>();
            chunkSideCullSystem = space.GetOrCreateSystem<ChunkSideCullingSystem>();
            chunkUpdateSystem = space.GetOrCreateSystem<ChunkToRendererSystem>();
            chunkSideSystem = space.GetOrCreateSystem<ChunkSidesSystem>();
            AddSystemToUpdateList(chunkMeshBuildSystem);
            AddSystemToUpdateList(chunkMeshEndSystem);
            AddSystemToUpdateList(chunkSideCullSystem);
            AddSystemToUpdateList(chunkUpdateSystem);
            AddSystemToUpdateList(chunkSideSystem);
            //chunkBuildStarterSystem = space.GetOrCreateSystem<ChunkBuildStarterSystem>();
            //AddSystemToUpdateList(chunkBuildStarterSystem);
            chunkWeightBuilder = space.GetOrCreateSystem<ChunkWeightBuilder>();
            AddSystemToUpdateList(chunkWeightBuilder);
            // player x voxel systems
            chunkStreamSystem = space.GetOrCreateSystem<ChunkStreamSystem>();
            chunkStreamEndSystem = space.GetOrCreateSystem<ChunkStreamEndSystem>();
            voxelRaycastSystem = space.GetOrCreateSystem<VoxelRaycastSystem>();
            voxelPreviewSystem = space.GetOrCreateSystem<VoxelPreviewSystem>();
            characterRaycastSystem = space.GetOrCreateSystem<CharacterRaycastSystem>();
            AddSystemToUpdateList(chunkStreamSystem);
            AddSystemToUpdateList(chunkStreamEndSystem);
            AddSystemToUpdateList(voxelRaycastSystem);
            AddSystemToUpdateList(voxelPreviewSystem);
            AddSystemToUpdateList(characterRaycastSystem);
            chunkMapStarterSystem = space.GetOrCreateSystem<ChunkMapStarterSystem>();
            AddSystemToUpdateList(chunkMapStarterSystem);

            worldStreamSystem = space.GetOrCreateSystem<WorldStreamSystem>();
            AddSystemToUpdateList(worldStreamSystem);

            chunkMapBuilderSystem = space.GetOrCreateSystem<ChunkMapBuilderSystem>();
            AddSystemToUpdateList(chunkMapBuilderSystem);
            chunkMapCompleterSystem = space.GetOrCreateSystem<ChunkMapCompleterSystem>();
            AddSystemToUpdateList(chunkMapCompleterSystem);


            if (Bootstrap.DebugChunks)
            {
                debugChunkSystem = space.GetOrCreateSystem<DebugChunkSystem>();
                AddSystemToUpdateList(debugChunkSystem);
            }
            if (!Bootstrap.isRenderChunks)
            {
                chunkSideSystem.Enabled = false;
            }
            SetLinks();
        }
        private DebugChunkSystem debugChunkSystem;

        void SetLinks()
        {
            worldSpawnSystem.chunkSpawnSystem = chunkSpawnSystem;
            chunkSpawnSystem.worldSpawnSystem = worldSpawnSystem;
            chunkSpawnSystem.voxelSpawnSystem = voxelSpawnSystem;
            voxelSpawnSystem.worldSpawnSystem = worldSpawnSystem;
            voxelSpawnSystem.chunkSpawnSystem = chunkSpawnSystem;
            chunkUpdateSystem.chunkSpawnSystem = chunkSpawnSystem;
            voxelRaycastSystem.worldSpawnSystem = worldSpawnSystem;
            voxelRaycastSystem.chunkSpawnSystem = chunkSpawnSystem;
            characterRaycastSystem.voxelRaycastSystem = voxelRaycastSystem;
            characterRaycastSystem.voxelPreviewSystem = voxelPreviewSystem;
            // voxel interaction
            chunkStreamEndSystem.worldSpawnSystem = worldSpawnSystem;
            chunkMeshEndSystem.worldSpawnSystem = worldSpawnSystem;
            chunkMeshEndSystem.chunkSpawnSystem = chunkSpawnSystem;
            worldStreamSystem.worldSpawnSystem = worldSpawnSystem;
            worldStreamSystem.chunkSpawnSystem = chunkSpawnSystem;
        }

        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            voxelRaycastSystem.cameraSystem = cameraSystemGroup.cameraSystem;
        }
    }
}
