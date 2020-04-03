using System.IO;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Zoxel
{
    /// <summary>
    /// Add Components to things
    ///      they are updated when things update
    /// Players are auto saved every 5 seconds
    ///     - they are marked as dirty on more important things like inventory updates etc
    /// Uses Json and IOStream to load and save files
    /// Load Components added onto new things for loading
    /// Once loaded a player it will tell that world to load chunks
    /// 
    /// Players
    /// NPCs
    /// Chunks - based on worlds (maps)
    /// ChunkCharacters (character maps, like monsterspawner)
    /// </summary>
    [DisableAutoCreation]
    public class SaveSystem : ComponentSystem
    {
        public PlayerSpawnSystem playerSpawnSystem;
        public string saveGameName = "";

        protected override void OnUpdate()
        {
            // for all players, if updated, save them (ones with a savePlayer component

            // for all LoadPlayer components - load the players

            // for all chunks (with SaveChunk on it) - save if updated (these components added when updated voxels)


        }

        /// <summary>
        /// Universe(essentially the game) > World > chunk.. world has a MapID (metaID)
        /// </summary>
        public bool HasChunkInSaveGame(string saveGameName, string mapName, float3 chunkPosition)
        {
            return false;
        }

        private string GetPlayerPath()
        {
            return Application.persistentDataPath + "/" + saveGameName + "/Players/";
        } 
        private string[] GetPlayerPaths()
        {
            return System.IO.Directory.GetDirectories(GetPlayerPath());
        } 
        public string[] GetPlayerSlots()
        {
            var folders = GetPlayerPaths();
            string[] names = new string[folders.Length];
            for (int i = 0; i < folders.Length; i++)
            {
                names[i] = System.IO.Path.GetFileName(folders[i]);
                //Debug.LogError(i + ": " + names[i]);
            }
            return names;
        }

        public static string GetSavePath()
        {
            return Application.persistentDataPath + "/";
        }
        public string GetFolderPath(string typeName, string entityName)
        {
            if (saveGameName == "")
            {
                saveGameName = "Error";
            }
            return GetSavePath() + saveGameName + "/" + typeName + "/" + entityName + "/";    // Players/playername/
        }

        public void CreateNewSaveGame(string newSaveName)
        {
            saveGameName = newSaveName;
            string saveGamePath = GetSavePath() + saveGameName + "/";
            if (!System.IO.Directory.Exists(saveGamePath))
            {
                System.IO.Directory.CreateDirectory(saveGamePath);
            }
            string playersPath = saveGamePath + "Players/";
            if (!System.IO.Directory.Exists(playersPath))
            {
                System.IO.Directory.CreateDirectory(playersPath);
            }
            string mapsPath = saveGamePath + "Maps/";
            if (!System.IO.Directory.Exists(mapsPath))
            {
                System.IO.Directory.CreateDirectory(mapsPath);
            }
        }

        public void SaveGame()
        {
            //Entity character = Bootstrap.instance.GetPlayerMonster();
            Debug.LogError("Saving: " + playerSpawnSystem.controllers.Count + " Players.");
            foreach (Entity character in playerSpawnSystem.controllers.Values)
            {
                SavePlayer(character);
            }
        }

         public void DeletePlayer(int id)
         {
            var folders = GetPlayerPath() + id.ToString();
            if (System.IO.Directory.Exists(folders))
            {
                Debug.Log("Deleting Player save folder path: " + folders);
                System.IO.Directory.Delete(folders, true);
            }
         }

        private void SavePlayer(Entity character)
        {
            ZoxID zoxID = EntityManager.GetComponentData<ZoxID>(character);
            string saveGamePath = GetSavePath() + saveGameName + "/";
            string playerPath = saveGamePath + "Players/" + zoxID.id.ToString();
            if (!System.IO.Directory.Exists(playerPath))
            {
                System.IO.Directory.CreateDirectory(playerPath);
            }
            SaveComponentData<Translation>(character, "Players", zoxID.id.ToString());
            SaveComponentData<Rotation>(character, "Players", zoxID.id.ToString());
            SaveComponentData<Inventory>(character, "Players", zoxID.id.ToString());
            SaveComponentData<Stats>(character, "Players", zoxID.id.ToString());
            SaveComponentData<Skills>(character, "Players", zoxID.id.ToString());
            SaveComponentData<Equipment>(character, "Players", zoxID.id.ToString());
            SaveComponentData<QuestLog>(character, "Players", zoxID.id.ToString());
            if (World.EntityManager.HasComponent<CameraLink>(character))
            {
                CameraLink cameraLink = EntityManager.GetComponentData<CameraLink>(character);
                Entity camera = cameraLink.camera;// CameraSystem.cameras[cameraLink.cameraID];
                SaveComponentData<FirstPersonCamera>(camera, "Players", zoxID.id.ToString());
            }
        }

        /* public void LoadGame()
         {
             Entity character = Bootstrap.instance.GetPlayerMonster();
             LoadPlayer(character);
         }*/

        /// <summary>
        /// Players can chose any  character in the save game folder - then it will be loaded from that
        /// </summary>
        public void LoadPlayer(Entity character)
        {
            ZoxID zoxID = EntityManager.GetComponentData<ZoxID>(character);
            LoadComponentData<Translation>(character, "Players", zoxID.id.ToString());
            LoadComponentData<Rotation>(character, "Players", zoxID.id.ToString());
            LoadComponentData<Inventory>(character, "Players", zoxID.id.ToString());
            LoadComponentData<Stats>(character, "Players", zoxID.id.ToString());
            LoadComponentData<Skills>(character, "Players", zoxID.id.ToString());
            LoadComponentData<Equipment>(character, "Players", zoxID.id.ToString());
            LoadComponentData<QuestLog>(character, "Players", zoxID.id.ToString());
            if (World.EntityManager.HasComponent<CameraLink>(character))
            {
                CameraLink cameraLink = EntityManager.GetComponentData<CameraLink>(character);
                Entity camera = cameraLink.camera;// CameraSystem.cameras[cameraLink.cameraID];
                LoadComponentData<FirstPersonCamera>(camera, "Players", zoxID.id.ToString());
                FirstPersonCamera firstPersonCamera = EntityManager.GetComponentData<FirstPersonCamera>(camera);
                firstPersonCamera.enabled = 1;
                EntityManager.SetComponentData(camera, firstPersonCamera);
            }
        }

        public bool HasAnySaveGames()
        {
            //string translationPath = Application.persistentDataPath + "/" + savePath + "_Translation.txt";
            var folders = System.IO.Directory.GetDirectories(Application.persistentDataPath + "/");
            Debug.Log("Save Folder Path is: " + Application.persistentDataPath + "/ " + folders.Length);
            // check folders in the save path
            // if no folders, no save games
            return folders.Length > 0;
        }

        public string[] GetSaveSlots()
        {
            var folders = System.IO.Directory.GetDirectories(Application.persistentDataPath + "/");
            string[] names = new string[folders.Length];
            for (int i = 0; i < folders.Length; i++)
            {
                names[i] = System.IO.Path.GetFileName(folders[i]);
                //Debug.LogError(i + ": " + names[i]);
            }
            return names;
        }

        private string ToJson<T>(T obj)
        {
            //return JsonConvert.SerializeObject(obj);
            return JsonUtility.ToJson(obj);
        }
        private T FromJson<T>(string json) where T : struct, IComponentData
        {
            if (json == "")
            {
                return default(T);
            }
            return JsonUtility.FromJson<T>(json);
            //return JsonConvert.DeserializeObject<T>(json);
        }

        private void SaveComponentData<T>(Entity e, string typeName, string entityName) where T : struct, IComponentData
        {
            string filePath = GetFolderPath(typeName, entityName) + typeof(T).Name.ToString() + ".txt";
            //Debug.LogError("Saving [" + entityName + "] [" + typeof(T).Name.ToString() + "] to File at: " + filePath);
            string json;
            /*if (typeof(BlitzSerializeable<T>) == typeof(T))
            {
                BlitzSerializeable<T> component = EntityManager.GetComponentData<BlitzSerializeable<T>>(e);
                json = component.GetJson();
            }*/
            if (typeof(T) == typeof(Inventory))
            {
                if (EntityManager.HasComponent<Inventory>(e) == false)
                {
                    Debug.LogError("Character has no inventory..");
                    json = "";
                }
                else
                {
                    Inventory component = EntityManager.GetComponentData<Inventory>(e);
                    json = component.GetJson();
                }
            }
            else if (typeof(T) == typeof(Stats))
            {
                if (World.EntityManager.HasComponent<Stats>(e))
                {
                    Stats component = EntityManager.GetComponentData<Stats>(e);
                    json = component.GetJson();
                } 
                else
                {
                    json = "";
                }
            }
            else if (typeof(T) == typeof(Skills))
            {
                if (World.EntityManager.HasComponent<Skills>(e))
                {
                    Skills component = EntityManager.GetComponentData<Skills>(e);
                    json = component.GetJson();
                }
                else
                {
                    json = "";
                }
            }
            else if (typeof(T) == typeof(Equipment))
            {
                if (World.EntityManager.HasComponent<Equipment>(e))
                {
                    Equipment component = EntityManager.GetComponentData<Equipment>(e);
                    json = component.GetJson();
                }
                else
                {
                    json = "";
                }
            }
            else if (typeof(T) == typeof(QuestLog))
            {
                if (World.EntityManager.HasComponent<QuestLog>(e))
                {
                    QuestLog component = EntityManager.GetComponentData<QuestLog>(e);
                    json = component.GetJson();
                }
                else
                {
                    json = "";
                }
            }
            else
            {
                T component = EntityManager.GetComponentData<T>(e);
                json = ToJson(component);
            }
            //Debug.Log("Saving Json to: " + filePath + " of type " + typeof(T).ToString() + "\n" + json);
            System.IO.File.WriteAllText(filePath, json);
        }

        private void LoadComponentData<T>(Entity e, string typeName, string entityName) where T : struct, IComponentData
        {
            string filePath = GetFolderPath(typeName, entityName) + typeof(T).Name.ToString() + ".txt";
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                //Debug.Log("Loading Json from: " + filePath + " of type " + typeof(T).ToString() + "\n" + json);
                if (typeof(Inventory) == typeof(T))
                {
                    Inventory component = Inventory.FromJson(json);
                    EntityManager.SetComponentData(e, component);
                }
                else if (typeof(Stats) == typeof(T))
                {
                    Stats component = Stats.FromJson(json);
                    EntityManager.SetComponentData(e, component);
                }
                else if (typeof(Skills) == typeof(T))
                {
                    Skills component = Skills.FromJson(json);
                    EntityManager.SetComponentData(e, component);
                }
                else if (typeof(Equipment) == typeof(T))
                {
                    Equipment component = Equipment.FromJson(json);
                    component.dirty = 1;
                    EntityManager.SetComponentData(e, component);
                }
                else if (typeof(QuestLog) == typeof(T))
                {
                    QuestLog component = QuestLog.FromJson(json);
                    EntityManager.SetComponentData(e, component);
                }
                else
                {
                    T component = FromJson<T>(json);
                    EntityManager.SetComponentData(e, component);
                }
            }
            else
            {
                Debug.Log("Save File does not exist: " + filePath);
            }
        }
    }

}