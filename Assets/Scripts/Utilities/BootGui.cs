using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Zoxel.Voxels;
using UnityEngine.InputSystem;

namespace Zoxel
{
    // atm
    // j for switching cameras

    public class BootGui : MonoBehaviour
    {
        public int debugGUIType;
        private float deltaTime = 0.0f;
        public Bootstrap booty;
        public Color uiColor = Color.blue;
        public Color uiColor2 = Color.red;

        public void OnGUI()
        {
            if (booty.GetSystems() == null)
            {
                GUI.color = uiColor;
                GUILayout.Label("SystemsManager is null.");
            }
            GUI.color = uiColor2;
            GUILayout.Label(GetFPSText());
            GUI.color = uiColor;
            if (debugGUIType == 1)
            {
                GUILayout.Label("Game [" + ((GameState)(booty.EntityManager.GetComponentData<Game>(booty.game).state)) + "]");
                GUILayout.Label("   > [" + ((GameState)(booty.EntityManager.GetComponentData<Game>(booty.game).newState)) + "]");
                GUILayout.Label("Worlds " + booty.GetSystems().voxelSystemGroup.worldSpawnSystem.worlds.Count);
                GUILayout.Label("Chunks " + booty.GetSystems().voxelSystemGroup.chunkSpawnSystem.chunks.Count);
                GUILayout.Label("ChunkRenders " + booty.GetSystems().voxelSystemGroup.chunkSpawnSystem.chunkRenders.Count);

                GUILayout.Label(" - ");
                GUILayout.Label("Controllers " + booty.GetSystems().playerSystemGroup.playerSpawnSystem.controllers.Count);
                GUILayout.Label("Characters " + booty.GetSystems().characterSystemGroup.characterSpawnSystem.characters.Count);
                GUILayout.Label("- Temp -");
                GUILayout.Label("Bullets " + booty.GetSystems().bulletSystemGroup.bulletSpawnSystem.bullets.Count);
                GUILayout.Label("Popups " + booty.GetSystems().uiSystemGroup.damagePopupSystem.popups.Count);
                GUILayout.Label("Popups " + booty.GetSystems().itemSystemGroup.itemSpawnSystem.items.Count);

                GUILayout.Label("Player Stuff");
                GUILayout.Label("InventoryUIs " + booty.GetSystems().uiSystemGroup.inventoryUISpawnSystem.uis.Count);
                // old stuff
                GUILayout.Label(" - ");
                GUILayout.Label("Turrets " + TurretSpawnerSystem.turrets.Count);
                GUILayout.Label("StatBars " + StatbarSystem.frontBars.Count);
                //GUILayout.Label("Cameras " + CameraSystem.cameras.Count);
            }
            if (debugGUIType == 2)
            {
                GUILayout.Label("Debugging Data [" + booty.data.name + "]");
                GUILayout.Label("Maps " + booty.data.maps.Count);
                GUILayout.Label("Waves " + booty.data.waves.Count);
                GUILayout.Label("Cameras " + booty.data.cameras.Count);
                GUILayout.Label("Characters " + booty.data.characters.Count);
                GUILayout.Label("Turrets " + booty.data.turrets.Count);
                GUILayout.Label("Bullets " + booty.data.bullets.Count);

                GUILayout.Label("Stats " + booty.data.stats.Count);
                GUILayout.Label("Skills " + booty.data.skills.Count);
                GUILayout.Label("Items " + booty.data.items.Count);
                GUILayout.Label("Quests " + 0);// data.quests.Count);
                GUILayout.Label("Dialogues " + booty.data.dialogues.Count);
            }
            if (debugGUIType == 3)
            {
                foreach (var player in booty.GetSystems().playerSystemGroup.playerSpawnSystem.controllers.Values)
                {
                    ShowPlayerUI(player);
                }
               /**/
            }
            if (debugGUIType == 4)
            {
                DebugAIStates();
            }
            //ShowFPS();
        }

