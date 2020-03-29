using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;

namespace Zoxel
{
    [DisableAutoCreation]
    public class StatBarFaderSystem : ComponentSystem
	{
        public UIDatam uiData;

        protected override void OnUpdate()
        {
            // Need to do a check for timing with taking damage
            Entities.WithAll<StatBarUI>().ForEach((Entity frontbar, ref StatBarUI statbar, ref ZoxID zoxID) => // , ref RenderMesh renderer
            {
                Entity backbar = StatbarSystem.backBars[zoxID.id];
                if (statbar.isDead == 1 || statbar.isTakingDamage == 0)
                {
                    if (statbar.visible == 1)
                    {
                        statbar.visible = 0;
                        statbar.timeStateChanged = UnityEngine.Time.time;
                    }
                }
                else
                {
                    if (statbar.visible == 0)
                    {
                        statbar.visible = 1;
                        statbar.timeStateChanged = UnityEngine.Time.time;
                        Debug.LogError("Starting to fade in (will be visible)!");
                    }
                }
                float timePassed = (UnityEngine.Time.time - statbar.timeStateChanged);
                float fadeSpeed = 1;
                if (statbar.visible == 1)
                {
                    fadeSpeed = uiData.fadeIn;
                }
                else
                {
                    fadeSpeed = uiData.fadeOut;
                }
                if (statbar.timeStateChanged != 0 && timePassed < fadeSpeed)    // deactivate after 3 seconds
                {
                    statbar.isFading = 1;
                    float warpedTime = timePassed / fadeSpeed;
                    if (statbar.isDead == 1 || statbar.isTakingDamage == 0)
                    {
                        FadeBarEntity(frontbar, uiData.frontbarColor, new float2(1f, 0), warpedTime);
                        FadeBarEntity(backbar, uiData.backbarColor, new float2(1f, 0), warpedTime);
                    }
                    else
                    {
                        FadeBarEntity(frontbar, uiData.frontbarColor, new float2(0, 1f), warpedTime);
                        FadeBarEntity(backbar, uiData.backbarColor, new float2(0, 1f), warpedTime);
                    }
                }
                else
                {
                    if (statbar.isFading == 1)
                    {
                        statbar.isFading = 0;
                        if (statbar.isDead == 1 || statbar.isTakingDamage == 0)
                        {
                            FadeBarEntity(frontbar, uiData.frontbarColor, new float2(1f, 0), 1);
                            FadeBarEntity(backbar, uiData.backbarColor, new float2(1f, 0), 1);
                        }
                        else
                        {
                            FadeBarEntity(frontbar, uiData.frontbarColor, new float2(0, 1f), 1);
                            FadeBarEntity(backbar, uiData.backbarColor, new float2(0, 1f), 1);
                        }
                    }
                }
            });
		}

		public void FadeBarEntity(Entity entity, Color fadeFrom, float2 alphas, float deltaTime)
		{
            RenderMesh render = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
            Color newColor = new Color();
            newColor.r = fadeFrom.r;
            newColor.g = fadeFrom.g;
            newColor.b = fadeFrom.b;
            newColor.a = math.lerp(alphas.x, alphas.y, deltaTime);
            SetMaterial(render.material, newColor);
        }

        public static void SetMaterial(Material material, Color newColor)
        {
            material.SetColor("_BaseColor", newColor);
        }
	}
}