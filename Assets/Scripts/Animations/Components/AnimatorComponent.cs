using Unity.Entities;
using System;

namespace Zoxel.Animations
{
    public class AnimatorComponent : ComponentDataProxy<Animator> { }

    // finally the animator component i need to add to any monsters with animator data (timmy)
    [Serializable]
    public struct Animator : IComponentData
    {
        // world spawned in!
        public byte isWalking;
        public byte didUpdate;

        public BlitableArray<AnimationData> data;
    }
}
