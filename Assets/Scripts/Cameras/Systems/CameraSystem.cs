using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.InputSystem;
using Zoxel.UI; // should probaly keep this in GameStartSystem rather then relying on spawning a camera...

namespace Zoxel
{

    /// <summary>
    /// Needs to spawn camera gameobject, with the copyToGameobject thing
    /// Needs to spawn a render texture
    /// Needs to position and spawn a render texture in the canvas
    /// if more then 1 needs to split them up
    /// Can be on screen is optional - sometimes you can open a window to watch your minion fight!
    /// </summary>
    [DisableAutoCreation] //, UpdateAfter(typeof(CharacterSpawnSystem))]
    public class CameraSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;
        public GameObject canvas;
        public Dictionary<int, CameraDatam> meta = new Dictionary<int, CameraDatam>();
        private EntityArchetype firstPersonCameraArchtype;
        /*private EntityArchetype thirdPersonCameraArchtype;*/
        public Dictionary<int, Entity> cameras = new Dictionary<int, Entity>();
        public Dictionary<int, GameObject> cameraObjects = new Dictionary<int, GameObject>();
        public Dictionary<int, GameObject> cameraUIs = new Dictionary<int, GameObject>();
        public Dictionary<int, RenderTexture> cameraRenders = new Dictionary<int, RenderTexture>();
        public Dictionary<int, CameraData> cameraDatas = new Dictionary<int, CameraData>();

        // Data
        //public static List<CharacterDatam> monsterData = new List<CharacterDatam>();

        // queue
        /*private static List<int> commandIDs = new List<int>();
        private static List<float3> commandPositions = new List<float3>();
        private static List<quaternion> commandRotations = new List<quaternion>();
        private static List<int> commandFollowIDs = new List<int>();
        private static List<CameraData> commandDatas = new List<CameraData>();*/

        private float2 screenDimensions;

