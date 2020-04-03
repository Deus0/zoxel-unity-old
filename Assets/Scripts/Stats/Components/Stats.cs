using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System;

namespace Zoxel
{

    public class StatsComponent : ComponentDataProxy<Stats> { }

    /// <summary>
    /// Each character has stats and things
    /// TODO: Make level a special stat - that has experience gained as a value - the experience and requiredExperience are values in it.
    /// </summary>
    [Serializable]
	public struct Stats : IComponentData
    {
        // Died (Make into its own component)
        public byte willDie;
        // Make into its own component
        public byte leveledUp;
        public byte leveledUpEffects;

        public byte attributesApplied;
        //public BlitableArray<byte> regeningStates;              // 1 if is regenning, 0 if not
        //public byte regenCompleted;                             // 1 if state finished and need to change fade state of bars
        // leveledUpAmount - incase they got alot of xp
        // main
        public BlitableArray<Staz> stats;
		public BlitableArray<StateStaz> states;
        public BlitableArray<RegenStaz> regens;
        public BlitableArray<AttributeStaz> attributes;
        public BlitableArray<Level> levels;

        public void Dispose()
        {
            //regeningStates.Dispose();
            stats.Dispose();
            states.Dispose();
            regens.Dispose();
            attributes.Dispose();
            levels.Dispose();
        }

        [Serializable]
        public struct SerializeableStats
        {
            public Staz[] stats;
            public StateStaz[] states;
            public RegenStaz[] regens;
            public AttributeStaz[] attributes;
            public Level[] levels;
        }

        public int GetIconIndex(Staz stat)
        {
            int preIndex = 0;
            for (int i = 0; i < stats.Length; i++)
            {
                if (stat.id == stats[i].id)
                {
                    return i + preIndex;
                }
            }
            return -1;
        }
        public int GetIconIndex(Level level)
        {
            int preIndex = stats.Length + states.Length + attributes.Length + regens.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                if (level.id == levels[i].id)
                {
                    return i + preIndex;
                }
            }
            return -1;
        }

        #region UsedForSpawningInCharacterSpawner

        public Stats Clone()
        {
            //Debug.LogError("Cloning states with: " + attributesApplied.ToString() + " attributes applied.");
            Stats newStats = new Stats();
            newStats.stats = new BlitableArray<Staz>(stats.Length, Allocator.Persistent);
            for (int i = 0; i < newStats.stats.Length; i++)
            {
                //Debug.LogError("Stat " + i + " is at: " + stats[i].value);
                newStats.stats[i] = stats[i];
            }
            newStats.states = new BlitableArray<StateStaz>(states.Length, Allocator.Persistent);
            for (int i = 0; i < newStats.states.Length; i++)
            {
                newStats.states[i] = states[i];
            }
            newStats.regens = new BlitableArray<RegenStaz>(regens.Length, Allocator.Persistent);
            for (int i = 0; i < newStats.regens.Length; i++)
            {
                newStats.regens[i] = regens[i];
            }
            newStats.attributes = new BlitableArray<AttributeStaz>(attributes.Length, Allocator.Persistent);
            for (int i = 0; i < newStats.attributes.Length; i++)
            {
                newStats.attributes[i] = attributes[i];
            }
            newStats.levels = new BlitableArray<Level>(levels.Length, Allocator.Persistent);
            for (int i = 0; i < newStats.levels.Length; i++)
            {
                newStats.levels[i] = levels[i];
            }
            return newStats;
        }
        #endregion

        #region SerializableBlittableArray
        public void InitializeStats(int count)
        {
            stats = new BlitableArray<Staz>(count, Allocator.Persistent);
            for (int j = 0; j < stats.Length; j++)
            {
                stats[j] = new Staz { };
            }
        }
        public void InitializeStates(int count)
        {
            states = new BlitableArray<StateStaz>(count, Allocator.Persistent);
            for (int j = 0; j < states.Length; j++)
            {
                states[j] = new StateStaz { };
            }
        }

