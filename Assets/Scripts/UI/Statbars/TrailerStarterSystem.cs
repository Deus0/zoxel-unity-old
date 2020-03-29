using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

namespace Zoxel
{

    [DisableAutoCreation]
    public class TrailerStarterSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<UITrailer>().ForEach((Entity e, ref UITrailer trailer) =>
            {
                //if (characterSpawnSystem.characters.ContainsKey(zoxID.id))
                if (World.EntityManager.Exists(trailer.character))
                {
                    Translation characterPosition = World.EntityManager.GetComponentData<Translation>(trailer.character);
                    trailer.position = characterPosition.Value;
                    //Debug.DrawLine(trailer.position, trailer.position + new float3(0, trailer.heightAddition, 0), Color.green);
                }
                //else {
                //    Debug.LogError("Remove this guy.");
                //}
            });
        }
    }
}