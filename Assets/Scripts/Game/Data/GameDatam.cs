using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Zoxel.Voxels;

namespace Zoxel
{
    // Features:
    //      Spawn a world - or level - and have a set of levels to use in the game
    //      A title screen for game, including data for the menus etc
    
    // ToDo:
    //      for td mode
    //          spawn a ghost character with 20 starting gold
    //          starting gold for player contained in their spawn
    //          use the ghost to walk to the spawned turret to summon it
    //          turrets to take time to be intialized
    //      
    //      CameraDatam - to contain the view, controls, etc
    //          - Topdown, LockedTopdown, Thirdperson (over shoulder), First Person
    //          - CameraDatam also decides controls!
    //          - ControllerData - contains  control data
    //          - Thirdperson, firstperson, click controls, etc

    [CreateAssetMenu(fileName = "Game", menuName = "ZoxelSettigs/Game")]
    public class GameDatam : ScriptableObject//or monobehaviour
    {
        public int id;
        [Header("Settings")]
        public GameType mode;

        [Header("General UI")]
        public UIDatam uiData;
        public Material voxelColorMaterial;
        public Material quadMaterial;

        [Header("New Game")]
        //public Vector3 playerSpawnPosition = new Vector3(40, 1.5f, 16.5f);
        //public byte hasWaves;
        public CharacterDatam startingCharacter;
        public DialogueDatam startingDialogue;
        public CameraDatam startingCamera;  // can do something like a way point main menu camera in a random map
        public MapDatam startingMap;
        public MapDatam mainMenuMap;

        [Header("Game")]
        public List<MapDatam> maps;     // may contain infinite map, and generation details here!
        public List<CameraDatam> cameras;
        public List<WaveDatam> waves;

        [Header("Combat")]
        public List<CharacterDatam> characters;
        // generate from skills
        public List<TurretDatam> turrets;
        public List<ClassDatam> classes;
        public List<BulletDatam> bullets;

        [Header("Story")]
        public List<StatDatam> stats;
        public List<SkillDatam> skills;
        public List<ItemDatam> items;
        public List<QuestDatam> quests;
        public List<DialogueDatam> dialogues;

        [Header("Art")]
        public List<VoxelDatam> voxels;
        public List<VoxDatam> models;
        public List<SkeletonDatam> skeletons;
        public List<MusicDatam> musics;
        public List<SoundDatam> audios;

        // UI

        [ContextMenu("Generate ID")]
        public void GenerateID()
        {
            id = Bootstrap.GenerateUniqueID();
        }
        [ContextMenu("GrabDataFromFolders")]
        public void GrabDataFromFolders()
        {
            Debug.Log("To Do.");
        }

        #region dictionaries

        public Dictionary<int, DialogueDatam> GetDialogues()
        {
            var meta = new Dictionary<int, DialogueDatam>();
            foreach (DialogueDatam data in dialogues)
            {
                if (data.dialogueTree.id == 0)
                {
                    data.dialogueTree.id = Bootstrap.GenerateUniqueID();
                }
                meta.Add(data.dialogueTree.id, data);
            }
            return meta;
        }
        public Dictionary<int, ClassDatam> GetClasses()
        {
            var meta = new Dictionary<int, ClassDatam>();
            foreach (ClassDatam character in classes)
            {
                if (character == null)
                {
                    Debug.LogError("Character is null in game data");
                    continue;
                }
                if (meta.ContainsKey(character.Value.id))
                {
                    Debug.LogError("Duplicate character ID in game data: " + character.name);
                    continue;
                }
                if (character.Value.id == 0)
                {
                    //character.Value.id = Bootstrap.GenerateUniqueID();
                    Debug.LogError(character.name + " has no ID.");
                    continue;

                }
                meta.Add(character.Value.id, character);
            }
            return meta;
        }

        public Dictionary<int, CharacterDatam> GetCharacters()
        {
            var meta = new Dictionary<int, CharacterDatam>();
            foreach (CharacterDatam character in characters)
            {
                if (character == null)
                {
                    Debug.LogError("Character is null in game data");
                    continue;
                }
                if (meta.ContainsKey(character.Value.id))
                {
                    Debug.LogError("Duplicate character ID in game data: " + character.name);
                    continue;
                }
                if (character.Value.id == 0)
                {
                    //character.Value.id = Bootstrap.GenerateUniqueID();
                    Debug.LogError(character.name + " has no ID.");
                    continue;

                }
                meta.Add(character.Value.id, character);
            }
            return meta;
        }

        public Dictionary<int, BulletDatam> GetBullets()
        {
            var meta = new Dictionary<int, BulletDatam>();
            foreach (BulletDatam bullet in bullets)
            {
                if (bullet.Value.id == 0)
                {
                    bullet.Value.id = Bootstrap.GenerateUniqueID();
                }
                meta.Add(bullet.Value.id, bullet);
            }
            return meta;
        }

        public Dictionary<int, SkillDatam> GetSkills()
        {
            Dictionary<int, SkillDatam> skillsMeta = new Dictionary<int, SkillDatam>();
            foreach (SkillDatam skill in skills)
            {
                if (skill.Value.id == 0)
                {
                   // skill.GenerateID();
                    Debug.LogError(skill.name + " has no ID.");
                    continue;

                }
                skillsMeta.Add(skill.Value.id, skill);
            }
            return skillsMeta;
        }

        public Dictionary<int, CameraDatam> GetCameras()
        {
            var meta = new Dictionary<int, CameraDatam>();
            foreach (CameraDatam camera in cameras)
            {
                if (camera.Value.id == 0)
                {
                    camera.Value.id = Bootstrap.GenerateUniqueID();
                }
                meta.Add(camera.Value.id, camera);
            }
            return meta;
        }

        public Dictionary<int, ItemDatam> GetItems()
        {
            Dictionary<int, ItemDatam> itemsMeta = new Dictionary<int, ItemDatam>();
            foreach (ItemDatam item in items)
            {
                if (item.Value.id == 0)
                {
                    //item.Value.id = Bootstrap.GenerateUniqueID();

                    Debug.LogError(item.name + " has no ID.");
                    continue;
                }
                itemsMeta.Add(item.Value.id, item);
            }
            return itemsMeta;
        }

        public Dictionary<int, StatDatam> GetStats()
        {
            Dictionary<int, StatDatam> statsMeta = new Dictionary<int, StatDatam>();
            foreach (StatDatam stat in stats)
            {
                if (stat.Value.id == 0)
                {
                    Debug.LogError(stat.name + " has no ID.");
                    continue;
                    //stat.Value.id = Bootstrap.GenerateUniqueID();
                }
                statsMeta.Add(stat.Value.id, stat);
            }
            return statsMeta;
        }

        public Dictionary<int, QuestDatam> GetQuests()
        {
            Dictionary<int, QuestDatam> questsMeta = new Dictionary<int, QuestDatam>();
            foreach (QuestDatam quest in quests)
            {
                if (quest.Value.id == 0)
                {
                    Debug.LogError(quest.name + " has no ID.");
                    continue;
                   // item.Value.id = Bootstrap.GenerateUniqueID();
                }
                questsMeta.Add(quest.Value.id, quest);
            }
            return questsMeta;
        }

        #endregion
    }
}