        protected override void OnCreate()
        {
            base.OnCreate();
            screenDimensions = new float2(Screen.width, Screen.height);
            firstPersonCameraArchtype = World.EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld),
                // mein
                typeof(ZoxID),
                typeof(CameraSynch),
                typeof(FirstPersonCamera),
                typeof(CharacterUIList)
            );
            /*thirdPersonCameraArchtype = World.EntityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(CharacterToCamera),
                typeof(FollowerCamera)
            );*/
        }

        public void Clear()
        {
            foreach (KeyValuePair<int, Entity> entityKeyValue in cameras)
            {
                DestroyCamera(entityKeyValue.Key, false);
            }
            cameras.Clear();
            cameraObjects.Clear();
            cameraRenders.Clear();
            cameraUIs.Clear();
            cameraDatas.Clear();
        }

        public void DestroyCamera(int key, bool isRemoveFromList = true)
        {
            if (!cameras.ContainsKey(key))
            {
                return;
            }
            if (World.EntityManager.Exists(cameras[key]))
            {
                World.EntityManager.DestroyEntity(cameras[key]);
            }
            if (cameraObjects.ContainsKey(key))
            {
                GameObject.Destroy(cameraObjects[key]);
            }
            if (cameraRenders.ContainsKey(key))
            {
                GameObject.Destroy(cameraRenders[key]);
            }
            if (cameraUIs.ContainsKey(key))
            {
                //GameObject.Destroy(cameraUIs[key].transform.parent.gameObject);
                GameObject.Destroy(cameraUIs[key]);
            }
            if (isRemoveFromList)
            {
                cameras.Remove(key);
                cameraObjects.Remove(key);
                cameraRenders.Remove(key);
                cameraUIs.Remove(key);
                cameraDatas.Remove(key);
            }
        }


        /// <summary>
        /// Sets gameobject positions to entity translation positions
        /// </summary>
        public void SynchCameras()
        {
            foreach (KeyValuePair<int, GameObject> KVP in cameraObjects)
            {
                float3 cameraTranslationPosition = World.EntityManager.GetComponentData<Translation>(cameras[KVP.Key]).Value;
                if (float.IsNaN(cameraTranslationPosition.x))
                {
                    World.EntityManager.SetComponentData(cameras[KVP.Key], new Translation { Value = KVP.Value.transform.position });
                }
                else
                {
                    KVP.Value.transform.position = cameraTranslationPosition;
                }
                KVP.Value.transform.rotation = World.EntityManager.GetComponentData<Rotation>(cameras[KVP.Key]).Value;
            }
        }


        public int SpawnCamera(CameraData cameraData)
        {
            int id = Bootstrap.GenerateUniqueID();
            float3 spawnPosition = float3.zero;
            quaternion spawnRotation = quaternion.identity;
            if (Camera.main != null)
            {
                spawnPosition = Camera.main.transform.position;
                spawnRotation = Camera.main.transform.rotation;
            }
            SpawnCamera(id, spawnPosition, spawnRotation, new Entity(), cameraData);
            return id;
        }

        public int SpawnCameraOnCharacter(Entity attachEntity, CameraData cameraData)
        {
            int id = Bootstrap.GenerateUniqueID();
            float3 spawnPosition = World.EntityManager.GetComponentData<Translation>(attachEntity).Value;
            quaternion spawnRotation = World.EntityManager.GetComponentData<Rotation>(attachEntity).Value;
            SpawnCamera(id, spawnPosition, spawnRotation, attachEntity, cameraData);
            return id;
        }

        public void ConnectCameraToCharacter(Entity camera, Entity attachEntity)
        {
            if (World.EntityManager.Exists(camera) == false)
            {
                Debug.LogError("Cannot connect camera to character as camera id is false.");
                return;
            }
            if (World.EntityManager.Exists(attachEntity) == false)
            {
                Debug.LogError("Cannot connect camera to character as character id is false.");
                return;
            }
            int zoxID = World.EntityManager.GetComponentData<ZoxID>(camera).id;
            //Debug.LogError("attaching camera " + cameraID + " to character " + characterID);
            CameraData cameraData = cameraDatas[zoxID];
            //Entity characterEntity = characterSpawnSystem.characters[characterID];
            //Entity cameraEntity = cameras[zoxID];
            Translation characterTranslation = World.EntityManager.GetComponentData<Translation>(attachEntity);
            //AudioManager.instance.SpawnMonsterSpawnSound(characterTranslation.Value);
            if (World.EntityManager.HasComponent<CameraLink>(attachEntity))
            {
                //Debug.LogError("Connecting Camera to character: " + characterEntity.Index + "::" + cameraID);
                CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(attachEntity);
                cameraLink.camera = camera;
                cameraLink.fov = cameraObjects[zoxID].GetComponent<Camera>().fieldOfView; //camera.fieldOfView;
                World.EntityManager.SetComponentData(attachEntity, cameraLink);
            }
            if (World.EntityManager.HasComponent<CameraLink>(camera))
            {
                World.EntityManager.RemoveComponent<CameraLink>(camera);
            }
            int characterID = World.EntityManager.GetComponentData<ZoxID>(attachEntity).id;
            CharacterLink characterToCamera = new CharacterLink {
                character = attachEntity
            };
            if (World.EntityManager.HasComponent<CharacterLink>(camera))
            {
                World.EntityManager.SetComponentData(camera, characterToCamera);
            }
            else
            {
                World.EntityManager.AddComponentData(camera, characterToCamera);
            }
            if (cameraData.cameraType == 0)
            {
                //Debug.LogError("Setting camera position offset: " + cameraData.firstPersonCamera.cameraAddition);
                //Debug.LogError("Setting camera position offset2: " + World.EntityManager.GetComponentData<FirstPersonCamera>(cameras[cameraID]).Value.cameraAddition);
                World.EntityManager.SetComponentData(camera, new ZoxID {
                    id = zoxID,
                    creatorID = characterID
                });
                World.EntityManager.SetComponentData(camera, new CameraSynch
                {
                    Value = attachEntity,
                    localPosition = cameraData.firstPersonCamera.cameraAddition
                });
               // Debug.LogError("Setting camera FirstPersonCamera: " + cameraEntity.Index);
               // Debug.LogError("    Pre camera FirstPersonCamera enabled: " + World.EntityManager.GetComponentData<FirstPersonCamera>(cameraEntity).enabled);
                World.EntityManager.SetComponentData(camera, new FirstPersonCamera
                {
                    enabled = 1,
                    Value = cameraData.firstPersonCamera
                });
                //Debug.LogError("    Post camera FirstPersonCamera enabled: " + World.EntityManager.GetComponentData<FirstPersonCamera>(cameraEntity).enabled);
            }
        }

        public void RemoveCameraFromCharacter(int cameraID, int characterID)
        {
            Debug.LogError("TODO: Remove Cameras");
        }

        private Entity SpawnCamera(int id, float3 spawnPosition, quaternion spawnRotation, Entity attachEntity, CameraData cameraData)
        {
            if (cameraData.isRenderTexture == 0)
            {
                if (Camera.main)
                {
                    //Camera.main.gameObject.tag = "Untagged";
                    Camera.main.gameObject.SetActive(false);
                    if (canvas)
                    {
                        canvas.SetActive(false);
                    }
                }
            }
            else
            {
                if (canvas)
                {
                    canvas.SetActive(true);
                }
            }
            //AudioManager.instance.SpawnMonsterSpawnSound(spawnPosition);
            cameraDatas.Add(id, cameraData);
            GameObject cameraObject = SpawnCameraGameObject(id, cameraData, spawnPosition, spawnRotation);
            cameraObjects.Add(id, cameraObject);
            var cameraEntity = AddECSComponents(cameraObject, id, attachEntity, cameraData);
            Camera camera = cameraObject.GetComponent<Camera>();
            if (World.EntityManager.Exists(attachEntity))
            {
                //Entity charaterEntity = characterSpawnSystem.characters[followID];
                if (cameraData.cameraType == ((byte)CameraDataType.ThirdPerson))
                {
                    Translation characterTranslation = World.EntityManager.GetComponentData<Translation>(attachEntity);
                    Rotation characterRotation = World.EntityManager.GetComponentData<Rotation>(attachEntity);
                    spawnPosition = characterTranslation.Value + new float3(0, cameraData.followCameraData.cameraAddition.y, 0);
                    spawnPosition += math.rotate(characterRotation.Value, new float3(0, 0, cameraData.followCameraData.cameraAddition.z));
                    Quaternion newRotation = new Quaternion(characterRotation.Value.value.x,
                        characterRotation.Value.value.y, characterRotation.Value.value.z, characterRotation.Value.value.w);
                    float3 newRotation2 = newRotation.eulerAngles;
                    newRotation2.x = cameraData.followCameraData.cameraRotation.x;
                    spawnRotation = Quaternion.Euler(newRotation2);
                }
                if (World.EntityManager.HasComponent<CameraLink>(attachEntity))
                {
                    CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(attachEntity);
                    cameraLink.camera = cameraEntity;
                    cameraLink.fov = camera.fieldOfView;
                    World.EntityManager.SetComponentData(attachEntity, cameraLink);
                }
            }
            else
            {
                World.EntityManager.AddComponentData(cameras[id], new CameraLink
                {
                    camera = cameraEntity,
                    fov = camera.fieldOfView
                });
            }
            // Setup camera object
            if (cameraData.isRenderTexture == 1)
            {
                CreateRenderTexture(camera, cameraData, id);
            }
            else
            {
                Camera.SetupCurrent(camera);
                camera.tag = "MainCamera";
            }
            Debug.Log("Spawned camera: " + id);
            ResizeRenderTextures();
            return cameraEntity;
        }

        private Entity AddECSComponents(GameObject cameraObject, int id, Entity attachEntity, CameraData cameraData)
        {
            Entity camera = World.EntityManager.CreateEntity(firstPersonCameraArchtype);
            cameras.Add(id, camera);
            World.EntityManager.SetComponentData(camera, new Translation { Value = cameraObject.transform.position });
            World.EntityManager.SetComponentData(camera, new Rotation { Value = cameraObject.transform.rotation });
            World.EntityManager.SetComponentData(camera, new CameraSynch
            {
                Value = attachEntity,
                localPosition = cameraData.firstPersonCamera.cameraAddition
            });
            int creatorID = 0;
            if (World.EntityManager.Exists(attachEntity))
            {
                creatorID = World.EntityManager.GetComponentData<ZoxID>(attachEntity).id;
                World.EntityManager.SetComponentData(camera, new CharacterLink { character = attachEntity });
            }
            World.EntityManager.SetComponentData(camera, new ZoxID
            {
                id = id,
                creatorID = creatorID
            });
            return camera;
        }

        private GameObject SpawnCameraGameObject(int id, CameraData cameraData, float3 spawnPosition, quaternion spawnRotation)
        {
            CameraDatam cameraMeta = meta[cameraData.id];
            GameObject cameraObject = GameObject.Instantiate(cameraMeta.gameCameraPrefab);
            //GameObject cameraObject = new GameObject("Camera[" + id + "]");
            cameraObject.name = "Camera[" + id + "]";
            cameraObject.transform.position = spawnPosition;
            cameraObject.transform.rotation = spawnRotation;
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = cameraMeta.gameCameraPrefab.GetComponent<Camera>().fieldOfView;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.depth = -1;
            //camera.allowHDR = true;
            //camera.allowMSAA = false;
            //camera.renderingPath = RenderingPath.UsePlayerSettings;
            //camera.useOcclusionCulling = true;
            //camera.targetDisplay = 0;
            //camera.allowHDR = true;
            //camera.
            // add audio listener too!
            // add camera, post processing
            /*if (cameraData.isPostProcessing == 1)
            {
                AddPostProcessing(cameraObject);
            }*/
            return cameraObject;
        }

        /*private void AddPostProcessing(GameObject cameraObject)
        {
#if POST_PROCESSING
                PostProcessVolume volume = cameraObject.AddComponent<PostProcessVolume>();
                volume.isGlobal = true;
                volume.profile = profile;
                volume.weight = 1;
                PostProcessLayer layer = cameraObject.AddComponent<PostProcessLayer>();
                layer.volumeTrigger = cameraObject.transform;
                layer.volumeLayer = 1;
                layer.Init(resources);
#endif
        }*/

        public void CreateRenderTexture(Camera camera, CameraData cameraData, int id)
        {
            int screenWidth = Screen.width; // 1920
            int screenHeight = Screen.height; // 1080
            Debug.Log("Created camera with width and height of: " + screenWidth + " x " + screenHeight);
            camera.forceIntoRenderTexture = true;
            // Create UI! assign new texture
            GameObject uiObject = new GameObject();
            uiObject.name = camera.gameObject.name;
            UnityEngine.UI.RawImage image = uiObject.AddComponent<UnityEngine.UI.RawImage>();
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas)
            {
                RectTransform parent = canvas.GetComponent<RectTransform>();
                if (canvas.transform.Find("Cameras"))
                {
                    parent = canvas.transform.Find("Cameras").GetComponent<RectTransform>();
                }
                uiObject.transform.SetParent(parent);
            }
            uiObject.transform.localScale = new Vector3(1, 1, 1);
            cameraUIs.Add(id, uiObject);
            CreateRenderTextureTexture(id, cameraData, camera, image);//, textureWidth, textureHeight);
        }

        private void ResizeRenderTextures()
        {
            foreach (GameObject imageUI in cameraUIs.Values)
            {
                RectTransform rect = imageUI.GetComponent<RectTransform>();
                if (rect.parent == null)
                {
                    continue;
                }
                RectTransform parent = rect.parent.gameObject.GetComponent<RectTransform>();
                // set each as half
                if (parent.childCount == 1)
                {
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = parent.sizeDelta;
                }
                if (parent.childCount == 2)
                {
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        RectTransform child = parent.GetChild(i).GetComponent<RectTransform>();
                        if (i == 0)
                        {
                            child.anchorMin = new Vector2(0, 0.5f);
                            child.anchorMax = new Vector2(1, 1f);
                        }
                        else
                        {
                            child.anchorMin = new Vector2(0, 0);
                            child.anchorMax = new Vector2(1, 0.5f);
                        }
                        child.anchoredPosition = Vector2.zero;
                        child.sizeDelta = new float2(parent.sizeDelta.x, parent.sizeDelta.y / 2f);
                    }
                }
                // set each as a quarter
                if (parent.childCount >= 3 || parent.childCount == 4)
                {
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        RectTransform child = parent.GetChild(i).GetComponent<RectTransform>();
                        if (i == 0)
                        {
                            child.anchorMin = new Vector2(0, 0.5f);
                            child.anchorMax = new Vector2(0.5f, 1f);
                        }
                        else if (i == 1)
                        {
                            child.anchorMin = new Vector2(0.5f, 0.5f);
                            child.anchorMax = new Vector2(1f, 1f);
                        }
                        else if (i == 2)
                        {
                            child.anchorMin = new Vector2(0, 0);
                            child.anchorMax = new Vector2(0.5f, 0.5f);
                        }
                        else
                        {
                            child.anchorMin = new Vector2(0.5f, 0);
                            child.anchorMax = new Vector2(1f, 0.5f);
                        }
                        child.anchoredPosition = Vector2.zero;
                        child.sizeDelta = new float2(parent.sizeDelta.x, parent.sizeDelta.y / 2f);
                    }
                }
            }
            foreach (int id in cameras.Keys)
            {
                // change dimensions of textures
                RecreateRenderTexture(id);
            }
        }

        private void RecreateRenderTexture(int id)
        {
            if (cameraUIs.ContainsKey(id))
            {
                CreateRenderTextureTexture(id, cameraDatas[id],
                    cameraObjects[id].GetComponent<Camera>(),
                    cameraUIs[id].GetComponent<UnityEngine.UI.RawImage>());
            }
        }

        private void CreateRenderTextureTexture(int id, CameraData cameraData, Camera camera, UnityEngine.UI.RawImage image)//, int textureWidth, int textureHeight)
        {
           // Debug.LogError("NewRender Size: " + image.rectTransform.rect.size.ToString());
            int textureWidth = (int)(image.rectTransform.rect.size.x) / cameraData.textureDivision;
            int textureHeight = (int)(image.rectTransform.rect.size.y) / cameraData.textureDivision;
            if (cameraRenders.ContainsKey(id))
            {
                GameObject.Destroy(cameraRenders[id]);
                cameraRenders.Remove(id);
            }
            RenderTexture newTexture = new RenderTexture(textureWidth, textureHeight, 16, UnityEngine.RenderTextureFormat.ARGB32);
            newTexture.filterMode = FilterMode.Point;
            newTexture.name = camera.gameObject.name;
            image.texture = newTexture;
            camera.targetTexture = newTexture;
            cameraRenders.Add(id, newTexture);
            if (cameras.ContainsKey(id) == false)
            {
                Debug.LogError("Camera does not exist: " + id);
                return;
            }

            if (World.EntityManager.HasComponent<CameraLink>(cameras[id]))
            {
                CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(cameras[id]);
                cameraLink.aspectRatio = ((float)textureWidth) / ((float)textureHeight);
                World.EntityManager.SetComponentData(cameras[id], cameraLink);
            }
            else
            {
                int characterID = World.EntityManager.GetComponentData<ZoxID>(cameras[id]).creatorID;
                if (characterSpawnSystem.characters.ContainsKey(characterID))
                {
                    if (World.EntityManager.HasComponent<CameraLink>(characterSpawnSystem.characters[characterID]))
                    {
                        CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(characterSpawnSystem.characters[characterID]);
                        cameraLink.aspectRatio = ((float)textureWidth) / ((float)textureHeight);
                        World.EntityManager.SetComponentData(characterSpawnSystem.characters[characterID], cameraLink);
                    }
                }
            }
            // connected UIs
            CharacterUIList uiList = World.EntityManager.GetComponentData<CharacterUIList>(cameras[id]);
            Entity[] uis = uiList.uis.ToArray();
            for (int i = 0; i < uis.Length; i++)
            {
                if (World.EntityManager.Exists(uis[i]))
                {
                    CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(uis[i]);
                    cameraLink.aspectRatio = ((float)textureWidth) / ((float)textureHeight);
                    World.EntityManager.SetComponentData(uis[i], cameraLink);
                    //World.EntityManager.SetComponentData(uis[i], orbitor);
                    OrbitCamera orbitor = World.EntityManager.GetComponentData<OrbitCamera>(uis[i]);
                    UIUtilities.UpdateOrbiter(World.EntityManager, uis[i], orbitor.orbitPosition, orbitor.lerpSpeed);
                }
            }
        }

        public float2 GetScreenPosition(int cameraIndex)
        {
            if (cameraIndex != -1 && cameras.ContainsKey(cameraIndex))
            {
                float2 mousePosition = float2.zero;
                if (Mouse.current != null)
                {
                    mousePosition = Mouse.current.position.ReadValue();
                }
                if (Touchscreen.current != null)
                {
                    mousePosition = Touchscreen.current.touches[0].position.ReadValue();
                }
                if (cameraRenders.ContainsKey(cameraIndex))
                {
                    return GetScreenPosition(cameraIndex, mousePosition);
                }
                else
                {
                    return mousePosition;
                }
            }
            return new float2(-1, -1);
        }

        public float2 GetScreenPosition(int cameraIndex, float2 mousePosition)
        {
            if (cameraIndex != -1 && cameraRenders.ContainsKey(cameraIndex))
            {
                float2 screenPosition = new float2(mousePosition.x, mousePosition.y);
                RectTransform rect = cameraUIs[cameraIndex].transform.GetComponent<RectTransform>();
                RectTransform parentRect = cameraUIs[cameraIndex].transform.parent.GetComponent<RectTransform>();
                float2 min = new float2
                {
                    x = (Screen.width - rect.rect.width * rect.lossyScale.x),
                    y = (Screen.height - rect.rect.height * rect.lossyScale.y)
                };
                screenPosition.x -= min.x * parentRect.pivot.x; //.rect.position.x;
                screenPosition.y -= min.y * parentRect.pivot.y; // Minus half of the difference, so 72 * .5 = 36, so the bottom is 36
                Vector2 stretchedRectSize = new Vector2(rect.rect.width * rect.lossyScale.x, rect.rect.height * rect.lossyScale.y);
                if ((screenPosition.x >= 0 && screenPosition.x < stretchedRectSize.x
                    && screenPosition.y >= 0 && screenPosition.y < stretchedRectSize.y))
                {
                    RenderTexture texture = cameraRenders[cameraIndex];
                    screenPosition.x *= texture.width / stretchedRectSize.x;
                    screenPosition.y *= texture.height / stretchedRectSize.y;
                    return screenPosition;
                }
            }
            return new float2(-1, -1);
        }

        #region MainCamera


        /// <summary>
        /// Used for billboards to look at camera
        /// </summary>
        /// <returns></returns>
        public Camera GetMainCamera()
        {
            Camera mainCamera = null;
            if (Camera.main)
            {
                mainCamera = Camera.main;
            }
            foreach (GameObject o in cameraObjects.Values)
            {
                mainCamera = o.GetComponent<Camera>();
                if (mainCamera)
                {
                    return mainCamera;
                }
            }
            return mainCamera;
        }

        public float3 GetMainCameraPosition()
        {
            float3 cameraPosition = float3.zero;
            if (Camera.main)
            {
                cameraPosition = Camera.main.transform.position;
            }
            foreach (GameObject o in cameraObjects.Values)
            {
                cameraPosition = o.transform.position;
            }
            return cameraPosition;
        }
        #endregion

        #region SpawningDespawning
        public static void SpawnCameraController(EntityManager EntityManager, CameraData data, Controller controller, int controllerID)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new SpawnCameraCommand
            {
                id = controllerID,
                data = data,
                isController = 1,
                controller = controller
            });
        }

        private struct SpawnCameraCommand : IComponentData
        {
            public int id;
            public Controller controller;   // when i create a new camera + controller
            public Entity attachEntity;
            public float3 position;
            public quaternion rotation;
            public CameraData data;
            public byte isController;
        }
        private struct RemoveCameraCommand : IComponentData
        {
            public int id;
        }
        public PlayerSpawnSystem playerSystem;
        protected override void OnUpdate()
        {
            if (screenDimensions.x != Screen.width || Screen.height != screenDimensions.y)
            {
                screenDimensions.x = Screen.width;
                screenDimensions.y = Screen.height;
                //Debug.Log("Screen size has changed: " + screenDimensions.ToString());
                //.if (cameraRenders.Count > 0)
                ResizeRenderTextures();
            }
            Entities.WithAll<SpawnCameraCommand>().ForEach((Entity e, ref SpawnCameraCommand command) =>
            {
                if (command.isController == 1)
                {
                    Entity camera = SpawnCamera(command.id, command.position, command.rotation, command.attachEntity, command.data);
                    World.EntityManager.AddComponentData(camera, command.controller);
                    //Debug.LogError("Adding New Controller: " + command.id);
                    MenuSpawnSystem.SpawnUI(EntityManager, camera, "MainMenu");
                    EntityManager.DestroyEntity(playerSystem.controllers[command.id]);
                    playerSystem.controllers.Remove(command.id);
                    playerSystem.controllers.Add(command.id, camera); // adds with device id
                }
                World.EntityManager.DestroyEntity(e);
            });
            Entities.WithAll<RemoveCameraCommand>().ForEach((Entity e, ref RemoveCameraCommand command) =>
            {
                //RemoveUI(command.character);
                World.EntityManager.DestroyEntity(e);
            });
        }
        #endregion
    }
}