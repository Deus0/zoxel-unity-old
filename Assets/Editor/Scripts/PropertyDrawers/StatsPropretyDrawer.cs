using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    [CustomPropertyDrawer(typeof(Stats))]
    public class StatsDrawerUIE : PropertyDrawer
    {
        SerializedProperty cachedProperty;
        Stats stats;
        VisualElement statsHeader;
        VisualTreeAsset statsPrefab;
        List<StatDatam> statDatams;
        private List<VisualElement> statUIs = new List<VisualElement>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }

        private List<StatDatam> LoadFolder(string folderPath)
        {
            List<StatDatam> statDatams = new List<StatDatam>();
            var files = System.IO.Directory.GetFiles(folderPath);
            foreach (var filepath in files)
            {
                if (filepath.Contains(".meta") == false)
                {
                    string newPath = filepath.Substring(filepath.IndexOf("Assets/Data/Stats/"));
                    var statA = AssetDatabase.LoadAssetAtPath<StatDatam>(newPath);
                    statDatams.Add(statA);
                }
            }
            return statDatams;
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            cachedProperty = property;
            stats = (Stats)property.GetValue();
            var container = new VisualElement();
            container.styleSheets.Add(Resources.Load<StyleSheet>("StatsEditor/StatsEditor"));
            statsPrefab = Resources.Load<VisualTreeAsset>("StatsEditor/StatsEditor");
            // first clone ui into container
            var statsHeaderLoader = Resources.Load<VisualTreeAsset>("StatsEditor/StatsHeader");
            statsHeaderLoader.CloneTree(container);
            statsHeader = container.Query("StatsHeader").First();
            Button clearButton = container.Query("ClearButton").First() as Button;
            if (clearButton != null)
            {
                clearButton.clicked += () =>
                {
                    Clear();
                };
            }

            Button baseButton = container.Query("BaseStatsButton").First() as Button;
            if (baseButton != null)
            {
                baseButton.clicked += () =>
                {
                    LoadUI(StatType.Base);
                };
            }
            baseButton = container.Query("StatesButton").First() as Button;
            if (baseButton != null)
            {
                baseButton.clicked += () =>
                {
                    LoadUI(StatType.State);
                };
            }
            baseButton = container.Query("RegensButton").First() as Button;
            if (baseButton != null)
            {
                baseButton.clicked += () =>
                {
                    LoadUI(StatType.Regen);
                };
            }
            Button attributesButton = container.Query("AttributesButton").First() as Button;
            if (attributesButton != null)
            {
                attributesButton.clicked += () =>
                {
                    LoadUI(StatType.Attribute);
                };
            }
            attributesButton = container.Query("LevelsButton").First() as Button;
            if (attributesButton != null)
            {
                attributesButton.clicked += () =>
                {
                    LoadUI(StatType.Level);
                };
            }
            // then get the prefab
            //var statsPrefab = container.Query("StatID");
            LoadStatMeta();

            // should be find new ones or update current ones
            
            return container;
        }

        private void LoadUI(StatType loadType)
        {
            Clear(); 
            foreach (StatDatam statDatam in statDatams)
            {
                if (statDatam.type == loadType)
                {
                    AddStatUI(statDatam);
                }
            }
        }

        void LoadStatMeta()
        {
            statDatams = new List<StatDatam>();
            statDatams.AddRange(LoadFolder(Application.dataPath + "/Data/Stats/"));
            var subDirectories = System.IO.Directory.GetDirectories(Application.dataPath + "/Data/Stats/");
            foreach (string subDirectory in subDirectories)
            {
                //Debug.LogError("Loading SubDirectory: " + subDirectory);
                statDatams.AddRange(LoadFolder(subDirectory));
            }
        }

        void AddStatUI(StatDatam statDatam)
        {
            int indexOf = stats.GetStatIndex(statDatam);
            //quickToolVisualTree.CloneTree(container);
            // add new statsid to our container
            //var statUI = new VisualElement(statsPrefab);
            statsPrefab.CloneTree(statsHeader);
            var parentFoldout = statsHeader.Query("StatID").First();
            statUIs.Add(parentFoldout);
            parentFoldout.name = statDatam.name;
            Foldout statsFoldout = parentFoldout as Foldout;
            var template = parentFoldout.Query("TemplateStat").First();
            VisualElement inputA = null;
            VisualElement inputB = null;

            Image icon = template.Query("IconImage").First() as Image;
            if (icon != null)
            {
                icon.image = statDatam.texture.texture;
            }
            Label descriptionLabel = template.Query("StatDescription").First() as Label;
            if (icon != null)
            {
                descriptionLabel.text = statDatam.description;
            }
            Button initButton = template.Query("InitiateButton").First() as Button;
            if (statsFoldout != null)
            {
                SetStatLabel(statsFoldout, statDatam, initButton, indexOf != -1);
            }
            if (initButton != null)
            {
                initButton.clicked += () =>
                {
                    OnInitButtonClicked(statDatam, initButton, inputA, inputB, statsFoldout);
                };
            }

            foreach (var child in template.Children())
            {
                //Debug.LogError("Child Template names: " + child.name);
                if (child.name == "StatValue")
                {
                    inputA = child;
                    InitializeInputA(inputA, statDatam, stats, indexOf);
                }
                else if (child.name == "StatMaxValue")
                {
                    inputB = child;
                    InitializeInputB(inputB, statDatam, stats, indexOf);
                }
                /*else if (child.name == "IconImage")
                {
                    Image icon = child as Image;
                    if (icon != null)
                    {
                        icon.image = statDatam.texture.texture;
                    }
                }
                else if (child.name == "StatDescription")
                {
                    (child as Label).text = statDatam.description;
                }*/
            }
        }

        private void Clear()
        {
            Debug.Log("Clearing Stat UIs");
            foreach(var e in statUIs)
            {
                e.parent.Remove(e);
            }
            statUIs.Clear();
        }

        private void InitializeInputA(VisualElement inputA, StatDatam statDatam, Stats stats, int statIndex)
        {
            TextField statInput = inputA as TextField;
            if (statIndex != -1)
            {
               // inputA.visible = true;
                SetVisibility(inputA, true);
                if (statDatam.type == StatType.Base)
                {
                    statInput.value = stats.stats[statIndex].value.ToString();
                }
                else if (statDatam.type == StatType.State)
                {
                    statInput.value = stats.states[statIndex].value.ToString();
                }
                else if (statDatam.type == StatType.Regen)
                {
                    statInput.value = stats.regens[statIndex].value.ToString();
                }
                else if (statDatam.type == StatType.Attribute)
                {
                    statInput.value = stats.attributes[statIndex].multiplier.ToString();
                }
                else if (statDatam.type == StatType.Level)
                {
                    statInput.value = stats.levels[statIndex].experienceRequired.ToString();
                }
            }
            else
            {
                //inputA.visible = false;
                SetVisibility(inputA, false);
            }
            statInput.RegisterValueChangedCallback((eventInfo) =>
            {
                OnInputAChanged(eventInfo, statDatam);
            });
        }

        private void OnInputAChanged(ChangeEvent<string> eventInfo, StatDatam statDatam)
        {
            Stats stats = (Stats)cachedProperty.GetValue();
            int statIndex = stats.GetStatIndex(statDatam);
            if (statDatam.type == StatType.Base)
            {
                stats.SetStatValue(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.State)
            {
                stats.SetStateValue(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Regen)
            {
                stats.SetRegenValue(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Attribute)
            {
                stats.SetAttributeValue(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Level)
            {
                stats.SetLevelValue(statIndex, int.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
        }

        private void InitializeInputB(VisualElement inputB, StatDatam statDatam, Stats stats, int statIndex)
        {
            #region StatMaxValue
            TextField statInput = inputB as TextField;
            if (statIndex != -1 && statDatam.type != StatType.Base)
            {
               // inputB.visible = true;
                SetVisibility(inputB, true);
                if (statDatam.type == StatType.State)
                {
                    statInput.value = stats.states[statIndex].maxValue.ToString();
                }
                else if (statDatam.type == StatType.Regen)
                {
                    statInput.value = stats.regens[statIndex].rate.ToString();
                }
                else if (statDatam.type == StatType.Attribute)
                {
                    statInput.value = stats.attributes[statIndex].multiplier.ToString();
                }
                else if (statDatam.type == StatType.Level)
                {
                    statInput.value = stats.levels[statIndex].experienceRequired.ToString();
                }
            }
            else
            {
                //inputB.visible = false;
                SetVisibility(inputB, false);
            }

            if (statDatam.type == StatType.State)
            {
                statInput.label = "Max Value";
            }
            else if (statDatam.type == StatType.Regen)
            {
                statInput.label = "Rate";
            }
            else if (statDatam.type == StatType.Attribute)
            {
                statInput.label = "Multiplier";
            }
            else if (statDatam.type == StatType.Level)
            {
                statInput.label = "Experience Required";
            }
            statInput.RegisterValueChangedCallback((eventInfo) =>
            {
                OnInputBChanged(eventInfo, statDatam);
            });
            #endregion
        }

        private void OnInputBChanged(ChangeEvent<string> eventInfo, StatDatam statDatam)
        {
            if (eventInfo.newValue == "Rate" || eventInfo.newValue == "Multiplier" || eventInfo.newValue == "Experience Required")
            {
                //Debug.LogError("New Value is weird: " + eventInfo.newValue);
                return;
            }
            Stats stats = (Stats)cachedProperty.GetValue();
            int statIndex = stats.GetStatIndex(statDatam);
            if (statDatam.type == StatType.State)
            {
                stats.SetStateMaxValue(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Regen)
            {
                //Debug.LogError("eventInfo.newValue: " + eventInfo.eventTypeId + "::" + eventInfo.newValue);
                stats.SetRegenRate(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Attribute)
            {
                stats.SetAttributeMultiplier(statIndex, float.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
            else if (statDatam.type == StatType.Level)
            {
                stats.SetLevelExperienceRequired(statIndex, int.Parse(eventInfo.newValue));
                cachedProperty.SetValue(stats);
            }
        }

        private void OnInitButtonClicked(StatDatam statDatam, Button initButton, VisualElement buttonA, VisualElement buttonB, Foldout statsFoldout)
        {
            Stats stats = (Stats)cachedProperty.GetValue();
            int statIndex = stats.GetStatIndex(statDatam);
            if (statIndex == -1)
            {
                statIndex = stats.AddStat(statDatam);
            }
            else
            {
                //Debug.LogError("Removing Stat at: " + statIndex);
                stats.RemoveStat(statDatam, statIndex);
                statIndex = -1;
            }
            if (buttonA != null)
            {
                SetVisibility(buttonA, statIndex != -1);
                SetVisibility(buttonB, statIndex != -1 && statDatam.type != StatType.Base);
                //buttonA.visible = statIndex != -1;
                //buttonB.visible = statIndex != -1 && statDatam.type != StatType.Base;
            }
            else
            {
                Debug.LogError("Lambda has nulled button reference.");
            }
            /*if (statIndex == -1)
            {
                initButton.text = "Add " + statDatam.name + "";
                statsFoldout.text = statDatam.name + " (0)"; //  "[" + statDatam.Value.id + "] " +
            }
            else
            {
                initButton.text = "Remove " + statDatam.name;
                statsFoldout.text = statDatam.name + " (X)"; // "[" + statDatam.Value.id + "] " +
            }*/
            SetStatLabel(statsFoldout, statDatam, initButton, statIndex != -1);
            cachedProperty.SetValue(stats);
        }

        private void SetVisibility(VisualElement ve, bool state)
        {
            var display = ve.style.display;
            if (!state)
            {
                display.value = DisplayStyle.None;
            }
            else
            {
                display.value = DisplayStyle.Flex;
            }
            ve.style.display = display;
        }

        private void SetStatLabel(Foldout statsFoldout, StatDatam statDatam, Button initButton, bool hasStat)
        {
            if (!hasStat)
            {
                statsFoldout.text = "   - no " + statDatam.name; //  "[" + statDatam.Value.id + "] " +
                if (initButton != null)
                {
                    initButton.text = "Give " + statDatam.name;
                }
            }
            else
            {
                statsFoldout.text = "Has " + statDatam.name; //  "[" + statDatam.Value.id + "] " +
                if (initButton != null)
                {
                    initButton.text = "Take " + statDatam.name + " Away";
                }
            }
        }
    }
}