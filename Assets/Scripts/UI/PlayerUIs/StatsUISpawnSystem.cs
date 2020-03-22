using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zoxel.Voxels;

namespace Zoxel
{

    [DisableAutoCreation]
    public class StatsUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, StatDatam> meta = new Dictionary<int, StatDatam>();

        public override void OnSelectedButton(int characterID, int arrayIndex)
        {
            if (characterSpawnSystem.characters.ContainsKey(characterID) == false)
            {
                return;
            }
            Entity character = characterSpawnSystem.characters[characterID];
            Stats stats = World.EntityManager.GetComponentData<Stats>(character);
            if (arrayIndex >= 0 && arrayIndex < stats.stats.Length)
            {
                //SetTooltipText(characterID, meta[stats.stats[arrayIndex].id].name);
                return;
            }
            arrayIndex -= stats.stats.Length;
            if (arrayIndex >= 0 && arrayIndex < stats.states.Length)
            {
                //SetTooltipText(characterID, meta[stats.states[arrayIndex].id].name + " " + stats.states[arrayIndex].maxValue);
                return;
            }
            arrayIndex -= stats.states.Length;
            if (arrayIndex >= 0 && arrayIndex < stats.regens.Length)
            {
                //SetTooltipText(characterID, meta[stats.regens[arrayIndex].id].name);
                return;
            }
            arrayIndex -= stats.regens.Length;
            if (arrayIndex >= 0 && arrayIndex < stats.attributes.Length)
            {
                //SetTooltipText(characterID, meta[stats.attributes[arrayIndex].id].name);
                return;
            }
        }