        public void InitializeRegens(int count)
        {
            regens = new BlitableArray<RegenStaz>(count, Allocator.Persistent);
            for (int j = 0; j < regens.Length; j++)
            {
                regens[j] = new RegenStaz { };
            }
        }
        public void InitializeAttributes(int count)
        {
            attributes = new BlitableArray<AttributeStaz>(count, Allocator.Persistent);
            for (int j = 0; j < attributes.Length; j++)
            {
                attributes[j] = new AttributeStaz { };
            }
        }
        public void InitializeLevels(int count)
        {
            levels = new BlitableArray<Level>(count, Allocator.Persistent);
            for (int j = 0; j < levels.Length; j++)
            {
                levels[j] = new Level { };
            }
        }

        public SerializeableStats GetSerializeableClone()
        {
            SerializeableStats myClone = new SerializeableStats();
            myClone.stats = stats.ToArray();
            myClone.states = states.ToArray();
            myClone.regens = regens.ToArray();
            myClone.attributes = attributes.ToArray();
            myClone.levels = levels.ToArray();
            return myClone;
        }
        
        public string GetJson()
        {
            SerializeableStats myClone = GetSerializeableClone();
            return UnityEngine.JsonUtility.ToJson(myClone);
        }
        public static Stats FromJson(string json)
        {
            if (json == null || json == "")
            {
                return new Stats();
            }
            SerializeableStats myClone = UnityEngine.JsonUtility.FromJson<SerializeableStats>(json);
            Stats stats = new Stats { };
            if (myClone.stats == null)
            {
                Debug.LogError("JSON Null inside stats:\n" + json);
                return new Stats();
            }
            stats.FromClone(myClone);
            return stats;
        }

        public void FromClone(SerializeableStats myClone)
        {
            InitializeStats(myClone.stats.Length);
            if (myClone.stats != null)
            {
                for (int i = 0; i < myClone.stats.Length; i++)
                {
                    stats[i] = myClone.stats[i];
                }
            }
            else
            {
                stats = new BlitableArray<Staz>(0, Allocator.Persistent);
            }
            if (myClone.states != null)
            {
                InitializeStates(myClone.states.Length);
                for (int i = 0; i < myClone.states.Length; i++)
                {
                    states[i] = myClone.states[i];
                }
            }
            else
            {
                states = new BlitableArray<StateStaz>(0, Allocator.Persistent);
            }
            if (myClone.regens != null)
            {
                InitializeRegens(myClone.regens.Length);
                for (int i = 0; i < myClone.regens.Length; i++)
                {
                    regens[i] = myClone.regens[i];
                }
            }
            else
            {
                regens = new BlitableArray<RegenStaz>(0, Allocator.Persistent);
            }
            if (myClone.attributes != null)
            {
                InitializeAttributes(myClone.attributes.Length);
                for (int i = 0; i < myClone.attributes.Length; i++)
                {
                    attributes[i] = myClone.attributes[i];
                }
            }
            else
            {
                attributes = new BlitableArray<AttributeStaz>(0, Allocator.Persistent);
            }
            if (myClone.levels != null)
            {
                InitializeLevels(myClone.levels.Length);
                for (int i = 0; i < myClone.levels.Length; i++)
                {
                    levels[i] = myClone.levels[i];
                }
            }
            else
            {
                levels = new BlitableArray<Level>(0, Allocator.Persistent);
            }
        }
        #endregion

