using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Zoxel
{
    public struct NavigationElementUI : IComponentData
    {
        public float animationTime;
        private byte selected;
        private byte hasFinalizedColor;
        public float timeSelected;
        public byte originalColorR;
        public byte originalColorG;
        public byte originalColorB;
        public byte selectedColorR;
        public byte selectedColorG;
        public byte selectedColorB;

        public void Select(float timeSelected_)
        {
            hasFinalizedColor = 0;
            selected = 1;
            timeSelected = timeSelected_;
        }

        public void Deselect(float timeSelected_)
        {
            if (selected == 1)
            {
                hasFinalizedColor = 0;
                selected = 0;
                timeSelected = timeSelected_;
            }
        }

        public Color GetOriginalColor()
        {
            return new Color(
                (originalColorR) / 255f,
                (originalColorG) / 255f,
                (originalColorB) / 255f);
        }
        public Color GetSelectedColor()
        {
            return new Color(
                ((int)selectedColorR) / 255f,
                ((int)selectedColorG) / 255f,
                ((int)selectedColorB) / 255f);
        }

        public float GetAnimationLerp(float currentTime)
        {
            return (currentTime - timeSelected) / animationTime;
        }

        public bool IsSelectedAnimation(float currentTime)
        {
            return selected == 1 && GetAnimationLerp(currentTime) <= animationTime;
        }
        public bool IsSelectedFinalAnimation(float currentTime)
        {
            if (selected == 1 && hasFinalizedColor == 0 && GetAnimationLerp(currentTime) > animationTime)
            {
                hasFinalizedColor = 1;
                return true;
            }
            return false;
        }

        public bool IsDeselectedAnimation(float currentTime)
        {
            return selected == 0 && GetAnimationLerp(currentTime) <= animationTime;
        }
        public bool IsDeselectedFinalAnimation(float currentTime)
        {
            if (selected == 0 && hasFinalizedColor == 0 && GetAnimationLerp(currentTime) > animationTime
               )
            {
                hasFinalizedColor = 1;
                return true;
            }
            return false;
        }
    }

    [DisableAutoCreation]
    public class SelectedUISystem : ComponentSystem
    {
        //public Color defaultColor = Color.green;
        //public Color selectedColor = Color.red;

        protected override void OnUpdate()
        {
            // if selected or not, lerp button colour
            Entities.WithAll<NavigationElementUI>().ForEach((Entity e, ref NavigationElementUI element) =>
            {
                float time = UnityEngine.Time.realtimeSinceStartup; // time.tim
                if (element.IsSelectedAnimation(time))
                {
                   // Debug.LogError("Setting to selected");
                    LerpEntityColor(e, element.GetOriginalColor(), element.GetSelectedColor(), element.GetAnimationLerp(time));
                }
                else if (element.IsDeselectedAnimation(time))
                {
                    //Debug.LogError("Setting to original");
                    LerpEntityColor(e, element.GetSelectedColor(), element.GetOriginalColor(), element.GetAnimationLerp(time));
                }
                else if(element.IsSelectedFinalAnimation(time))
                {
                    LerpEntityColor(e, element.GetOriginalColor(), element.GetSelectedColor(), 1);
                }
                else if(element.IsDeselectedFinalAnimation(time))
                {
                    LerpEntityColor(e, element.GetSelectedColor(), element.GetOriginalColor(), 1);
                }
            });
        }

        public void LerpEntityColor(Entity entity, Color fromColor, Color toColor, float delta)
        {
            //Debug.LogError("Animating to " + toColor.ToString() + " ::: " + delta);
            RenderMesh render = World.EntityManager.GetSharedComponentData<RenderMesh>(entity);
            SetMaterial(render.material, Color.Lerp(fromColor, toColor, delta));
        }

        public static void SetMaterial(Material material, Color newColor)
        {
            material.SetColor("_BaseColor", newColor);
        }
    }
}
