using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;

namespace Zoxel
{
    public struct EntityBunch
    {
        public Entity[] entities;

        public void Clear(EntityManager entityManager)
        {
            foreach (Entity e in entities)
            {
                if (entityManager.HasComponent<RenderText>(e))
                {
                    entityManager.GetComponentData<RenderText>(e).DestroyLetters(entityManager);
                }
                if (entityManager.Exists(e))
                {
                    entityManager.DestroyEntity(e);
                }
            }
        }
    }

    // first make skillbarUI use this system
    public static class UIUtilities
    {
        private static EntityArchetype panelArchtype;
        private static EntityArchetype iconArchtype;

        public static Entity SpawnText(EntityManager EntityManager, Entity parent, string text) //, float2 iconSize)
        {
            return SpawnText(EntityManager, parent, text, float3.zero); //  iconSize, 
        }

        public static Entity SpawnText(EntityManager EntityManager, Entity parent, string text,
            float3 positionOffset, byte ColorR = 0, byte ColorG = 255, byte ColorB = 0, float fontSizer = 0.014f) // float2 iconSize, 
        {
            float2 fontSize = new float2(fontSizer, fontSizer);
            RenderText renderText = new RenderText();
            //renderText.alignment = 1;
            renderText.fontSize = fontSize.x;
            renderText.colorR = ColorR;
            renderText.colorG = ColorG;
            renderText.colorB = ColorB;
            renderText.SetText(text);
            float2 panelSize = renderText.GetPanelSize();
            Entity textEntity = SpawnVisualElement(
                        EntityManager,
                        parent,
                        positionOffset, // + ((new float3(iconSize.x, -iconSize.y, 0) / 2f) - new float3(0, -fontSize.y, 0) / 2f),
                        panelSize,
                        null,
                        null);
            // should get size from text when set - it will set size depending on individual font sizes
            /*if (isCentred == 1)
            {
                renderText.offsetX = -panelSize.x * 0.5f;// uiDatam.skillbarIconSize / 2f;
            }*/
            EntityManager.AddComponentData(textEntity, renderText);
            return textEntity;
        }

        public static float3 GetOrbitAnchors(UIAnchoredPosition anchor, float3 orbitPosition, float2 panelSize)
        {
            float2 originalOrbitValue = new float2(orbitPosition.x, orbitPosition.y);
            if (anchor == UIAnchoredPosition.BottomLeft)
            {
                orbitPosition.x = -0.5f + panelSize.x / 2f;
                orbitPosition.y = 0.5f - panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.BottomMiddle)
            {
                orbitPosition.y = 0.5f - panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.BottomRight)
            {
                orbitPosition.x = 0.5f - panelSize.x / 2f;
                orbitPosition.y = 0.5f - panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.TopRight)
            {
                orbitPosition.x = 0.5f - panelSize.x / 2f;
                orbitPosition.y = -0.5f + panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.TopMiddle)
            {
                orbitPosition.y = -0.5f + panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.TopLeft)
            {
                orbitPosition.x = -0.5f + panelSize.x / 2f;
                orbitPosition.y = -0.5f + panelSize.y / 2f;
            }
            else if (anchor == UIAnchoredPosition.Left)
            {
                orbitPosition.x = -0.5f + panelSize.x / 2f;
            }
            else if (anchor == UIAnchoredPosition.Right)
            {
                orbitPosition.x = 0.5f - panelSize.x / 2f;
            }

            orbitPosition.x += originalOrbitValue.x;
            orbitPosition.y += originalOrbitValue.y;
            return orbitPosition;
        }

        public static Entity SpawnCharacterUI(EntityManager EntityManager, 
            Entity parent, 
            Material baseMaterial)
        {
            return SpawnCharacterUI(EntityManager, parent,  float3.zero, float2.zero, baseMaterial);
        }

