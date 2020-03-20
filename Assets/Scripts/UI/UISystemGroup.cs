using Unity.Entities;


namespace Zoxel
{

    public class UISystemGroup : ComponentSystemGroup
    {
        // Core
        private StatbarPositionerSystem statBarSystem;
        private StatBarUpdaterSystem statbarUpdateSystem;
        private TrailerPositionerSystem trailerPositionerSystem;
        private TrailerStarterSystem trailerStarterSystem;
        private StatBarFaderSystem statbarFaderSystem;
        public DamagePopupSystem damagePopupSystem;
        public CrosshairSpawnSystem crosshairSpawnSystem;
        public DialogueSystem dialogueSystem;
        public RenderTextSystem renderTextSystem;
        public NavigateUISystem navigateUISystem;
        public NavigateUICompleterSystem navigateUICompleterSystem;
        public SelectedUISystem selectedUISystem;
        public GridUISystem gridUISystem;
        private NavigateStartSystem navigateStartSystem;

        // Player
        public MenuSpawnSystem menuSpawnSystem;
        public ActionbarSystem actionbarSpawnSystem;
        public StatsUISpawnSystem statsUISpawnSystem;
        public SkillbookUISpawnSystem skillbookUISpawnSystem;
        public InventoryUISpawnSystem inventoryUISpawnSystem;
        public EquipmentUISpawnSystem equipmentUISpawnSystem;
        public QuestLogUISpawnSystem questLogUISpawnSystem;
        public MapUISpawnSystem mapUISpawnSystem;
        public DialogueUISpawnSystem dialogueUISpawnSystem;
        public StatbarSystem statbarSystem;

        public PanelUISystem panelUISystem;

        public void Clear()
        {
            //crosshairSpawnSystem.Clear();
            statbarSystem.Clear();
            actionbarSpawnSystem.Clear();
            inventoryUISpawnSystem.Clear();
            skillbookUISpawnSystem.Clear();
        }

        public void Initialize(Unity.Entities.World space)
        {
            // stat bars
            statbarSystem = space.GetOrCreateSystem<StatbarSystem>();
            AddSystemToUpdateList(statbarSystem);
            statBarSystem = space.GetOrCreateSystem<StatbarPositionerSystem>();
            AddSystemToUpdateList(statBarSystem);
            statbarUpdateSystem = space.GetOrCreateSystem<StatBarUpdaterSystem>();
            AddSystemToUpdateList(statbarUpdateSystem);
            trailerPositionerSystem = space.GetOrCreateSystem<TrailerPositionerSystem>();
            AddSystemToUpdateList(trailerPositionerSystem);
            trailerStarterSystem = space.GetOrCreateSystem<TrailerStarterSystem>();
            AddSystemToUpdateList(trailerStarterSystem);
            statbarFaderSystem = space.GetOrCreateSystem<StatBarFaderSystem>();
            AddSystemToUpdateList(statbarFaderSystem);
            damagePopupSystem = space.GetOrCreateSystem<DamagePopupSystem>();
            AddSystemToUpdateList(damagePopupSystem);
            navigateUISystem = space.GetOrCreateSystem<NavigateUISystem>();
            AddSystemToUpdateList(navigateUISystem);
            navigateUICompleterSystem = space.GetOrCreateSystem<NavigateUICompleterSystem>();
            AddSystemToUpdateList(navigateUICompleterSystem);
            gridUISystem = space.GetOrCreateSystem<GridUISystem>();
            AddSystemToUpdateList(gridUISystem);
            navigateStartSystem = space.GetOrCreateSystem<NavigateStartSystem>();
            AddSystemToUpdateList(navigateStartSystem);
            

            selectedUISystem = space.GetOrCreateSystem<SelectedUISystem>();
            AddSystemToUpdateList(selectedUISystem);
            panelUISystem = space.GetOrCreateSystem<PanelUISystem>();
            AddSystemToUpdateList(panelUISystem);

            // Player UIs
            crosshairSpawnSystem = space.GetOrCreateSystem<CrosshairSpawnSystem>();
            AddSystemToUpdateList(crosshairSpawnSystem);
            actionbarSpawnSystem = space.GetOrCreateSystem<ActionbarSystem>();
            AddSystemToUpdateList(actionbarSpawnSystem);
            inventoryUISpawnSystem = space.GetOrCreateSystem<InventoryUISpawnSystem>();
            AddSystemToUpdateList(inventoryUISpawnSystem);
            equipmentUISpawnSystem = space.GetOrCreateSystem<EquipmentUISpawnSystem>();
            AddSystemToUpdateList(equipmentUISpawnSystem);

            statsUISpawnSystem = space.GetOrCreateSystem<StatsUISpawnSystem>();
            AddSystemToUpdateList(statsUISpawnSystem);

            skillbookUISpawnSystem = space.GetOrCreateSystem<SkillbookUISpawnSystem>();
            AddSystemToUpdateList(skillbookUISpawnSystem);

            dialogueUISpawnSystem = space.GetOrCreateSystem<DialogueUISpawnSystem>();
            AddSystemToUpdateList(dialogueUISpawnSystem);
            dialogueSystem = space.GetOrCreateSystem<DialogueSystem>();
            AddSystemToUpdateList(dialogueSystem);
            questLogUISpawnSystem = space.GetOrCreateSystem<QuestLogUISpawnSystem>();
            AddSystemToUpdateList(questLogUISpawnSystem);
            menuSpawnSystem = space.GetOrCreateSystem<MenuSpawnSystem>();
            AddSystemToUpdateList(menuSpawnSystem);
            renderTextSystem = space.GetOrCreateSystem<RenderTextSystem>();
            AddSystemToUpdateList(renderTextSystem);
            mapUISpawnSystem = space.GetOrCreateSystem<MapUISpawnSystem>();
            AddSystemToUpdateList(mapUISpawnSystem);

            SetLinks();
        }
        void SetLinks()
        {
            navigateUICompleterSystem.statsUISpawnSystem = statsUISpawnSystem;
            navigateUICompleterSystem.inventoryUISpawnSystem = inventoryUISpawnSystem;
            navigateUICompleterSystem.actionbarSpawnSystem = actionbarSpawnSystem;
            navigateUICompleterSystem.questLogUISpawnSystem = questLogUISpawnSystem;
            navigateUICompleterSystem.menuSpawnSystem = menuSpawnSystem;
            navigateUICompleterSystem.skillbookUISpawnSystem = skillbookUISpawnSystem;
            navigateUICompleterSystem.dialogueUISpawnSystem = dialogueUISpawnSystem;
        }

        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            menuSpawnSystem.cameraSystem = cameraSystemGroup.cameraSystem;
            damagePopupSystem.cameraSystem = cameraSystemGroup.cameraSystem;
        }
        public void CombineWithPlayers(PlayerSystemGroup playerSystemGroup)
        {
            menuSpawnSystem.playerSpawnSystem = playerSystemGroup.playerSpawnSystem;
        }
        public void CombineWithGame(GameSystemGroup gameSystemGroup)
        {
            menuSpawnSystem.saveSystem = gameSystemGroup.saveSystem;
        }