        private void ShowPlayerUI(Entity playerCharacterEntity)
        {
            GUILayout.Label("Debugging Player [" + playerCharacterEntity.Index + "]");
            Translation positioner = booty.GetSystems().space.EntityManager.GetComponentData<Translation>(playerCharacterEntity);
            GUILayout.Label("Player at: " + positioner.Value.ToString());
            Targeter targeter = booty.EntityManager.GetComponentData<Targeter>(playerCharacterEntity);
            if (targeter.hasTarget == 0)
            {
                GUILayout.Label("Has no target.");
            }
            else
            {
                GUILayout.Label("Target: " + targeter.nearbyCharacter.character.Index);
            }
            Stats stats = booty.GetSystems().space.EntityManager.GetComponentData<Stats>(playerCharacterEntity);
            GUILayout.Label("Player Attributes Applied: " + stats.attributesApplied.ToString());
            if (stats.stats.Length > 0)
            {
                GUILayout.Label("Level [" + stats.stats[0].value + "]");
            }
            else
            {
                GUILayout.Label("Level [null]");
            }
            if (stats.stats.Length > 1)
            {
                GUILayout.Label("Stat Points [" + stats.stats[1].value + "]");
            }
            else
            {
                GUILayout.Label("Stat Points [null]");
            }
            GUILayout.Label("Health [" + stats.states[0].value + " out of " + stats.states[0].maxValue + "]");
            if (stats.regens.Length > 0)
            {
                GUILayout.Label("HealthRegen [" + stats.regens[0].value + " at rate of " + stats.regens[0].rate + "]");
            }
            else
            {
                GUILayout.Label("HealthRegen [null]");
            }
            if (stats.attributes.Length > 0)
            {
                GUILayout.Label("Strength [" + stats.attributes[0].value + " at rate of " + stats.attributes[0].multiplier
                    + " and bonus value of: " + +stats.attributes[0].previousAdded + "] Added to health.");
            }
            else
            {
                GUILayout.Label("Strength [null]");
            }


            Skills skills = booty.GetSystems().space.EntityManager.GetComponentData<Skills>(playerCharacterEntity);
            GUILayout.Label("Skill Selected [" + skills.selectedSkillIndex + "]");
            EntityManager manager = booty.GetSystems().space.EntityManager;
            Inventory inventory = manager.GetComponentData<Inventory>(playerCharacterEntity);
            GUILayout.Label("Displaying Items for unnamed character with max items of [" + inventory.items.Length + "].");
            /*for (int i = 0; i < inventory.items.Length; i++)
            {
                if (inventory.items[i].metaID == 0)
                {
                    //GUILayout.Label("Item [" + i + "] is " + item.name + " x" + inventory.items[i].quantity);
                }
                else
                {
                    //ItemDatam item = booty.GetSystems().itemSpawnSystem.meta[inventory.items[i].metaID];
                    //GUILayout.Label("Item [" + i + "] is " + item.name + " x" + inventory.items[i].quantity);
                }
            }*/
        }

        private void DebugAIStates()
        {
            EntityManager manager = booty.GetSystems().space.EntityManager;
            //for (int i = 0; i < booty.systemsManager.characterSpawnSystem.characters.Count; i++)
            int count = 1;
            foreach (Entity character in booty.GetSystems().characterSystemGroup.characterSpawnSystem.characters.Values)
            {
                if (manager.HasComponent<AIState>(character))
                {
                    AIState state = manager.GetComponentData<AIState>(character);
                    ZoxID zoxID = manager.GetComponentData<ZoxID>(character);
                    GUILayout.Label("[" + count + "] State: " + ((AIStateType)(state.state)) + ", Clan: " + zoxID.clanID + ", Creator: " + zoxID.creatorID + ", ID: " + zoxID.id);
                    Mover mover = manager.GetComponentData<Mover>(character);
                    Wander wander = manager.GetComponentData<Wander>(character);
                    Targeter targeter = manager.GetComponentData<Targeter>(character);
                    //GUILayout.Label("       [" + count + "] Mover: " + mover.disabled + ", Wander: " + wander.disabled + ", Target ID: " + targeter.target.Index);
                }
                else
                {
                    ZoxID zoxID = manager.GetComponentData<ZoxID>(character);
                    GUILayout.Label("[" + count + "] Clan: " + zoxID.clanID + ", Creator: " + zoxID.creatorID + ", ID: " + zoxID.id);
                    if (manager.HasComponent<Shooter>(character))
                    {
                        Shooter shooter = manager.GetComponentData<Shooter>(character);
                        Targeter targeter = manager.GetComponentData<Targeter>(character);
                        //GUILayout.Label("       Shooter: " + Quaternion.ToEulerAngles(shooter.shootRotation).ToString() + ", Target ID: " + targeter.targetID);
                    }
                }
                count++;
                if (count == 31)
                {
                    break;
                }
            }
        }

