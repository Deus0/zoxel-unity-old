using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel
{
    public struct Damage : IComponentData
    {
        public Entity defender;
        public Entity attacker;
        public int hitType; // 0 for melee, 1 for ranged
        public float damage;
    }
    [DisableAutoCreation]
    public class DamageSystem : ComponentSystem
    {
        public StatbarSystem statbarSystem;
        public DamagePopupSystem damagePopupSystem;
        public CharacterSpawnSystem characterSpawnSystem;
        public StatsUISpawnSystem statsUISpawnSystem;

        public static void AddDamage(EntityManager EntityManager, Entity attacker, Entity defender, int hitType, float damage)
        {
            Damage damageC = new Damage
            {
                attacker = attacker,
                defender = defender,
                damage = damage,
                hitType = hitType
            };
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, damageC);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<Damage>().ForEach((Entity e, ref Damage damage) =>
            {
                if (!World.EntityManager.HasComponent<DyingOne>(damage.defender))
                {
                    ApplyDamage(damage.attacker, damage.defender, damage.damage);
                }
                World.EntityManager.DestroyEntity(e);
            });
        }

        private void ApplyDamage(Entity attacker, Entity defender, float damageDone)
        {
            // now get components
            if (!(World.EntityManager.HasComponent<Stats>(attacker) && World.EntityManager.HasComponent<Stats>(defender)))
            {
                return;
            }
            ZoxID defenderID = World.EntityManager.GetComponentData<ZoxID>(defender);
            Stats defenderStats = World.EntityManager.GetComponentData<Stats>(defender);
            ZoxID attackerID = World.EntityManager.GetComponentData<ZoxID>(attacker);
            Stats attackerStats = World.EntityManager.GetComponentData<Stats>(attacker);
            Translation defenderTranslation = World.EntityManager.GetComponentData<Translation>(defender);
            int healthIndex = 0;
            // decrease health
            if (defenderStats.states.Length == 0)
            {
                return; // cannot apply damage to gods
            }
            StateStaz healthStat = defenderStats.states[healthIndex];
            if (damageDone > 0 && healthStat.IsMaxValue())
            {
                // add regening to them
                RegenCompleterSystem.StartRegening(World.EntityManager, defender);
            }
            healthStat.value -= damageDone;
            //Debug.Log("Damage done: " + damageDone + " to " + defender.Index
            //    + " - " + defenderStats.states[0].value + " out of " + defenderStats.states[0].maxValue);
            if (healthStat.value < 0)
            {
                healthStat.value = 0;
            }
            defenderStats.states[healthIndex] = healthStat;
            // health pop up here
            damagePopupSystem.SpawnPopup(damageDone, defenderTranslation.Value);

            // Death
            if (healthStat.value == 0)// && defenderStats.isDead == 0)
            {
                //Debug.LogError("Defender ID has died: " + defender.Index + " at " + UnityEngine.Time.time);
                // Give rewards to Victor
                int metaID = 0;
                if (World.EntityManager.HasComponent<Character>(defender))
                {
                    metaID = World.EntityManager.GetComponentData<Character>(defender).metaID;
                }
                RewardVictor(ref defenderStats, attacker, ref attackerStats, defenderID.clanID,
                    metaID);
                defenderStats.willDie = 1;
                //defenderStats.timeDied = UnityEngine.Time.time;
                // Spawn Item
                //body.velocity = float3.zero;
                // give xp to attacker
            }
            else
            {
                // Attack back!
                RespondToAttack(defender, attacker);
                TriggerStatbar(defender, defenderID.id);
            }
            World.EntityManager.SetComponentData(defender, defenderStats);
        }

        private void TriggerStatbar(Entity entity, int defenderID)
        {
            if (!StatbarSystem.frontBars.ContainsKey(defenderID))
            {
                //Debug.LogError("Spawning healthbar for npc.");
                statbarSystem.SpawnNPCBar(entity);
            }
            if (StatbarSystem.frontBars.ContainsKey(defenderID))
            {
                // also make sure to turn on gui
                var frontbarEntity = StatbarSystem.frontBars[defenderID];
                StatBarUI statBarUI = World.EntityManager.GetComponentData<StatBarUI>(frontbarEntity);
                if (statBarUI.isTakingDamage == 0)
                {
                    statBarUI.isTakingDamage = 1;
                   //statBarUI.timeStateChanged = UnityEngine.Time.time;
                    //statBarUI.targetValue = targetValue;
                    World.EntityManager.SetComponentData(frontbarEntity, statBarUI);
                    //Debug.LogError("NPC now taking damage.");
                    //UnityEngine.Debug.LogError("Character " + barIndex + " is now taking damage.");
                }
            }
            else
            {
                Debug.LogError("Character does not have a health bar: " + defenderID);
            }
        }

        private void RewardVictor(ref Stats deadMonster, Entity attacker, ref Stats attackerStats, int defenderClanID, int defenderMetaID)
        {
            //int levelID = 0;
            /*if (!(StatsIndexes.experience < attackerStats.states.Length))
            {
                Debug.LogError("States are too small, doesnt havexp in them: " + attackerStats.states.Length);
                return;
            }*/
            if (attackerStats.levels.Length == 0)
            {
                return;
            }
            // should create a KillSystem that gives rewards
            //StateStaz experience = attackerStats.states[StatsIndexes.experience];
            Level attackerLevel = attackerStats.levels[0];
            // should pick up items called Soul Orbs that give you experience instead
            int levelValue = 0;
            if (deadMonster.levels.Length > 0)
            {
                levelValue = deadMonster.levels[0].value;
            }
            float experienceGiven = (levelValue + 1) * UnityEngine.Random.Range(0.5f, 1.5f);
            ZoxID attackerID = World.EntityManager.GetComponentData<ZoxID>(attacker);
            if (attackerID.creatorID == 0)
            {
                // normal experience
               // Debug.LogError("Giving experience: " + experienceGiven);
                attackerLevel.experienceGained += experienceGiven;// adding health as experience, shouldnt be based on their level?
            }
            else
            {
                if (characterSpawnSystem.characters.ContainsKey(attackerID.creatorID) == false)
                {
                    //Debug.LogError("Creator does not exist.");
                    attackerLevel.experienceGained += experienceGiven;
                }
                else
                {
                    Entity summonerEntity = characterSpawnSystem.characters[attackerID.creatorID];
                    if (World.EntityManager.HasComponent<Stats>(summonerEntity))
                    {
                        Stats summoner = World.EntityManager.GetComponentData<Stats>(summonerEntity);
                        if (summoner.levels.Length > 0)
                        {
                            //StateStaz summonerExp = summoner.states[StatsIndexes.experience];
                            Level summonerLevel = summoner.levels[0];
                            summonerLevel.experienceGained += experienceGiven / 2f;
                            if (summonerLevel.experienceGained >= summonerLevel.experienceRequired)
                            {
                                summoner.leveledUp = 1;
                            }
                            summoner.levels[0] = summonerLevel;
                            World.EntityManager.SetComponentData(summonerEntity, summoner);
                            attackerLevel.experienceGained += experienceGiven / 2f;
                            StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, summonerEntity, StatType.Level, 0);
                        }
                        else
                        {
                            attackerLevel.experienceGained += experienceGiven;
                        }
                    }
                    else
                    {
                        attackerLevel.experienceGained += experienceGiven;
                        //Debug.LogError("Creator exists. But has no stats.");
                        //experience.value += experienceGiven;// adding health as experience, shouldnt be based on their level?
                    }
                }
            }
            if (attackerLevel.experienceGained >= attackerLevel.experienceRequired)
            {
                attackerStats.leveledUp = 1;
            }
            attackerStats.levels[0] = attackerLevel;
            World.EntityManager.SetComponentData(attacker, attackerStats);
            StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, attacker, StatType.Level, 0);

            // give quest completion stat to the character

            if (characterSpawnSystem.characters.ContainsKey(attackerID.id))
            {
                Entity characterEntity = characterSpawnSystem.characters[attackerID.id];
                // get questlog
                if (World.EntityManager.HasComponent<QuestLog>(characterEntity))
                {
                    QuestLog questLog = World.EntityManager.GetComponentData<QuestLog>(characterEntity);
                    if (questLog.OnKilledCharacter(defenderMetaID))
                    {
                        World.EntityManager.SetComponentData(characterEntity, questLog);
                        // update questlog UI
                        //Bootstrap.instance.systemsManager.questLogUISpawnSystem.UpdateUI(createdID.id);
                    }
                }
            }
        }

        private void RespondToAttack(Entity defenderEntity, Entity attackerEntity)
        {
            Targeter defenderTargeter = World.EntityManager.GetComponentData<Targeter>(defenderEntity);
            if (defenderTargeter.hasTarget == 0)
            {
                ZoxID attackerID = World.EntityManager.GetComponentData<ZoxID>(attackerEntity);
                Translation attackerPosition = World.EntityManager.GetComponentData<Translation>(attackerEntity);
                Translation defenderPosition = World.EntityManager.GetComponentData<Translation>(defenderEntity);
                // Now set to attack the attacker
                defenderTargeter.hasTarget = 1;
                defenderTargeter.nearbyCharacter = new NearbyCharacter
                {
                    character = attackerEntity, // attackerID.id;
                    clan = attackerID.clanID,
                    position = attackerPosition.Value,
                    distance = math.distance(defenderPosition.Value, attackerPosition.Value)
                };
                //defenderTargeter.targetClanID = attackerID.clanID;
                ////targeter.targetID = attackerEntityID;
                //defenderTargeter.targetPosition = attackerPosition.Value;
                //defenderTargeter.targetDistance = 
                World.EntityManager.SetComponentData(defenderEntity, defenderTargeter);
                //Debug.LogError("Defender will now attack its target: " + attackerID.id);
            }
        }
    }
}