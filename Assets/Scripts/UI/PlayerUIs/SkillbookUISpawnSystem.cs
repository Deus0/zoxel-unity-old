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
    public class SkillbookUISpawnSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, SkillDatam> meta = new Dictionary<int, SkillDatam>();

        public override void OnSelectedButton(int characterID, int arrayIndex)
        {
            Entity character = characterSpawnSystem.characters[characterID];
            Skills stats = World.EntityManager.GetComponentData<Skills>(character);
            if (arrayIndex >= 0 && arrayIndex < stats.skills.Length)
            {
                //SetTooltipText(characterID, meta[stats.skills[arrayIndex].id].name);
            }
        }

        //public override void OnClickedButton(int characterID, int arrayIndex)
        //{
            //int originalArrayIndex = arrayIndex;
            //Entity character = characterSpawnSystem.characters[characterID];
            //Stats stats = World.EntityManager.GetComponentData<Stats>(character);
        //}

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            // spawn stat icon for each thing
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(character);
            Skills skills = World.EntityManager.GetComponentData<Skills>(character);

            #region StatIcons
            List<Entity> statIcons = new List<Entity>();
            //List<Entity> statTexts = new List<Entity>();
            float2 iconSize = uiDatam.defaultIconSize;

            for (int i = 0; i < skills.skills.Length; i++)
            {
                int metaID = skills.skills[i].id;
                if (meta.ContainsKey(metaID))
                {
                    statIcons.Add(UIUtilities.SpawnButton(
                        World.EntityManager, 
                        panelUI,
                        float3.zero,
                        iconSize,
                        meta[metaID].texture.texture, uiDatam.defaultPlayerIcon));
                    Childrens textLink = new Childrens { children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent) };
                    textLink.children[0] = UIUtilities.SpawnText(World.EntityManager, statIcons[statIcons.Count - 1],
                        ((int)skills.skills[i].attackDamage).ToString()); //, iconSize);
                    World.EntityManager.AddComponentData(statIcons[statIcons.Count - 1], textLink);
                }
                else
                {
                    Debug.LogError("Trying to add meta id in StatsUI (Stats(" + i + ")) that doesn't exist: " + metaID);
                }
            }
            #endregion
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(statIcons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < statIcons.Count; i++)
            {
                children.children[i] = statIcons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                updated = 1,
                gridSize = uiDatam.inventoryGridSize,
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.003f, 0.003f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.SkillbookUI));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID.id,
                orbitDepth = uiDatam.orbitDepth,
                anchor = (byte)UIAnchoredPosition.Middle
            });
            OnSelectedButton(zoxID.id, 0);
        }

        #region Spawning-Removing
        public struct SpawnSkillbookUI : IComponentData
        {
            public Entity character;
        }

        public struct RemoveSkillbookUI : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnSkillbookUI { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveSkillbookUI { character = character });
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnSkillbookUI>().ForEach((Entity e, ref SpawnSkillbookUI command) =>
            {
                //float2 panelSize = GetGridPanelSize(columns, rows);
                SpawnUI(command.character, command);//, panelSize);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveSkillbookUI>().ForEach((Entity e, ref RemoveSkillbookUI command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion
    }
}
