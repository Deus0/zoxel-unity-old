using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Zoxel.Voxels;
using Zoxel.UI;

// should be a system:
//      Controller, Skills, Stats, Shooter
//          Controller for input
//          Skills for skill data - cast time etc
//          Stats for mana and damage modifiers
//          Shooter for actual shooting, like shoot position etc
//          
//      Do this for other systems / skills - their own unique systems
//
//      Idea: BulletTargettingSystem - bullet will target a npc and move closer to them
//      

namespace Zoxel
{
    // The idea is that skills will be activated by players, or by AI
    // SkillActivation will by rejected or not here

    [DisableAutoCreation]//, UpdateAfter(typeof(CameraProxySystem))]
    public class SkillsSystem : ComponentSystem
    {
        public ActionbarSystem actionbarSpawnSystem;
        public CharacterSpawnSystem characterSpawnSystem;
        public Dictionary<int, SkillDatam> meta;

        protected override void OnUpdate()
        {
            Entities.WithAll<ZoxID, Skills>().ForEach((Entity e, ref ZoxID zoxID, ref Skills skills) => // , ref RenderMesh renderer
            {
                if (skills.updated == 1)
                {
                    skills.updated = 0;
                    // update skills icons
                    actionbarSpawnSystem.SetSlotPosition(zoxID.id, skills.selectedSkillIndex);
                    // turn off or on raycasting
                    // change crosshair for skill
                    // change character animation
                    InitializeSkills(e, skills);
                }
                if (skills.triggered == 1)
                {
                    skills.triggered = 0;
                    if (skills.skills.Length == 0)
                    {
                        return; // no skill to activate
                    }
                    // activate the skills
                    SkillData datim = skills.skills[skills.selectedSkillIndex];
                    SkillDatam datum = meta[datim.id];
                    if (datum.Value.attackType == 1)
                    {
                        // trigger melee attack here
                        // ai should trigger the skills component
                        ActivateMeleeAttack(e, ref skills);
                    }
                    else if (datum.bullet != null)
                    {
                        ShootBullet(datum, e);
                    }
                    else if (datum.turret != null)
                    {
                        SpawnTurret(e, 0);
                    }
                    else if (datum.monster != null)
                    {
                        SpawnMonster(e, datum);
                    }
                    else if (datum.voxel != null)
                    {
                        SpawnVoxel(e, datum.voxel);
                    }
                }
            });
        }

        public void RemoveSkills(Entity e)
        {
            if (World.EntityManager.HasComponent<Shooter>(e) == true)
            {
                World.EntityManager.RemoveComponent<Shooter>(e);
            }
            if (World.EntityManager.HasComponent<CharacterRaycaster>(e) == true)
            {
                World.EntityManager.RemoveComponent<CharacterRaycaster>(e);
            }
            if (World.EntityManager.HasComponent<MeleeAttack>(e) == true)
            {
                World.EntityManager.RemoveComponent<MeleeAttack>(e);
            }
        }

        public void InitializeSkills(Entity e, Skills skills)
        {
            RemoveSkills(e);
            if (skills.selectedSkillIndex < 0 || skills.selectedSkillIndex >= skills.skills.Length)
            {
                //Debug.LogError("Selected skill index out of range: " + skills.selectedSkillIndex + " out of " + skills.skills.Length);
                return;
            }
            SkillData datim = skills.skills[skills.selectedSkillIndex];
            SkillDatam datum = meta[datim.id];
            if (datum.Value.attackType == 1)
            {
                AddMeleeAttack(e, datum);
            }
            else if (datum.bullet != null)
            {
                AddShooter(e, datum);
            }
            else
            {
                if (datum.turret != null || datum.monster != null || datum.voxel != null)
                {
                    AddCaster(e);
                }
            }
        }

        private void AddMeleeAttack(Entity e, SkillDatam datum)
        {
            if (World.EntityManager.HasComponent<MeleeAttack>(e) == false)
            {
                //Debug.Log("Adding MElee Component");
                World.EntityManager.AddComponentData(e, new MeleeAttack
                {
                    attackDamage = datum.Value.attackDamage,
                    attackCooldown = datum.Value.attackSpeed
                });
            }
        }
        private void AddCaster(Entity e)
        {
            if (World.EntityManager.HasComponent<CharacterRaycaster>(e) == false)
            {
                if (World.EntityManager.HasComponent<CameraLink>(e))
                {
                    CameraLink cameraLink = World.EntityManager.GetComponentData<CameraLink>(e);
                    World.EntityManager.AddComponentData(e, new CharacterRaycaster { camera = cameraLink.camera });
                } else
                {
                    World.EntityManager.AddComponentData(e, new CharacterRaycaster { });
                }
            }
        }

