using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using System.Collections.Generic;

/// <summary>
/// Flagged for rewrite - Due to using Referencing to lists of character entities
/// </summary>

using Unity.Rendering;

namespace Zoxel
{
    [DisableAutoCreation]
	public class CharacterDeathSystem : ComponentSystem // JobComponentSystem
	{
        public CharacterSpawnSystem characterSpawnSystem;
        public SkillsSystem skillsSystem;
        public static float deadbodyRemovalTime = 1f;
        //public static float shrinkSpeed = 1.5f;
        public PlayerSpawnSystem playerSpawnSystem;
        public ItemSpawnerSystem itemSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<Stats, ZoxID>().ForEach((Entity e, ref Stats stats, ref ZoxID zoxID) => // , ref RenderMesh renderer
            {
                if (stats.willDie == 1)
                {
                    StartDying(e, zoxID.id);
                }
            });
            Entities.WithAll<DyingOne, ZoxID>().ForEach((Entity e, ref DyingOne dyingOne, ref ZoxID zoxID) => // , ref RenderMesh renderer
            {
                // Shrinking!
                /*NonUniformScale scale = World.EntityManager.GetComponentData<NonUniformScale>(e);
                scale.Value = math.lerp(scale.Value, 0, UnityEngine.Time.deltaTime * shrinkSpeed);
                World.EntityManager.SetComponentData(e, scale);
                if (TurretSpawnerSystem.bases.ContainsKey(zoxID.id))
                {
                    NonUniformScale scale2 = World.EntityManager.GetComponentData<NonUniformScale>(TurretSpawnerSystem.bases[zoxID.id]);
                    scale2.Value = math.lerp(scale.Value, 0, UnityEngine.Time.deltaTime * shrinkSpeed);
                    World.EntityManager.SetComponentData(TurretSpawnerSystem.bases[zoxID.id], scale2);
                }*/
                if (UnityEngine.Time.time - dyingOne.timeOfDeath >= deadbodyRemovalTime)
                {
                    DestroyCharacter(e, zoxID.id);
                }
            });
        }

