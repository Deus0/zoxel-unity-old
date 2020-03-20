using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;

namespace Zoxel
{
    [DisableAutoCreation]
    public class TurretSpawnerSystem : ComponentSystem
    {
        // ecs
        private EntityArchetype turretArchtype;
        //private EntityArchetype turretBaseArchtype;
        public static Dictionary<int, Entity> turrets = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> bases = new Dictionary<int, Entity>();
        //private List<int> spawnedIDs = new List<int>();
        // data
        public static Dictionary<int, TurretDatam> meta = new Dictionary<int, TurretDatam>();
        public static List<TurretDatam> turretData = new List<TurretDatam>();
        public static List<int> commandsIDs = new List<int>();
        public static List<float3> commandsSpawn = new List<float3>();
        public static List<int> commandsType = new List<int>();
        public static List<int> commandsSummonerID = new List<int>();
        //static float turretScale = 0.5f;
        // queue

        public static int QueueTurret(float3 spawnPosition, int type, int summonerID)
        {
            int id = Bootstrap.GenerateUniqueID();
            commandsIDs.Add(id);
            commandsSpawn.Add(spawnPosition);
            commandsType.Add(type);
            commandsSummonerID.Add(summonerID);
            return id;
        }

        protected override void OnCreate()
        {
            turretArchtype = World.EntityManager.CreateArchetype(
                // id, name, etc
               // typeof(Turret),
                typeof(ZoxID),
                typeof(Stats),
                typeof(Targeter),
                typeof(Body),
                // combat
                typeof(Shooter),
                typeof(Aimer),
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                // renderer
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );
            /*turretBaseArchtype = entityManager.CreateArchetype(
                // transform
                typeof(Translation),
                typeof(Rotation),
                typeof(NonUniformScale),
                // renderer
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );*/
        }

