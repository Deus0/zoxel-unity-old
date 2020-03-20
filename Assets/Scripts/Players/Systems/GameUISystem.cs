using Unity.Entities;

namespace Zoxel
{
    [DisableAutoCreation]
    public class GameUISystem : ComponentSystem
    {
        public const int maxUIs = 6;
        public InventoryUISpawnSystem inventoryUISpawnSystem;
        public QuestLogUISpawnSystem questlogUISpawnSystem;
        public StatsUISpawnSystem statsUISpawnSystem;
        public MapUISpawnSystem mapUISpawnSystem;
        public DialogueUISpawnSystem dialogueUISpawnSystem;
        public SkillbookUISpawnSystem skillbookUISpawnSystem;
        public EquipmentUISpawnSystem equipmentUISpawnSystem;

        public void Clear()
        {
            // from game UI to in game - clear the player spawned UIs?
            mapUISpawnSystem.Clear();
            inventoryUISpawnSystem.Clear();
            statsUISpawnSystem.Clear();
            questlogUISpawnSystem.Clear();
            dialogueUISpawnSystem.Clear();
            skillbookUISpawnSystem.Clear();
            equipmentUISpawnSystem.Clear();
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Controller>().ForEach((Entity character, ref Controller controller) =>
            {
                if (World.EntityManager.HasComponent<Character>(character) == false)
                {
                    return;
                }
                if (UnityEngine.Time.realtimeSinceStartup - controller.stateChangedTime < 0.5f)
                {
                    return;
                }
                if (controller.gameState == ((byte)GameState.GameUI))
                {
                    if (controller.Value.buttonRB == 1)
                    {
                        controller.stateChangedTime = UnityEngine.Time.realtimeSinceStartup;
                        RemovePreviousUI(character, controller.gameUIIndex);
                        if (controller.gameUIIndex == maxUIs) // max
                        {
                            controller.gameUIIndex = 0;
                        }
                        else
                        {
                            controller.gameUIIndex += 1;
                        }
                        AddGameUI(character, controller.gameUIIndex);
                    }
                    else if (controller.Value.buttonLB == 1)
                    {
                        controller.stateChangedTime = UnityEngine.Time.realtimeSinceStartup;
                        RemovePreviousUI(character, controller.gameUIIndex);
                        if (controller.gameUIIndex == 0)
                        {
                            controller.gameUIIndex = maxUIs; // max
                        }
                        else
                        {
                            controller.gameUIIndex -= 1;
                        }
                        AddGameUI(character, controller.gameUIIndex);
                    }
                }
            });
        }

        private void RemovePreviousUI(Entity character, int gameUIIndex)
        {
            //UnityEngine.Debug.LogError("(Queueing) Removing " + gameUIIndex + " from character: " + character.Index);
            if (gameUIIndex == ((byte)PlayerUIType.StatsUI))
            {
                StatsUISpawnSystem.RemoveUI(EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.InventoryUI))
            {
                InventoryUISpawnSystem.RemoveUI(EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.QuestlogUI))
            {
                QuestLogUISpawnSystem.RemoveUI(EntityManager, character);
            }
            //else if (gameUIIndex == ((byte)PlayerUIType.DialogueUI))
            //{
            //    DialogueUISpawnSystem.RemoveUI(EntityManager, character);
            //}
            else if (gameUIIndex == ((byte)PlayerUIType.MapUI))
            {
                MapUISpawnSystem.RemoveUI(EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.SkillbookUI))
            {
                SkillbookUISpawnSystem.RemoveUI(EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.EquipmentUI))
            {
                EquipmentUISpawnSystem.RemoveUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.Menu))
            {
                MenuSpawnSystem.RemoveUI(World.EntityManager, character);
            }
        }

        public void AddGameUI(Entity character, int gameUIIndex)
        {
            //UnityEngine.Debug.LogError("(Queueing) Spawning " + gameUIIndex + " on character: " + character.Index);
            if (gameUIIndex == ((byte)PlayerUIType.StatsUI))
            {
               // UnityEngine.Debug.LogError("(Queueing) [StatsUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                StatsUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.InventoryUI))
            {
               // UnityEngine.Debug.LogError("(Queueing) [InventoryUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                InventoryUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.QuestlogUI))
            {
                //  UnityEngine.Debug.LogError("(Queueing) [QuestlogUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                QuestLogUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            //else if (gameUIIndex == ((byte)PlayerUIType.DialogueUI))
            //{
               // UnityEngine.Debug.LogError("(Queueing) [DialogueUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                //DialogueUISpawnSystem.SpawnUI(World.EntityManager, character);
            //}
            else if (gameUIIndex == ((byte)PlayerUIType.MapUI))
            {
              //  UnityEngine.Debug.LogError("(Queueing) [MapUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                MapUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.SkillbookUI))
            {
                //UnityEngine.Debug.LogError("(Queueing) [SkillbookUI] Spawning " + gameUIIndex + " on character: " + character.Index);
                SkillbookUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.EquipmentUI))
            {
                EquipmentUISpawnSystem.SpawnUI(World.EntityManager, character);
            }
            else if (gameUIIndex == ((byte)PlayerUIType.Menu))
            {
                MenuSpawnSystem.SpawnUI(World.EntityManager, character, "PauseMenu");
            }
        }
    }
}
