using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Zoxel
{

    [DisableAutoCreation]
    public class TrailerStarterSystem : ComponentSystem
    {
        public CharacterSpawnSystem characterSpawnSystem;

        protected override void OnUpdate()
        {
            Entities.WithAll<UITrailer, ZoxID>().ForEach((Entity littlebitchEntity, ref UITrailer trailer, ref ZoxID zoxID) =>
            {
                if (characterSpawnSystem.characters.ContainsKey(zoxID.id))
                {
                    Translation characterPosition = World.EntityManager.GetComponentData<Translation>(characterSpawnSystem.characters[zoxID.id]);
                    trailer.position = characterPosition.Value;
                }
            });
        }
    }
}