        // this should be in a StatUISystem -> with selection and other events handled..?
        public override void OnClickedButton(Entity player, Entity ui, int arrayIndex, ButtonType buttonType)
        {
            if (World.EntityManager.HasComponent<Inventory>(player) == false) {
                Debug.LogError("Character " + player.Index + " does not have stats component.");
                return;
            }
            int originalArrayIndex = arrayIndex;
            Stats stats = World.EntityManager.GetComponentData<Stats>(player);
            // stat points
            int statPointID = -510241704;   // gotten from game settings?

            // statpoints is a base
            #region Indexing
            int statArrayIndex = -1;
            for (int i = 0; i < stats.stats.Length; i++)
            {
                if (stats.stats[i].id == statPointID)
                {
                    statArrayIndex = i;
                    break;
                }
            }
            if (statArrayIndex == -1)
            {
                return;
            }
            #endregion

            Staz statPoints = stats.stats[statArrayIndex];
            if (statPoints.value == 0)
            {
                return;
            }
            // update a stat!
            if (arrayIndex >= 0 && arrayIndex < stats.stats.Length)
            {
                return;
            }
            arrayIndex -= stats.stats.Length;
            // update states!
            if (arrayIndex >= 0 && arrayIndex < stats.states.Length)
            {
                return;
            }
            arrayIndex -= stats.states.Length;
            // update regens!
            if (arrayIndex >= 0 && arrayIndex < stats.regens.Length)
            {
                return;
            }
            arrayIndex -= stats.regens.Length;
            // update attributes!
            if (arrayIndex >= 0 && arrayIndex < stats.attributes.Length)
            {
                StatUpdateSystem.UpdateStat(World.EntityManager, player, StatType.Attribute, arrayIndex, 1);
            }
            arrayIndex -= stats.attributes.Length;
            // update regens!
            if (arrayIndex >= 0 && arrayIndex < stats.levels.Length)
            {
                return;
            }
            // decrease stat points
            StatUpdateSystem.UpdateStat(World.EntityManager, player, StatType.Base, statArrayIndex, -1);
        }

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            // spawn stat icon for each thing
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
            Stats stats = World.EntityManager.GetComponentData<Stats>(character);
            #region StatIcons
            List<Entity> statIcons = new List<Entity>();
            //List<Entity> statTexts = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;
            for (int i = 0; i < stats.stats.Length; i++)
            {
                if (meta.ContainsKey(stats.stats[i].id) == false)
                {
                    continue;
                }
                statIcons.Add(UIUtilities.SpawnButton(
                    World.EntityManager,
                    panelUI,
                    float3.zero,
                    iconSize,
                    meta[stats.stats[i].id].texture.texture, uiDatam.defaultPlayerIcon));
                var textA = UIUtilities.SpawnText(World.EntityManager, statIcons[statIcons.Count - 1],
                    ((int)stats.stats[i].value).ToString());//, iconSize);
                Childrens children2 = new Childrens { };
                children2.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                children2.children[0] = textA;
                World.EntityManager.AddComponentData(statIcons[statIcons.Count - 1], children2);
            }
            for (int i = 0; i < stats.states.Length; i++)
            {
                if (meta.ContainsKey(stats.states[i].id) == false)
                {
                    continue;
                }
                statIcons.Add(UIUtilities.SpawnButton(
                    World.EntityManager,
                    panelUI,
                    float3.zero,
                    iconSize,
                    meta[stats.states[i].id].texture.texture, uiDatam.defaultPlayerIcon));
                var textA = UIUtilities.SpawnText(World.EntityManager, statIcons[statIcons.Count - 1],
                    ((int)stats.states[i].value).ToString());//,// iconSize,
                                   // new float3(0, iconSize.y / 2f, 0));
                //var textB = UIUtilities.SpawnText(World.EntityManager, statIcons[statIcons.Count - 1],
                //                    ((int)stats.states[i].maxValue).ToString());//, iconSize);
                Childrens children2 = new Childrens { };
                children2.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                children2.children[0] = textA;
                //children2.children[1] = textB;
                World.EntityManager.AddComponentData(statIcons[statIcons.Count - 1], children2);
            }
            for (int i = 0; i < stats.regens.Length; i++)
            {
                Entity icon = UIUtilities.SpawnButton(
                    World.EntityManager, 
                    panelUI,
                    float3.zero,
                    iconSize,
                    meta[stats.regens[i].id].texture.texture, uiDatam.defaultPlayerIcon);
                Entity textA = UIUtilities.SpawnText(World.EntityManager, icon, ((int)stats.regens[i].value).ToString());//, iconSize);
                statIcons.Add(icon);
                Childrens children2 = new Childrens { };
                children2.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                children2.children[0] = textA;
                World.EntityManager.AddComponentData(statIcons[statIcons.Count - 1], children2);
            }
            for (int i = 0; i < stats.attributes.Length; i++)
            {
                Entity icon = UIUtilities.SpawnButton(
                    World.EntityManager, 
                    panelUI,
                    float3.zero, 
                    iconSize,
                    meta[stats.attributes[i].id].texture.texture, uiDatam.defaultPlayerIcon);
                statIcons.Add(icon);
                var textA = UIUtilities.SpawnText(World.EntityManager, icon, ((int)stats.attributes[i].value).ToString());//, iconSize);
                //statTexts.Add(text);
                Childrens children2 = new Childrens { };
                children2.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                children2.children[0] = textA;
                World.EntityManager.AddComponentData(statIcons[statIcons.Count - 1], children2);
            }
            for (int i = 0; i < stats.levels.Length; i++)
            {
                Entity icon = UIUtilities.SpawnButton(
                    World.EntityManager,
                    panelUI,
                    float3.zero,
                    iconSize,
                    meta[stats.levels[i].id].texture.texture, uiDatam.defaultPlayerIcon);
                statIcons.Add(icon);
                var textA = UIUtilities.SpawnText(World.EntityManager, icon,
                    ((int)stats.levels[i].value).ToString());//, iconSize);
                /*var textB = UIUtilities.SpawnText(World.EntityManager, icon,
                                    ((int)stats.levels[i].experienceGained).ToString(),
                                    //iconSize,
                                    new float3(0, iconSize.y / 3, 0));
                var textC = UIUtilities.SpawnText(World.EntityManager, icon,
                                    ((int)stats.levels[i].experienceRequired).ToString(),
                                    //iconSize,
                                    new float3(0, 2 * iconSize.y / 3, 0));*/
                Childrens children2 = new Childrens { };
                children2.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                children2.children[0] = textA;
                /*children2.children[1] = textB;
                children2.children[2] = textC;*/
                World.EntityManager.AddComponentData(icon, children2);
            }
            #endregion
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(statIcons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < statIcons.Count; i++)
            {
                children.children[i] = statIcons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            //float2 iconSize = new float2(uiDatam.skillbarIconSize, uiDatam.skillbarIconSize);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                dirty = 1,
                gridSize = uiDatam.statsUIGridSize,
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.003f, 0.003f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.StatsUI));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID.id,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
            //characterStatUIs.Add(zoxID.id, CharacterStatUIData);
            //CreateSelected(character, panelUI, ((byte)PlayerUIType.StatsUI), iconSize);
            OnSelectedButton(zoxID.id, 0);
        }

        // on updated stat -> then set texts or whatevers

        #region Spawning-Removing
        public struct SpawnStatsUI : IComponentData
        {
            public Entity character;
        }

        public struct RemoveStatsUI : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnStatsUI { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveStatsUI { character = character });
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnStatsUI>().ForEach((Entity e, ref SpawnStatsUI command) =>
            {
                //float2 panelSize = GetGridPanelSize(columns, rows);
                SpawnUI(command.character, command);//, panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveStatsUI>().ForEach((Entity e, ref RemoveStatsUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<UpdateStatUICommand>().ForEach((Entity e, ref UpdateStatUICommand command) =>
            {
                UpdateStatUI(command);
                World.EntityManager.DestroyEntity(e);
            });
        }


        public static void OnUpdatedStat(EntityManager EntityManager, Entity character, StatType statType, int statIndex)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new UpdateStatUICommand
            {
                character = character,
                statType = (byte)statType,
                statIndex = (byte)statIndex
            });
        }
        public struct UpdateStatUICommand : IComponentData
        {
            public Entity character;
            public byte statType;
            public byte statIndex;
        }
        public void UpdateStatUI(UpdateStatUICommand command)
        {
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(command.character);
            if (!uis.ContainsKey(zoxID.id))
            {
                return; // character died
            }
            Stats stats = World.EntityManager.GetComponentData<Stats>(command.character);
            Entity ui = uis[zoxID.id];
            Childrens children = World.EntityManager.GetComponentData<Childrens>(ui);
            int uiArrayIndex = command.statIndex;

            if (command.statType == (byte)StatType.Base)
            {
                Staz stat = stats.stats[command.statIndex];
                Entity icon = children.children[uiArrayIndex];
                Childrens texts = World.EntityManager.GetComponentData<Childrens>(icon);
                
                Entity textEntity = texts.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
            }
            else
            {
                uiArrayIndex += stats.stats.Length;
            }
            if (command.statType == (byte)StatType.State)
            {
                StateStaz stat = stats.states[command.statIndex];
                Entity icon = children.children[uiArrayIndex];
                Childrens texts = World.EntityManager.GetComponentData<Childrens>(icon);

                Entity textEntity = texts.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);

                /*Entity textEntity2 = texts.children[0];
                RenderText renderText2 = World.EntityManager.GetComponentData<RenderText>(textEntity2);
                renderText2.SetText(((int)stat.maxValue).ToString());
                World.EntityManager.SetComponentData(textEntity2, renderText2);*/
            }
            else
            {
                uiArrayIndex += stats.states.Length;
            }
            if (command.statType == (byte)StatType.Regen)
            {
                RegenStaz stat = stats.regens[command.statIndex];
                Entity icon = children.children[uiArrayIndex];
                Childrens texts = World.EntityManager.GetComponentData<Childrens>(icon);
                
                Entity textEntity = texts.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
            }
            else
            {
                uiArrayIndex += stats.regens.Length;
            }
            if (command.statType == (byte)StatType.Attribute)
            {
                AttributeStaz stat = stats.attributes[command.statIndex];
                Entity icon = children.children[uiArrayIndex];
                Childrens texts = World.EntityManager.GetComponentData<Childrens>(icon);
                
                Entity textEntity = texts.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
            }
            else
            {
                uiArrayIndex += stats.attributes.Length;
            }
            if (command.statType == (byte)StatType.Level)
            {
                Level stat = stats.levels[command.statIndex];
                Entity icon = children.children[uiArrayIndex];
                Childrens texts = World.EntityManager.GetComponentData<Childrens>(icon);
                Entity textEntity = texts.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
                // experience required
                /*Entity textEntity2 = texts.children[1];
                RenderText renderText2 = World.EntityManager.GetComponentData<RenderText>(textEntity2);
                renderText2.SetText(((int)stat.experienceGained).ToString());
                World.EntityManager.SetComponentData(textEntity2, renderText2);
                // experience gained
                Entity textEntity3 = texts.children[2];
                RenderText renderText3 = World.EntityManager.GetComponentData<RenderText>(textEntity3);
                renderText3.SetText(((int)stat.experienceRequired).ToString());
                World.EntityManager.SetComponentData(textEntity3, renderText3);*/
            }
        }
        #endregion
    }
}
