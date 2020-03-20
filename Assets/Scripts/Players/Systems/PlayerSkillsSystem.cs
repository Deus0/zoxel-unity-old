using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel
{
    /// <summary>
    /// Player can activate or switch Skills using the Controller component
    /// </summary>
    [DisableAutoCreation]
    public class PlayerSkillsSystem : JobComponentSystem
    {
        [BurstCompile]
        struct PlayerSkillsJob : IJobForEach<Controller, Skills>
        {
            public void Execute(ref Controller controller, ref Skills skills)
            {
                if (controller.mappingType == 0)
                {
                    // switching skills
                    if (controller.Value.buttonRB == 1)
                    {
                        IncreaseSlotIndex(ref skills);
                    }
                    if (controller.Value.buttonLB == 1)
                    {
                        DecreaseSlotIndex(ref skills);
                    }
                    if (controller.Value.buttonRT == 1)
                    {
                        skills.triggered = 1;   // .skills[skills.selectedSkillIndex]
                    }
                }
            }

            public void SetSelectedSlot(ref Skills skills, int newSlotIndex)
            {
                skills.selectedSkillIndex = newSlotIndex;
                skills.updated = 1;
            }

            public void IncreaseSlotIndex(ref Skills skills)
            {
                skills.selectedSkillIndex++;
                skills.updated = 1;
                if (skills.selectedSkillIndex == skills.skills.Length)
                {
                    skills.selectedSkillIndex = 0;
                }
            }

            public void DecreaseSlotIndex(ref Skills skills)
            {
                skills.selectedSkillIndex--;
                skills.updated = 1;
                if (skills.selectedSkillIndex == -1)
                {
                    skills.selectedSkillIndex = skills.skills.Length - 1;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new PlayerSkillsJob { }.Schedule(this, inputDeps);
        }
    }
}