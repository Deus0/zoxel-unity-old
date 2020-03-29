using Unity.Entities;
using Zoxel.UI;

namespace Zoxel
{
    public class StatSystemGroup : ComponentSystemGroup
    {
        //private RegenStartSystem regenStartSystem;
        private RegenSystem regenSystem;
        private RegenCompleterSystem regenCompleterSystem;
        private LevelUpSystem levelUpSystem;
        private LevelUpEffectsSystem levelUpEffectsSystem;
        private AttributesSystem attributesSystem;
        private StatUpdateSystem statUpdateSystem;

        public void Initialize(Unity.Entities.World space)
        {
            //regenStartSystem = space.GetOrCreateSystem<RegenStartSystem>();
            regenSystem = space.GetOrCreateSystem<RegenSystem>();
            regenCompleterSystem = space.GetOrCreateSystem<RegenCompleterSystem>();
            levelUpSystem = space.GetOrCreateSystem<LevelUpSystem>();
            levelUpEffectsSystem = space.GetOrCreateSystem<LevelUpEffectsSystem>();
            attributesSystem = space.GetOrCreateSystem<AttributesSystem>();
            //AddSystemToUpdateList(regenStartSystem);
            AddSystemToUpdateList(regenSystem);
            AddSystemToUpdateList(regenCompleterSystem);
            AddSystemToUpdateList(levelUpSystem);
            AddSystemToUpdateList(levelUpEffectsSystem);
            AddSystemToUpdateList(attributesSystem);
            statUpdateSystem = space.GetOrCreateSystem<StatUpdateSystem>();
            AddSystemToUpdateList(statUpdateSystem);
            SetLinks();
        }
        void SetLinks()
        {

        }

        public void Clear()
        {

        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {

        }

        public void CombineWithUI(UISystemGroup uiSystemGroup)
        {
            //regenCompleterSystem.statsUISpawnSystem = uiSystemGroup.statsUISpawnSystem;
            levelUpEffectsSystem.statsUISpawnSystem = uiSystemGroup.statsUISpawnSystem;
        }

    }
}