        public void CombineWithVoxels(Voxels.VoxelSystemGroup voxelSystemGroup) //ChunkMapCompleterSystem chunkMapCompleterSystem)
        {
            mapUISpawnSystem.chunkMapSystem = voxelSystemGroup.chunkMapCompleterSystem;
            menuSpawnSystem.chunkSpawnSystem = voxelSystemGroup.chunkSpawnSystem;
            menuSpawnSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            trailerStarterSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            statbarUpdateSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            inventoryUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            statsUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            questLogUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            menuSpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            mapUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            navigateUICompleterSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            skillbookUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            equipmentUISpawnSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }

        public void SetMeta(GameDatam data)
        {
            inventoryUISpawnSystem.meta = data.GetItems();
            equipmentUISpawnSystem.meta = data.GetItems();
            questLogUISpawnSystem.meta = data.GetQuests();
            statsUISpawnSystem.meta = data.GetStats();
            dialogueUISpawnSystem.meta = data.GetDialogues();
            dialogueSystem.meta = data.GetDialogues();
            actionbarSpawnSystem.meta = data.GetSkills();
            skillbookUISpawnSystem.meta = data.GetSkills();
            menuSpawnSystem.classes = data.classes;
            menuSpawnSystem.startingCharacter = data.startingCharacter;
            menuSpawnSystem.startingMap = data.startingMap;
            SetUIData(data.uiData);
        }

        public void SetUIData(UIDatam uiData)
        {
            crosshairSpawnSystem.uiData = uiData;
            statbarSystem.uiData = uiData;
            statbarFaderSystem.uiData = uiData;
            actionbarSpawnSystem.uiDatam = uiData;
            inventoryUISpawnSystem.uiDatam = uiData;
            damagePopupSystem.uiDatam = uiData;
            statsUISpawnSystem.uiDatam = uiData;
            mapUISpawnSystem.uiDatam = uiData;
            questLogUISpawnSystem.uiDatam = uiData;
            dialogueUISpawnSystem.uiDatam = uiData;
            menuSpawnSystem.uiDatam = uiData;
            skillbookUISpawnSystem.uiDatam = uiData;
            renderTextSystem.uiData = uiData;
            gridUISystem.uiDatam = uiData;
            navigateStartSystem.uiDatam = uiData;
            equipmentUISpawnSystem.uiDatam = uiData;
            panelUISystem.uiData = uiData;
            dialogueSystem.uiDatam = uiData;
        }
    }
}
