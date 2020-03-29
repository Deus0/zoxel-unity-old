using Unity.Entities;
using Zoxel.UI;

namespace Zoxel
{
    [UpdateBefore(typeof(MovementSystemGroup))]
    public class PlayerSystemGroup : ComponentSystemGroup {
        private ControllerSystem controllerSystem;
        public PlayerInputSystem playerControllerSystem;
        public PlayerSkillsSystem playerSkillsSystem;
        public PlayerSpawnSystem playerSpawnSystem;
        private CursorStateSystem cursorStateSystem;
        public GameUISystem gameUISystem;
        private PlayerInteractSystem playerInteractSystem;

        public void Initialize(Unity.Entities.World space)
        {
            controllerSystem = space.GetOrCreateSystem<ControllerSystem>();
            playerControllerSystem = space.GetOrCreateSystem<PlayerInputSystem>();
            playerSkillsSystem = space.GetOrCreateSystem<PlayerSkillsSystem>();
            AddSystemToUpdateList(controllerSystem);
            AddSystemToUpdateList(playerControllerSystem);
            AddSystemToUpdateList(playerSkillsSystem);
            playerSpawnSystem = space.GetOrCreateSystem<PlayerSpawnSystem>();
            AddSystemToUpdateList(playerSpawnSystem);
            cursorStateSystem = space.GetOrCreateSystem<CursorStateSystem>();
            AddSystemToUpdateList(cursorStateSystem);
            gameUISystem = space.GetOrCreateSystem<GameUISystem>();
            AddSystemToUpdateList(gameUISystem);
            playerInteractSystem = space.GetOrCreateSystem<PlayerInteractSystem>();
            AddSystemToUpdateList(playerInteractSystem);
        }

        public void CombineWithUI(UISystemGroup uiSystemGroup)
        {
            gameUISystem.statsUISpawnSystem = uiSystemGroup.statsUISpawnSystem;
            gameUISystem.inventoryUISpawnSystem = uiSystemGroup.inventoryUISpawnSystem;
            gameUISystem.questlogUISpawnSystem = uiSystemGroup.questLogUISpawnSystem;
            gameUISystem.dialogueUISpawnSystem = uiSystemGroup.dialogueUISpawnSystem;
            gameUISystem.mapUISpawnSystem = uiSystemGroup.mapUISpawnSystem;
            gameUISystem.skillbookUISpawnSystem = uiSystemGroup.skillbookUISpawnSystem;
            gameUISystem.equipmentUISpawnSystem = uiSystemGroup.equipmentUISpawnSystem;
        }

        public void CombineWithGame(GameSystemGroup gameSystemGroup)
        {
            playerSpawnSystem.gameStartSystem = gameSystemGroup.gameStartSystem;
        }
    }
}