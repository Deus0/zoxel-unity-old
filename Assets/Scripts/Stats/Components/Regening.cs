using Unity.Entities;
using Unity.Collections;

namespace Zoxel
{
    public struct Regening : IComponentData
    {
        public byte finished;
        public BlitableArray<byte> stateUpdated;    // has state updated
        public BlitableArray<byte> stateMaxed;

        public void Initialize(int stateslength)
        {
            Dispose();
            stateUpdated = new BlitableArray<byte>(stateslength, Allocator.Persistent);
            stateMaxed = new BlitableArray<byte>(stateslength, Allocator.Persistent);
        }
        public void Dispose()
        {
            if (stateUpdated.Length > 0)
            {
                stateUpdated.Dispose();
                stateMaxed.Dispose();
            }
        }
    }
}