        #region EditorFunctions
        public int AddStat(StatDatam statDatam)
        {
            int indexOf = -1;
            if (statDatam.type == StatType.Base)
            {
                Staz[] priorStates = stats.ToArray();
                stats = new BlitableArray<Staz>(stats.Length + 1, Allocator.Persistent);
                for (int i = 0; i < priorStates.Length; i++)
                {
                    stats[i] = priorStates[i];
                }
                Staz newState = new Staz();
                newState.id = statDatam.Value.id;
                stats[priorStates.Length] = newState;
                indexOf = priorStates.Length;
            }
            else if (statDatam.type == StatType.State)
            {
                StateStaz[] priorStates = states.ToArray();
                states = new BlitableArray<StateStaz>(states.Length + 1, Allocator.Persistent);
                for (int i = 0; i < priorStates.Length; i++)
                {
                    states[i] = priorStates[i];
                }
                StateStaz newState = new StateStaz();
                newState.id = statDatam.Value.id;
                states[priorStates.Length] = newState;
                indexOf = priorStates.Length;
            }
            else if (statDatam.type == StatType.Regen)
            {
                RegenStaz[] priorStates = regens.ToArray();
                regens = new BlitableArray<RegenStaz>(regens.Length + 1, Allocator.Persistent);
                for (int i = 0; i < priorStates.Length; i++)
                {
                    regens[i] = priorStates[i];
                }
                RegenStaz newState = new RegenStaz();
                newState.id = statDatam.Value.id;
                newState.targetID = statDatam.targetStatID;
                regens[priorStates.Length] = newState;
                indexOf = priorStates.Length;
            }
            else if (statDatam.type == StatType.Attribute)
            {
                AttributeStaz[] priorStats = attributes.ToArray();
                attributes = new BlitableArray<AttributeStaz>(attributes.Length + 1, Allocator.Persistent);
                for (int i = 0; i < priorStats.Length; i++)
                {
                    attributes[i] = priorStats[i];
                }
                AttributeStaz newStat = new AttributeStaz();
                newStat.id = statDatam.Value.id;
                newStat.targetID = statDatam.targetStatID;
                attributes[priorStats.Length] = newStat;
                indexOf = priorStats.Length;
            }
            else if (statDatam.type == StatType.Level)
            {
                Level[] priorStats = levels.ToArray();
                levels = new BlitableArray<Level>(levels.Length + 1, Allocator.Persistent);
                for (int i = 0; i < priorStats.Length; i++)
                {
                    levels[i] = priorStats[i];
                }
                Level newStat = new Level();
                newStat.id = statDatam.Value.id;
                //newStat.targetID = statDatam.targetStatID;
                levels[priorStats.Length] = newStat;
                indexOf = priorStats.Length;
            }
            return indexOf;
        }