        public static Entity SpawnCharacterUI(EntityManager EntityManager, Entity character, float3 orbitPosition, float2 quadSize, Material baseMaterial)
        {
            if (panelArchtype.Valid == false)
            {
                panelArchtype = EntityManager.CreateArchetype(
                     typeof(PanelUI),
                     typeof(CameraLink),
                     typeof(OrbitCamera),
                     typeof(RenderBounds),
                     // transform
                     typeof(Translation),
                     typeof(Rotation),
                     typeof(NonUniformScale),
                     typeof(LocalToWorld),
                     // renderer
                     typeof(RenderMesh));
            }
            //float3 spawnPosition = EntityManager.GetComponentData<Translation>(character).Value;
            Material materialInstance = new Material(baseMaterial);
            //materialInstance.SetFloat("_QueueOffset", 100);
            Mesh panelMesh = MeshUtilities.CreateQuadMesh(quadSize);
            Entity characterUI = EntityManager.CreateEntity(panelArchtype);
            // fix bounds flickers mesh
            RenderBounds b = new RenderBounds { 
                Value =  new AABB { 
                    Extents =   new float3 (quadSize.x, quadSize.y, 0.5f)
                } 
            };
            EntityManager.SetComponentData(characterUI, b);
            EntityManager.SetComponentData(characterUI, new Rotation { Value = quaternion.identity });
            EntityManager.SetComponentData(characterUI, new NonUniformScale { Value = new float3(1, 1, 1) });
            EntityManager.SetSharedComponentData(characterUI, new RenderMesh {
                material = materialInstance,
                mesh = panelMesh,
                receiveShadows = true
            });
            if (EntityManager.HasComponent<CameraLink>(character))
            {
                CameraLink cameraLink = EntityManager.GetComponentData<CameraLink>(character);
                EntityManager.SetComponentData(characterUI, cameraLink);
                if (EntityManager.HasComponent<CharacterUIList>(cameraLink.camera))
                {
                    // update UI List
                    CharacterUIList uiList = EntityManager.GetComponentData<CharacterUIList>(cameraLink.camera);
                    // expand list
                    Entity[] oldUIs = uiList.uis.ToArray();
                    List<Entity> uisList = new List<Entity>();
                    for (int i = 0; i < oldUIs.Length; i++)
                    {
                        if (EntityManager.Exists(oldUIs[i]))
                        {
                            uisList.Add(oldUIs[i]);
                        }
                    }
                    uisList.Add(characterUI);
                    uiList.uis = new BlitableArray<Entity>(uisList.Count, Unity.Collections.Allocator.Persistent);
                    for (int i = 0; i < uisList.Count; i++)
                    {
                        uiList.uis[i] = uisList[i];
                    }
                    // uiList.uis[uis.Length] = characterUI;
                    EntityManager.SetComponentData(cameraLink.camera, uiList);
                }
            }
            else
            {
                Debug.LogError("No Camera link assigned on character..");
            }
            return characterUI;
        }

        public static void UpdateOrbiter(EntityManager EntityManager, Entity panelUI, float3 orbitPosition, float orbitLerpSpeed)
        {
            CameraLink cameraLink = EntityManager.GetComponentData<CameraLink>(panelUI);
            OrbitCamera orbitor = new OrbitCamera
            {
                orbitPosition = orbitPosition,
                lerpSpeed = orbitLerpSpeed
            };
            var frustumHeight = 2.0f * orbitor.orbitPosition.z * math.tan(math.radians(cameraLink.fov) * 0.5f);
            var frustumWidth = frustumHeight * cameraLink.aspectRatio;
            float positionX = orbitor.orbitPosition.x * frustumWidth;
            float positionY = -orbitor.orbitPosition.y * frustumHeight;
            orbitor.position = new float3(positionX, positionY, orbitor.orbitPosition.z);
            EntityManager.SetComponentData(panelUI, orbitor);
            if (EntityManager.Exists(cameraLink.camera))
            {
                float3 cameraPosition = EntityManager.GetComponentData<Translation>(cameraLink.camera).Value;
                quaternion cameraRotation = EntityManager.GetComponentData<Rotation>(cameraLink.camera).Value;
                EntityManager.SetComponentData(panelUI, new Translation { Value = orbitor.GetTranslatedPosition(cameraPosition, cameraRotation) });
            }
        }

