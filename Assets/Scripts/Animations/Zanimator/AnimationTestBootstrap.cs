/*using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Zoxel;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace AnimatorSystem
{
	public class AnimationTestBootstrap : MonoBehaviour
	{
		private EntityArchetype unitArchtype;
        //public Animation animator;
        //public Material material;
        //public Mesh mesh;
        public AnimatorDatam data;
        private List<Entity> entities = new List<Entity>();
        public List<Camera> cameras = new List<Camera>();
        // UI
        public CanvasGroup group;
        public RawImage cameraImage;
        public Text entitiesCounter;
        public Text fpsCounter;

        public float randomRange = 16f;
        private bool isProcessors;
        private bool isDownsized = true;
        private float timeBegun;
        private int animationSet;
        private int framesAddition;
        private int framesTotal;

        void Start()
        {
            Application.targetFrameRate = 30;

            if (Unity.Entities.World.DefaultGameObjectInjectionWorld != null)
            {
                unitArchtype =Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                    typeof(RenderMesh),
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(Scale),
                    typeof(LocalToWorld),
                    typeof(WorldRenderBounds)
                );
            }
            //Spawn();
            // create
            SwitchAnimation(0);
            StartAnimating();
            isDownsized = false;
            ToggleRenderScale();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (group.alpha == 0)
                {
                    group.alpha = 1;
                }
                else
                {
                    group.alpha = 0;
                }
            }
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                ToggleFog();
            }
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                TogglePostProcessing();
            }
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                ToggleRenderScale();
            }
            if (fpsCounter)
            {
                if (m_timeCounter < m_refreshTime)
                {
                    m_timeCounter += Time.deltaTime;
                    m_frameCounter++;
                }
                else
                {
                    //This code will break if you set your m_refreshTime to 0, which makes no sense.
                    m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                    m_frameCounter = 0;
                    m_timeCounter = 0.0f;
                }
                fpsCounter.text = "FPS [" + m_lastFramerate + "]";
            }
            //Debug.LogError("Frame Index: " + material.GetInt("_FrameIndex"));
        }
        int m_frameCounter = 0;
        float m_timeCounter = 0.0f;
        float m_lastFramerate = 0.0f;
        public float m_refreshTime = 0.5f;

        public void SpawnRandomly100()
        {
            SpawnRandomAmount(100);
        }
        public void SpawnRandomly1000()
        {
            SpawnRandomAmount(1000);
        }
        public void SpawnRandomly100000()
        {
            SpawnRandomAmount(10000);
        }

        private void SpawnRandomAmount(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                SpawnRandomly();
            }
        }
        public void SpawnRandomly()
        {
            var position = new Vector3(
                UnityEngine.Random.Range(-randomRange, randomRange),
                0,
                UnityEngine.Random.Range(-randomRange, randomRange)
            );
            quaternion rotation = quaternion.Euler(new float3(0, UnityEngine.Random.Range(-180, 180), 0));
            Spawn(position, rotation);
        }

        public void Spawn()
        {
            Spawn(float3.zero, quaternion.identity);
        }
        public void Spawn(float3 position, quaternion rotation)
        {
            if (Unity.Entities.World.DefaultGameObjectInjectionWorld == null)
            {
                Debug.Log("World.Active is null. Creating World.");
                Unity.Entities.World world = new Unity.Entities.World("World");
                world.CreateSystem<TransformSystemGroup>();
                world.CreateSystem<RenderMeshSystemV2>();
                world.CreateSystem<RenderBoundsUpdateSystem>();
                world.CreateSystem<CreateMissingRenderBoundsFromMeshRenderer>();
                world.CreateSystem<EndFrameTRSToLocalToWorldSystem>();
                Unity.Entities.World.Active = world;

                unitArchtype =Unity.Entities.World.Active.EntityManager.CreateArchetype(
                    typeof(RenderMesh),
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(Scale)
                );
            }
            var unit =Unity.Entities.World.Active.EntityManager.CreateEntity(unitArchtype);
           Unity.Entities.World.Active.EntityManager.SetComponentData(unit, new Translation { Value = position });
           Unity.Entities.World.Active.EntityManager.SetComponentData(unit, new Rotation { Value = rotation });
           Unity.Entities.World.Active.EntityManager.SetComponentData(unit, new Scale { Value = data.scale });
            //material.enableInstancing = true;
            RenderMesh renderer = new RenderMesh
            {
                material = new Material(data.material),
                mesh = data.mesh
            };
           Unity.Entities.World.Active.EntityManager.SetSharedComponentData(unit, renderer);
            entities.Add(unit);
            entitiesCounter.text = "Spawned [" + entities.Count + "]";
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            if (Unity.Entities.World.Active != null &&Unity.Entities.World.Active.EntityManager != null)
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    if (Unity.Entities.World.Active.EntityManager.Exists(entities[i]))
                    {
                       Unity.Entities.World.Active.EntityManager.DestroyEntity(entities[i]);
                    }
                }
            }
            entities.Clear();
            if (entitiesCounter)
            {
                entitiesCounter.text = "Spawned [" + entities.Count + "]";
            }
        }

        public void ToggleFog()
        {
            RenderSettings.fog = !RenderSettings.fog;
        }
        public void TogglePostProcessing()
        {
            isProcessors = !isProcessors;
            for (int i = 0; i < cameras.Count; i++)
            {
#if POST_PROCESSING
                cameras[i].GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>().enabled = isProcessors;
#endif
            }
        }

        public void ToggleRenderScale()
        {
            isDownsized = !isDownsized;
            int width = 1920;
            int height = 1080;
            if (isDownsized)
            {
                width = (int)(1920 / 8f);
                height = (int)(1080 / 8f);
            }
            RenderTexture texture = new RenderTexture(cameras[0].targetTexture);//width, height, 0, RenderTextureFormat.ARGB32);
            texture.width = width;
            texture.height = height;
            texture.name = "Camera";
            //texture.format = RenderTextureFormat.ARGB32;
            //texture.format = RenderTextureFormat.ARGBHalf;
            texture.filterMode = FilterMode.Point;
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].targetTexture = texture;
            }
            cameraImage.texture = texture as Texture;
        }

        public void StopAnimating()
        {
            //material.SetFloat("_AnimationSpeed", 0);
            //float frameIndex = (((Time.time - timeBegun) * 60 * 1) % framesTotal) + framesAddition;
            //material.SetInt("_FramesAddition", (int) frameIndex);
        }

        public void StartAnimating()
        {
            //material.SetFloat("_AnimationSpeed", 1);
            //material.SetInt("_FramesAddition", framesAddition);
            // set time begun again
        }

        public void HalfSpeed()
        {
            //material.SetFloat("_AnimationSpeed", 0.5f);
            //material.SetInt("_FramesAddition", framesAddition);
        }

        public void RandomAnimation()
        {
            float timeBegun =UnityEngine.Time.time;
            int randomAnimation;
            RenderMesh renderer;
            float animationTime;
            int framesAddition;
            int framesPerSecond;
            //MaterialPropertyBlock props = new MaterialPropertyBlock();
            foreach (Entity e in entities)
            {
                renderer =Unity.Entities.World.Active.EntityManager.GetSharedComponentData<RenderMesh>(e);
                Material material = new Material(renderer.material);
                randomAnimation = UnityEngine.Random.Range(0, 3);

                animationTime = (data.data.datas[randomAnimation].time);
                framesAddition = GetFramesAddition(randomAnimation);
                framesPerSecond = (data.data.datas[randomAnimation].frames);

                material.SetFloat("_TimeBegun", timeBegun);
                material.SetFloat("_AnimationTime", animationTime);
                material.SetInt("_FramesAddition", framesAddition);
                material.SetInt("_FramesPerSecond", framesPerSecond);
                renderer.material = material;
               Unity.Entities.World.Active.EntityManager.SetSharedComponentData(e, renderer);
                //renderer.material.SetMatrix(props);
                //renderer.material.SetBuffer("Props", props);
            }
        }

        private void SetAnimation(int animationIndex)
        {
            float timeBegun =UnityEngine.Time.time;
            RenderMesh renderer;
            int framesAddition = GetFramesAddition(animationIndex);
            float animationTime = (data.data.datas[animationIndex].time);
            int framesPerSecond = (data.data.datas[animationIndex].frames);
            foreach (Entity e in entities)
            {
                renderer =Unity.Entities.World.Active.EntityManager.GetSharedComponentData<RenderMesh>(e);
                Material material = (renderer.material); // new Material

                material.SetFloat("_TimeBegun", timeBegun);
                material.SetFloat("_AnimationTime", animationTime);
                material.SetInt("_FramesAddition", framesAddition);
                material.SetInt("_FramesPerSecond", framesPerSecond);
                renderer.material = material;
               Unity.Entities.World.Active.EntityManager.SetSharedComponentData(e, renderer);
            }
        }

        private int GetFramesAddition(int animationIndex)
        {
            int framesAddition = 0;
            for (int i = 0; i < animationIndex; i++)
            {
                framesAddition += data.data.datas[i].frames;
            }
            return framesAddition;
        }

        public void SwitchAnimation(int animationIndex)
        {
            SetAnimation(animationIndex);
        }

        public void SetCamera(int index)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].gameObject.SetActive(false);
            }
            cameras[index].gameObject.SetActive(true);
        }
    }
}
*/
//animator.Stop();
/*if (animationIndex == 0)
{
    material.SetInt("_FramesAddition", 0);
    material.SetInt("_FramesTotal", 60);
    //animator.clip = animator.GetClip("Walk");
}
else if(animationIndex == 1)
{
    material.SetInt("_FramesAddition", 60);
    material.SetInt("_FramesTotal", 30);
    //animator.clip = animator.GetClip("Idle");
}
else if (animationIndex == 2)
{
    material.SetInt("_FramesAddition", 104);
    material.SetInt("_FramesTotal", 120);
    //animator.clip = animator.GetClip("Dance");
}*/