        public void RemoveStat(StatDatam statDatam, int indexOf)
        {
            if (statDatam.type == StatType.Base)
            {
                List<Staz> priorStates = new List<Staz>(stats.ToArray());
                stats = new BlitableArray<Staz>(stats.Length - 1, Allocator.Persistent);
                priorStates.RemoveAt(indexOf);
                for (int i = 0; i < priorStates.Count; i++)
                {
                    stats[i] = priorStates[i];
                }
            }
            else if (statDatam.type == StatType.State)
            {
                List<StateStaz> priorStates = new List<StateStaz>(states.ToArray());
                states = new BlitableArray<StateStaz>(states.Length - 1, Allocator.Persistent);
                priorStates.RemoveAt(indexOf);
                for (int i = 0; i < priorStates.Count; i++)
                {
                    states[i] = priorStates[i];
                }
            }
            else if (statDatam.type == StatType.Regen)
            {
                List<RegenStaz> priorStates = new List<RegenStaz>(regens.ToArray());
                regens = new BlitableArray<RegenStaz>(regens.Length - 1, Allocator.Persistent);
                priorStates.RemoveAt(indexOf);
                for (int i = 0; i < priorStates.Count; i++)
                {
                    regens[i] = priorStates[i];
                }
            }
            else if (statDatam.type == StatType.Attribute)
            {
                List<AttributeStaz> priorStats = new List<AttributeStaz>(attributes.ToArray());
                attributes = new BlitableArray<AttributeStaz>(attributes.Length - 1, Allocator.Persistent);
                priorStats.RemoveAt(indexOf);
                for (int i = 0; i < priorStats.Count; i++)
                {
                    attributes[i] = priorStats[i];
                }
            }
            else if (statDatam.type == StatType.Level)
            {
                List<Level> priorStats = new List<Level>(levels.ToArray());
                levels = new BlitableArray<Level>(levels.Length - 1, Allocator.Persistent);
                priorStats.RemoveAt(indexOf);
                for (int i = 0; i < priorStats.Count; i++)
                {
                    levels[i] = priorStats[i];
                }
            }
        }
        public int GetStatIndex(StatDatam statDatam)
        {
            if (statDatam.type == StatType.Base)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    if (stats[i].id == statDatam.Value.id)
                    {
                        return i;
                    }
                }
            }
            else if (statDatam.type == StatType.Regen)
            {
                for (int i = 0; i < regens.Length; i++)
                {
                    if (regens[i].id == statDatam.Value.id)
                    {
                        return i;
                    }
                }
            }
            else if (statDatam.type == StatType.State)
            {
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i].id == statDatam.Value.id)
                    {
                        return i;
                    }
                }
            }
            else if (statDatam.type == StatType.Attribute)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].id == statDatam.Value.id)
                    {
                        return i;
                    }
                }
            }
            else if (statDatam.type == StatType.Level)
            {
                for (int i = 0; i < levels.Length; i++)
                {
                    if (levels[i].id == statDatam.Value.id)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void SetStatValue(int indexOf, float newValue)
        {
            Staz stat = stats[indexOf];
            stat.value = newValue;
            stats[indexOf] = stat;
        }
        public void SetStateValue(int indexOf, float newValue)
        {
            StateStaz stat = states[indexOf];
            stat.value = newValue;
            states[indexOf] = stat;
        }
        public void SetStateMaxValue(int indexOf, float newValue)
        {
            StateStaz stat = states[indexOf];
            stat.maxValue = newValue;
            states[indexOf] = stat;
            
        }
        public void SetRegenValue(int indexOf, float newValue)
        {
            RegenStaz stat = regens[indexOf];
            stat.value = newValue;
            regens[indexOf] = stat;
        }
        public void SetRegenRate(int indexOf, float newValue)
        {
            RegenStaz stat = regens[indexOf];
            stat.rate = newValue;
            regens[indexOf] = stat;
        }
        public void SetAttributeValue(int indexOf, float newValue)
        {
            AttributeStaz stat = attributes[indexOf];
            stat.value = newValue;
            attributes[indexOf] = stat;
        }
        public void SetAttributeMultiplier(int indexOf, float newValue)
        {
            AttributeStaz stat = attributes[indexOf];
            stat.multiplier = newValue;
            attributes[indexOf] = stat;
        }

        public void SetLevelValue(int indexOf, int newValue)
        {
            Level stat = levels[indexOf];
            stat.value = newValue;
            levels[indexOf] = stat;
        }
        public void SetLevelExperienceRequired(int indexOf, int newValue)
        {
            Level stat = levels[indexOf];
            stat.experienceRequired = newValue;
            levels[indexOf] = stat;
        }
        #endregion

        // others
        //public float goldDropped;
        // leveling up
        /*#region Testing
        public static Stats GenerateBasicStats()
        {
            Stats stats = new Stats
            {
                stats = new BlitableArray<Staz>(5, Allocator.Persistent),
                states = new BlitableArray<StateStaz>(2, Allocator.Persistent),
                regens = new BlitableArray<RegenStaz>(1, Allocator.Persistent)

            };
            Staz level = new Staz();
            //level.SetName("Level");
            level.value = 1;
            stats.stats[StatsIndexes.level] = level;
            Staz attackRange = new Staz();
            //attackRange.SetName("Attack Range");
            attackRange.value = 6;
            stats.stats[StatsIndexes.attackRange] = attackRange;
            Staz attackDamage = new Staz();
            //attackDamage.SetName("Attack Damage");
            attackDamage.value = 1;
            stats.stats[StatsIndexes.attackDamage] = attackDamage;
            Staz attackSpeed = new Staz();
            //attackSpeed.SetName("Attack Speed");
            attackSpeed.value = 1f;
            stats.stats[StatsIndexes.attackSpeed] = attackSpeed;
            Staz attackForce = new Staz();
            //attackForce.SetName("Attack Force");
            attackForce.value = 1;
            stats.stats[StatsIndexes.attackForce] = attackForce;
            StateStaz health = new StateStaz();
            //health.SetName("Health");
            health.maxValue = 6;
            health.value = health.maxValue;
            stats.states[StatsIndexes.health] = health;
            StateStaz experience = new StateStaz();
            //experience.SetName("Experience");
            experience.value = 0;
            experience.maxValue = 10;
            stats.states[StatsIndexes.experience] = experience;
            RegenStaz healthRegen = new RegenStaz();
            //healthRegen.SetName("HealthRegen");
            healthRegen.value = 0.01f;
            healthRegen.targetID = StatsIndexes.health; // index of updated stat
            stats.regens[StatsIndexes.healthRegen] = healthRegen;
            return stats;
        }
        #endregion*/

    }
}