        private void AddShooter(Entity e, SkillDatam datum)
        {
            //Debug.Log("Adding SHooter Component");
            if (World.EntityManager.HasComponent<Shooter>(e) == false)
            {
                /*int cameraID = 0;
                if (World.EntityManager.HasComponent<Controller>(e))
                {
                    cameraID = World.EntityManager.GetComponentData<Controller>(e).cameraID;
                }*/
                World.EntityManager.AddComponentData(e, new Shooter
                {
                    attackForce = datum.Value.attackForce,    // get this from stats!
                    attackDamage = datum.Value.attackDamage,
                    bulletMetaID = datum.bullet.Value.id//,
                                                        // cameraID = cameraID
                });
            }
        }

        private void SpawnVoxel(Entity e, VoxelDatam voxelMeta)
        {
            CharacterRaycaster caster = World.EntityManager.GetComponentData<CharacterRaycaster>(e);
            WorldBound worldBound = World.EntityManager.GetComponentData<WorldBound>(e);
            if (caster.DidCast() == 1)
            {
                //Debug.LogError("Spawning voxel at: " + caster.voxelPosition.ToString());
                VoxelSpawnSystem.QueueVoxel(caster.voxelPosition, worldBound.world, voxelMeta.Value.id);
            }
        }

        private void ActivateMeleeAttack(Entity e, ref Skills skills)
        {
            if (World.EntityManager.HasComponent<MeleeAttack>(e) == false)
            {
                Debug.LogError("Does not have melee attack: " + e.Index);
                SkillData datum = skills.skills[skills.selectedSkillIndex];
                World.EntityManager.AddComponentData(e, new MeleeAttack
                {
                    attackDamage = datum.attackDamage,
                    attackCooldown = datum.attackSpeed
                });
            }
            MeleeAttack meleeAttack = World.EntityManager.GetComponentData<MeleeAttack>(e);
            meleeAttack.triggered = 1;
            World.EntityManager.SetComponentData(e, meleeAttack);
            //Debug.LogError("Activating Melee Attack by [" + e.Index + "].");
        }

        private void ShootBullet(SkillDatam skillDatam, Entity e)
        {
            if (World.EntityManager.HasComponent<Shooter>(e) == false)
            {
                return;
            }
            Shooter shooter = World.EntityManager.GetComponentData<Shooter>(e);
            if (shooter.CanTrigger(UnityEngine.Time.time))
            {
                shooter.triggered = 1;
                Entity camera;
                if (World.EntityManager.HasComponent<CameraLink>(e))
                {
                    camera = World.EntityManager.GetComponentData<CameraLink>(e).camera;
                }
                else
                {
                    camera = new Entity();
                }
                if (World.EntityManager.Exists(camera))
                {
                    //GameObject shooterCamera = CameraSystem.cameraObjects[cameraID];
                    //Debug.LogError("Shooting with camera: " + shooterCamera.name);
                    float3 position = World.EntityManager.GetComponentData<Translation>(camera).Value;
                    quaternion rotation = World.EntityManager.GetComponentData<Rotation>(camera).Value;
                    shooter.shootPosition = position + math.rotate(rotation, new float3(0, 0, 0.1f));// shooterCamera.transform.position + shooterCamera.transform.forward * 0.1f;//aimer.originalPosition + math.mul(aimer.targetRotation, new float3(0, 0, 0.19f + 0.2f));
                    shooter.shootRotation = rotation;// shooterCamera.transform.rotation;
                }
                else
                {
                    Translation translation = World.EntityManager.GetComponentData<Translation>(e);
                    Rotation rotation = World.EntityManager.GetComponentData<Rotation>(e);
                    shooter.shootPosition = translation.Value;//aimer.originalPosition + math.mul(aimer.targetRotation, new float3(0, 0, 0.19f + 0.2f));

                    Targeter targeter = World.EntityManager.GetComponentData<Targeter>(e);
                    float3 normalBetween = math.normalizesafe(targeter.nearbyCharacter.position - translation.Value);
                    quaternion targetAngle = quaternion.LookRotationSafe(normalBetween, math.up());
                    //rotation.Value = QuaternionHelpers.slerpSafe(rotation.Value, targetAngle, mover.turnSpeed);
                    shooter.shootRotation = targetAngle;// rotation.Value;
                                                        //Debug.LogError("Shoot Camera ID is 0.");
                }
                AudioManager.instance.PlaySound(skillDatam.audio, shooter.shootPosition);
                World.EntityManager.SetComponentData(e, shooter);
                //Debug.LogError("Shooting Bullet from: " + shooter.shootPosition);
            }
        }

