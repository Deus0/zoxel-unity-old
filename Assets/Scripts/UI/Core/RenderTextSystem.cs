using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;

namespace Zoxel
{

    [DisableAutoCreation]
    public class RenderTextSystem : ComponentSystem
    {
        public UIDatam uiData;

        protected override void OnUpdate()
        {
            // if updated
            //      - add/remove difference of letters
            //      - set textures for fonts
            //      - set positions for all letters

            Entities.WithAll<RenderText>().ForEach((Entity e, ref RenderText renderText) =>
            {
                if (renderText.updated == 1)
                {
                    renderText.updated = 0;
                    // do the entity creation/destruction here for new font
                    UpdateRenderText(e, ref renderText);
                }
            });
        }

        public void UpdateRenderText(Entity renderTextEntity, ref RenderText renderText)//, float3 position, float fontSize, FontDatam font)
        {
            int desiredLength = renderText.fontIndexes.Length;
            int actualLength = renderText.letters.Length;
            //UnityEngine.Debug.LogError("Spawning Render Text. Desired Length is: " + desiredLength + " and actualLength is: " + actualLength);
            if (desiredLength > actualLength)
            {
                //UnityEngine.Debug.Log("Adding [" + (desiredLength - actualLength) + "] New Characters.");
                BlitableArray<Entity> newLetters = new BlitableArray<Entity>(desiredLength, Unity.Collections.Allocator.Persistent);
                for (int i = 0; i < desiredLength; i++)
                {
                    if (i < renderText.letters.Length)
                    {
                        newLetters[i] = renderText.letters[i];
                    }
                    else
                    {
                        newLetters[i] = SpawnNewFontLetter(ref renderText, renderTextEntity, renderText.fontSize);
                    }
                }
                if (renderText.letters.Length > 0)
                {
                    renderText.letters.Dispose();
                }
                renderText.letters = newLetters;
            }
            // Remove Excess Digits
            if (actualLength > desiredLength)
            {
                BlitableArray<Entity> newLetters = new BlitableArray<Entity>(desiredLength, Unity.Collections.Allocator.Persistent);
                //UnityEngine.Debug.Log("Removing [" + (actualLength - desiredLength) + "] Old Characters.");
                for (int i = 0; i < renderText.letters.Length; i++)
                {
                    if (i < newLetters.Length)
                    {
                        newLetters[i] = renderText.letters[i];
                    }
                    else
                    {
                        World.EntityManager.DestroyEntity(renderText.letters[i]);
                    }
                }
                if (renderText.letters.Length > 0)
                {
                    renderText.letters.Dispose();
                }
                renderText.letters = newLetters;
            }
            // now set them all
            float3 offset = float3.zero;
            // first get total size
            // now we know the offset for centring
            if (renderText.alignment == 0)
            {
                // no alignment
                offset.x = ((-desiredLength - 1f) / 2f) * renderText.fontSize;// should be offset by half total size
            }
            if (renderText.alignment == 1)
            {
                // right aligned
                offset.x = (-desiredLength - 0.5f) * renderText.fontSize + renderText.offsetX;
            }
            if (renderText.offsetX != 0)
            {
                offset.x = renderText.offsetX;
            }
            offset.z = -0.005f;
            for (int i = 0; i < renderText.letters.Length; i++)
            {
                // set position first
                offset.x += renderText.fontSize;  // should be based on particular font
                if (renderText.fontIndexes[i] != 255)
                {
                    Translation translation = World.EntityManager.GetComponentData<Translation>(
                        renderText.letters[i]);
                    translation.Value = offset;
                    World.EntityManager.SetComponentData(renderText.letters[i], translation);

                    // Set Texture
                    // should check if changed or not
                    Unity.Rendering.RenderMesh render =
                        World.EntityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(renderText.letters[i]);
                    render.material.SetTexture("_BaseMap", uiData.font.textures[(int)renderText.fontIndexes[i]]);
                    World.EntityManager.SetSharedComponentData(renderText.letters[i], render);
                    SetLetterColor(World.EntityManager, renderText.letters[i], renderText.GetColor());
                }
                else
                {
                    SetLetterColor(World.EntityManager, renderText.letters[i],
                        new UnityEngine.Color(0, 0, 0, 0));
                }
            }
        }

        public Entity SpawnNewFontLetter(ref RenderText renderText, Entity panelUI, float fontSize)
        {
            Entity letter = UIUtilities.SpawnVisualElement(
                    World.EntityManager, 
                    panelUI, 
                    new float3(0, 0, -0.005f),
                    new float2(fontSize, fontSize),
                    null, 
                    uiData.font.material, 8000);
            return letter;
        }

        public static void SetLetterColor(EntityManager entityManager, Entity letter, UnityEngine.Color newColor)
        {
            Unity.Rendering.RenderMesh render = entityManager.GetSharedComponentData<Unity.Rendering.RenderMesh>(letter);
            if (render.material != null)
            {
                render.material.SetColor("_BaseColor", newColor);
            }
            entityManager.SetSharedComponentData(letter, render);
        }
    }
}

/*UnityEngine.Random.Range(155f / 255f, 1f),
UnityEngine.Random.Range(11f / 255f, 111f / 255f),
UnityEngine.Random.Range(11f / 255f, 111f / 255f),
UnityEngine.Random.Range(211f / 255f, 1f))*/
