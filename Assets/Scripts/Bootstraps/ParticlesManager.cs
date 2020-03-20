using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.Experimental.Audio;

namespace Zoxel
{
    public struct ParticleSpawnCommand
    {
        public string name;
        public float life;
        public GameObject particlesInstance;
        public string deathParticleName;
        public float deathParticleLife;
    }

    public class ParticlesManager : MonoBehaviour
    {
        public static ParticlesManager instance;
        public List<GameObject> particles;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
        }


        private GameObject SpawnParticles(string name)
        {
            if (Bootstrap.isParticles == false)
            {
                return null;
            }
            int index = -1;
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].name == name)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                return null;
            }
            GameObject particlesInstance = GameObject.Instantiate(particles[index]);
            particlesInstance.hideFlags = HideFlags.HideInHierarchy;
            particlesInstance.name = name;
            return particlesInstance;
        }
        public void PlayParticles(ParticleSpawnCommand command, Entity e, EntityManager manager)
        {
            command.particlesInstance = SpawnParticles(command.name);
            if (command.particlesInstance != null)
            {
                StartCoroutine(FollowEntity(command, e, manager));
            }
        }

        private IEnumerator FollowEntity(ParticleSpawnCommand command, Entity e, EntityManager manager)
        {
            float timeBegun = UnityEngine.Time.time;
            while ((command.life == 0) || (Time.time - timeBegun <= command.life))
            {
                if (manager.Exists(e))
                {
                    if (command.name.Contains("Bullet") && !manager.HasComponent<Bullet>(e))
                    {
                        break;
                    }
                    else
                    {
                        Translation translation = manager.GetComponentData<Translation>(e);
                        command.particlesInstance.transform.position = translation.Value;
                    }
                }
                else
                {
                    break;
                }
                yield return null;
            }
            if (command.life != 0)
            {
                yield return new WaitForSeconds(10); // cleanup, for particles that arnt looping
            }
            // create a death particle
            Vector3 position = command.particlesInstance.transform.position;
            Destroy(command.particlesInstance);
            if (command.deathParticleName != null && command.deathParticleName.Trim() != "")
            {
                command.particlesInstance = SpawnParticles(command.deathParticleName);
                if (command.particlesInstance != null)
                {
                    command.particlesInstance.transform.position = position;
                    Destroy(command.particlesInstance, command.deathParticleLife);
                }
                else
                {
                    Debug.LogError("Failure to spawn particles: " + command.deathParticleName);
                }
            }
        }
    }

}