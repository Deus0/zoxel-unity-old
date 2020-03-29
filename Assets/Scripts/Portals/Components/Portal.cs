using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public class PortalComponent : ComponentDataProxy<Portal> { }

    [System.Serializable]
    public struct Portal : IComponentData
    {
        public int id;
        public Entity linkedPortal;
        public byte hasCamera;
    }
}
