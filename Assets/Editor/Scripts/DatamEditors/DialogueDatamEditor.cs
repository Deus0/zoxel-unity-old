using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Zoxel
{
    [CustomEditor(typeof(DialogueDatam))]
    public class DialogueDatamEditor : Editor
    {
        SerializedObject cachedObject;
        DialogueDatam dialogueDatam;
        VisualElement main;
        VisualTreeAsset blockPrefab;
        private List<VisualElement> uis = new List<VisualElement>();

        public override VisualElement CreateInspectorGUI()
        {    
            var container = new VisualElement();
            cachedObject = serializedObject;

            //cachedProperty = property;
            //cachedProperty = serializedObject;

            dialogueDatam = (DialogueDatam)cachedObject.targetObject;

            container.styleSheets.Add(Resources.Load<StyleSheet>("DialogueEditor/DialogueStyle"));
            
            // set up main first
            var dialogueMainLoader = Resources.Load<VisualTreeAsset>("DialogueEditor/DialogueMain");
            dialogueMainLoader.CloneTree(container);
            main = container.Query("Content").First();

            // header buttons
            Button addBranchButton = container.Query("AddBranchButton").First() as Button;
            if (addBranchButton != null)
            {
                addBranchButton.clicked += () =>
                {
                    AddBranchClicked(dialogueDatam);
                };
            }

            // blocks
            blockPrefab = Resources.Load<VisualTreeAsset>("DialogueEditor/DialogueBlock");
            LoadUI();

            // Draw the legacy IMGUI base
            //var imgui = new IMGUIContainer(OnInspectorGUI);
            // Create property fields.
            // Add fields to the container.
            //container.Add(new PropertyField(serializedObject.FindProperty("stats")));
            //container.Add(imgui);

            return container;
        }



        private void Clear()
        {
            Debug.Log("Clearing Dialogue UIs");
            foreach(var e in uis)
            {
                e.parent.Remove(e);
            }
            uis.Clear();
        }

        private void LoadUI()
        {
            Clear(); 
            //foreach (var block in dialogueDatam.dialogueTree.branches)
            for (int i = 0; i < dialogueDatam.dialogueTree.branches.Length; i++)
            {
                var block = dialogueDatam.dialogueTree.branches[i];
                // can add filters here
                uis.Add(LoadBlockUI(dialogueDatam, block));
            }
        }

        //List<Foldout> linksFoldouts = new List<Foldout>();
        

        private VisualElement LoadBlockUI(DialogueDatam dialogueDatam, DialogueBranch block) {
            
            blockPrefab.CloneTree(main);
            var blockUI = main.Query("DialogueBlock").Last() as Foldout;
            blockUI.text = "Branch [" + block.id + "]";
            
            TextField dialogueInput = blockUI.Query("DialogueInput").First() as TextField;
            if (dialogueInput != null) {
                //Debug.LogError("Setting speech to: " + block.speech);
                dialogueInput.value = block.speech;
                dialogueInput.RegisterValueChangedCallback((eventInfo) =>
                {
                    OnSpeechUpdated(eventInfo, dialogueDatam, block);
                });
            }
            
            Toggle isPlayerToggle = blockUI.Query("IsPlayerToggle").First() as Toggle;
            if (isPlayerToggle != null) {
                if (block.speakerType == 1) {
                    isPlayerToggle.value = true;
                } else {
                    isPlayerToggle.value = false;
                }
                isPlayerToggle.RegisterValueChangedCallback((eventInfo) =>
                {
                    OnSpeakerTypeUpdated(eventInfo, dialogueDatam, block);
                });
            }

            Button deleteButton = blockUI.Query("DeleteButton").First() as Button;
            if (deleteButton != null)
            {
                deleteButton.clicked += () =>
                {
                    RemoveBranchClicked(dialogueDatam, block, blockUI);
                };
            }
            
            Button newLinkButton = blockUI.Query("AddLinkButton").First() as Button;
            if (newLinkButton != null)
            {
                newLinkButton.clicked += () =>
                {
                    AddBranchLink(newLinkButton.parent, dialogueDatam, block); // , newLinkButton
                };
            }

            //var linksFoldoutPrefab = blockUI.Query("Links").First() as Foldout;
            for (int i = 0; i < block.links.Length; i++)
            {
                AddLinkUI(newLinkButton.parent, dialogueDatam, block, i);
            }

            //linkPrefab.parent.Remove(linkPrefab);
            // for every other dialogue block in the tree, add a new linkChoice

            // add new LinkUI - FoldoutUI for every link in the data

            // Have to also do set text

            // Set character Name

            // Set Next with Dropdown

            return blockUI;
            // set up buttons here
            // character name text
            // input for speech
            // dropdown for type of Dialogue it is (can change to action)
            // Dropdown for which one it will go to next (blank if missing)

            //var parentFoldout = statsHeader.Query("StatID").First();
        }

        private void AddLinkUI(VisualElement parent, DialogueDatam dialogueDatam, DialogueBranch branch, int linkIndex)
        {
            Foldout linksFoldout = new Foldout();
            linksFoldout.name = "Links";
            parent.Add(linksFoldout);
            if (linkIndex < 0 || linkIndex >= branch.links.Length)
            {
                linksFoldout.text = "[Errored]";
            }
            else if (branch.links[linkIndex] != 0)
            {
                linksFoldout.text = "Next Branch [" + branch.links[linkIndex] + "]";
            }
            else
            {
                linksFoldout.text = "[Unlinked]";
            }
            for (int i = 0; i < dialogueDatam.dialogueTree.branches.Length; i++)
            {
                AddLinkChoiceUI(branch, dialogueDatam.dialogueTree.branches[i], linksFoldout, linkIndex);
            }
            linksFoldout.value = false;
        }

        private void AddLinkChoiceUI(DialogueBranch branchA, DialogueBranch branchB, Foldout linksFoldout, int linkIndex)
        {
            if (linksFoldout == null)
            {
                Debug.LogError("AddLinkUI failed with null parent.");
                return;
            }
            if (branchB.id != branchA.id)
            {
                // add a link choice
                //linkPrefabVS.CloneTree(linkPrefab.parent);
                //Button linkButton = blockUI.Query("LinkPrefab").Last() as Button;
                Button linkButton = new Button();
                linksFoldout.Query("unity-content").First().Add(linkButton);
                linkButton.text = "[" + branchB.id + "]";
                if (linkButton != null)
                {
                    int nextID = branchB.id;
                    string nextSpeech = branchB.speech;
                    linkButton.clicked += () =>
                    {
                        OnLinkClicked(linksFoldout, dialogueDatam, branchA, linkIndex, nextID, nextSpeech);
                    };
                }
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                DrawDefaultInspector();
            }
            catch (System.NullReferenceException e)
            {
                UnityEngine.Debug.Log("e: " + e.ToString());
            }
        }

        
        private void AddBranchClicked(DialogueDatam dialogueDatam)
        {
            var branch = new DialogueBranch();
            branch.id = Bootstrap.GenerateUniqueID();
            uis.Add(LoadBlockUI(dialogueDatam, branch));
            dialogueDatam.dialogueTree.AddBranch(branch);
            //cachedProperty.SetValue(stats);
            for (int i = 0; i < dialogueDatam.dialogueTree.branches.Length - 1; i++)
            {
                var linkUIs = uis[i].Query("Links");
                //for (int j = 0; j < linkUIs.Count; j++)
                //foreach (var linkUI in linkUIs.Descendents)
                int linkIndex = 0;
                linkUIs.ForEach((linkUI) =>
                {
                    AddLinkChoiceUI(dialogueDatam.dialogueTree.branches[i], branch, linkUI as Foldout, linkIndex);
                    linkIndex++;
                });
            }
            EditorUtility.SetDirty(cachedObject.targetObject);
        }

        private void RemoveBranchClicked(DialogueDatam dialogueDatam, DialogueBranch branch, VisualElement ui)
        {
            // remove any link uis from other branches
            for (int i = 0; i < uis.Count; i++)
            {
                if (uis[i] != ui) {
                    var linkUIs = uis[i].Query("Links");
                    //for (int j = 0; j < linkUIs.Count; j++)
                    //foreach (var linkUI in linkUIs.Descendents)
                    linkUIs.ForEach((linkUI) =>
                        {
                            var content = linkUI.Query("unity-content").First().Query<Button>();
                            content.ForEach((childButton) => {
                                if (childButton.text == "[" + branch.id + "]")
                                {
                                    childButton.parent.Remove(childButton); // remove the link to branch
                                }
                            });
                        });
                }
            }
            dialogueDatam.dialogueTree.RemoveBranch(branch);
            // remove the UI as well
            uis.Remove(ui);
            ui.parent.Remove(ui);
            // check for branches - if they are linked to this branch
            for (int i = 0; i < dialogueDatam.dialogueTree.branches.Length; i++) {
                var otherBranch = dialogueDatam.dialogueTree.branches[i];
                for (int j = 0; j < otherBranch.links.Length; j++) {
                    if (otherBranch.links[j] == branch.id) {
                        // reset branch link
                        dialogueDatam.SetBranchLink(otherBranch, j, 0);
                        // also set the link ui
                        uis[i].Query<Foldout>("Links").ToList()[j].text = "[Unlinked]";
                    }
                }
            }
        }

        private void AddBranchLink(VisualElement parent, DialogueDatam dialogueDatam, DialogueBranch branch)
        {
            branch = dialogueDatam.AddBranchLink(branch);
            AddLinkUI(parent, dialogueDatam, branch, branch.links.Length - 1);
            EditorUtility.SetDirty(cachedObject.targetObject);
        }

        private void OnSpeechUpdated(ChangeEvent<string> eventInfo, DialogueDatam dialogueDatam, DialogueBranch branch)
        {
            dialogueDatam.SetBranchSpeech(branch, eventInfo.newValue);
            EditorUtility.SetDirty(cachedObject.targetObject);
        }

        private void OnSpeakerTypeUpdated(ChangeEvent<bool> eventInfo, DialogueDatam dialogueDatam, DialogueBranch branch) 
        {
            dialogueDatam.SetSpeakerType(branch, eventInfo.newValue);
            EditorUtility.SetDirty(cachedObject.targetObject);
        }

        private void OnLinkClicked(Foldout linksFoldout,  DialogueDatam dialogueDatam, DialogueBranch branch, int linkIndex, int nextID, string nextSpeech)
        {
            if (nextSpeech == null) {
                nextSpeech = "";
            }
            // set id to the text
            if (linkIndex >= branch.links.Length)
            {
                Debug.LogError("Link Index out of bounds: " + branch.id + ":" + linkIndex);
            }
            else
            {
                branch = dialogueDatam.SetBranchLink(branch, linkIndex, nextID);
                //branch.links[linkIndex] = nextID;
                linksFoldout.text = "Next Branch [" + branch.links[linkIndex] + "]: " + 
                    nextSpeech.Substring(0, Mathf.Min(nextSpeech.Length, 16)) + "..";
                linksFoldout.value = false;
                EditorUtility.SetDirty(cachedObject.targetObject);
            }
        }

    }

}
