using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    public class BodyComponent : ComponentDataProxy<Body> { }

    [System.Serializable]
    public struct Body : IComponentData
    {
        public float3 size;
    }
}
