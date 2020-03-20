using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Zoxel
{

    public struct RenderText : IComponentData
    {
        // the speech string
        public byte updated;
        public BlitableArray<byte> fontIndexes;
        public BlitableArray<Entity> letters;
        public float fontSize;
        public float offsetX;
        public byte colorR;
        public byte colorG;
        public byte colorB;
        public byte alignment;

        public float2 GetPanelSize()
        {
            return new float2(fontSize * letters.Length, fontSize);
        }

        public void DestroyLetters(EntityManager entityManager)
        {
            if (letters.Length > 0)
            {
                Entity[] entities = letters.ToArray();
                foreach (Entity e in entities)
                {
                    if (entityManager.Exists(e))
                    {
                        entityManager.DestroyEntity(e);
                    }
                }
                letters.Dispose();
            }
        }

        /// <summary>
        /// Only supports lowercase atm
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (fontIndexes.Length > 0)
            {
                fontIndexes.Dispose();
            }
            fontIndexes = StringToBytes(text.ToLower(), fontIndexes, out updated);
        }
        public void SetText(BlitableArray<byte> newFontIndexes)
        {
            fontIndexes = newFontIndexes;
            updated = 1;
        }
        public static BlitableArray<byte> StringToBytes(string text, BlitableArray<byte> fontIndexes, out byte updated)
        {
            List<byte> fontIndexesList = new List<byte>();
            int numbersCount = 10;
            int aInt = (int)'a';
            int zInt = (int)'z';
            for (int i = 0; i < text.Length; i++)
            {
                int singleDigit = (int)text[i];
                byte fontIndex = 0;
                if (text[i] == ' ')
                {
                    // byte is 255 for spaces!
                    fontIndex = (byte)255;
                }
                else if (singleDigit >= aInt && singleDigit <= zInt)
                {
                    fontIndex = (byte)((singleDigit - aInt) + numbersCount);
                }
                else
                {
                    if (int.TryParse(text[i].ToString(), out singleDigit))
                    {
                        fontIndex = (byte)singleDigit;
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Unsupported character [" + text[i] + "]");
                        continue;
                    }
                }
                fontIndexesList.Add(fontIndex);
                //UnityEngine.Debug.LogError("Adding character: " + fontIndex + " at: " + fontIndexesList.Count);
            }
            if (fontIndexes.Length == fontIndexesList.Count)
            {
                bool isSame = true;
                for (int i = 0; i < fontIndexes.Length; i++)
                {
                    if (fontIndexes[i] != fontIndexesList[i])
                    {
                        isSame = false;
                        break;
                    }
                }
                if (isSame)
                {
                    updated = 0;
                    return fontIndexes;
                }
            }
            fontIndexes = new BlitableArray<byte>(fontIndexesList.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < fontIndexes.Length; i++)
            {
                fontIndexes[i] = fontIndexesList[i];
            }
            updated = 1;
            return fontIndexes;
        }
    }
}