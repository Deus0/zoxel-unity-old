using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel
{

    public struct DialogueTree
    {
        public int id;
        public BlitableArray<DialogueBranch> branches;

        #region Serialization
        
        [Serializable]
        public struct SerializableDialogueTree
        {
            public int id;
            public DialogueBranch.SerializeableDialogueBranch[] branches;

            public DialogueTree GetRealOne()
            {
                DialogueTree dialogueTree = new DialogueTree();
                if (branches != null)
                {
                    dialogueTree.branches = new BlitableArray<DialogueBranch>(branches.Length, Allocator.Persistent);
                    for (int i = 0; i < branches.Length; i++)
                    {
                        dialogueTree.branches[i] = branches[i].GetReverseClone();
                    }
                }
                else
                {
                    dialogueTree.branches = new BlitableArray<DialogueBranch>(0, Allocator.Persistent);
                }
                return dialogueTree;
            }
        }

        public SerializableDialogueTree GetSerializeableClone()
        {
            SerializableDialogueTree clone = new SerializableDialogueTree();
            clone.id = id;
            clone.branches = new DialogueBranch.SerializeableDialogueBranch[branches.Length];
            var normalBranches = branches.ToArray();
            for (int i = 0; i < branches.Length; i++)
            {
                clone.branches[i] = normalBranches[i].GetSerializeableClone();
            }
            return clone;
        }  
        #endregion

        public void AddBranch(DialogueBranch branch)
        {
            // first store branches
            var branches_ = branches.ToArray();
            // now create new array
            if (branches.Length > 0) {
                branches.Dispose();
            }
            branches = new BlitableArray<DialogueBranch>(branches.Length + 1, Allocator.Persistent);
            // set old one
            for (int i = 0; i < branches.Length - 1; i++) {
                branches[i] = branches_[i];
            }
            // set new one
            branches[branches.Length - 1] = branch;
        }

        public void RemoveBranch(DialogueBranch branch) {
            if (branches.Length == 0) {
                return;
            }
            int index = -1;
            for (int i = 0; i < branches.Length; i++) {
                if (branches[i].id == branch.id) {
                    index = i;
                    break;
                }
            }
            if (index == -1) {
                return;
            }
            // first store branches
            var originalBranches = branches.ToArray();
            // now create new array
            if (branches.Length > 0) {
                branches.Dispose();
            }
            branches = new BlitableArray<DialogueBranch>(originalBranches.Length - 1, Allocator.Persistent);
            // set old one
            for (int i = 0; i < originalBranches.Length; i++) {
                if (i < index) {
                    branches[i] = originalBranches[i];
                }
                else if (i > index) {
                    branches[i - 1] = originalBranches[i];
                }
            }
        }      
    }
}