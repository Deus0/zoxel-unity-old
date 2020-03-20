using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    [DisableAutoCreation]
    public class NavigateStartSystem : ComponentSystem
    {
        public UIDatam uiDatam;

        protected override void OnUpdate()
        {
            Entities.WithAll<PanelUI, Childrens>().ForEach((Entity e, ref PanelUI panelUI, ref Childrens childrens) =>
            {
                if (panelUI.navigationDirty == 1)
                {
                    panelUI.navigationDirty = 0;
                    // should use children to get an array of all entities with button component attached
                    Entity[] entities = childrens.GetButtons(World.EntityManager);
                    if (panelUI.id == (byte)(PlayerUIType.Menu))
                    {
                        AddNavigation(e, entities, Color.gray, Color.cyan);
                    }
                    else
                    {
                        AddNavigation(e, entities, Color.gray, Color.white);
                    }

                }
            });
        }

        // If update positions in UI, update this
        // for all entities, add a navigationElement in it
        //      navigationStarterSystem will handle links
        //      calculate links here for closest positions
        //  SelectedSystem - Lerps colour of UI when selected

        /// <summary>
        /// For all children add navigation components
        /// NavigationStarter system will calculate the rest
        /// </summary>
        public void AddNavigation(Entity panelUI, Entity[] entities, Color originalColor, Color selectedColor)//, int characterID)
        {
            if (entities.Length == 0)
            {
                return; // no need for navigation without buttons.
            }
            //PanelUI panel = World.EntityManager.GetComponentData<PanelUI>(panelUI);
            Entity camera = World.EntityManager.GetComponentData<CameraLink>(panelUI).camera;
            if (World.EntityManager.Exists(camera) == false)
            {
                Debug.LogError("Cannot add navigation as camera on panel does not exist.");
                return;
            }
            NavigateUI navigation = new NavigateUI();
            List<float3> positions = new List<float3>();
            List<Entity> navigationParents = new List<Entity>();
            if (World.EntityManager.HasComponent<CharacterLink>(camera)) {
                navigation.character = World.EntityManager.GetComponentData<CharacterLink>(camera).character;
            }
            else {
                navigation.character = camera;
            }
            for (int i = 0; i < entities.Length; i++)
            {
                positions.Add(World.EntityManager.GetComponentData<Translation>(entities[i]).Value);
                navigationParents.Add(panelUI);
                // add a navigationElement UI here
                if (World.EntityManager.HasComponent<NavigationElementUI>(entities[i]))
                {
                    World.EntityManager.RemoveComponent<NavigationElementUI>(entities[i]);
                }
                World.EntityManager.AddComponentData(entities[i],
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
            List<Entity> entitiesList = new List<Entity>();
            entitiesList.AddRange(entities);
            navigation.Initialize(entitiesList, positions, navigationParents);
            navigation.SelectFirst(EntityManager, entities[0]);
            // add this to camera? otherwise is AI add it to character
            if (World.EntityManager.HasComponent<NavigateUI>(camera))
            {
                World.EntityManager.SetComponentData(camera, navigation);
            }
            else
            {
                World.EntityManager.AddComponentData(camera, navigation);
            }
        }

    }


}
