using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class BulletComponent : ComponentDataProxy<Bullet> { }

    [System.Serializable]
    public struct Bullet : IComponentData
    {
        public float damage;
        public float timeStarted;
        public float2 lifetime;
        public int metaID;
        //public int id;
        //public int clanID;

    }
}