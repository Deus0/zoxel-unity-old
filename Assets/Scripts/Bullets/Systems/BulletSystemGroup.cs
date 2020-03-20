using Unity.Entities;

namespace Zoxel
{

    public class BulletSystemGroup : ComponentSystemGroup
    {
        public BulletSpawnSystem bulletSpawnSystem;
        private BulletHitSystem bulletDamageSystem;
        private BulletHitCompleterSystem bulletHitCompleterSystem;
        private BulletDeathSystem bulletDeathSystem;
        private BulletShrinkSystem bulletShrinkSystem;

        public void Initialize(Unity.Entities.World space)
        {
            bulletSpawnSystem = space.GetOrCreateSystem<BulletSpawnSystem>();
            bulletDamageSystem = space.GetOrCreateSystem<BulletHitSystem>();
            bulletHitCompleterSystem = space.GetOrCreateSystem<BulletHitCompleterSystem>();
            bulletDeathSystem = space.GetOrCreateSystem<BulletDeathSystem>();
            bulletShrinkSystem = space.GetOrCreateSystem<BulletShrinkSystem>();
            AddSystemToUpdateList(bulletSpawnSystem);
            AddSystemToUpdateList(bulletDamageSystem);
            AddSystemToUpdateList(bulletHitCompleterSystem);
            AddSystemToUpdateList(bulletDeathSystem);
            AddSystemToUpdateList(bulletShrinkSystem);
            SetLinks();
        }
        void SetLinks()
        {
            bulletDamageSystem.bulletDeathSystem = bulletDeathSystem;
            bulletDeathSystem.bulletSpawnSystem = bulletSpawnSystem;
            bulletDamageSystem.bulletSpawnSystem = bulletSpawnSystem;
            bulletHitCompleterSystem.bulletDeathSystem = bulletDeathSystem;
            bulletHitCompleterSystem.bulletSpawnSystem = bulletSpawnSystem;
        }
        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            bulletHitCompleterSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }

        public void CombineWithSkills(SkillSystemGroup skillSystemGroup)
        {
            bulletHitCompleterSystem.DamageSystem = skillSystemGroup.damageSystem;
        }
        public void CombineWithStats(StatSystemGroup statSystemGroup)
        {
            //bulletHitCompleterSystem.DamageSystem = statSystemGroup.damageSystem;
        }

        public void Clear()
        {
            bulletSpawnSystem.Clear();
            bulletSpawnSystem.Clear();
        }
        public void SetMeta(GameDatam data)
        {
            bulletSpawnSystem.meta = data.GetBullets();
        }


    }
}
