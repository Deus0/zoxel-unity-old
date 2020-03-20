using UnityEngine;
using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.UIElements.Runtime;

namespace Zoxel
{
    public class UserInterfaceHandler : MonoBehaviour
    {
        //public string firstSelectedName;
         /*private float lastUpdatedNavigation;
        private float navigateMenuSpeed = 0.6f;
        private readonly float stickThreshold = 0.8f;
        private int focusedButtonIndex;
        private string focusedButton;
        // events for bootstrap
        public Action<Button> OnButtonPressed;
        public Action<Slider> OnSliderUpdated;
        public Action<Toggle> OnToggleUpdated;
       private PanelRenderer panel;
        private List<NavigationElement> visualElements = new List<NavigationElement>();


        [Serializable]
        public struct NavigationElement
        {
            public string name;
            public float2 position;
            public List<NavigationElementPointer> pointers;
        }

        [Serializable]
        public struct NavigationElementPointer
        {
            public string target;
            public NavigationElementDirection direction;
        }

        [Serializable]
        public enum NavigationElementDirection
        {
            None,
            Down,
            Up,
            Left,
            Right
        }

        public void SetLabel(EndGameReason reason)
        {
            PanelRenderer panel = GetComponent<PanelRenderer>();
            var labels = panel.visualTree.Query<Label>();    //EndOfGameLabel
            labels.ForEach((label) =>
            {
                if (label.name == "EndOfGameLabel")
                {
                    label.text = reason.ToString();
                }
            });
        }

        [ContextMenu("Debug")]
        public void DebugNavigation()
        {
            // draw all next pointers
            for (int i = 0; i < visualElements.Count - 1; i++)
            {
                Debug.DrawLine(new Vector3(visualElements[i].position.x, visualElements[i].position.y, 0),
                    new Vector3(visualElements[i + 1].position.x, visualElements[i + 1].position.y, 0), Color.red, 10);
            }
        }
        [ContextMenu("DebugSelected")]
        public void DebugSelected()
        {
            Debug.LogError(visualElements[focusedButtonIndex].name + " is selected.");
        }
        private void Awake()
        {
            panel = GetComponent<PanelRenderer>();
            lastUpdatedNavigation = Time.time;
        }

        void Start()
        {
            StartCoroutine(DelayedSetupUI());
        }

        private IEnumerator DelayedSetupUI()
        {
            yield return null;
            SetupLoadUI();
            yield return null;
            yield return null;
            yield return null;
            SetupUI();
        }

        private void Update()
        {
            Entity gameEntity = Bootstrap.instance.game;
            Game game = Bootstrap.instance.EntityManager.GetComponentData<Game>(gameEntity);
            if (game.state == (byte)GameState.MainMenu
                || game.state == (byte)GameState.PauseScreen
                || game.state == (byte)GameState.NewGameScreen
                || game.state == (byte)GameState.SaveGamesScreen
                || game.state == (byte)GameState.RespawnScreen)
            {
                // get gamepad
                foreach (Gamepad pad in Gamepad.all)
                {
                    NavigateUI(pad);
                    TriggerUI(pad);
                }
            }
        }

        #region UIElementsPart

        private void SetupLoadUI()
        {
            if (name == "LoadGameScreen")
            {
                Button template = panel.visualTree.Query<Button>("TemplateSlot");
                VisualElement parent = template.parent;
                string[] saveFiles = Bootstrap.instance.saveManager.GetSaveSlots();
                for (int i = 0; i < saveFiles.Length; i++)
                {
                    Button newButton = new Button();
                    newButton.text = saveFiles[i];
                    newButton.name = "SaveSlot_" + i;
                    newButton.focusable = true;
                    parent.Add(newButton);
                    //AddVisualElement(newButton);
                }
                //template.SetEnabled(false);
                parent.Remove(template);
            }
        }

        private void SetupUI()
        {
            var toolButtons = panel.visualTree.Query<Button>();
            var sliders = panel.visualTree.Query<Slider>();
            var textFields = panel.visualTree.Query<TextField>();
            var toggles = panel.visualTree.Query<Toggle>();
            var foldouts = panel.visualTree.Query<Foldout>();
            visualElements.Clear();
            toolButtons.ForEach((button) =>
            {
                AddVisualElement(button);
            });
            foldouts.ForEach((foldout) =>
            {
                AddVisualElement(foldout);
            });
            textFields.ForEach((textField) =>
            {
                AddVisualElement(textField);
            });
            sliders.ForEach((slider) =>
            {
                AddVisualElement(slider);
                Slider sliderRefernce = slider;
                slider.RegisterValueChangedCallback((changeEvent) =>
                {
                    if (sliderRefernce != null && OnSliderUpdated != null)
                    {
                        OnSliderUpdated.Invoke(sliderRefernce);
                    }
                });
            });
            toggles.ForEach((toggle) =>
            {
                AddVisualElement(toggle);
                Toggle toggleRefernce = toggle;
                toggle.RegisterValueChangedCallback((changeEvent) =>
                {
                    if (toggleRefernce != null && OnToggleUpdated != null)
                    {
                        OnToggleUpdated.Invoke(toggleRefernce);
                    }
                });
            });
#if UNITY_EDITOR
            var colorFields = panel.visualTree.Query<UnityEditor.UIElements.ColorField>();
            colorFields.ForEach((colorField) =>
            {
                AddVisualElement(colorField);
            });
#endif
            CreateNavigationLinks();
            foldouts.ForEach((foldout) =>
            {
                foldout.value = false;
            });
        }

        private void AddVisualElement(VisualElement button)
        {
            if (button.focusable && button.name != ""
                && !button.name.Contains("unity-"))
            {
                if (isLog)
                    Debug.LogError("Adding new Button to navigation system [" + visualElements.Count + "]: " + button.localBound.position);
                visualElements.Add(new NavigationElement
                {
                    name = button.name,
                    position = button.worldBound.position
                });
                Button buttonRefernce = button as Button;
                if (buttonRefernce != null)
                {
                    buttonRefernce.clickable.clicked += () =>
                    {
                        if (buttonRefernce != null && OnButtonPressed != null)
                        {
                            OnButtonPressed.Invoke(buttonRefernce);
                        }
                    };
                }
            }
        }

        private void TriggerVisualElement(string buttonName)
        {
            Button button = panel.visualTree.Query<Button>(buttonName);
            if (button != null)
            {
                Clickable clickable = button.clickable;
                MouseDownEvent mouseDownEvent = MouseDownEvent.GetPooled();
                MethodInfo dynMethod = clickable.GetType().GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(clickable, new object[] { mouseDownEvent });
                mouseDownEvent.Dispose();
            }
            else
            {
                Foldout foldout = panel.visualTree.Query<Foldout>(buttonName);
                if (foldout != null)
                {
                    foldout.value = !foldout.value;
                }
            }
        }

        private void SlideSlider(string elementName, NavigationElementDirection direction)
        {
            Slider slider = panel.visualTree.Query<Slider>(elementName);
            if (slider != null)
            {
                if (direction == NavigationElementDirection.Right)
                {
                    slider.value = slider.value + 1;
                    //Debug.Log("Increasing: " + elementName + " slider value.");
                }
                else
                {
                    slider.value -= 1;
                    //Debug.Log("Decreasing: " + elementName + " slider value.");
                }
            }
        }

        private void FocusElement(string buttonName)
        {
            VisualElement element = panel.visualTree.Query<VisualElement>(buttonName);
            if (element != null)
            {
                //Debug.Log("Focusing element: " + buttonName);
                element.Focus();
                if (element.enabledInHierarchy == false)
                {
                    Debug.LogError("NOT VISIBLE OMG");
                }
            }
            else
            {
                Debug.LogError("Cannot focus element: " + buttonName + " as it is not found in the visual tree.");
            }
        }
        #endregion

        #region UserInput
        private void NavigateUI(Gamepad pad)
        {
            float2 leftStick = pad.leftStick.ReadValue();
            if (leftStick.y >= -stickThreshold && leftStick.y <= stickThreshold
                && leftStick.x >= -stickThreshold && leftStick.x <= stickThreshold)
            {
                lastUpdatedNavigation = Time.realtimeSinceStartup - navigateMenuSpeed;
            }
            else if (Time.realtimeSinceStartup - lastUpdatedNavigation >= navigateMenuSpeed)
            {
                // this one
                if (leftStick.y > stickThreshold)
                {
                    lastUpdatedNavigation = Time.realtimeSinceStartup;
                    NavigateUI(NavigationElementDirection.Up);
                }
                else if (leftStick.y < -stickThreshold)
                {
                    lastUpdatedNavigation = Time.realtimeSinceStartup;
                    NavigateUI(NavigationElementDirection.Down);
                }
                else if (leftStick.x < -stickThreshold)
                {
                    lastUpdatedNavigation = Time.realtimeSinceStartup;
                    if (!NavigateUI(NavigationElementDirection.Left))
                    {
                        SlideSlider(focusedButton, NavigationElementDirection.Left);
                    }
                }
                else if (leftStick.x > stickThreshold)
                {
                    lastUpdatedNavigation = Time.realtimeSinceStartup;
                    if (!NavigateUI(NavigationElementDirection.Right))
                    {
                        SlideSlider(focusedButton, NavigationElementDirection.Right);
                    }
                }
            }
        }

        private void TriggerUI(Gamepad pad)
        {
            if (visualElements.Count == 0)
            {
                return;
            }
            if (pad.aButton.wasPressedThisFrame)
            {
                focusedButton = visualElements[focusedButtonIndex].name;
                TriggerVisualElement(focusedButton);
            }
        }
        #endregion

        #region GamepadNavigation
        public bool isLog = false;

        private void OnEnable()
        {
            lastUpdatedNavigation = Time.realtimeSinceStartup - navigateMenuSpeed;
            //focusedButtonIndex = 0;
            StartCoroutine(FocusFirstRoutine());
        }

        /// <summary>
        /// Returns if on the correct axis
        /// </summary>
        private bool NavigateUI(NavigationElementDirection direction)
        {
            if (visualElements.Count == 0)
            {
                return true;
            }
            NavigationElement selectedElement = visualElements[focusedButtonIndex];
            int pointerIndex = -1;
            for (int i = 0; i < selectedElement.pointers.Count; i++)
            {
                if (direction == selectedElement.pointers[i].direction)
                {
                    pointerIndex = i;
                    break;
                }
            }
            if (pointerIndex == -1)
            {
                //Debug.LogError("Pointer Index is -1");
                return false;   // didnt do the thing
            }
            int newIndex = focusedButtonIndex;
            for (int i = 0; i < visualElements.Count; i++)
            {
                if (visualElements[i].name == selectedElement.pointers[pointerIndex].target)
                {
                    newIndex = i;
                    break;
                }
            }
            if (newIndex != focusedButtonIndex)
            {
                FocusElement(newIndex);
            }
            return true;
            //newIndex = Mathf.Clamp(newIndex, 0, visualElements.Count - 1);
        }

        private void FocusElement(int newIndex)
        {
            //if (newIndex != focusedButtonIndex)
            if (focusedButtonIndex >= 0 && focusedButtonIndex < visualElements.Count)
            {
                focusedButtonIndex = newIndex;
                focusedButton = visualElements[focusedButtonIndex].name;
                FocusElement(focusedButton);
            }
        }

        private IEnumerator FocusFirstRoutine()
        {
            yield return null;
            yield return null;
            yield return null;
            FocusFirstButton();
        }

        [ContextMenu("FocusFirst")]
        public void FocusFirstButton()
        {
            if (gameObject.activeSelf && panel.visualTree != null && visualElements.Count > 0)
            {
                FocusElement(focusedButtonIndex);
            }
            else
            {
                if (!gameObject.activeSelf)
                {
                    Debug.LogError("Cannot focus first button as (gameobject is inactive)..: " + name);
                }
                if (panel.visualTree == null)
                {
                    Debug.LogError("Cannot focus first button as (visual tree is null)..: " + name);
                }
                if (visualElements.Count == 0)
                {
                    //Debug.LogError("Cannot focus first button as (visualElements Count is 0)..: " + name);
                }
            }
        }

        /// <summary>
        /// Sort navigation elements by position, top to bottom, left to right.
        /// </summary>
        private void CreateNavigationLinks()
        {
            visualElements.Sort((NavigationElement elementA, NavigationElement elementB) =>
            {
                if (elementA.position.y == elementB.position.y)
                {
                    //Debug.LogError(elementB.name + " and " + elementA.name + " are at Y: " + elementB.position.y);
                    return (int)(elementA.position.x - elementB.position.x);
                }
                else
                {
                    return (int)(elementA.position.y - elementB.position.y);
                }
            });
            // now add directions
            if (isLog)
                Debug.LogError("Total of Elements [" + visualElements.Count + "] for " + name);
            for (int i = 0; i < visualElements.Count; i++)
            {
                NavigationElement element = visualElements[i];
                element.pointers = new List<NavigationElementPointer>();
                if (i != visualElements.Count - 1)
                {
                    if (element.position.y == visualElements[i + 1].position.y)
                    {
                        element.pointers.Add(new NavigationElementPointer { target = visualElements[i + 1].name, direction = NavigationElementDirection.Right });
                        for (int j = i + 1; j < visualElements.Count; j++)
                        {
                            if (element.position.y != visualElements[j].position.y)
                            {
                                element.pointers.Add(new NavigationElementPointer { target = visualElements[j].name, direction = NavigationElementDirection.Down });
                                break;
                            }
                        }
                        if (isLog)
                            Debug.LogError("Allocating Forward Direction as (Right) for [" + i + "]: " + element.position + ", Next: " + visualElements[i + 1].position);
                    }
                    else
                    {
                        if (isLog)
                            Debug.LogError("Allocating Forward Direction as (Down) for [" + i + "]: " + element.position + ", Next: " + visualElements[i + 1].position);
                        element.pointers.Add(new NavigationElementPointer { target = visualElements[i + 1].name, direction = NavigationElementDirection.Down });
                    }
                }
                else if (isLog)
                {
                    Debug.LogError("Allocating Forward Direction as (None) for [" + i + "]");
                }
                if (i != 0)
                {
                    if (element.position.y == visualElements[i - 1].position.y)
                    {
                        element.pointers.Add(new NavigationElementPointer { target = visualElements[i - 1].name, direction = NavigationElementDirection.Left });
                        for (int j = i - 1; j >= 0; j--)
                        {
                            if (element.position.y != visualElements[j].position.y)
                            {
                                element.pointers.Add(new NavigationElementPointer { target = visualElements[j].name, direction = NavigationElementDirection.Up });
                                break;
                            }
                        }
                        if (isLog)
                            Debug.LogError("Allocating Reverse Direction as (Left) for [" + i + "]: " + element.position + ", Previous: " + visualElements[i - 1].position);
                    }
                    else
                    {
                        element.pointers.Add(new NavigationElementPointer { target = visualElements[i - 1].name, direction = NavigationElementDirection.Up });
                        if (isLog)
                            Debug.LogError("Allocating Reverse Direction as (Up) for [" + i + "]: " + element.position + ", Previous: " + visualElements[i - 1].position);
                    }
                }
                visualElements[i] = element;
            }
        }
        #endregion
        */
    }
}