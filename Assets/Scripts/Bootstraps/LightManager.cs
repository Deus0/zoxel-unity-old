using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.Audio;

namespace Zoxel
{
    public class LightManager : MonoBehaviour
    {
        public static LightManager instance;
        public List<GameObject> lights;

        void Awake()
        {
            instance = this;
        }

        private void DeactivateAllLights()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].SetActive(false);
            }
        }

        public void SetLight(string lightName)
        {
            DeactivateAllLights();

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i].name == lightName)
                {
                    lights[i].SetActive(true);
                    break;
                }
            }
        }
    }

}