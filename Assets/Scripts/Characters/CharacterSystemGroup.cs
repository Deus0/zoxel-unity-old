using Unity.Entities;
using Zoxel.UI;

namespace Zoxel
{
    public class CharacterSystemGroup : ComponentSystemGroup
    {
        public CharacterDeathSystem characterDeathSystem;
        public CharacterSpawnSystem characterSpawnSystem;
        public TurretSpawnerSystem turretSpawnSystem;

        public void Initialize(Unity.Entities.World space)
        {
            characterSpawnSystem = space.GetOrCreateSystem<CharacterSpawnSystem>();
            characterDeathSystem = space.GetOrCreateSystem<CharacterDeathSystem>();
            turretSpawnSystem = space.GetOrCreateSystem<TurretSpawnerSystem>();
            AddSystemToUpdateList(characterSpawnSystem);
            AddSystemToUpdateList(characterDeathSystem);
            AddSystemToUpdateList(turretSpawnSystem);
            SetLinks();
        }
        void SetLinks()
        {
            characterDeathSystem.characterSpawnSystem = characterSpawnSystem;
        }

        public void Clear()
        {
            characterSpawnSystem.Clear();
            turretSpawnSystem.Clear();
        }

        public void SetMeta(GameDatam data)
        {
            characterSpawnSystem.meta = data.GetCharacters();
            characterSpawnSystem.classMeta = data.GetClasses();
            characterSpawnSystem.items = data.GetItems();
        }

        public void CombineWithPlayers(PlayerSystemGroup playerSystemGroup)
        {
            characterSpawnSystem.playerSpawnSystem = playerSystemGroup.playerSpawnSystem;
            characterDeathSystem.playerSpawnSystem = playerSystemGroup.playerSpawnSystem;
        }
        public void CombineWithVoxels(Zoxel.Voxels.VoxelSystemGroup voxelSystemGroup)
        {
            characterSpawnSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
        }
        public void CombineWithItems(ItemSystemGroup itemSystemGroup)
        {
            characterDeathSystem.itemSpawnSystem = itemSystemGroup.itemSpawnSystem;
        }

        public void CombineWithSkills(SkillSystemGroup skillSystemGroup)
        {
            characterSpawnSystem.skillsSystem = skillSystemGroup.skillsSystem;
            characterDeathSystem.skillsSystem = skillSystemGroup.skillsSystem;
        }
            
        public void CombineWithUI(UISystemGroup uiSystemGroup)
        {
            characterSpawnSystem.actionbarSpawnSystem = uiSystemGroup.actionbarSpawnSystem;
            characterSpawnSystem.inventoryUISpawnSystem = uiSystemGroup.inventoryUISpawnSystem;
            characterSpawnSystem.statbarSystem = uiSystemGroup.statbarSystem;
            turretSpawnSystem.statbarSystem = uiSystemGroup.statbarSystem;
        }
        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            characterSpawnSystem.cameraSystem = cameraSystemGroup.cameraSystem;
        }
        public void CombineWithGame(GameSystemGroup gameSystemGroup)
        {
            characterSpawnSystem.saveSystem = gameSystemGroup.saveSystem;
            characterSpawnSystem.gameStartSystem = gameSystemGroup.gameStartSystem;
        }
    }
}
