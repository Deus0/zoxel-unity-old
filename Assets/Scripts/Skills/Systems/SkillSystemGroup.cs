using Unity.Entities;

namespace Zoxel
{


    [UpdateAfter(typeof(CameraSystemGroup))]
    public class SkillSystemGroup : ComponentSystemGroup
    {
        public DamageSystem damageSystem;
        public SkillsSystem skillsSystem;
        private MeleeDamageSystem meleeDamageSystem;
        private MeleeCompleterSystem meleeCompleterSystem;
        private ShootSystem shootSystem;
        private ShootCompleterSystem shootCompleterSystem;

        public void Initialize(Unity.Entities.World space)
        {
            skillsSystem = space.GetOrCreateSystem<SkillsSystem>();
            damageSystem = space.GetOrCreateSystem<DamageSystem>();
            shootSystem = space.GetOrCreateSystem<ShootSystem>();
            shootCompleterSystem = space.GetOrCreateSystem<ShootCompleterSystem>();
            meleeDamageSystem = space.GetOrCreateSystem<MeleeDamageSystem>();
            meleeCompleterSystem = space.GetOrCreateSystem<MeleeCompleterSystem>();
            AddSystemToUpdateList(skillsSystem);
            AddSystemToUpdateList(damageSystem);
            AddSystemToUpdateList(shootSystem);
            AddSystemToUpdateList(shootCompleterSystem);
            AddSystemToUpdateList(meleeDamageSystem);
            AddSystemToUpdateList(meleeCompleterSystem);
            SetLinks();
        }


        void SetLinks()
        {

        }

        public void Clear()
        {

        }
        public void SetMeta(GameDatam data)
        {
            skillsSystem.meta = data.GetSkills();
        }
        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            skillsSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            meleeCompleterSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            damageSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }
        public void CombineWithUI(UISystemGroup uiSystemGroup)
        {
            damageSystem.statbarSystem = uiSystemGroup.statbarSystem;
            damageSystem.damagePopupSystem = uiSystemGroup.damagePopupSystem;
            damageSystem.statsUISpawnSystem = uiSystemGroup.statsUISpawnSystem;
            skillsSystem.actionbarSpawnSystem = uiSystemGroup.actionbarSpawnSystem;
        }

        public void CombineWithBullets(BulletSystemGroup bulletSystemGroup)
        {

            shootCompleterSystem.bulletSpawnSystem = bulletSystemGroup.bulletSpawnSystem;
        }
    }
}
