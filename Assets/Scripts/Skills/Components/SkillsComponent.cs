using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

namespace Zoxel
{
    public class SkillsComponent : ComponentDataProxy<Skills> { }

    /// <summary>
    /// Each character has a list of skills they can use
    /// </summary>
    [System.Serializable]
    public struct Skills : IComponentData
    {
        //public int id;
        public BlitableArray<SkillData> skills;
        // will go pewpew?
        public int selectedSkillIndex;
        public byte triggered;  // actually using skills!
        public byte updated; // updating UI, setting new components to entity

        public void Dispose()
        {
            if (skills.Length > 0)
            {
                skills.Dispose();
            }
        }
        public void Initialize(List<SkillDatam> startingSkills)
        {
            Dispose();
            skills = new BlitableArray<SkillData>(startingSkills.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < startingSkills.Count; i++)
            {
                skills[i] = startingSkills[i].Value;
            }
        }

        public void Initialize(ClassDatam classer)
        {
            if (classer != null)
            {
                //if (skills.Length == 0)
                {
                    skills = new BlitableArray<SkillData>(classer.startingSkills.Count, Unity.Collections.Allocator.Persistent);
                    for (int i = 0; i < classer.startingSkills.Count; i++)
                    {
                        skills[i] = classer.startingSkills[i].Value;
                    }
                }
            }
        }

        #region SerializableBlittableArray
        public void Initialize(int count)
        {
            skills = new BlitableArray<SkillData>(count, Allocator.Persistent);
            for (int j = 0; j < skills.Length; j++)
            {
                skills[j] = new SkillData { };
            }
        }

        [System.Serializable]
        public struct SerializeableSkills
        {
            public SkillData[] skills;
        }

        public string GetJson()
        {
            SerializeableSkills myClone = new SerializeableSkills();
            myClone.skills = skills.ToArray();
            return UnityEngine.JsonUtility.ToJson(myClone);
        }
        public static Skills FromJson(string json)
        {
            if (json == null || json == "" )
            {
                return new Skills();
            }
            SerializeableSkills myClone = UnityEngine.JsonUtility.FromJson<SerializeableSkills>(json);
            Skills skills = new Skills { };
            if (myClone.skills == null)
            {
                Debug.LogError("JSON Null inside stats:\n" + json);
                return new Skills();
            }
            skills.Initialize(myClone.skills.Length);
            for (int i = 0; i < myClone.skills.Length; i++)
            {
                skills.skills[i] = myClone.skills[i];
            }
            return skills;
        }
        #endregion
    }
}
