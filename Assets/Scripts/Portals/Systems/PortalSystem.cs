
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;

namespace Zoxel
{
    //[UpdateAfter(typeof(TravelerSystem))]
    //[UpdateAfter(typeof(TransformSystemGroup))]
    //[UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PortalSystem : ComponentSystem
    {
        public static float portalOpenDistance = 6;
        private const float portalDowngrade = 2;
        private const int recursionLimit = 1;
        private const float nearClipOffset = 0.05f;
        private const float nearClipLimit = 0.2f;
        private const bool isNormalRender = false;
        private Camera mainCamera;
        public Dictionary<int, Camera> portalCameras = new Dictionary<int, Camera>();
        public Dictionary<int, RenderTexture> portalCameraTextures = new Dictionary<int, RenderTexture>();

        protected override void OnCreate()
        {
            Enabled = false;
        }

        public static int GenerateUniqueID()
        {
            return System.Guid.NewGuid().GetHashCode();
        }

        // also create a portal camera for it

        public Entity SpawnPortal(float3 position, quaternion rotation,float2 portalSize)
        {
            var portalMesh = CreateQuadMesh(portalSize);
            float2 textureSize = new float2((1024 * portalSize.x) / portalDowngrade, (1024 * portalSize.y)  / portalDowngrade);
            Entity e = World.EntityManager.CreateEntity();
            int id = GenerateUniqueID();
            World.EntityManager.AddComponentData(e, new Portal{ id = id } );
            World.EntityManager.AddComponentData(e, new Translation{ Value = position } );
            World.EntityManager.AddComponentData(e, new Rotation { Value = rotation } );
            World.EntityManager.AddComponentData(e, new NonUniformScale { Value = new float3(1, 1, 1) }); // new float3(1, 1, 0.01f) } );
            World.EntityManager.AddComponentData(e, new LocalToWorld { } );
            World.EntityManager.AddComponentData(e, new RenderBounds {
                Value = new AABB
                {
                    Extents = new float3(portalMesh.bounds.extents.x, portalMesh.bounds.extents.y, 1)
                }
            });
            Material material = new Material(Shader.Find("Custom/Portal"));
            material.SetInt("displayMask", 1);
            material.name = "PortalMaterial[" + id + "]";
            material.SetColor("_InactiveColour", Color.black);
            material.enableInstancing = true;
            World.EntityManager.AddSharedComponentData(e, new RenderMesh { 
                mesh = portalMesh,
                material = material
            });
            Camera portalCamera = SpawnPortalCamera(id, textureSize);
            portalCameras.Add(id, portalCamera);
            SpawnPortalCameraTexture(id, portalCamera, textureSize);
            portalCameraTextures.Add(id, portalCamera.targetTexture);
            return e;
        }
        
        public static Mesh CreateQuadMesh(float2 size)
        {
            Mesh mesh = new Mesh();
            Vector3[] newVerts = new Vector3[4];
            newVerts[0] = new Vector3(-0.5f * size.x, -0.5f * size.y, 0);
            newVerts[1] = new Vector3(0.5f * size.x, -0.5f * size.y, 0);
            newVerts[2] = new Vector3(0.5f * size.x, 0.5f * size.y, 0);
            newVerts[3] = new Vector3(-0.5f * size.x, 0.5f * size.y, 0);
            mesh.vertices = newVerts;
            int[] indices = new int[6];
            indices[0] = 2;
            indices[1] = 1;
            indices[2] = 0;
            indices[3] = 3;
            indices[4] = 2;
            indices[5] = 0;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);
            mesh.uv = uvs;
            mesh.colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    
        Camera SpawnPortalCamera(int id, float2 textureSize)
        {
            //Camera mainCamera = Camera.main;
            GameObject newCamera = new GameObject(); //GameObject.Instantiate(mainCamera.gameObject);
            Camera portalCamera = newCamera.AddComponent<Camera>();
            portalCamera.enabled = isNormalRender;
            //portalCameraTextures.Add(id, portalCamera.viewTexture);
            newCamera.name = "PortalCamera[" + id + "]";
            //newCamera.tag = "Untagged";
            return portalCamera;
        }        
        
        void SpawnPortalCameraTexture(int id, Camera portalCamera, float2 textureSize)
        {
            var viewTexture = new RenderTexture((int)textureSize.x, (int)textureSize.y, 0);
            portalCamera.targetTexture = viewTexture;
            viewTexture.filterMode = FilterMode.Point;
        }

        public void LinkPortals(Entity portalA, Entity portalB)
        {
            var portalAport = World.EntityManager.GetComponentData<Portal>(portalA);
            portalAport.linkedPortal = portalB;
            World.EntityManager.SetComponentData(portalA, portalAport);
            var portalBport = World.EntityManager.GetComponentData<Portal>(portalB);
            portalBport.linkedPortal = portalA;
            World.EntityManager.SetComponentData(portalB, portalBport);

            RenderMesh portalALinkedRenderMesh = World.EntityManager.GetSharedComponentData<RenderMesh>(portalAport.linkedPortal);
            portalALinkedRenderMesh.material.SetTexture("_MainTex", portalCameraTextures[portalAport.id]);
            World.EntityManager.SetSharedComponentData(portalAport.linkedPortal, portalALinkedRenderMesh);
            
            RenderMesh portalBLinkedRenderMesh = World.EntityManager.GetSharedComponentData<RenderMesh>(portalBport.linkedPortal);
            portalBLinkedRenderMesh.material.SetTexture("_MainTex", portalCameraTextures[portalBport.id]);
            World.EntityManager.SetSharedComponentData(portalBport.linkedPortal, portalBLinkedRenderMesh);
        }

        public void UnlinkPortals(Entity portalA, Entity portalB)
        {

        }

        // this isn't perfect as doesn't get the centre point properly of the object
        private Bounds CreateBounds(RenderBounds otherBounds, Matrix4x4 localToWorld)
        {
            Bounds newBounds = new Bounds();
            newBounds.center = localToWorld.MultiplyPoint(otherBounds.Value.Center);
            newBounds.extents = localToWorld.MultiplyVector(otherBounds.Value.Extents);
            newBounds.extents = new float3(newBounds.extents.x, newBounds.extents.y, 1);
            return newBounds;
        }


        private void SetMainCamera()
        {
            var newMain = Camera.main;
            if (newMain != mainCamera)
            {
                mainCamera = newMain;
                if (mainCamera != null)
                {
                    foreach(var portalCamera in portalCameras.Values)
                    {
                        //portalCamera.shadows = mainCamera.shadows;
                        portalCamera.depth = mainCamera.depth - 1;
                        portalCamera.backgroundColor = mainCamera.backgroundColor;
                        portalCamera.farClipPlane = mainCamera.farClipPlane; // portalFarClipPlane;
                        portalCamera.clearFlags = mainCamera.clearFlags;
                        portalCamera.fieldOfView = mainCamera.fieldOfView;
                    }
                }
            }
        }


        protected override void OnUpdate() {}
        public void ManualUpdate()
        {
            SetMainCamera();
            //Debug.LogError("UPdating Portals.");
            Entities.WithAll<Portal, LocalToWorld, Translation, RenderBounds>().ForEach((Entity e, ref Portal portal, 
                    ref LocalToWorld localToWorld, ref Translation translation, ref RenderBounds renderBounds) =>
            {
                // spawn portal camera
                // for all portals 
                var portalCamera = portalCameras[portal.id];
                var portalRenderMesh =  World.EntityManager.GetSharedComponentData<RenderMesh>(e);
                var portalMatrix = localToWorld.Value; //World.EntityManager.GetComponentData<LocalToWorld>(e).Value;
                var portalBounds = CreateBounds(renderBounds, portalMatrix); // portalRenderMesh.mesh.bounds;
                var portalPosition = translation.Value; //World.EntityManager.GetComponentData<Translation>(e).Value;
                var portalForward = math.normalize(localToWorld.Forward);  //World.EntityManager.GetComponentData<LocalToWorld>(e).Forward);
                if (float.IsNaN(portalBounds.center.x))
                {
                    return;
                }
                var linkedPortalRenderMesh =  World.EntityManager.GetSharedComponentData<RenderMesh>(portal.linkedPortal);
                var linkedPortalPosition = World.EntityManager.GetComponentData<Translation>(portal.linkedPortal).Value;
                var linkedPortalMatrix = World.EntityManager.GetComponentData<LocalToWorld>(portal.linkedPortal).Value;
                var linkedPortalBounds = CreateBounds(World.EntityManager.GetComponentData<RenderBounds>(portal.linkedPortal), linkedPortalMatrix); //linkedPortalRenderMesh.mesh.bounds;
                //var portalLinkedCamera = portalLinkedCameras[portal.id];
                var linkedPortalMaterial = linkedPortalRenderMesh.material;
                if (float.IsNaN(linkedPortalBounds.center.x))
                {
                    Debug.LogError("NANIII ALERT");
                    return;
                }

                float3 cameraPosition = float3.zero;
                Matrix4x4 cameraMatrix = Matrix4x4.identity;
                if (mainCamera != null)
                {
                    cameraPosition = mainCamera.transform.position;
                    cameraMatrix = mainCamera.transform.localToWorldMatrix;
                }
                var distanceToCamera = math.distance(cameraPosition, linkedPortalPosition);
                bool didRender = false;
                if (distanceToCamera <= portalOpenDistance)
                {
                    didRender = Render(
                        mainCamera,
                        mainCamera.projectionMatrix, cameraMatrix, 
                        portalCamera, 
                        portalMatrix, portalBounds,
                        linkedPortalMatrix, linkedPortalBounds,
                        portalPosition, portalForward,
                        e, ref portalRenderMesh, distanceToCamera);
                }
                if (!didRender)
                {
                    linkedPortalMaterial.SetInt("displayMask", 0);
                }
                else
                {
                    linkedPortalMaterial.SetInt("displayMask", 1);
                }
            });
        }

        public bool Render(
                Camera playerCamera, 
                Matrix4x4 playerProjectionMatrix,Matrix4x4 playerTransformMatrix,
                Camera portalCamera, 
                Matrix4x4 portalMatrix, Bounds portalBounds,
                Matrix4x4 linkedPortalMatrix, Bounds linkedPortalBounds, 
                float3 portalPosition, float3 portalForward,
                Entity portal, ref RenderMesh renderMesh, float distanceToCamera)//, Material linkedPortalMaterial)
        {
            // Skip rendering the view from this portal if player is not looking at the linked portal
            //Debug.LogError(linkedPortalBounds);
            if (distanceToCamera > 2 && !CameraUtility.VisibleFromCamera (linkedPortalBounds, playerCamera))
            {
                return false;
            }
            Matrix4x4 localToWorldMatrix = playerTransformMatrix;//playerCamera.transform.localToWorldMatrix;
            var renderPositions = new float3[recursionLimit];
            var renderRotations = new quaternion[recursionLimit];
            int startIndex = 0;
            portalCamera.projectionMatrix = playerProjectionMatrix; //playerCamera.projectionMatrix;
            Matrix4x4 linkedPortalWorldToLocalMatrix = math.inverse(linkedPortalMatrix);
            for (int i = 0; i < recursionLimit; i++) 
            {
                if (i > 0)
                {
                    // No need for recursive rendering if linked portal is not visible through this portal
                    if (!CameraUtility.BoundsOverlap (portalMatrix, portalBounds, linkedPortalMatrix, linkedPortalBounds, portalCamera))
                    {
                        break;
                    }
                }
                localToWorldMatrix = portalMatrix * linkedPortalWorldToLocalMatrix * localToWorldMatrix;
                int renderOrderIndex = recursionLimit - i - 1;
                renderPositions[renderOrderIndex] = ((Vector3) localToWorldMatrix.GetColumn(3));
                if (float.IsNaN(renderPositions[renderOrderIndex].x))
                {
                    //Debug.LogError("NAN in portal system.");
                    return false;
                }
                renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;
                startIndex = renderOrderIndex;
            }
            // Hide portal mesh so that camera can see through portal
            var preMesh = renderMesh.mesh;
            renderMesh.mesh = null;
            World.EntityManager.SetSharedComponentData(portal, renderMesh);
            //linkedPortalMaterial.SetInt ("displayMask", 0);
            for (int i = startIndex; i < recursionLimit; i++)
            {
                portalCamera.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
                //var preFarClipPlane = playerCamera.farClipPlane;
                //playerCamera.farClipPlane = portalCamera.farClipPlane;
                //SetNearClipPlane(playerCamera, portalCamera, portalPosition, portalForward);
                SetNearClipPlane(playerCamera, portalCamera, portalPosition, portalForward);
                portalCamera.Render();
                //playerCamera.farClipPlane = preFarClipPlane;
            }
            renderMesh.mesh = preMesh;
            World.EntityManager.SetSharedComponentData(portal, renderMesh);
            return true;
        }

        // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
        // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
        void SetNearClipPlane(Camera playerCamera, Camera portalCamera, float3 portalPosition, float3 portalForward)
        {
            float3 portalCameraPosition = portalCamera.transform.position;
            var dot = (int) math.ceil(math.sign(math.dot(portalForward, portalPosition - portalCameraPosition))); 
            var portalCameraMatrixInverse = portalCamera.worldToCameraMatrix;
            float3 camSpacePos = portalCameraMatrixInverse.MultiplyPoint (portalPosition);
            float3 camSpaceNormal = portalCameraMatrixInverse.MultiplyVector(portalForward) * dot;
            float camSpaceDst = -(math.dot(camSpacePos, camSpaceNormal)) + nearClipOffset;
            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs (camSpaceDst) > nearClipLimit)
            {
                float4 clipPlaneCameraSpace = new float4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
                // Update projection based on new clip plane
                // Calculate matrix with player cam so that player camera settings (fov, etc) are used
                portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            } else {
                portalCamera.projectionMatrix = playerCamera.projectionMatrix;
            }
        }
    }
}

// Learning resource:
// http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
    
//if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
//{
    /* if (viewTexture != null)
    {
        viewTexture.Release();
    }*/
    // Render the view from the portal camera to the view texture
    // Display the view texture on the screen of the linked portal
//}