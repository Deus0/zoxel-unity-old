using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Zoxel.Voxels;

namespace Zoxel
{
    /// <summary>
    /// Base class for player UIs
    /// </summary>
    [DisableAutoCreation]
    public class PlayerUISpawnSystem : ComponentSystem
    {
        public Dictionary<int, Entity> uis = new Dictionary<int, Entity>();
        public UIDatam uiDatam;
        public CharacterSpawnSystem characterSpawnSystem;

        protected override void OnUpdate() { }

        protected virtual void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData) { }
        public virtual void OnClickedButton(Entity player, Entity ui, int arrayIndex, ButtonType buttonType) { }
        public virtual void OnSelectedButton(int characterID, int arrayIndex) { }
        public virtual void OnSelectedButton(int characterID, int uiIndex, int arrayIndex) { }

        public virtual void Clear()
        {
            foreach (int key in uis.Keys)
            {
                RemoveEntity(key, false);
            }
            uis.Clear();
        }

        protected void RemoveUI(Entity character)
        {
            ZoxID id = World.EntityManager.GetComponentData<ZoxID>(character);
            RemoveEntity(id.id);
        }

        private void RemoveEntity(int id, bool isRemove = true)
        {
            if (uis.ContainsKey(id))
            {
                Entity e = uis[id];
                if (World.EntityManager.HasComponent<Childrens>(e))
                {
                    Childrens childrens = World.EntityManager.GetComponentData<Childrens>(e);
                    childrens.DestroyEntities(World.EntityManager);
                }
                if (World.EntityManager.HasComponent<RenderText>(e))
                {
                    RenderText text = World.EntityManager.GetComponentData<RenderText>(e);
                    text.DestroyLetters(World.EntityManager);
                }
                if (World.EntityManager.HasComponent<OutlineLink>(e))
                {
                    var outline = World.EntityManager.GetComponentData<OutlineLink>(e);
                    World.EntityManager.DestroyEntity(outline.outline);
                }
                World.EntityManager.DestroyEntity(e);
                if (isRemove)
                {
                    uis.Remove(id);
                }
            }
        }

        protected void SpawnUI(Entity parent, object spawnData, Material overrideMaterial = null)
        {
            if (World.EntityManager.Exists(parent) == false)
            {
                Debug.LogError("Cannot spawn UI on non existing parent.");
                return;
            }
            if (World.EntityManager.HasComponent<ZoxID>(parent) == false)
            {
                Debug.LogError("Character does not have ZoxID");
                return;
            }
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(parent).id;
            if (uis.ContainsKey(zoxID))
            {
                //Debug.LogError("UI already contains key: " + zoxID);
                return;
            }
            Material panelMaterial = uiDatam.defaultPlayerPanel;
            if (overrideMaterial != null)
            {
                panelMaterial = overrideMaterial;
            }
            Entity panelUI = UIUtilities.SpawnPanel(
                World.EntityManager,
                parent,
                panelMaterial,
                uiDatam.defaultPlayerOutline);
            //World.EntityManager.AddComponentData(panelUI, new ZoxID { id = zoxID });
            uis.Add(zoxID, panelUI);
            OnSpawnedPanel(parent, panelUI, spawnData);
        }

        public void SetText(int characterID, int iconArrayIndex, int newValue)
        {
            if (uis.ContainsKey(characterID))
            {
                Entity ui = uis[characterID];
                Childrens children = World.EntityManager.GetComponentData<Childrens>(ui);
                Entity icon = children.children[iconArrayIndex];
                Childrens iconChildren = World.EntityManager.GetComponentData<Childrens>(icon);
                Entity textEntity = iconChildren.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(newValue.ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
            }
        }
        /*public void SetText(int characterID, Level level)
        {
            if (uis.ContainsKey(characterID))
            {
                Stats stats = World.EntityManager.GetComponentData<Stats>(characterSpawnSystem.characters[characterID]);
                int iconArrayIndex =  stats.GetIconIndex(level);
                if (iconArrayIndex == -1)
                {
                    return;
                }
                Entity ui = uis[characterID];
                Childrens children = World.EntityManager.GetComponentData<Childrens>(ui);
                Entity icon = children.children[iconArrayIndex];
                Childrens iconChildren = World.EntityManager.GetComponentData<Childrens>(icon);
                // value
                Entity textEntity = iconChildren.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)level.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
                // experience required
                Entity textEntity2 = iconChildren.children[1];
                RenderText renderText2 = World.EntityManager.GetComponentData<RenderText>(textEntity2);
                renderText2.SetText(((int)level.experienceGained).ToString());
                World.EntityManager.SetComponentData(textEntity2, renderText2);
                // experience gained
                Entity textEntity3 = iconChildren.children[2];
                RenderText renderText3 = World.EntityManager.GetComponentData<RenderText>(textEntity3);
                renderText3.SetText(((int)level.experienceRequired).ToString());
                World.EntityManager.SetComponentData(textEntity3, renderText3);
            }
        }
        public void SetText(int characterID, Staz stat)
        {
            if (uis.ContainsKey(characterID))
            {
                Stats stats = World.EntityManager.GetComponentData<Stats>(characterSpawnSystem.characters[characterID]);
                int iconArrayIndex = stats.GetIconIndex(stat);
                if (iconArrayIndex == -1)
                {
                    return;
                }
                Entity ui = uis[characterID];
                Childrens children = World.EntityManager.GetComponentData<Childrens>(ui);
                Entity icon = children.children[iconArrayIndex];
                Childrens iconChildren = World.EntityManager.GetComponentData<Childrens>(icon);
                // value
                Entity textEntity = iconChildren.children[0];
                RenderText renderText = World.EntityManager.GetComponentData<RenderText>(textEntity);
                renderText.SetText(((int)stat.value).ToString());
                World.EntityManager.SetComponentData(textEntity, renderText);
            }
        }*/
    }
}