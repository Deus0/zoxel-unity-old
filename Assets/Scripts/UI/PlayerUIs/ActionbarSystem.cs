using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    /// <summary>
    /// Todo
    /// Make flash white when gotten attacked for .3 seconds
    /// Make Actionbar and Skillbook and Assignment to actionbars
    /// </summary>
    [DisableAutoCreation]
    public class ActionbarSystem : PlayerUISpawnSystem
    {
        public Dictionary<int, SkillDatam> meta = new Dictionary<int, SkillDatam>();

        protected override void OnSpawnedPanel(Entity character, Entity panelUI, object spawnData)
        {
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(character).id;
            Skills skills = World.EntityManager.GetComponentData<Skills>(character);
            //EntityBunch iconGroup = new EntityBunch();
            List<Entity> statIcons = new List<Entity>();
            float2 iconSize = uiDatam.actionbarIconSize;
            for (int i = 0; i < uiDatam.skillbarIconsCount; i++)
            {
                Texture2D iconTexture = null;
                if (i < skills.skills.Length)
                {
                    SkillDatam skillMeta = meta[skills.skills[i].id];
                    if (skillMeta != null && skillMeta.texture)
                    {
                        iconTexture = skillMeta.texture.texture;
                    }
                }
                //float3 position = GetHorizontalListPosition(i, uiDatam.skillbarIconsCount);
                Entity icon = UIUtilities.SpawnVisualElement(
                    World.EntityManager,
                    panelUI,
                    new float3(0, 0, 0.1f),
                    iconSize,
                    iconTexture,
                    uiDatam.defaultPlayerIcon);
                statIcons.Add(icon);
                Color originalColor = Color.gray;
                Color selectedColor = Color.white;
                World.EntityManager.AddComponentData(icon,
                    new NavigationElementUI
                    {
                        animationTime = uiDatam.buttonAnimationTime,
                        timeSelected = UnityEngine.Time.realtimeSinceStartup,
                        originalColorR = (byte)(originalColor.r * 255),
                        originalColorG = (byte)(originalColor.g * 255),
                        originalColorB = (byte)(originalColor.b * 255),
                        selectedColorR = (byte)(selectedColor.r * 255),
                        selectedColorG = (byte)(selectedColor.g * 255),
                        selectedColorB = (byte)(selectedColor.b * 255),
                    });
            }
            //iconGroup.entities = statIcons.ToArray();
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(statIcons.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < statIcons.Count; i++)
            {
                children.children[i] = statIcons[i];
            }
            World.EntityManager.AddComponentData(panelUI, children);
            //Debug.LogError("Adding GridUI to new Inventory.");
            World.EntityManager.AddComponentData(panelUI, new GridUI
            {
                updated = 1,
                gridSize = new float2(5, 1),
                iconSize = iconSize,
                margins = new float2(0.003f, 0.003f),
                padding = new float2(0.003f, 0.003f),
            });
            byte uiIndex = ((byte)((int)PlayerUIType.Actionbar));
            World.EntityManager.SetComponentData(panelUI, new PanelUI
            {
                id = uiIndex,
                characterID = zoxID,
                orbitDepth = uiDatam.actionbarPosition.z,
                anchor = (byte)UIAnchoredPosition.BottomMiddle,
                positionOffset = new float2(uiDatam.actionbarPosition.x, uiDatam.actionbarPosition.y)
            });
            OnSelectedButton(zoxID, 0);
            SetSlotPosition(zoxID, 0);
        }

        public void SetSlotPosition(int id, int index)
        {
            if (uis.ContainsKey(id))
            {
                // get buttons
                var buttons = World.EntityManager.GetComponentData<Childrens>(uis[id]);
                foreach (var b in buttons.children.ToArray())
                {
                    NavigationElementUI b2 = World.EntityManager.GetComponentData<NavigationElementUI>(b);
                    b2.Deselect(UnityEngine.Time.time);
                    World.EntityManager.SetComponentData(b, b2);

                }
                var button = buttons.children[index];
                NavigationElementUI buttonSelection = World.EntityManager.GetComponentData<NavigationElementUI>(button);
                buttonSelection.Select(UnityEngine.Time.time);
                World.EntityManager.SetComponentData(button, buttonSelection);

            }
        }

        #region Spawning-Removing
        public struct SpawnActionbar : IComponentData
        {
            public Entity character;
        }
        public struct RemoveActionbar : IComponentData
        {
            public Entity character;
        }
        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnActionbar { character = character });
        }
        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveActionbar { character = character });
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            Entities.WithAll<SpawnActionbar>().ForEach((Entity e, ref SpawnActionbar command) =>
            {
                SpawnUI(command.character, command, uiDatam.actionbarPanel);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveActionbar>().ForEach((Entity e, ref RemoveActionbar command) =>
            {
                RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion
    }
}

// References
//public UIUtilities UIUtilities;
//public UIDatam uiData;
//public Material iconMaterialSelected;
// others
//private List<Entity> queue = new List<Entity>();
//public Dictionary<int, Entity> skillbars = new Dictionary<int, Entity>();
//public Dictionary<int, EntityBunch> skillbarIcons = new Dictionary<int, EntityBunch>();
//private Dictionary<int, Entity> selectedSlotIcons = new Dictionary<int, Entity>();
// sizes / values
//private float3 orbitPosition;
//private float2 panelSize;
//private bool isInit = false;
//float iconSize = uiData.skillbarIconSize;
//Entity barUI = UIUtilities.SpawnCharacterUI(character, orbitPosition, panelSize, uiData.skillbarPanel);
//skillbars.Add(statsID, barUI);
// spawn icons for skills
//EntityBunch entityBunch = new EntityBunch();
//entityBunch.entities = new Entity[uiDatam.skillbarIconsCount];
/*entityBunch.entities[i] = 
    UIUtilities.SpawnVisualElement(
        panelUI,
        GetIconPosition(i), 
        new float2(iconSize, iconSize),
        iconTexture, 
        uiData.skillbarIcon);*/
// Entity text = SpawnText(icon, ((int)inventory.items[i].quantity).ToString());
//statTexts.Add(text);
/*skillbarIcons.Add(statsID, entityBunch);
Entity selectionIcon = UIUtilities.SpawnVisualElement(
    barUI, GetIconPosition(0), new float2(iconSize, iconSize),
    uiData.selectedDefault, 
    uiData.selectedMaterial);
selectedSlotIcons.Add(statsID, selectionIcon);*/

/*void InitValues()
{
    if (!isInit)
    {
        isInit = true;
        orbitPosition = uiData.skillbarOrbitPosition;
        float iconSize = uiData.skillbarIconSize;
        panelSize.x = uiData.skillbarIconsCount * iconSize + (uiData.skillbarIconsCount - 1) * uiData.skillbarPadding + uiData.skillbarMargins.x * 2f;
        panelSize.y = iconSize + uiData.skillbarMargins.y * 2f;
        orbitPosition = UIUtilities.GetOrbitAnchors(UIAnchoredPosition.BottomMiddle, orbitPosition, panelSize);
        orbitPosition.y -= 0.02f;
    }
}

private float3 GetIconPosition(int index)
{
    float iconSize = uiData.skillbarIconSize;
    return new float3(
         uiData.skillbarMargins.x + index * (iconSize + uiData.skillbarPadding) - (panelSize.x / 2f) + (iconSize / 2f),
        -uiData.skillbarMargins.y + (panelSize.y / 2f) - (iconSize / 2f),
        uiData.skillbarIconPositionZ);
}

public void Clear()
{
    foreach (Entity e in skillbars.Values)
    {
        if (World.EntityManager.Exists(e))
        {
            World.EntityManager.DestroyEntity(e);
        }
    }
    skillbars.Clear();
    foreach (Entity e in selectedSlotIcons.Values)
    {
        if (World.EntityManager.Exists(e))
        {
            World.EntityManager.DestroyEntity(e);
        }
    }
    selectedSlotIcons.Clear();
}*/

/*#region Queue
public void QueueSkillbar(Entity entity)
{
    queue.Add(entity);
}

protected override void OnUpdate()
{
    InitValues();
    while (queue.Count > 0)
    {
        int index = queue.Count - 1;
        SpawnSkillbar(queue[index]);
        queue.RemoveAt(index);
    }
}
#endregion*/
