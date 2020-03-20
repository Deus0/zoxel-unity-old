using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Core;
using UnityEngine;

namespace Zoxel
{

    [DisableAutoCreation]
    public class NavigateUICompleterSystem : ComponentSystem
    {
        public const int maxUIs = 5;
        public CharacterSpawnSystem characterSpawnSystem;
        public StatsUISpawnSystem statsUISpawnSystem;
        public InventoryUISpawnSystem inventoryUISpawnSystem; 
        public ActionbarSystem actionbarSpawnSystem;
        //public UIUtilities UIUtilities;
        public QuestLogUISpawnSystem questLogUISpawnSystem;
        public MenuSpawnSystem menuSpawnSystem;
        public SkillbookUISpawnSystem skillbookUISpawnSystem;
        public DialogueUISpawnSystem dialogueUISpawnSystem;

        protected override void OnUpdate()
        {
            // draw debug lines between UIs
            // Draw debug lines between navigational elements
            //, ref Parent parent, Parent
            Entities.WithAll<NavigateUI>().ForEach((Entity e, ref NavigateUI navigate) =>
            {
                if (navigate.selectedIndex < 0)
                {
                    Debug.LogError("navigate.elementIndex: " + navigate.selectedIndex + " out of total: " + navigate.navigationElements.Length);
                    return;
                }
                if (navigate.selectedIndex >= navigate.navigationElements.Length)
                {
                    if (navigate.navigationElements.Length != 0)
                    {
                        Debug.LogError("navigate.elementIndex2: " + navigate.selectedIndex + " out of total: " 
                                + navigate.navigationElements.Length);
                    }
                    return;
                }
                if (navigate.clicked == 1)
                {
                    navigate.clicked = 0;
                    // buttonClicked
                    int clickedIndex = navigate.selectedIndex;
                    NavigateUIElement navigateUIElement = navigate.navigationElements[clickedIndex];
                    clickedIndex = navigateUIElement.targetIndex;
                    //Debug.LogError("NavigateUICompleterSystem Entities.WithAll - navigate.selectedIndex: " + navigate.selectedIndex
                    //     + ", navigateUIElement.targetIndex: " + navigateUIElement.targetIndex);
                    OnButtonClicked(navigate.character, navigateUIElement.ui, clickedIndex);
                }
                if (navigate.updated == 1)
                {
                    navigate.updated = 0;
                    int clickedIndex = navigate.selectedIndex;
                    NavigateUIElement navigateUIElement = navigate.navigationElements[clickedIndex];
                    clickedIndex = navigateUIElement.targetIndex;
                    //NavigateUIElement navigateUIElement = navigate.navigationElements[navigate.navigationIndex];
                    navigate.SetSelected(World.EntityManager, navigateUIElement.entity);
                    //OnSelected(navigate.characterID, navigateUIElement.uiIndex, clickedIndex);
                }
                /*byte uiIndex = navigate.navigationElements[navigate.selectedIndex].uiIndex;
                if (navigate.lastUIIndex != uiIndex)
                {
                    navigate.lastUIIndex = uiIndex;
                }*/
            });
        }

        private void OnButtonClicked(Entity player, Entity ui, int arrayIndex)
        {
            if (World.EntityManager.Exists(ui) == false)
            {
                return;
            }
            //Debug.LogError("NavigationUICompleter - OnButtonClicked: " + (PlayerUIType)(uiIndex) + ":" + arrayIndex);
            //NavigateUIElement navigate = navigation.navigationElements[navigation.navigationIndex];
            byte uiIndex = World.EntityManager.GetComponentData<PanelUI>(ui).id;
            if (uiIndex == ((byte)PlayerUIType.InventoryUI))
            {
                // Debug.LogError("Item Clicked: " + navigate.arrayIndex);
                inventoryUISpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.StatsUI))
            {
               statsUISpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.QuestlogUI))
            {
               questLogUISpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.SkillbookUI))
            {
                skillbookUISpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.Menu))
            {
                menuSpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.DialogueUI))
            {
                dialogueUISpawnSystem.OnClickedButton(player, ui, arrayIndex);
            }
            else { 
                Debug.LogError("ClickedButton Event not implemented.");
            }
        }

        private void OnSelected(int characterID, int uiIndex, int arrayIndex)
        {
           // NavigateUIElement navigate = navigation.navigationElements[navigation.navigationIndex];
            if (uiIndex == ((byte)PlayerUIType.InventoryUI))
            {
                //Debug.Log("New Item Selected: " + navigate.arrayIndex);
                inventoryUISpawnSystem.OnSelectedButton(characterID, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.StatsUI))
            {
                //Debug.LogError("New Stat Selected: " + navigate.arrayIndex);
                statsUISpawnSystem.OnSelectedButton(characterID, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.QuestlogUI))
            {
                //Debug.LogError("New Stat Selected: " + navigate.arrayIndex);
               questLogUISpawnSystem.OnSelectedButton(characterID, arrayIndex);
            }
            else if (uiIndex == ((byte)PlayerUIType.SkillbookUI))
            {
                skillbookUISpawnSystem.OnSelectedButton(characterID, arrayIndex);
            }
            else
            {
                // Debug.LogError("New Selected with Unknown UI: " + navigate.arrayIndex);
                menuSpawnSystem.OnSelectedButton(characterID, uiIndex, arrayIndex);
            }
        }
    }
}