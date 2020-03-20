using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Zoxel
{

    [DisableAutoCreation]
    public class MeleeCompleterSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;
        protected override void OnUpdate()
        {
            Entities.WithAll<MeleeAttack, ZoxID, Targeter>().ForEach((Entity e, ref MeleeAttack hitter, ref ZoxID zoxID, ref Targeter targeter) =>
            {
                if (hitter.didHit == 1)
                {
                    hitter.didHit = 0;
                    DamageSystem.AddDamage(World.EntityManager, e, targeter.nearbyCharacter.character, 0, hitter.attackDamage);
                }
            });
        }
    }
}