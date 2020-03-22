using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;

namespace Zoxel
{
    /// <summary>
    /// todo: link it to characterUIs - and remove when removing character
    /// </summary>
    [DisableAutoCreation]
    public class CrosshairSpawnSystem : ComponentSystem
    {
        public UIDatam uiData;

        #region Spawning-Removing
        private struct SpawnCrosshair : IComponentData
        {
            public Entity character;
        }

        private struct RemoveCrosshair : IComponentData
        {
            public Entity character;
        }

        public static void SpawnUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnCrosshair { character = character });
        }

        public static void RemoveUI(EntityManager EntityManager, Entity character)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new RemoveCrosshair { character = character });
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCrosshair>().ForEach((Entity e, ref SpawnCrosshair command) =>
            {
                SpawnUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveCrosshair>().ForEach((Entity e, ref RemoveCrosshair command) =>
            {
                //RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }

        private Entity SpawnUI(Entity character)
        {
            Entity crosshair = UIUtilities.SpawnPanel(World.EntityManager,
                character, 
                uiData.crosshairMaterial,
                null,
                //uiData.crosshairPosition,
                new float2(uiData.crosshairSize, uiData.crosshairSize));
            UIUtilities.UpdateOrbiter(World.EntityManager, crosshair, new float3(0, 0, 0.5f), uiData.crosshairLerpSpeed);
            /*OrbitCamera orbit = World.EntityManager.GetComponentData<OrbitCamera>(crosshair);
            orbit.lerpSpeed = uiData.crosshairLerpSpeed;
            World.EntityManager.SetComponentData(crosshair, orbit);*/
            //crosshairs.Add(World.EntityManager.GetComponentData<ZoxID>(character).id, crosshair);
            return crosshair;
        }
        #endregion

    }
}
