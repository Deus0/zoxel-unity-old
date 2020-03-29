using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public class PortalCameraComponent : ComponentDataProxy<PortalCamera> { }

    [System.Serializable]
    public struct PortalCamera : IComponentData
    {
        public Entity portal;
    }
}