        private void ShowFPS()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.0f, 0.8f, 0.2f, 1.0f);
            string text = GetFPSText();
            /*if (isContinueWalking)
            {
                text += "\nAuto Walking";
            }*/
            /*if (booty.systemsManager.waveSystem.enabled == 1)
            {
                text += "\nTime Until Next Wave [" + booty.systemsManager.waveSystem.timeUntilNextWave + "s]";
            }*/
            //GUI.Label(rect, text, style);
        }

        private string GetFPSText()
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            return text;
        }

        public void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
            {
                debugGUIType = 0;
            }
            if (UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame)
            {
                debugGUIType = 1;
            }
            if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame)
            {
                debugGUIType = 2;
            }
            if (UnityEngine.InputSystem.Keyboard.current.f4Key.wasPressedThisFrame)
            {
                debugGUIType = 3;
            }
            if (UnityEngine.InputSystem.Keyboard.current.f5Key.wasPressedThisFrame)
            {
                debugGUIType = 4;
            }
            if (UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame)
            {
                ToggleAutoWalk();
            }
            if (UnityEngine.InputSystem.Keyboard.current.f7Key.wasPressedThisFrame)
            {
                ToggleCameraMode();
            }

            /*if (UnityEngine.InputSystem.Keyboard.current.jKey.wasPressedThisFrame)
            {
                Entity e = CameraSystem.cameras[CameraSystem.GetMainCameraIndex()];
                CharacterToCamera camera = booty.GetSystems().space.EntityManager.GetComponentData<CharacterToCamera>(e);
                //bool wasFound = false;
                foreach (int key in booty.GetSystems().characterSystemGroup.characterSpawnSystem.characters.Keys)
                {
                    if (camera.characterID != key)
                    {
                        camera.characterID = key;
                        break;
                    }
                }
                booty.GetSystems().space.EntityManager.SetComponentData<CharacterToCamera>(e, camera);
            }*/
        }

        private void ToggleCameraMode()
        {
            // switchcamera
            /* booty.systemsManager.cameraSystem.Clear();
            Translation body = booty.systemsManager.space.EntityManager.GetComponentData<Translation>(
                booty.systemsManager.characterSpawnSystem.characters[booty.GetPlayerCharacterID()]);
            CameraSystem.QueueCamera(body.Value, 
                Quaternion.Euler(booty.data.startingCamera.Value.rotation),
                booty.GetPlayerCharacterID(), 
                booty.data.cameras[1].Value);*/
        }
        private void ToggleAutoWalk()
        {
            // turn on walk mode
            /*isContinueWalking = !isContinueWalking;
            Entity e = booty.GetPlayerMonster(); //booty.systemsManager.characterSpawnSystem.characters[booty.GetPlayerCharacterID()];
            Controller controller = booty.GetSystems().space.EntityManager.GetComponentData<Controller>(e);
            if (isContinueWalking)
            {
                controller.autoWalkForward = 1;
            }
            else
            {
                controller.autoWalkForward = 0;
            }
            booty.GetSystems().space.EntityManager.SetComponentData(e, controller);*/
        }
    }

}