        public static Entity SpawnButtonWithText(EntityManager EntityManager, Entity panelUI, float3 position, float buttonFontSize, string text, Material baseIconMaterial)
        {
            Childrens children = new Childrens { };
            children.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
            //float buttonFontSize = 0.01f;
            Entity button = SpawnButton(
                        EntityManager,
                        panelUI,
                        position,
                        (new float2(buttonFontSize * text.Length * 1f, buttonFontSize)),
                        null, 
                        baseIconMaterial);
            RenderText buttonText = new RenderText { };
            buttonText.fontSize = buttonFontSize;
            buttonText.SetText(text);
            EntityManager.AddComponentData(button, buttonText);
            RenderTextSystem.SetLetterColor(EntityManager, button, Color.green);
            children.children[0] = button;
            EntityManager.AddComponentData(panelUI, children);
            return button;
        }

        public static Entity SpawnButton(EntityManager EntityManager, Entity parent, Vector3 localPosition, float2 iconSize, Texture2D iconTexture, Material baseIconMaterial)
        {
            Entity e = SpawnVisualElement(EntityManager, parent, localPosition, iconSize, iconTexture, baseIconMaterial);
            EntityManager.AddComponentData(e, new Zoxel.UI.Button { });
            return e;
        }
            
        public static Entity SpawnVisualElement(EntityManager EntityManager, Entity parent, Vector3 localPosition, float2 iconSize, Texture2D iconTexture, Material baseIconMaterial, int queuePriority = 5000)
        {
            if (iconArchtype.Valid == false)
            {
                iconArchtype = EntityManager.CreateArchetype(
                    typeof(Parent),
                    typeof(LocalToParent),
                    typeof(LocalToWorld),
                    // transform
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(NonUniformScale),
                    // renderer
                    typeof(RenderMesh));
            }
            Material iconMaterialInstance = null;
            if (baseIconMaterial != null)
            {
                iconMaterialInstance = new Material(baseIconMaterial);
                iconMaterialInstance.enableInstancing = true;
                //iconMaterialInstance.renderQueue = 6666;
                // iconMaterialInstance.SetFloat("_QueueOffset", queuePriority);
                if (iconTexture != null)
                {
                    iconMaterialInstance.SetTexture("_BaseMap", iconTexture);
                }
            }
            Mesh iconMesh = MeshUtilities.CreateQuadMesh(iconSize);
            Entity newBar = EntityManager.CreateEntity(iconArchtype);
            if (EntityManager.Exists(parent))
            {
                EntityManager.SetComponentData(newBar, new Parent { Value = parent });
            }
            else
            {
                EntityManager.RemoveComponent<Parent>(newBar);
                EntityManager.RemoveComponent<LocalToParent>(newBar);
                EntityManager.AddComponentData(newBar, new FaceCameraComponent { });
            }
            EntityManager.SetComponentData(newBar, new Translation { Value = localPosition });
            EntityManager.SetComponentData(newBar, new Rotation { Value = quaternion.identity });
            EntityManager.SetComponentData(newBar, new NonUniformScale { Value = new float3(1, 1, 1) });
            EntityManager.SetSharedComponentData(newBar, new RenderMesh
            {
                material = iconMaterialInstance,
                mesh = iconMesh,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.Off
            });
            RenderBounds b = new RenderBounds
            {
                Value = new AABB
                {
                    Extents = new float3(iconSize.x, iconSize.y, 0.5f)
                }
            };
            EntityManager.AddComponentData(newBar, b);
            return newBar;
        }
    }
}