        private void StartDying(Entity statsEntity, int zoxID)
        {
            float3 position = World.EntityManager.GetComponentData<Translation>(statsEntity).Value;
            if (World.EntityManager.HasComponent<Character>(statsEntity))
            {
                CharacterDatam characterDatam = characterSpawnSystem.meta[World.EntityManager.GetComponentData<Character>(statsEntity).metaID];
                for (int i = 0; i < characterDatam.dropItems.Count; i++)
                {
                    ItemDatam itemToDrop = characterDatam.dropItems[i].GetItem();
                    itemSpawnSystem.QueueItem(position,// + new float3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)),
                        itemToDrop, characterDatam.dropItems[i].GetQuantity());
                }
            }

            playerSpawnSystem.RemoveControllerCharacter(statsEntity);

            // game stuff
            if (World.EntityManager.HasComponent<Skills>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Skills>(statsEntity);
            }
            skillsSystem.RemoveSkills(statsEntity);
            if (World.EntityManager.HasComponent<Stats>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Stats>(statsEntity);
            }
            if (World.EntityManager.HasComponent<BulletHitTaker>(statsEntity))
            {
                World.EntityManager.RemoveComponent<BulletHitTaker>(statsEntity);
            }
            // movement
            if (World.EntityManager.HasComponent<Body>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Body>(statsEntity);
            }
            if (World.EntityManager.HasComponent<WorldBound>(statsEntity))
            {
                World.EntityManager.RemoveComponent<WorldBound>(statsEntity);
            }
            if (World.EntityManager.HasComponent<BodyForce>(statsEntity))
            {
                World.EntityManager.RemoveComponent<BodyForce>(statsEntity);
            }
            if (World.EntityManager.HasComponent<BodyTorque>(statsEntity))
            {
                World.EntityManager.RemoveComponent<BodyTorque>(statsEntity);
            }
            if (World.EntityManager.HasComponent<BodyInnerForce>(statsEntity))
            {
                World.EntityManager.RemoveComponent<BodyInnerForce>(statsEntity);
            }
            // ai
            if (World.EntityManager.HasComponent<Targeter>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Targeter>(statsEntity);
            }
            if (World.EntityManager.HasComponent<Wander>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Wander>(statsEntity);
            }
            if (World.EntityManager.HasComponent<Mover>(statsEntity))
            {
                World.EntityManager.RemoveComponent<Mover>(statsEntity);
            }
            if (World.EntityManager.HasComponent<AIState>(statsEntity))
            {
                World.EntityManager.RemoveComponent<AIState>(statsEntity);
            }
            else
            {
                CrosshairSpawnSystem.RemoveUI(World.EntityManager, statsEntity);
            }

            // int uiIndex = StatbarSystem.GetListIndex(statsComponent.id);
            if (StatbarSystem.frontBars.ContainsKey(zoxID))
             //   if (uiIndex != -1)
            {
                Entity characterHealthBar = StatbarSystem.frontBars[zoxID];
                StatBarUI statBar1 = World.EntityManager.GetComponentData<StatBarUI>(characterHealthBar);
                statBar1.isDead = 1;
                statBar1.targetPercentage = 0;
                //statBar1.timeStateChanged = UnityEngine.Time.time;
                World.EntityManager.SetComponentData(characterHealthBar, statBar1);
            }
            if (characterSpawnSystem.characters.ContainsKey(zoxID))
            {
                characterSpawnSystem.characters.Remove(zoxID);
            }
            World.EntityManager.AddComponentData(statsEntity, new DyingOne
            {
                timeOfDeath = UnityEngine.Time.time
            });
            if (TurretSpawnerSystem.bases.ContainsKey(zoxID))
            {
                World.EntityManager.AddComponentData(TurretSpawnerSystem.bases[zoxID], new ScaleLerper
                {
                    createdTime = UnityEngine.Time.time,
                    lifeTime = deadbodyRemovalTime,
                    scaleBegin = World.EntityManager.GetComponentData<NonUniformScale>(TurretSpawnerSystem.bases[zoxID]).Value,
                scaleEnd = float3.zero
                });
            }
            World.EntityManager.AddComponentData(statsEntity, new ScaleLerper
            {
                createdTime = UnityEngine.Time.time,
                lifeTime = deadbodyRemovalTime,
                scaleBegin = World.EntityManager.GetComponentData<NonUniformScale>(statsEntity).Value,
                scaleEnd = float3.zero
            });
        }

        public void DestroyCharacter(int characterID)
        {
            if (characterSpawnSystem.characters.ContainsKey(characterID))
            {
                DestroyCharacter(characterSpawnSystem.characters[characterID], characterID);
                characterSpawnSystem.characters.Remove(characterID);
            }
        }

        void DestroyCharacter(Entity statsEntity, int characterID)
        {
            if (StatbarSystem.frontBars.ContainsKey(characterID))
            {
                World.EntityManager.DestroyEntity(StatbarSystem.frontBars[characterID]);
                StatbarSystem.frontBars.Remove(characterID);
            }
            if (StatbarSystem.backBars.ContainsKey(characterID))
            {
                World.EntityManager.DestroyEntity(StatbarSystem.backBars[characterID]);
                StatbarSystem.backBars.Remove(characterID);
            }
            if (TurretSpawnerSystem.turrets.ContainsKey(characterID))
            {
                // remove this turret
                World.EntityManager.DestroyEntity(TurretSpawnerSystem.bases[characterID]);
                TurretSpawnerSystem.bases.Remove(characterID);
                TurretSpawnerSystem.turrets.Remove(characterID);
            }
            // Delete Character
            World.EntityManager.DestroyEntity(statsEntity);
        }
    }
}