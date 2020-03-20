using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    // Things to add
    // fuck i wish i kept my old game data
    // quests need requirements
    // example: one block is a list of tasks - so a contract with someone else
    // i.e. ill go kill 3 sheep for you and collect 10 herbs
    // there are rewards or punishments for this
    // a chain of these quests is called a quest chain, but essentially the dialogue tree will give you a new quest after you hand that in
    
    public enum QuestObjective
    {
        KillCharacter,
        HaveItem,
        PlaceBlock
    }

    [Serializable]
    public struct QuestBlock
    {
        public int completed;
        public int maxCompleted;
        public int targetID;    // meta id of target
        public byte targetType; // type of objective
        private byte updated;

        public bool HasUpdated()
        {
            if (updated == 1)
            {
                updated = 0;
                return true;
            }
            return false;
        }

        // return true if updated for UI
        public bool OnKilledCharacter(int metaID)
        {
            if (targetType == 0)
            {
                if (targetID == metaID)
                {
                    //Debug.LogError("TargetID IS  metaID: " + targetID + "::" + metaID + ":" + (completed + 1) + " out of " + maxCompleted);
                    if (completed != maxCompleted)
                    {
                        completed++;
                        updated = 1;
                        return true;
                    }
                }
                //else
                //{
                    //Debug.LogError("TargetID nto metaID: " + targetID + "::" + metaID);
                //}
            }
            return false;
        }
    }

    [Serializable]
    public struct QuestData
    {
        public int id;
        public List<QuestBlock> questBlocks;

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
    }

    [CreateAssetMenu(fileName = "Quest", menuName = "Zoxel/Quest")]
    public class QuestDatam : ScriptableObject
    {
        public QuestData Value;
        public TextureDatam texture;

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            Value.GenerateID();
        }
    }
}