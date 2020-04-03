using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel
{

    /// <summary>
    /// Some examples:
    /// Load Level (Game)
    /// Screen Shake
    /// Delay Dialogue (pauses it)
    /// </summary>
    /*[Serializable]
    public struct DialogueAction
    {
        public int id;
        public int nextId;
        public string action;   // function name!
        public int speakerID;   // person who performs action! keep 0 if a system action
    }*/

    [Serializable]
    public enum DialogueBlockType
    {
        Speech,
        Action,
        Options
    }

    // also add things like If blocks - Switch Blocks - with output
    // Or something that does multiple things at once - like two characters speaking at once!
    // An Options block - Gives a choice of dialogue to player (or npc)

    /// <summary>
    /// Bone data
    /// Exported from a maya import with bones
    /// Used to spawn bone entities and to animate in game
    /// </summary>
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Zoxel/Dialogue")]//, order = 7)]
    public class DialogueDatam : ScriptableObject, ISerializationCallbackReceiver
    {
        public DialogueTree dialogueTree;

        public void GenerateID()
        {
            dialogueTree.id = Bootstrap.GenerateUniqueID();
        }

        public DialogueBranch AddBranchLink(DialogueBranch oldBranch)
        {
            for (int i = 0; i < dialogueTree.branches.Length; i++)
            {
                if (dialogueTree.branches[i].id == oldBranch.id)
                {
                    var branch = dialogueTree.branches[i];
                    branch.AddBranchLink();
                    dialogueTree.branches[i] = branch;
                    return branch;
                }
            }
            Debug.LogError("Could not add branch link: " + oldBranch.id);
            return oldBranch;
        }

        public void SetSpeakerType(DialogueBranch oldBranch, bool newSpeakerType)
        {
            for (int i = 0; i < dialogueTree.branches.Length; i++)
            {
                if (dialogueTree.branches[i].id == oldBranch.id)
                {
                    var branch = dialogueTree.branches[i];
                    branch.SetSpeakerType(newSpeakerType);
                    dialogueTree.branches[i] = branch;
                    break;
                }
            }
        }

        public void SetBranchSpeech(DialogueBranch oldBranch, string newSpeech)
        {
            for (int i = 0; i < dialogueTree.branches.Length; i++)
            {
                if (dialogueTree.branches[i].id == oldBranch.id)
                {
                    var branch = dialogueTree.branches[i];
                    branch.SetSpeech(newSpeech);
                    dialogueTree.branches[i] = branch;
                    break;
                }
            }
        }

        public DialogueBranch SetBranchLink(DialogueBranch oldBranch, int linkIndex, int linkBranchID)
        {
            for (int i = 0; i < dialogueTree.branches.Length; i++)
            {
                if (dialogueTree.branches[i].id == oldBranch.id)
                {
                    var branch = dialogueTree.branches[i];
                    if (linkIndex >= branch.links.Length)
                    {
                        Debug.LogError("Link Index out of bounds: " + oldBranch.id + ":" + linkIndex);
                    }
                    else {
                        branch.links[linkIndex] = linkBranchID;
                        dialogueTree.branches[i] = branch;
                    }
                    return branch;
                }
            }
            Debug.LogError("Could not set branch link: " + oldBranch.id);
            return oldBranch;
        }

        [ContextMenu("Generate Missing IDs")]
        public void GenerateMissingIds()
        {
            for (int i = 0; i < dialogueTree.branches.Length; i++)
            {
                if (dialogueTree.branches[i].id == 0)
                {
                    dialogueTree.branches[i].GenerateID();
                }
            }
        }
        
        #region SerializeableComponents

        [HideInInspector]
        public DialogueTree.SerializableDialogueTree clone;

        public void OnBeforeSerialize()
        {
            clone = dialogueTree.GetSerializeableClone();
            //Debug.LogError("OnBeforeSerialize - Branches: " + clone.branches.Length);
        }

        public void OnAfterDeserialize()
        {
            dialogueTree = clone.GetRealOne(); // unless actuall i dont need to serialize some basic stuff? 
            // dialogueTree.FromClone(clone);
            //Debug.LogError("OnAfterDeserialize - Branches: " + dialogueTree.branches.Length);
        }
        #endregion
    }
}

        /*[ContextMenu("Generate All IDs (Warning: Breaks Links)")]
        public void GenerateIds()
        {
            for (int i = 0; i < data.Count; i++)
            {
                data[i].GenerateID();
            }
        }*/