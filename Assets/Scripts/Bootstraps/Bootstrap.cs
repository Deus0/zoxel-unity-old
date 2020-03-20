using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    // ideas
    // A log that shows world generation happening when making the game
    //      Procedurally create factions at start of game generation - save game data
    //      also create a world biome map
    //      on top of this create town locations
    //      on top of this create rivers and mountains
    // genetics that creates character models
    // skeleton system
    // game mode should spawn other systems depending on game type
    // dynamic animations - like swinging arms and legs and IK for feet positions

    /// <summary>
    /// Boot Code for Zoxel
    /// </summary>
    [RequireComponent(typeof(AudioManager), typeof(ParticlesManager))]
    public class Bootstrap : MonoBehaviour//, ICustomBootstrap
    {
        public static Bootstrap instance;
        // Debug Settings - put this in a datam file
        public static bool isAudio = true;
        public static bool isRenderChunks = true;
        public static bool isParticles = true;
        public static bool isStreamChunks = true;
        public bool isMonsters = false;
        public bool DebugColliders = true;
        // debugs
        public static bool DebugChunks = false;
        public static bool DebugMeshWeights = false;

        public int renderDistance = 3;
        public int loadDistance = 4;
        public int mapResolution = 4;
        
        public bool isBiomeMaps;
        [Header("Data")]
        public GameDatam data;
        public Entity game;
        private SystemsManager sys;
        public GameObject canvas;
        private Entity gameText;
        private Entity startText;

        public Material voxelMaterial;

        #region Booting

        public static Material GetVoxelMaterial()
        {
            // could be new Material if need fading!
            return Bootstrap.instance.voxelMaterial;
        }
        public EntityManager EntityManager
        {
            get { 
                if (sys == null) {
                    return null;
                }
                return sys.space.EntityManager; 
            }
        }
        public void Awake()
        {
            instance = this;
            Debug.Log("Awakening the Booty.");
        }

        public bool Initialize(string systems)
        {
            Debug.Log("Initializing the Booty.");
            return true;
        }

        public void Start()
        {
            sys = new SystemsManager(data, "ZoxelGame");
            sys.voxelSystemGroup.voxelPreviewSystem.Test();
            gameText = UIUtilities.SpawnText(EntityManager, new Entity(), "Zoxel", 
                new float3(0, 1f, 0.3f), 
                0, 255, 255, 0.07f);
            startText = UIUtilities.SpawnText(EntityManager, new Entity(), "Press Any Key to Start",
                new float3(0, 0.85f, 0.3f),
                255, 0, 0, 0.022f);
            game = sys.gameSystemGroup.gameStartSystem.CreateGame(data);
            sys.gameSystemGroup.gameStartSystem.OnStarted += () =>
            {
                EntityManager.DestroyEntity(gameText);
                EntityManager.DestroyEntity(startText);
            };
            sys.uiSystemGroup.menuSpawnSystem.game = game;
        }


        public void LateUpdate()
        {
            //sys.UpdateUnitySystems();
            if (sys != null && sys.cameraSystemGroup != null) {
                sys.cameraSystemGroup.cameraSystem.SynchCameras();
            }
        }

        public static int GetRenderDistance()
        {
            if (instance)
            {
                return instance.renderDistance;
            }
            return 3;
        }

        public static int GetLoadDistance()
        {
            if (instance)
            {
                return instance.loadDistance;
            }
            return 4;
        }

        public static int GenerateUniqueID()
        {
            return System.Guid.NewGuid().GetHashCode();
        }
        #endregion


        #region EditorFunctions
        public SystemsManager GetSystems()
        {
            return sys;
        }

#if UNITY_EDITOR
        [ContextMenu("Open Save Folder")]
        public void OpenSaveFolder()
        {
            //Debug.LogError("Opening Save Folder: " + SaveManager.GetSavePath());
            string argument = "/C start" + " " + SaveSystem.GetSavePath();
            Debug.Log("Save Folder: " + argument);
            //var process = System.Diagnostics.Process.Start("cmd.exe", argument);
    #if UNITY_EDITOR_WIN
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = argument;// "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
            process.StartInfo = startInfo;
            process.Start();
    #endif
        }
#endif

        [Header("Test Functions")]
        public QuestDatam quest;
        public ItemDatam item;
        public List<SkillDatam> skills;
        public MapDatam map;
        public StatDatam stat;
        public int amountToSpawn;
        public int clanToSpawn;
        public float3 spawnPosition = new float3(8, 42, 8);
        public CharacterDatam character;
        public int GameID
        {
            get { return EntityManager.GetComponentData<Game>(game).id; }
        }
        public int WorldID
        {
            get { return EntityManager.GetComponentData<Game>(game).mapID; }
        }

        [ContextMenu("GiveStatToPlayers")]
        public void GiveStatToPlayers()
        {
            foreach (var player in sys.playerSystemGroup.playerSpawnSystem.controllers.Values)
            {
                Stats stats = EntityManager.GetComponentData<Stats>(player);
                int didAdd = stats.AddStat(stat);
                if (didAdd != -1)
                {
                    EntityManager.SetComponentData(player, stats);
                }
            }
        }

        [ContextMenu("GiveQuestToPlayers")]
        public void GiveQuestToPlayers()
        {
            if (quest != null)
            {
                foreach (var player in sys.playerSystemGroup.playerSpawnSystem.controllers.Values)
                {
                    QuestLog questlog = EntityManager.GetComponentData<QuestLog>(player);
                    System.Collections.Generic.List<QuestDatam> quests = new System.Collections.Generic.List<QuestDatam>();
                    quests.Add(quest);
                    questlog.Initialize(quests);
                    EntityManager.SetComponentData(player, questlog);
                }
            }
        }

        [ContextMenu("GiveItemToPlayers")]
        public void GiveItemToPlayers()
        {
            if (quest != null)
            {
                foreach (var player in sys.playerSystemGroup.playerSpawnSystem.controllers.Values)
                {
                    Inventory inventory = EntityManager.GetComponentData<Inventory>(player);
                    System.Collections.Generic.List<ItemDatam> items = new System.Collections.Generic.List<ItemDatam>();
                    items.Add(item);
                    inventory.InitializeItems(9, items);
                    EntityManager.SetComponentData(player, inventory);
                }
            }
        }

        [ContextMenu("GiveSkillsToPlayers")]
        public void GiveSkillsToPlayers()
        {
            foreach (var player in sys.playerSystemGroup.playerSpawnSystem.controllers.Values)
            {
                Skills skillsC = EntityManager.GetComponentData<Skills>(player);
                skillsC.Initialize(skills);
                EntityManager.SetComponentData(player, skillsC);
                sys.skillSystemGroup.skillsSystem.InitializeSkills(player, skillsC);
            }
        }

        [ContextMenu("GiveExperienceToPlayers")]
        public void GiveExperienceToPlayers()
        {
            foreach (var player in sys.playerSystemGroup.playerSpawnSystem.controllers.Values)
            {
                foreach (var defender in sys.characterSystemGroup.characterSpawnSystem.characters.Values)
                {
                    if (player.Index != defender.Index)
                    {
                        DamageSystem.AddDamage(EntityManager, player, defender, 0, UnityEngine.Random.Range(1, 12));
                        //break;
                    }
                }
            }
        }

        /// <summary>
        /// loading new map should reposition players after if their characters already exist
        /// </summary>
        [ContextMenu("LoadMap")]
        public void LoadMap()
        {

        }

        [ContextMenu("SpawnCharacters")]
        public void SpawnCharacters()
        {
            CharacterSpawnSystem.SpawnNPCs(EntityManager,
                WorldID, character.Value.id, clanToSpawn, spawnPosition, amountToSpawn);
        }

        public IEnumerator RunSpawnCharactersTest()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(20);
                SpawnCharacters();
            }
        }
        #endregion

    }

}