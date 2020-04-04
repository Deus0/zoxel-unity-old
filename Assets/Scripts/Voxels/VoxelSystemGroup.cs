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
        ChunkToRendererSystem chunkToRendererSystem;
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
        private ChunkRendererAnimationSystem chunkRendererAnimationSystem;
        private ChunkRenderSystem chunkRenderSystem;
        private DebugChunkSystem debugChunkSystem;
        private RenderSystem renderSystem;

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
            chunkToRendererSystem.meta = voxelSpawnSystem.meta;
            chunkToRendererSystem.voxelIDs = voxelSpawnSystem.voxelIDs;

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
            AddSystemToUpdateList(worldSpawnSystem);
            chunkSpawnSystem = space.GetOrCreateSystem<ChunkSpawnSystem>();
            AddSystemToUpdateList(chunkSpawnSystem);
            chunkRenderSystem = space.GetOrCreateSystem<ChunkRenderSystem>();
            AddSystemToUpdateList(chunkRenderSystem);
            voxelSpawnSystem = space.GetOrCreateSystem<VoxelSpawnSystem>();
            AddSystemToUpdateList(voxelSpawnSystem);
            
            // mesh gen
            chunkSideSystem = space.GetOrCreateSystem<ChunkSidesSystem>();
            AddSystemToUpdateList(chunkSideSystem);
            chunkSideCullSystem = space.GetOrCreateSystem<ChunkSideCullingSystem>();
            AddSystemToUpdateList(chunkSideCullSystem);
            chunkToRendererSystem = space.GetOrCreateSystem<ChunkToRendererSystem>();
            AddSystemToUpdateList(chunkToRendererSystem);
            chunkMeshBuildSystem = space.GetOrCreateSystem<ChunkMeshBuilderSystem>();
            AddSystemToUpdateList(chunkMeshBuildSystem);
            chunkMeshEndSystem = space.GetOrCreateSystem<ChunkMeshEndingSystem>();
            AddSystemToUpdateList(chunkMeshEndSystem);

            chunkWeightBuilder = space.GetOrCreateSystem<ChunkWeightBuilder>();
            AddSystemToUpdateList(chunkWeightBuilder);

            // maps
            chunkMapStarterSystem = space.GetOrCreateSystem<ChunkMapStarterSystem>();
            AddSystemToUpdateList(chunkMapStarterSystem);
            chunkMapBuilderSystem = space.GetOrCreateSystem<ChunkMapBuilderSystem>();
            AddSystemToUpdateList(chunkMapBuilderSystem);
            chunkMapCompleterSystem = space.GetOrCreateSystem<ChunkMapCompleterSystem>();
            AddSystemToUpdateList(chunkMapCompleterSystem);

            // player streaming
            worldStreamSystem = space.GetOrCreateSystem<WorldStreamSystem>();
            AddSystemToUpdateList(worldStreamSystem);
            chunkStreamSystem = space.GetOrCreateSystem<ChunkStreamSystem>();
            AddSystemToUpdateList(chunkStreamSystem);
            chunkStreamEndSystem = space.GetOrCreateSystem<ChunkStreamEndSystem>();
            AddSystemToUpdateList(chunkStreamEndSystem);

            // interact
            voxelRaycastSystem = space.GetOrCreateSystem<VoxelRaycastSystem>();
            AddSystemToUpdateList(voxelRaycastSystem);
            voxelPreviewSystem = space.GetOrCreateSystem<VoxelPreviewSystem>();
            AddSystemToUpdateList(voxelPreviewSystem);
            characterRaycastSystem = space.GetOrCreateSystem<CharacterRaycastSystem>();
            AddSystemToUpdateList(characterRaycastSystem);
            
            renderSystem = space.GetOrCreateSystem<RenderSystem>();
            AddSystemToUpdateList(renderSystem);

            if (Bootstrap.instance.isAnimateRenders)
            {
                chunkRendererAnimationSystem = space.GetOrCreateSystem<ChunkRendererAnimationSystem>();
                AddSystemToUpdateList(chunkRendererAnimationSystem);
            }
            
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

        void SetLinks()
        {
            worldSpawnSystem.chunkSpawnSystem = chunkSpawnSystem;
            chunkSpawnSystem.worldSpawnSystem = worldSpawnSystem;
            chunkSpawnSystem.voxelSpawnSystem = voxelSpawnSystem;
            voxelSpawnSystem.worldSpawnSystem = worldSpawnSystem;
            voxelSpawnSystem.chunkSpawnSystem = chunkSpawnSystem;
            voxelRaycastSystem.worldSpawnSystem = worldSpawnSystem;
            voxelRaycastSystem.chunkSpawnSystem = chunkSpawnSystem;
            characterRaycastSystem.voxelRaycastSystem = voxelRaycastSystem;
            characterRaycastSystem.voxelPreviewSystem = voxelPreviewSystem;
            // voxel interaction
            chunkStreamEndSystem.worldSpawnSystem = worldSpawnSystem;
            //chunkMeshEndSystem.worldSpawnSystem = worldSpawnSystem;
            //chunkMeshEndSystem.chunkSpawnSystem = chunkSpawnSystem;
            worldStreamSystem.worldSpawnSystem = worldSpawnSystem;
            worldStreamSystem.chunkSpawnSystem = chunkSpawnSystem;
            worldStreamSystem.chunkRenderSystem = chunkRenderSystem;
            chunkToRendererSystem.chunkSpawnSystem = chunkSpawnSystem;
            chunkToRendererSystem.chunkRenderSystem = chunkRenderSystem;
            if (chunkRenderSystem != null)
            {
                chunkRenderSystem.worldSpawnSystem = worldSpawnSystem;
                chunkRenderSystem.voxelSpawnSystem = voxelSpawnSystem;
            }
        }

        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            voxelRaycastSystem.cameraSystem = cameraSystemGroup.cameraSystem;
        }
    }
}
