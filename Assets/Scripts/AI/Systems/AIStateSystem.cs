using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>


namespace Zoxel
{
    // states
    // 0 is idle
    // 1 is wander
    // 4 is aggressive
    // 2 is follow
    // 3 is flee?

    public enum AIStateType
    {
        Idle,
        Wander,
        Follow,
        Flee,
        Attack
    }

    public static class AIStates
    {
        public static byte Idle = 0;
        public static byte Wander = 1;
        public static byte Follow = 2;
        public static byte Flee = 3;
        public static byte Attack = 4;
    }

    /// <summary>
    /// Moves between aggression, idle and wander states
    /// </summary>
    [DisableAutoCreation]
    public class AIStateSystem : JobComponentSystem
    {
        [BurstCompile]
        struct AIStateJob : IJobForEach<ZoxID, Targeter, AIState, Mover, Skills>
        {
            [ReadOnly]
            public float time;
            
            public void Execute(ref ZoxID zoxID, ref Targeter targeter, ref AIState brain, ref Mover mover, ref Skills skills)
            {
                // Put in KillTargetSystem!
                // put in an activate skill system for AI
                // Depending on AI states and information about environment it will trigger its skills
                if (brain.state == 4 && targeter.hasTarget == 1)
                {
                    skills.triggered = 1;
                }
                else
                {
                    skills.triggered = 0;
                }
                //melee.enabled = 0;
                // Go IDLE if no target set
                if (targeter.hasTarget == 0)  //  && targeter.targetID == -1
                {
                    // if attacking, set to idle
                    if (brain.state == 2 || brain.state == 4)
                    {
                        SetBrainState(0, ref brain, ref mover); //, ref wanderer);
                        brain.lastIdled = time;
                    }
                    else if (brain.state == 0)
                    {
                        // idle already, wander if pass time
                        if (time - brain.lastIdled >= brain.idleTime)
                        {
                            SetBrainState(1, ref brain, ref mover);
                            // if has an owner
                            //if (zoxID.creatorID == 0)
                            {
                                // wander again//, ref wanderer);
                            }
                            //else
                            {
                                /*targeter.targetID = zoxID.creatorID;
                                targeter.targetClanID = zoxID.clanID;
                                SetBrainState(2, ref brain, ref mover, ref wanderer);*/
                            }
                        }
                    }
                }
                // Otherwise attack! or Flee if enemy and non hostile!
                else
                {
                    // if not attacking, start attacking! (if clans are aggressive!)
                    if (targeter.nearbyCharacter.clan != zoxID.clanID)
                    {
                        if (brain.isAggressive == 1)
                        {
                            SetBrainState(4, ref brain, ref mover);//, ref wanderer);
                        }
                    }
                    else
                    {
                        SetBrainState(2, ref brain, ref mover);//, ref wanderer);   // follow otherwise if same clan and targetting
                    }
                }
            }

            private void SetBrainState(byte newState, ref AIState brain, ref Mover mover)//, ref Wander wanderer)
            {
                if (brain.state != newState)
                {
                    // set new state
                    brain.state = newState;

                    /*if (brain.state == 1)
                    {
                        // if was wandering
                        wanderer.disabled = 0;
                    }
                    else
                    {
                        wanderer.disabled = 1;
                    }*/
                    if (brain.state == 4 || brain.state == 2)
                    {
                        mover.disabled = 0;
                    }
                    else
                    {
                        mover.disabled = 1;
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new AIStateJob { time = UnityEngine.Time.time }.Schedule(this, inputDeps);
        }
    }
}