        public void Clear()
        {
            foreach (Entity e in turrets.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            turrets.Clear();
            foreach (Entity e in bases.Values)
            {
                if (World.EntityManager.Exists(e))
                {
                    World.EntityManager.DestroyEntity(e);
                }
            }
            bases.Clear();
        }

        protected override void OnUpdate()
        {
            if (commandsSpawn.Count > 0)
            {
                int commandIndex = commandsSpawn.Count - 1;
                float3 newPosition = commandsSpawn[commandIndex];
                int type = commandsType[commandIndex];
                int summonerID = commandsSummonerID[commandIndex];
                int spawnID = commandsIDs[commandIndex];
                SpawnTurret(spawnID, newPosition, type, summonerID);
                commandsSpawn.RemoveAt(commandIndex);
                commandsSummonerID.RemoveAt(commandIndex);
                commandsType.RemoveAt(commandIndex);
                commandsIDs.RemoveAt(commandIndex);
            }
        }

        private void SetOrAddComponent<T>(Entity characterEntity, T component) where T : struct, IComponentData
        {
            if (World.EntityManager.HasComponent<T>(characterEntity))
            {
                World.EntityManager.SetComponentData<T>(characterEntity, component);
            }
            else
            {
                World.EntityManager.AddComponentData<T>(characterEntity, component);
            }
        }
        private void SetOrAddSharedComponent<T>(Entity characterEntity, T component) where T : struct, ISharedComponentData
        {
            if (World.EntityManager.HasComponent<T>(characterEntity))
            {
                World.EntityManager.SetSharedComponentData<T>(characterEntity, component);
            }
            else
            {
                World.EntityManager.AddSharedComponentData<T>(characterEntity, component);
            }
        }

        private static float turretScale = 0.5f;
        void SpawnTurret(int spawnID, float3 initialPosition, int type, int summonerID)
        {
            // Set the default
            if (turretData[type] == null)
            {
                Debug.LogError("Data is null for turret");
                return;
            }
            //spawnPosition = spawnPosition + new float3(0, -0.5f, 0);
            initialPosition -= new float3(0, 0.5f, 0);
            float3 basePosition = initialPosition + turretScale * (new float3(0, turretData[type].baseMesh.bounds.extents.y, 0));
            float3 headPosition = basePosition + turretScale * (new float3(0, turretData[type].baseMesh.bounds.extents.y + turretData[type].headMesh.bounds.extents.y, 0));
            TurretDatam datam = turretData[type];
            TurretData data = datam.Value;
            BulletDatam bulletDatam = datam.bullet;
            quaternion spawnRotation = quaternion.identity;
            Entity entity = World.EntityManager.CreateEntity(turretArchtype);
            SetOrAddComponent(entity,
                new ZoxID
                {
                    id = spawnID,
                    clanID = summonerID
                });
            //SetOrAddComponent(entity, Stats.GenerateBasicStats());

            // Rendering
            SetOrAddComponent(entity, new Translation { Value = headPosition });
            SetOrAddComponent(entity, new Rotation { Value = spawnRotation });
            SetOrAddComponent(entity, new NonUniformScale {
                    Value = new float3(turretScale, turretScale, turretScale)
            });
            RenderMesh newRenderer = new RenderMesh {
                material = turretData[type].material,
                mesh = turretData[type].headMesh,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true
            };
            SetOrAddSharedComponent(entity, newRenderer);
            // Turret parts
            SetOrAddComponent(entity, new Shooter {
                attackDamage = data.attackDamage,
                attackForce = data.attackForce,
                bulletMetaID = bulletDatam.Value.id
            });
            Unity.Mathematics.Random random = new Unity.Mathematics.Random();
            random.InitState((uint)spawnID);
            SetOrAddComponent(entity, new Aimer {
                id = spawnID,
                uniqueness = spawnID,
                turnSpeed = data.turnSpeed,
                random = random,
                targetRotation = spawnRotation,
                originalPosition = headPosition,
                offsetZ = turretData[type].headMesh.bounds.extents.z
            });
            //Debug.LogError("Turret offsetZ: " + turretData[type].headMesh.bounds.extents.z);
            SetOrAddComponent(entity, 
                new Targeter
                {
                    Value = data.seek
                });

            // Turret Base
            Entity entityBase = World.EntityManager.CreateEntity();
            World.EntityManager.AddComponentData(entityBase, new Translation { Value = basePosition });
            World.EntityManager.AddComponentData(entityBase, new Rotation{});
            World.EntityManager.AddComponentData(entityBase, new NonUniformScale {
                Value = new float3(turretScale, turretScale, turretScale) });
            RenderMesh baseMesh = new RenderMesh {
                material = turretData[type].material,
                mesh = turretData[type].baseMesh,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true
            };
            World.EntityManager.AddSharedComponentData(entityBase, baseMesh);
            World.EntityManager.AddComponent<LocalToWorld>(entityBase);
            // add things!
            turrets.Add(spawnID, entity);
            bases.Add(spawnID, entityBase);
            statbarSystem.QueueBar(entity);//, 0.5f);
            AudioManager.instance.PlaySound(datam.spawnSound, basePosition);
        }
        public StatbarSystem statbarSystem;

        // publics

        public float GetGoldCost(int type)
        {
            return turretData[type].Value.goldCost;
        }

        public bool CanPlaceTurret(float3 spawnPosition)
        {
            //for (int i = 0; i < TurretSpawnerSystem.turrets.Count; i++)
            foreach (Entity e in turrets.Values)
            {
                Translation position = World.EntityManager.GetComponentData<Translation>(e);
                if (spawnPosition.x >= position.Value.x - 0.25f && spawnPosition.x <= position.Value.x + 0.25f &&
                    spawnPosition.y >= position.Value.y - 0.25f && spawnPosition.y <= position.Value.y + 0.25f &&
                    spawnPosition.z >= position.Value.z - 0.25f && spawnPosition.z <= position.Value.z + 0.25f)
                {
                    return false;
                }
            }
            return true;
        }

        public Entity GetTurret(float3 checkPosition)
        {
            foreach (Entity e in turrets.Values)
            {
                Translation position = World.EntityManager.GetComponentData<Translation>(e);
                if (checkPosition.x >= position.Value.x - 0.25f && checkPosition.x <= position.Value.x + 0.25f &&
                    checkPosition.y >= position.Value.y - 0.25f && checkPosition.y <= position.Value.y + 0.25f &&
                    checkPosition.z >= position.Value.z - 0.25f && checkPosition.z <= position.Value.z + 0.25f)
                {
                    //Debug.LogError("Turret already in this position");
                    return e;
                }
            }
            return new Entity();
        }
    }
}

//System.Guid myGUID = System.Guid.NewGuid();
//uint uniqueness = ((uint)myGUID.ToByteArray()[0]) + 1;
