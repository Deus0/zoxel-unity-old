using Unity.Entities;

namespace Zoxel
{
    /// <summary>
    /// Rename to LevelUpCompleterSystem
    ///     StatsUI
    ///     ParticleEffects
    ///     PlaySound
    /// </summary>
    [DisableAutoCreation]
    public class LevelUpEffectsSystem : ComponentSystem
    {
        public StatsUISpawnSystem statsUISpawnSystem;
        protected override void OnUpdate()
        {
            Entities.WithAll<Stats, ZoxID>().ForEach((Entity e, ref Stats stats, ref ZoxID zoxID) =>
            {
                if (stats.leveledUpEffects == 1)
                {
                    stats.leveledUpEffects = 0;
                    // should have index of what leveled up!
                    if (stats.levels.Length > 0)
                    {
                        ParticlesManager.instance.PlayParticles(new ParticleSpawnCommand { name = "LevelUp", life = 3 }, e, World.EntityManager);
                        //UnityEngine.Debug.Log("Leveling Up for character: " + stats.id + " to level " + level.value);
                        Level level = stats.levels[0];
                        //UpdateStatUI(level, zoxID.id);
                        StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, e, StatType.Level, 0);
                        StatsUISpawnSystem.OnUpdatedStat(World.EntityManager, e, StatType.Base, 0);
                        //statsUISpawnSystem.SetText(zoxID.id, level);
                        //statsUISpawnSystem.SetText(zoxID.id, stats.stats[0]);
                        // play level up sound
                    }
                }
            });
        }

        /*protected void UpdateStatUI(StateStaz stat, int arrayIndex, int characterID, )
        {
            // they should just use render text instead of list of entities lol
            // should really just StatUISystem->Get characterID UI->GetStatText
            if (Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs.ContainsKey(characterID))
            {
                CharacterStatUIData icons = Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs[characterID];
                StatIcon icon = icons.icons[stat.id];
                icon.digits = Bootstrap.instance.systemsManager.UIUtilities.UpdateNumbers(icon.digits, icon.icon, ((int)stat.value).ToString());
                icons.icons[stat.id] = icon;
                Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs[characterID] = icons;
            }
        }

        protected void UpdateStatUI(Staz stat, int characterID)
        {
            if (Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs.ContainsKey(characterID))
            {
                // they should just use render text instead of list of entities lol
                // should really just StatUISystem->Get characterID UI->GetStatText
                CharacterStatUIData icons = Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs[characterID];
                StatIcon icon = icons.icons[stat.id];
                icon.digits = Bootstrap.instance.systemsManager.UIUtilities.UpdateNumbers(icon.digits, icon.icon, ((int)stat.value).ToString());
                icons.icons[stat.id] = icon;
                Bootstrap.instance.systemsManager.statsUISpawnSystem.characterStatUIs[characterID] = icons;
            }
        }*/
    }
}
