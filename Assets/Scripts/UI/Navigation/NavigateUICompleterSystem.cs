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

    public enum ButtonType
    {
        None,
        StartButton,
        SelectButton,
        ButtonA,
        ButtonB,
        ButtonX,
        ButtonY
    }

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
            Entities.WithAll<NavigateUI, Controller>().ForEach((Entity e, ref NavigateUI navigate, ref Controller controller) =>
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
                ButtonType buttonType = ButtonType.None;
                if (controller.Value.buttonA == 1)
                {
                    buttonType = ButtonType.ButtonA;
                }
                if (controller.Value.startButton == 1)
                {
                    buttonType = ButtonType.StartButton;
                }
                if (controller.Value.buttonX == 1)
                {
                    buttonType = ButtonType.ButtonX;
                }
                if (buttonType != ButtonType.None)
                {
                    int clickedIndex = navigate.selectedIndex;
                    NavigateUIElement navigateUIElement = navigate.navigationElements[clickedIndex];
                    clickedIndex = navigateUIElement.targetIndex;
                    OnButtonClicked(navigate.character, navigateUIElement.ui, clickedIndex, buttonType);
                }
                // when selection updates
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
            });
        }

        // what is the best way to handle button clicks
        //  maybe a generic one? after the inventory button is clicked i add the component of clicked onto the ui
        //      inside the inventorySystem i lookout for that component, as well as PanelUI, then i handle event there
        //      This removes any links
        private void OnButtonClicked(Entity player, Entity ui, int arrayIndex, ButtonType buttonType)
        {
            if (World.EntityManager.Exists(ui) == false)
            {
                return;
            }
            byte uiIndex = World.EntityManager.GetComponentData<PanelUI>(ui).id;
            if (uiIndex == ((byte)PlayerUIType.InventoryUI))
            {
                inventoryUISpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
            }
            else if (uiIndex == ((byte)PlayerUIType.StatsUI))
            {
               statsUISpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
            }
            else if (uiIndex == ((byte)PlayerUIType.QuestlogUI))
            {
               questLogUISpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
            }
            else if (uiIndex == ((byte)PlayerUIType.SkillbookUI))
            {
                skillbookUISpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
            }
            else if (uiIndex == ((byte)PlayerUIType.Menu))
            {
                menuSpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
            }
            else if (uiIndex == ((byte)PlayerUIType.DialogueUI))
            {
                dialogueUISpawnSystem.OnClickedButton(player, ui, arrayIndex, buttonType);
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