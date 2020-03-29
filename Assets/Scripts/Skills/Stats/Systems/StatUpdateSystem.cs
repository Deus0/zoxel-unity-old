using Unity.Entities;

namespace Zoxel
{
    /*public enum StatType
    { 
        Base,
        State,
        Regen,
        Attribute,
        Level
    }*/
    // another system should check if stat has updated, then update the UI
    // or create a command for stat updated, so the UI will be updated
    // the idea is to create unlinked systems - decoupled

    [DisableAutoCreation]
    public class StatUpdateSystem : ComponentSystem
    {

        public struct UpdateAttributeCommand : IComponentData
        {
            public Entity character;
            public byte statType;
            public byte statIndex;
            public int amount;             // make sure to check statpoints available
            // attribute meta id - later when i start removing and adding attributes
        }

        public static void UpdateStat(EntityManager EntityManager, Entity character, StatType statType, int attributeIndex, int amount)
        {
            Entity e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new UpdateAttributeCommand
            {
                character = character,
                statType = (byte) statType,
                statIndex = (byte) attributeIndex,
                amount = amount
            });
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<UpdateAttributeCommand>().ForEach((Entity e, ref UpdateAttributeCommand command) =>
            {
                //if (characterSpawnSystem.characters.ContainsKey(command.characterID))
                if (World.EntityManager.Exists(command.character))
                {
                    //Entity character = characterSpawnSystem.characters[command.characterID];
                    Stats stats = World.EntityManager.GetComponentData<Stats>(command.character);
                    int statIndex = (int)(command.statIndex);
                    if ((StatType)command.statType == StatType.Attribute)
                    {
                        if (statIndex >= 0 && statIndex < stats.attributes.Length)
                        {
                            AttributeStaz attribute = stats.attributes[statIndex];
                            attribute.value += command.amount;
                            stats.attributes[statIndex] = attribute;
                            stats.attributesApplied = 0;
                            World.EntityManager.SetComponentData(command.character, stats);
                            StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, command.character, (StatType)command.statType, command.statIndex);
                        }
                    }
                    else if ((StatType)command.statType == StatType.Base)
                    {
                        if (statIndex >= 0 && statIndex < stats.stats.Length)
                        {
                            Staz stat = stats.stats[statIndex];
                            stat.value += command.amount;
                            stats.stats[statIndex] = stat;
                            World.EntityManager.SetComponentData(command.character, stats);
                            StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, command.character, (StatType)command.statType, command.statIndex);
                        }
                    }
                    // StatUISystem.OnUpdated(command.characterIndex, attribute);
                }
                World.EntityManager.DestroyEntity(e);
                //SetText(characterID, originalArrayIndex, (int)improveStat.value);
            });
        }
    }
}

// These are disabled from updating
/*Staz improveStat = stats.stats[arrayIndex];
improveStat.value++;
stats.stats[arrayIndex] = improveStat;
UpdateCharacterStatUI(characterID, improveStat.id, (int)improveStat.value);*/
/*StateStaz improveStat = stats.states[arrayIndex];
improveStat.value += 5;
improveStat.maxValue += 5;
stats.states[arrayIndex] = improveStat;
SetText(characterID, originalArrayIndex, (int)improveStat.value);*/
/*RegenStaz improveStat = stats.regens[arrayIndex];
improveStat.value += 3;
stats.regens[arrayIndex] = improveStat;
SetText(characterID, originalArrayIndex, (int)improveStat.value);*/
