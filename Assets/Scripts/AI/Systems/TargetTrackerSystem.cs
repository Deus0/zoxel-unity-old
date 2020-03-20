using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections;
using System.Collections.Generic;

namespace Zoxel
{
    [DisableAutoCreation]
    public class TargetTrackerSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            // and keep positions up to date
            Entities.WithAll<Targeter>().ForEach((Entity e, ref Targeter targeter) =>
            {
                if (targeter.hasTarget == 1 && World.EntityManager.Exists(targeter.nearbyCharacter.character))
                {
                    targeter.nearbyCharacter.position =
                        World.EntityManager.GetComponentData<Translation>(targeter.nearbyCharacter.character).Value;
                    targeter.nearbyCharacter.distance = math.distance(targeter.nearbyCharacter.position,
                        World.EntityManager.GetComponentData<Translation>(e).Value);
                }
            });
        }

    }
}