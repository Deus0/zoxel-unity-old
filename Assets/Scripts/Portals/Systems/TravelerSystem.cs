using Unity.Entities;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel
{
    [UpdateBefore(typeof(CameraSystemGroup))]
    //[UpdateAfter(typeof(PortalSystem))]
    //[UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class TravelerSystem : ComponentSystem
    {
        public static float nearClipPlaneAddition = 0.08f;
		private EntityQuery portalQuery;

		protected override void OnCreate()
        {
            base.OnCreate();
			portalQuery = GetEntityQuery(ComponentType.ReadOnly<Portal>(), ComponentType.ReadOnly<Translation>());
		}

        protected override void OnUpdate()
        {
            Entities.WithAll<Traveler, Translation, Rotation>().ForEach((Entity e, ref Traveler traveler, ref Translation translation, ref Rotation rotation) =>
            {
                var characterPosition = translation.Value;
                float3 offsetCameraPosition = float3.zero;
                
                Camera playerCam = Camera.main;
                if (playerCam != null)
                {
                    float3 cameraPosition = playerCam.transform.position;
                    offsetCameraPosition = cameraPosition - characterPosition;
                    characterPosition = cameraPosition;
                }

                if (World.EntityManager.Exists(traveler.portal))
                {
                    var portalPosition = traveler.originalPortalPosition;
                    //Debug.DrawLine(characterPosition, portalPosition, Color.cyan);
                    // if close to portal - within like 4 units or something idk
                    float distanceToPortal = math.distance(characterPosition, portalPosition);
                    traveler.distanceToPortal = distanceToPortal;
                    var portalForward = World.EntityManager.GetComponentData<LocalToWorld>(traveler.portal).Forward;
                    RepositionPortal(playerCam, traveler, characterPosition, portalForward);
                    int sideOfPortal = SideOfPortal(characterPosition, portalPosition, portalForward);
                    if (distanceToPortal <= 1.5f)
                    {
                        if (sideOfPortal != traveler.portalSide)
                        {
                            if (UnityEngine.Time.time - traveler.lastTeleported >= 1)
                            {
                                traveler.lastTeleported = UnityEngine.Time.time;
                                //Debug.LogError("Teleported character.");
                                TeleportTraveler(ref translation, ref rotation, ref traveler, e);
                                RepositionPortal(playerCam, traveler, translation.Value + offsetCameraPosition, World.EntityManager.GetComponentData<LocalToWorld>(traveler.portal).Forward);
                                SearchForPortal(ref traveler, translation.Value + offsetCameraPosition);
                            }
                   
                        }
                    }
                    else
                    {
                        traveler.portalSide = sideOfPortal;
                        SearchForPortal(ref traveler, characterPosition);
                    }
                }
                else
                {
                    SearchForPortal(ref traveler, characterPosition);
                }
            });
        }

        private void RepositionPortal(Camera playerCam, Traveler traveler, float3 cameraPosition, float3 portalForward)
        {
            const float portalRepositionDistance = 0.5f;    // 0.5
            float nearClipPlane = playerCam.nearClipPlane + nearClipPlaneAddition;
            float halfHeight = nearClipPlane * Mathf.Tan(playerCam.fieldOfView * portalRepositionDistance * Mathf.Deg2Rad);
            float halfWidth = halfHeight * playerCam.aspect;
            float dstToNearClipPlaneCorner = new Vector3 (halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;
            //Transform screenT = screen.transform;
            bool camFacingSameDirAsPortal = math.dot(portalForward, traveler.originalPortalPosition  - cameraPosition) > 0;
            float3 newPosition = portalForward * screenThickness * ((camFacingSameDirAsPortal) ? portalRepositionDistance : -portalRepositionDistance); //  Vector3.forward
            World.EntityManager.SetComponentData(traveler.portal, new Translation { Value = traveler.originalPortalPosition + newPosition });
        }
        private int SideOfPortal(float3 characterPosition, float3 portalPosition, float3 portalForward)
        {
            return (int) math.ceil(math.sign(math.dot(characterPosition - portalPosition, portalForward))); 
        }

        private void TeleportTraveler(ref Translation travelerPosition, ref Rotation travelerRotation, ref Traveler traveler, Entity travelerEntity) //, float3 characterPosition, float3 portalPosition)
        {
            // Traveler
            var travelerMatrix = World.EntityManager.GetComponentData<LocalToWorld>(travelerEntity).Value;
            
            // Portal
            Entity portalEntity = traveler.portal;
            var portalPosition = World.EntityManager.GetComponentData<Translation>(portalEntity).Value;
            var portalRotation = World.EntityManager.GetComponentData<Rotation>(portalEntity).Value;
            var portalMatrixInverse =  math.inverse(World.EntityManager.GetComponentData<LocalToWorld>(portalEntity).Value);

            // linked portal
            var linkedPortalEntity = World.EntityManager.GetComponentData<Portal>(portalEntity).linkedPortal;
            var linkedPortalPosition = World.EntityManager.GetComponentData<Translation>(linkedPortalEntity).Value;
            var linkedPortalRotation = World.EntityManager.GetComponentData<Rotation>(linkedPortalEntity).Value;
            var linkedPortalMatrix = World.EntityManager.GetComponentData<LocalToWorld>(linkedPortalEntity).Value;

            float3 offsetFromPortal = travelerPosition.Value - portalPosition;
            float3 newOffset = math.mul(linkedPortalRotation, (math.mul(math.inverse(portalRotation), offsetFromPortal)));
            travelerPosition.Value = linkedPortalPosition + newOffset;
            quaternion newRotation = math.mul((linkedPortalRotation), (math.mul(math.inverse(portalRotation), travelerRotation.Value)));
            travelerRotation.Value = newRotation;

            traveler.lastCheckedClosestPortal = UnityEngine.Time.time - 1f;
        }

        private void SearchForPortal(ref Traveler traveler, float3 characterPosition)
        {
            traveler.hasChecked = 0;
            // find new portal
            if (UnityEngine.Time.time - traveler.lastCheckedClosestPortal >= 1f)
            { 
                traveler.lastCheckedClosestPortal = UnityEngine.Time.time;
                var portals = portalQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
                float closestDistance = PortalSystem.portalOpenDistance;
                int closestIndex = -1;
                float3 closestPosition = float3.zero;
                for (int i = 0; i < portals.Length; i++)
                {
                    // distance to
                    float newDistance = math.distance(portals[i].Value, characterPosition);
                    if (newDistance < closestDistance)
                    {
                        closestDistance = newDistance;
                        closestIndex = i;
                        closestPosition = portals[i].Value;
                    }
                }
                if (closestIndex != -1)
                {
                    var portalEntities = portalQuery.ToEntityArray(Allocator.TempJob);
                    if (!World.EntityManager.Exists(traveler.portal) || traveler.portal.Index != portalEntities[closestIndex].Index)
                    {
                        if (World.EntityManager.Exists(traveler.portal))
                        {
                            World.EntityManager.SetComponentData(traveler.portal, new Translation { Value = traveler.originalPortalPosition });
                        }
                        if (World.EntityManager.Exists(traveler.portal))
                        {
                            World.EntityManager.SetComponentData(traveler.portal, new Translation { Value = traveler.originalPortalPosition });
                        }
                        traveler.portal = portalEntities[closestIndex];
                        traveler.originalPortalPosition = closestPosition;
                        traveler.portalSide = SideOfPortal(characterPosition, closestPosition,
                                World.EntityManager.GetComponentData<LocalToWorld>(traveler.portal).Forward);
                    }
                    portalEntities.Dispose();
                }
                portals.Dispose();
            }
        }
    }
}
