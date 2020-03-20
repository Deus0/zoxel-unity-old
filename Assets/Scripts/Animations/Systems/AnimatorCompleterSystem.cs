using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Zoxel;
using Unity.Rendering;
using System;

namespace Zoxel.Animations
{
    // a component system that sets the renderers values if the animator state has changed last frame

    [DisableAutoCreation]
    public class AnimatorEndSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            // Entities.ForEach processes each set of ComponentData on the main thread. This is not the recommended
            // method for best performance. However, we start with it here to demonstrate the clearer separation
            // between ComponentSystem Update (logic) and ComponentData (data).
            // There is no update logic on the individual ComponentData.
            Entities.WithAll<Animator>().ForEach((Entity e, ref Animator animator) => // , ref RenderMesh renderer
            {
                // set renderer if updated
                if (animator.didUpdate == 1)
                {
                    animator.didUpdate = 0;
                    // update renderer
                    SetAnimation(ref animator, e);
                }
            });
        }

        private void SetAnimation(ref Animator animator, Entity e)
        {
            if (animator.data.Length <= 1)
            {
                if (animator.data.Length == 0)
                {
                    UnityEngine.Debug.LogError("Animator for entity doesnt have animations.");
                }
                return;
            }
            int animationIndex = 0;
            if (animator.isWalking == 0)
            {
                animationIndex = 0;
            }
            else
            {
                if (animator.data.Length > 1)
                    animationIndex = 1;
            }
            float timeBegun = UnityEngine.Time.time;
            RenderMesh renderer;
            //UnityEngine.Debug.LogError("Setting new Animation to: " + animationIndex);
            int framesAddition = GetFramesAddition(ref animator, animationIndex);
            float animationTime = (animator.data[animationIndex].time);
            int framesPerSecond = (animator.data[animationIndex].frames);
            renderer = World.EntityManager.GetSharedComponentData<RenderMesh>(e);
            UnityEngine.Material material = (renderer.material); // new Material
            material.SetFloat("_TimeBegun", timeBegun);
            material.SetFloat("_AnimationTime", animationTime);
            material.SetInt("_FramesAddition", framesAddition);
            material.SetInt("_FramesPerSecond", framesPerSecond);
            //material.SetInt("_AnimationSpeed", 1);
            renderer.material = material;
            World.EntityManager.SetSharedComponentData(e, renderer);
        }

        private int GetFramesAddition(ref Animator data, int animationIndex)
        {
            int framesAddition = 0;
            for (int i = 0; i < animationIndex; i++)
            {
                framesAddition += data.data[i].frames;
            }
            return framesAddition;
        }
    }
}