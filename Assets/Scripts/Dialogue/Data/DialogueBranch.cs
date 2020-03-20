using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel {

    [Serializable]
    public struct DialogueBranch
    {
        public int id;
        public byte speakerType;
        public string speech;   // convert this to blitablearray of bytes!
        public BlitableArray<int> links;

        public void InitializeLinks(int newCount)
        {
            links = new BlitableArray<int>(newCount, Allocator.Persistent);
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = 0;
            }
        }

        public void AddBranchLink()
        {
            var oldLinks = links.ToArray();
            links = new BlitableArray<int>(links.Length + 1, Allocator.Persistent);
            for (int i = 0; i < links.Length - 1; i++)
            {
                links[i] = oldLinks[i];
            }
            links[links.Length - 1] = 0;
        }

        public void SetSpeech(string newSpeech)
        {
            speech = newSpeech;
        }

        public void SetSpeakerType(bool newSpeakerType) {
            if (newSpeakerType) {
                speakerType = 1;
            }
            else {
                speakerType = 0;
            }
        }

        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }

        // anything else?
        // shaking events
        // audio
        // animations during from characters

        
        public SerializeableDialogueBranch GetSerializeableClone() {
            SerializeableDialogueBranch clone = new SerializeableDialogueBranch();
            clone.id = id;
            clone.speakerType = speakerType;
            clone.speech = speech;
            clone.links = links.ToArray();
            return clone;
        }

        [Serializable]
        public struct SerializeableDialogueBranch
        {
            public int id;
            public byte speakerType;
            public string speech;   // for testing for now
            public int[] links;

            public DialogueBranch GetReverseClone() {
                DialogueBranch branch = new DialogueBranch();
                branch.id = id;
                branch.speech = speech;
                branch.links = new BlitableArray<int>(links.Length, Allocator.Persistent);
                for (int i = 0; i < links.Length; i++)
                {
                    branch.links[i] = links[i];
                }
                return branch;
            }
        }
    }
}