        private void SpawnMonster(Entity e, SkillDatam datam)
        {
            if (World.EntityManager.HasComponent<CharacterRaycaster>(e) == false)
            {
                return;
            }
            int monsterID = datam.monster.Value.id;
            ZoxID zoxID = World.EntityManager.GetComponentData<ZoxID>(e);
            CharacterRaycaster caster = World.EntityManager.GetComponentData<CharacterRaycaster>(e);
            WorldBound worldBound = World.EntityManager.GetComponentData<WorldBound>(e);
            if (caster.DidCast() == 1)
            {
                ZoxID stats = World.EntityManager.GetComponentData<ZoxID>(e);
                //caster.triggered = 1;
                World.EntityManager.SetComponentData(e, caster);
                int clanID = stats.clanID;
                if (datam.Value.isSpawnHostile == 1)
                {
                    clanID = Bootstrap.GenerateUniqueID();
                }
                CharacterSpawnSystem.SpawnNPC(World.EntityManager, worldBound.world, monsterID, clanID, caster.voxelPosition, zoxID.id);
                /*if (datam.Value.isSpawnHostile != 1)
                {
                    Entity npc = characterSpawnSystem.characters[spawnedID];
                    ZoxID spawnedZoxID = World.EntityManager.GetComponentData<ZoxID>(npc);
                    spawnedZoxID.creatorID = zoxID.id;
                    World.EntityManager.SetComponentData(npc, spawnedZoxID);
                }*/
                AudioManager.instance.PlaySound(datam.audio, caster.voxelPosition);
                //Debug.LogError("Spawning Turret at: " + caster.voxelPosition.ToString());
            }
        }

        private void SpawnTurret(Entity e, int turretID)
        {
            CharacterRaycaster caster = World.EntityManager.GetComponentData<CharacterRaycaster>(e);
            if (caster.DidCast() == 1)
            {
                ZoxID stats = World.EntityManager.GetComponentData<ZoxID>(e);
                TurretSpawnerSystem.QueueTurret(caster.voxelPosition, turretID, stats.clanID);
                //caster.triggered = 1;
                //World.EntityManager.SetComponentData(e, caster);
                //Debug.LogError("Spawning Turret at: " + caster.voxelPosition.ToString());
            }
        }

        /*public int GetSelectedSkillID(int characterID)
        {
            if (CharacterSpawnSystem.characters.ContainsKey(characterID))
            {
                Entity e = CharacterSpawnSystem.characters[characterID];
                Skills skills = World.EntityManager.GetComponentData<Skills>(e);
                return skills.skills[skills.selectedSkillIndex].id;
            }
            return -1;
        }
        public int GetSelectedSlot(int characterID)
        {
            if (CharacterSpawnSystem.characters.ContainsKey(characterID))
            {
                Entity e = CharacterSpawnSystem.characters[characterID];
                Skills skills = World.EntityManager.GetComponentData<Skills>(e);
                return skills.selectedSkillIndex;
            }
            return -1;
        }

        public void SetSelectedSlot(int characterID, int newSlotIndex)
        {
            if (CharacterSpawnSystem.characters.ContainsKey(characterID))
            {
                Entity e = CharacterSpawnSystem.characters[characterID];
                Skills skills = World.EntityManager.GetComponentData<Skills>(e);
                skills.selectedSkillIndex = newSlotIndex;
                skills.hasUpdated = 1;
                World.EntityManager.SetComponentData(e, skills);
            }
        }

        public void IncreaseSlotIndex(int characterID)
        {
            if (CharacterSpawnSystem.characters.ContainsKey(characterID))
            {
                Entity e = CharacterSpawnSystem.characters[characterID];
                Skills skills = World.EntityManager.GetComponentData<Skills>(e);
                skills.selectedSkillIndex++;
                skills.hasUpdated = 1;
                if (skills.selectedSkillIndex == skills.skills.Length)
                {
                    skills.selectedSkillIndex = 0;
                }
                World.EntityManager.SetComponentData(e, skills);
            }
        }

        public void DecreaseSlotIndex(int characterID)
        {
            if (CharacterSpawnSystem.characters.ContainsKey(characterID))
            {
                Entity e = CharacterSpawnSystem.characters[characterID];
                Skills skills = World.EntityManager.GetComponentData<Skills>(e);
                skills.selectedSkillIndex--;
                skills.hasUpdated = 1;
                if (skills.selectedSkillIndex == -1)
                {
                    skills.selectedSkillIndex = skills.skills.Length - 1;
                }
                World.EntityManager.SetComponentData(e, skills);
            }
        }
        */
    }
}