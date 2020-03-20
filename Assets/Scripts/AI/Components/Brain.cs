using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Zoxel
{
    /// <summary>
    /// Responsible for current monster actions
    /// </summary>
    public struct Brain : IComponentData
    {
        public int state;   // 0 is wander
    }

}