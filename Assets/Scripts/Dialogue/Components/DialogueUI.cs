using Unity.Entities;

namespace Zoxel
{

    public struct DialogueUI : IComponentData
    {
        // meta
        public int treeID;
        public int branchID; // dialogue branch
        public byte completedTree;
        public byte endIndex;
        public byte maxIndex;
        public byte confirmedChoice;
        public byte hasSpawnedButtons;
        // data
        public BlitableArray<byte> fontIndexes;
        public byte centred;
        // state
        public float timeBegun;
        public float timePerLetter;
        public float timePerLetterMin;
        public float timePerLetterMax;

        public void SetText(string text, ref RenderText newText)
        {
            byte updatedText;
            fontIndexes = RenderText.StringToBytes(text, fontIndexes, out updatedText);
            endIndex = 0;
            maxIndex = (byte)text.Length;
            newText.SetText(new BlitableArray<byte>(0, Unity.Collections.Allocator.Persistent));
        }

        public void RandomizeCooldown()
        {
            timeBegun = UnityEngine.Time.time;
            timePerLetter = UnityEngine.Random.Range(timePerLetterMin, timePerLetterMax);
        }

        public bool HasFinished()
        {
            return endIndex == maxIndex + 1;
        }

        public void IncreaseIndex(ref RenderText newText)
        {
            if (endIndex >= maxIndex + 1)
            {
                endIndex = 0;
            }
            if ((int)endIndex > fontIndexes.Length)
            {
                return;
            }
            //UnityEngine.Debug.LogError("Changing End Index: " + endIndex);
            BlitableArray<byte> culledArray = new BlitableArray<byte>((int)endIndex, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < (int)endIndex; i++)
            {
                culledArray[i] = fontIndexes[i];
            }
            newText.SetText(culledArray);
            endIndex++;
            //newText.SetText("hello world".Substring(0, (int)(endIndex)));
        }
    }
}
