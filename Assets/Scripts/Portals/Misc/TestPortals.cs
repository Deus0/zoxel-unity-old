using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;

namespace Zoxel
{
    [Serializable]
    public struct Portals {
        public GameObject portalLocationA;
        public GameObject portalLocationB;
        private Entity portalA;
        private Entity portalB;

        public void SpawnPortals(PortalSystem portalSystem, float2 portalSize)
        {
            portalA = portalSystem.SpawnPortal(
                portalLocationA.transform.position,
                portalLocationA.transform.rotation,
                portalSize);
            portalB = portalSystem.SpawnPortal(
                portalLocationB.transform.position,
                portalLocationB.transform.rotation,
                portalSize);  
            portalSystem.LinkPortals(portalA, portalB);
        }
    }

    public class TestPortals : MonoBehaviour
    {
        public static TestPortals instance;
        [Header("Testing")]
        public float2 portalSize = new float2(3,3);
        public List<Portals> portals;
        public GameObject traveler;

        void Start()
        {
            instance = this;
            SpawnPortals();
            //StartCoroutine(SpawnPortalsSoon());
        }

        [ContextMenu("SpawnPortals")]
        public void SpawnPortals()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var portalSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PortalSystem>();
            //portalSystem.mainCamera = Camera.main;
            for (int i = 0; i < portals.Count; i++)
            {
                portals[i].SpawnPortals(portalSystem, portalSize);
            }
        }
        
        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.E) && Time.timeScale < 2)
            {
                Time.timeScale += 0.1f;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q) && Time.timeScale > 0.1f)
            {
                Time.timeScale -= 0.1f;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1;
            }
            /*if (isClickSpawn)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit))
                    {
                        RepositionPortal(portalA, hit);
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit))
                    {
                        RepositionPortal(portalB, hit);
                    }
                }
            }*/
        }

        void OnGUI()
        {
            if (Time.timeScale != 1) {
                GUILayout.Label("TimeScale: " + Time.timeScale);
            }
        }

        void LateUpdate()
        {
            if (traveler)
            {
                var translation = traveler.GetComponent<TranslationProxy>().Value;
                translation.Value = traveler.transform.position;
                traveler.GetComponent<TranslationProxy>().Value = translation;
                var rotation = traveler.GetComponent<RotationProxy>().Value;
                rotation.Value = traveler.transform.rotation;
                traveler.GetComponent<RotationProxy>().Value = rotation;
                var local = traveler.GetComponent<LocalToWorldProxy>().Value;
                local.Value = traveler.transform.localToWorldMatrix;
                traveler.GetComponent<LocalToWorldProxy>().Value = local;
                /*traveler.GetComponent<CharacterController>().enabled = true;*/
            }
        }

        private void RepositionPortal(Entity portal, RaycastHit hit)
        {
            /*float3 hitNormal = hit.normal;
            //Debug.LogError("Hit: " + hitNormal);
            float3 newPosition = new float3(hit.point.x, transform.position.y + portalSize.y / 2f, hit.point.z) + hitNormal * 0.01f;
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(portal, new Translation{ Value = newPosition});
            //quaternion rotation = Quaternion.LookRotation(-hitNormal);
            quaternion rotation = Quaternion.Euler(hitNormal);
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(portal, new Rotation { Value = rotation });*/
            //Debug.DrawLine(newPosition, newPosition + hitNormal, Color.red, 30);
        }
    }
}