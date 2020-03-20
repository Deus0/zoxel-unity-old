using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    public struct DamagePopup : IComponentData
    {
        public float createdTime;
        public float lifeTime;
    }

    [DisableAutoCreation]
    public class DamagePopupSystem : ComponentSystem
    {
        public CameraSystem cameraSystem;
        public List<Entity> popups = new List<Entity>();
        //public UIUtilities UIUtilities;
        public UIDatam uiDatam;

        protected override void OnUpdate()
        {
            Entities.WithAll<DamagePopup>().ForEach((Entity e, ref DamagePopup popup) =>
            {
                if (UnityEngine.Time.time - popup.createdTime >= popup.lifeTime)
                {
                    popups.Remove(e);
                    World.EntityManager.DestroyEntity(e);
                }
            });
        }

        public void SpawnPopup(float damage, float3 position)
        {
            int damageInt = (int)math.round(damage);
            string damageString = damageInt.ToString();
            if (damageInt >= 0)
            {
                quaternion cameraRotation = cameraSystem.GetMainCamera().transform.rotation;
                float delay = UnityEngine.Random.Range(0.1f, 0.2f);
                float halfWidth = (damageString.Length * uiDatam.fontSize * 0.6f) / 2f;
                for (int i = 0; i < damageString.Length; i++)
                {
                    float lifetime = UnityEngine.Random.Range(uiDatam.popupLifetime.x, uiDatam.popupLifetime.y);
                    int singleDigit = int.Parse(damageString[i].ToString());
                    // for each digit spawn one, add a offset so they centre in the middle
                    float3 offset = new float3(i * uiDatam.fontSize * 0.6f - halfWidth, 0, 0);
                    offset = math.rotate(cameraRotation, offset);
                    float3 positionBegin = position + new float3(0, 0.1f + uiDatam.fontSize / 2f, 0) + offset;
                    float3 positionEnd = positionBegin + new float3(
                            UnityEngine.Random.Range(uiDatam.popupVariationX.x, uiDatam.popupVariationX.y),
                            UnityEngine.Random.Range(uiDatam.popupVariationY.x, uiDatam.popupVariationY.y),
                            UnityEngine.Random.Range(uiDatam.popupVariationZ.x, uiDatam.popupVariationZ.y));
                    Entity textEntity = SpawnNumber(singleDigit, positionBegin);
                    RenderTextSystem.SetLetterColor(World.EntityManager, textEntity, Color.red);
                    World.EntityManager.AddComponentData(textEntity, new PositionLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = lifetime,
                        positionBegin = positionBegin,
                        positionEnd = positionEnd
                    });
                    World.EntityManager.AddComponentData(textEntity, new ScaleLerper
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = lifetime - delay,
                        delay = delay,
                        scaleBegin = new float3(1, 1, 1),
                        scaleEnd = new float3(0, 0, 0)
                    });
                    World.EntityManager.AddComponentData(textEntity, new DamagePopup
                    {
                        createdTime = UnityEngine.Time.time,
                        lifeTime = lifetime,
                    });
                    popups.Add(textEntity);
                }
            }
        }
        public Entity SpawnNumber(int number, float3 position)
        {
            return UIUtilities.SpawnVisualElement(
                World.EntityManager,
                new Entity(), 
                position,
                new float2(uiDatam.fontSize, uiDatam.fontSize),
                uiDatam.font.numbers[number], uiDatam.font.material, 8000);
        }
    }
}

// rotation positions
//positionBegin = math.rotate(cameraRotation, positionBegin);
//positionEnd = math.rotate(cameraRotation, positionEnd);