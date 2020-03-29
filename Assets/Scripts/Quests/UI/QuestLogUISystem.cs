using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using Zoxel.UI;

namespace Zoxel
{
    // TODO: Remove UI after removing panel

    public struct QuestlogUI : IComponentData
    {
        public byte id;
    }

    /// <summary>
    /// Displays a line of text for the name of the quest
    ///     underneath displays icons for each quest objective - and a number for the amount of objectives complete 
    /// </summary>
    [DisableAutoCreation]
    public class QuestLogUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, QuestDatam> meta = new Dictionary<int, QuestDatam>();
        

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            if (World.EntityManager.HasComponent<QuestLog>(character) == false)
            {
                Debug.LogError("Tried to add character without questlog.");
                return;
            }
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(character).id;
            QuestLog questlog = World.EntityManager.GetComponentData<QuestLog>(character);

            //EntityBunch iconGroup = new EntityBunch();
            //EntityBunch textGroup = new EntityBunch();
            List<Entity> statIcons = new List<Entity>();
            List<Entity> statTexts = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;
            for (int i = 0; i < questlog.quests.Length; i++)
            {
                Texture2D iconTexture = null;
                if (questlog.quests[i].metaID != 0)
                {
                    QuestDatam questMeta = meta[questlog.quests[i].metaID];
                    if (questMeta != null && questMeta.texture)
                    {
                        iconTexture = questMeta.texture.texture;
                    }
                }
                float3 position = float3.zero;// GetGridPosition(i, 3, 3);
                Entity icon = UIUtilities.SpawnVisualElement(
                    World.EntityManager,
                    panelUI,
                    position,
                    iconSize, 
                    iconTexture, uiDatam.defaultPlayerIcon);
                //Entity text = SpawnText(icon, ((int)questlog.quests[i].GetCompleted()).ToString());
                Entity text = UIUtilities.SpawnText(World.EntityManager, icon, ((int)questlog.quests[i].GetCompleted()).ToString());//, iconSize);
                statIcons.Add(icon);
                statTexts.Add(text);
            }
            //iconGroup.entities = statIcons.ToArray(); // new Entity[stats.stats.Length];
            //textGroup.entities = statTexts.ToArray(); // new Entity[stats.stats.Length];

            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(statIcons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < statIcons.Count; i++)
            {
                children.children[i] = statIcons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                dirty = 1,
                gridSize = uiDatam.questlogGridSize,
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.003f, 0.003f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.QuestlogUI));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
            OnSelectedButton(zoxID, 0);
        }
        /*protected override float2 GetIconSize()
        {
            return new float2(uiDatam.skillbarIconSize, uiDatam.skillbarIconSize);
        }*/

        public override void OnSelectedButton(int characterID, int arrayIndex)
        {
            Entity character = characterSpawnSystem.characters[characterID];
            QuestLog questlog = World.EntityManager.GetComponentData<QuestLog>(character);
            if (arrayIndex >= 0 && arrayIndex < questlog.quests.Length)
            {
                //SetTooltipText(characterID, meta[questlog.quests[arrayIndex].metaID].name + " " + questlog.quests[arrayIndex].GetCompleted());
            }
        }

        /*private float3 GetNumberPosition()
        {
            return new float3(uiDatam.skillbarIconSize * 0.3f, -uiDatam.skillbarIconSize * 0.3f, -0.001f);
        }

        private float GetFontSize()
        {
            return uiDatam.skillbarIconSize * 0.3f;
        }

        public Entity SpawnDigit(Entity icon, int singleDigit, int arrayIndex, int totalDigits)
        {
            return UIUtilities.SpawnDigit(icon, GetNumberPosition(), GetFontSize(), singleDigit, arrayIndex, totalDigits);
        }

        public void SetDigitPosition(Entity digit, int arrayIndex, int totalDigits)
        {
            UIUtilities.SetDigitPosition(digit, GetNumberPosition(), GetFontSize(), arrayIndex, totalDigits);
        }*/

        #region Spawning-Removing

        public struct SpawnQuestlogUI : IComponentData
        {
            public Entity character;
        }

        public struct RemoveQuestlogUI : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnQuestlogUI { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveQuestlogUI { character = character });
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnQuestlogUI>().ForEach((Entity e, ref SpawnQuestlogUI command) =>
            {
                //float2 panelSize = GetGridPanelSize(3, 3);
                SpawnUI(command.character, command ); //,panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveQuestlogUI>().ForEach((Entity e, ref RemoveQuestlogUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });

            // put this in quest completer system
            Entities.WithAll<QuestLog>().ForEach((Entity e, ref QuestLog questLog) =>
            {
                if (questLog.updated == 1)
                {
                    questLog.updated = 0;
                    //Debug.LogError("QuestLog has updated with: " + questLog.quests.Length);
                    for (int i = 0; i < questLog.quests.Length; i++)
                    {
                        if (questLog.quests[i].HasUpdated())
                        {
                            // update quest ui
                            // RefreshQuestUI(World.EntityManager.GetComponentData<ZoxID>(e).id, i);
                            //Debug.LogError("Quest has updated " + i + ": " + questLog.quests[i].GetCompleted());
                            for (int j = 0; j < questLog.quests[i].blocks.Length; j++)
                            {
                                if (questLog.quests[i].blocks[j].HasUpdated())
                                {
                                    // update questblock UI
                                    //Debug.LogError("Quest Block has updated [" + questLog.quests[i].blocks[j].completed +
                                     //   " out of " + questLog.quests[i].blocks[j].maxCompleted + "]");
                                }
                            }
                        }
                        //else
                        //{
                            //Debug.LogError("Quest did not update at: " + i);
                        //}
                    }
                }
            });
        }
        #endregion
    }
}
