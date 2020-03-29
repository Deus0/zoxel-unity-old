using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Core;
using UnityEngine;
using Unity.Collections;

namespace Zoxel.UI
{
    [DisableAutoCreation]
    public class NavigateUISystem : JobComponentSystem
    {
        [BurstCompile]
        struct SystemJob : IJobForEach< NavigateUI, Controller> // Translation,
        {
            [ReadOnly]
            public float time;
            const float navigationThreshold = 0.6f;

            public void Execute(ref NavigateUI navigate, ref Controller controller) // ref Translation position, 
            {
                if (navigate.navigationElements.Length == 0)
                {
                    return;
                }
                if (controller.Value.leftStick.x == 0 && controller.Value.leftStick.y == 0)
                {
                    navigate.lastMoved = time - 0.5f;
                    return;
                }
                if (time - navigate.lastMoved < 0.5f)
                {
                    return;
                }
                if (controller.Value.leftStick.x >= -navigationThreshold && controller.Value.leftStick.x <= navigationThreshold
                    && controller.Value.leftStick.y >= -navigationThreshold && controller.Value.leftStick.y <= navigationThreshold)
                {
                    return;
                }
                navigate.lastMoved = time;
                byte right = ((byte)NavigationUIDirection.Right);
                byte left = ((byte)NavigationUIDirection.Left);
                byte up = ((byte)NavigationUIDirection.Up);
                byte down = ((byte)NavigationUIDirection.Down);
                float3 thisPosition = navigate.navigationElements[navigate.selectedIndex].targetPosition;
                //Debug.LogError("Looking for current nodes: " + thisPosition);
                for (int i = 0; i < navigate.navigationElements.Length; i++)
                {
                    /*if (navigate.navigationElements[i].nextPositionIndex == -1)
                    {
                        continue;
                    }*/
                    float3 checkPosition = navigate.navigationElements[i].previousPosition;
                    if (thisPosition.x == checkPosition.x
                        && thisPosition.y == checkPosition.y
                        && thisPosition.z == checkPosition.z)
                    {
                        if ((controller.Value.leftStick.x > navigationThreshold &&
                            navigate.navigationElements[i].direction == right)
                            || (controller.Value.leftStick.x < -navigationThreshold &&
                            navigate.navigationElements[i].direction == left)
                            || (controller.Value.leftStick.y > navigationThreshold &&
                            navigate.navigationElements[i].direction == up)
                            || (controller.Value.leftStick.y < -navigationThreshold &&
                            navigate.navigationElements[i].direction == down))
                        {
                            //if (navigate.navigationIndex != navigate.navigationElements[i].targetIndex)
                            {
                                // if ui ID hasnt changed - just move position
                                // otherwise change parent to one of the list of parents?
                                //Debug.LogError("New selectedIndex:: " + i);
                                navigate.selectedIndex = i;// navigate.navigationElements[i].targetIndex;    //i;
                                navigate.position = navigate.navigationElements[i].targetPosition;
                                //position.Value = navigate.position;
                                navigate.updated = 1;
                                //UnityEngine.Debug.LogError("Selecting Again: " + i + ":" + position.Value);
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new SystemJob { time = UnityEngine.Time.realtimeSinceStartup }.Schedule(this, inputDeps);
        }
    }
}