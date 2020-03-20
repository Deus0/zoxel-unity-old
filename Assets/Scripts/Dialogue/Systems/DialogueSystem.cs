using Unity.Entities;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Zoxel
{
    // TODO:
    //      Add choices - generate textboxes for choices after each line of dialogue is given
    //      Remove choices - remove navigation and choices from previous ones
    //      After end of all dialogue, instead of looping, set players controller to map back and close the entire dialogue panel
    [DisableAutoCreation]
    public class DialogueSystem : ComponentSystem
    {
        public Dictionary<int, DialogueDatam> meta;
        public UIDatam uiDatam;
        private const float buttonFontSize = 0.025f;

        protected override void OnUpdate()
        {
            Entities.WithAll<DialogueUI, RenderText>().ForEach((Entity e, ref DialogueUI dialogue, ref RenderText renderText) =>
            {
                if (UnityEngine.Time.time - dialogue.timeBegun >= dialogue.timePerLetter)
                {
                    if (dialogue.HasFinished())
                    {
                        // spawn next button now
                        // if has not spawned next buttons
                        if (dialogue.hasSpawnedButtons == 0)
                        {
                            dialogue.hasSpawnedButtons = 1;
                            DialogueDatam dialogueTree = meta[dialogue.treeID];
                            var currentBranch = dialogueTree.dialogueTree.branches[dialogue.branchID];
                            Childrens children = new Childrens { };
                            if (currentBranch.links.Length <= 1)
                            {
                                // spawn next button
                                children.children = new BlitableArray<Entity>(1, Unity.Collections.Allocator.Persistent);
                                float3 buttonPosition = new float3(0, (-renderText.fontSize / 2f - buttonFontSize / 2f), 0);
                                string dialogueOptionA = "Next";
                                children.children[0] = UIUtilities.SpawnButtonWithText(World.EntityManager, e, 
                                    buttonPosition, buttonFontSize, dialogueOptionA, uiDatam.menuButton);
                            }
                            else
                            {
                                children.children = new BlitableArray<Entity>(currentBranch.links.Length, Unity.Collections.Allocator.Persistent);
                                // spawn a button for all links
                                float3 offset = new float3();
                                children.children = new BlitableArray<Entity>(currentBranch.links.Length, Unity.Collections.Allocator.Persistent);
                                DialogueDatam dialogueDatam = meta[dialogue.treeID];
                                for (int i = 0; i < currentBranch.links.Length; i++)
                                {
                                    float3 buttonPosition = new float3(0, (-renderText.fontSize / 2f - buttonFontSize / 2f), 0);
                                    buttonPosition += offset;
                                    offset = buttonPosition;
                                    string dialogueOptionA = "Leave";
                                    for (int j = 0; j < dialogueDatam.dialogueTree.branches.Length; j++) {
                                        var otherBranch = dialogueDatam.dialogueTree.branches[j];
                                        if (otherBranch.id == currentBranch.links[i])
                                        {
                                            dialogueOptionA = otherBranch.speech;
                                            break;
                                        }
                                    }
                                    children.children[i] = UIUtilities.SpawnButtonWithText(World.EntityManager, e, 
                                        buttonPosition, buttonFontSize, dialogueOptionA, uiDatam.menuButton);
                                }
                            }
                            World.EntityManager.SetComponentData(e, children);
                            // set navigation dirty
                            var panelUI = World.EntityManager.GetComponentData<PanelUI>(e);
                            panelUI.navigationDirty = 1;
                            World.EntityManager.SetComponentData(e, panelUI);
                        }
                        // or spawn dialogue options
                        else if (dialogue.confirmedChoice != 0)
                        {
                            IncrementDialogue(e, ref dialogue, ref renderText);
                            dialogue.confirmedChoice = 0;
                            dialogue.hasSpawnedButtons = 0;
                            // remove previous buttons
                            Childrens childrens = World.EntityManager.GetComponentData<Childrens>(e);
                            childrens.DestroyEntities(World.EntityManager);
                            World.EntityManager.SetComponentData(e, new Childrens {});

                            var panelUI = World.EntityManager.GetComponentData<PanelUI>(e);
                            panelUI.navigationDirty = 1;
                            World.EntityManager.SetComponentData(e, panelUI);
                        }
                    }
                    else
                    {
                        IncrementLetters(ref dialogue, ref renderText);
                        if (dialogue.confirmedChoice != 0)
                        {
                            dialogue.confirmedChoice = 0;   // shouldn't be able to get here
                        }
                    }
                }
            });
        }

        private void OnCompletedDialogue(Entity e, ref DialogueUI dialogue, ref RenderText renderText)
        {
            //Debug.LogError("completedTree " + " 1!");
            dialogue.completedTree = 1;
            //dialogue.branchID = 0;
            //SetBranch(0, e, ref dialogue, ref renderText);
        }

        private void IncrementDialogue(Entity e, ref DialogueUI dialogue, ref RenderText renderText)
        {
            int linkChoice = dialogue.confirmedChoice - 1;
            DialogueDatam dialogueTree = meta[dialogue.treeID];
            var currentBranch = dialogueTree.dialogueTree.branches[dialogue.branchID];
            int branchID = -1;
            if (currentBranch.links.Length > 0 && linkChoice < currentBranch.links.Length)
            {
                //Debug.LogError("linkChoice was " + linkChoice);
                branchID = currentBranch.links[linkChoice];
            }
            if (branchID == -1)
            {
                OnCompletedDialogue(e, ref dialogue, ref renderText);
                return;
            }
            int linkIndex = -1;
            for (int i = 0; i < dialogueTree.dialogueTree.branches.Length; i++) 
            {
                if (dialogueTree.dialogueTree.branches[i].id == branchID) 
                {
                    linkIndex = i;
                    break;
                }
            }
            if (linkIndex == -1)
            {
                OnCompletedDialogue(e, ref dialogue, ref renderText);
            }
            else
            {
                SetBranch(linkIndex, e, ref dialogue, ref renderText);
            }
        }

        /// <summary>
        /// sets dialogue to a new speech bubble
        /// </summary>
        private void SetBranch(int newBranchID, Entity e, ref DialogueUI dialogue, ref RenderText renderText)
        {
            DialogueDatam dialogueTree = meta[dialogue.treeID];
            dialogue.branchID = newBranchID;
            var branch = dialogueTree.dialogueTree.branches[dialogue.branchID];
            dialogue.SetText(branch.speech, ref renderText);
            renderText.offsetX = ((-branch.speech.Length - 1f) / 2f) * renderText.fontSize;
            DialogueUISpawnSystem.RefreshPanelSize(World.EntityManager, e,
                renderText.fontSize, branch.speech.Length);
        }

        private void IncrementLetters(ref DialogueUI dialogue, ref RenderText renderText)
        {
            dialogue.RandomizeCooldown();
            dialogue.IncreaseIndex(ref renderText);
            //RefreshPanelSize(e, newText);
        }
    }

}