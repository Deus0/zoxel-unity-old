using Unity.Entities;
using Zoxel.Voxels;

namespace Zoxel.WorldGeneration
{
    public class WorldSystemGroup : ComponentSystemGroup
    {
        public static bool disableMonsterSpawning = false;
        public WorldGenerationStarterSystem worldGenerationStarterSystem;
        public WorldGenerationCompleterSystem worldGenerationCompleterSystem;
        //public BiomeGenerationSystem biomeGenerationSystem;
        public TerrainGenerationSystem terrainGenerationSystem;
        public TownGenerationSystem townGenerationSystem;
        public MonsterSpawnSystem monsterSpawnSystem;
        private BiomeGenerationSystem biomeGenerationSystem;
        private HeightMapGenerationSystem heightMapGenerationSystem;
        private ChunkMapBiomeBuilderSystem chunkMapBiomeBuilderSystem;

        public void CombineWithVoxels(VoxelSystemGroup voxelSystemGroup)
        {
            worldGenerationStarterSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
            if (!disableMonsterSpawning)
            {
                monsterSpawnSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
            }
        }
        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            if (!disableMonsterSpawning)
            {
                monsterSpawnSystem.cameraSystem = cameraSystemGroup.cameraSystem;
            }
        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            if (!disableMonsterSpawning)
            {
                monsterSpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            }
        }

        public void Initialize(Unity.Entities.World space)
        {
            biomeGenerationSystem = space.GetOrCreateSystem<BiomeGenerationSystem>();
            AddSystemToUpdateList(biomeGenerationSystem);
            heightMapGenerationSystem = space.GetOrCreateSystem<HeightMapGenerationSystem>();
            AddSystemToUpdateList(heightMapGenerationSystem);
            chunkMapBiomeBuilderSystem = space.GetOrCreateSystem<ChunkMapBiomeBuilderSystem>();
            AddSystemToUpdateList(chunkMapBiomeBuilderSystem);
            terrainGenerationSystem = space.GetOrCreateSystem<TerrainGenerationSystem>();
            AddSystemToUpdateList(terrainGenerationSystem);
            townGenerationSystem = space.GetOrCreateSystem<TownGenerationSystem>();
            AddSystemToUpdateList(townGenerationSystem);
            worldGenerationStarterSystem = space.GetOrCreateSystem<WorldGenerationStarterSystem>();
            AddSystemToUpdateList(worldGenerationStarterSystem);
            worldGenerationCompleterSystem = space.GetOrCreateSystem<WorldGenerationCompleterSystem>();
            AddSystemToUpdateList(worldGenerationCompleterSystem);
            if (!disableMonsterSpawning)
            {
                monsterSpawnSystem = space.GetOrCreateSystem<MonsterSpawnSystem>();
                AddSystemToUpdateList(monsterSpawnSystem);
            }
        }
    }
}
