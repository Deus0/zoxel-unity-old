using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

namespace Zoxel
{
    [System.Serializable]
    public struct Quest
    {
        public int metaID;
        public BlitableArray<QuestBlock> blocks;
        public byte updated;    // if need update this ui
        // how many for objectives it has
        //public int targetID;
        //public int completed;

        public int GetCompleted()
        {
            int completed = 0;
            foreach (QuestBlock block in blocks.ToArray())
            {
                completed += block.completed;
            }
            return completed;
        }

        public bool HasUpdated()
        {
            if (updated == 1)
            {
                updated = 0;
                return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public struct QuestLog : IComponentData
    {
        public byte updated;
        public BlitableArray<Quest> quests;

        public void Initialize(int length)
        {
            quests = new BlitableArray<Quest>(length, Allocator.Persistent);
        }
        public void Initialize(List<QuestDatam> meta = null)
        {
            quests = new BlitableArray<Quest>(meta.Count, Allocator.Persistent);
            for (int j = 0; j < quests.Length; j++)
            {
                if (meta != null && j < meta.Count)
                {
                    Quest newQuest = new Quest { metaID = meta[j].Value.id };
                    List<QuestBlock> blocks = meta[j].Value.questBlocks;
                    newQuest.blocks = new BlitableArray<QuestBlock>(blocks.Count, Allocator.Persistent);
                    for (int k = 0; k < blocks.Count; k++)
                    {
                        newQuest.blocks[k] = blocks[k];
                    }
                    quests[j] = newQuest;
                }
                else
                {
                    quests[j] = new Quest {  };
                }
            }
        }
        
        public bool OnKilledCharacter(int characterMetaID)
        {
            bool didUpdate = false;
            //UnityEngine.Debug.LogError("On killed character: " + characterMetaID + " with " + quests.Length + " quests.");
            for (int i = 0; i < quests.Length; i++)
            {
                //UnityEngine.Debug.LogError("Quest Blocks are: " + quests[i].blocks.Length + " quest blocks");
                Quest quest = quests[i];
                for (int j = 0; j < quests[i].blocks.Length; j++)
                {
                    QuestBlock block = quest.blocks[j];
                    if (block.OnKilledCharacter(characterMetaID))
                    {
                        updated = 1;
                        quest.updated = 1;
                        quest.blocks[j] = block;
                        quests[i] = quest;
                        didUpdate = true;
                        //UnityEngine.Debug.LogError("Quest Log Updated. On Killed character: " + quest.GetCompleted());
                    }
                }
            }
            return didUpdate;
        }

        #region SerializableBlittableArray
        [System.Serializable]
        public struct SerializeableQuestLog
        {
            public Quest[] quests;
        }
        public string GetJson()
        {
            SerializeableQuestLog myClone = new SerializeableQuestLog();
            myClone.quests = quests.ToArray();
            return UnityEngine.JsonUtility.ToJson(myClone);
        }
        public static QuestLog FromJson(string json)
        {
            SerializeableQuestLog myClone = UnityEngine.JsonUtility.FromJson<SerializeableQuestLog>(json);
            QuestLog questlog = new QuestLog { };
            questlog.Initialize(myClone.quests.Length);
            for (int i = 0; i < myClone.quests.Length; i++)
            {
                questlog.quests[i] = myClone.quests[i];
            }
            return questlog;
        }
        #endregion
    }
}