using Unity.Entities;
using System.Collections;
using System.Collections.Generic;

namespace Zoxel {
    
        public struct WorldStreamCommand : IComponentData
        {
            // data for new and old chunks
            public Entity world;
            public byte isModel;
            public BlitableArray<int> newIDs;
            public BlitableArray<byte> newRenders;
            public BlitableArray<int> oldIDs;

            public void SetIDs(List<int> worldsChunkIDs, List<int> oldChunkIDs)
            {
                newIDs = new BlitableArray<int>(worldsChunkIDs.Count, Unity.Collections.Allocator.Persistent);
                for (int i = 0; i < newIDs.Length; i++)
                {
                    newIDs[i] = worldsChunkIDs[i];
                }
                oldIDs = new BlitableArray<int>(oldChunkIDs.Count, Unity.Collections.Allocator.Persistent);
                for (int i = 0; i < oldIDs.Length; i++)
                {
                    oldIDs[i] = oldChunkIDs[i];
                }
            }
            public void SetRenders(Dictionary<int, bool> allRenders)
            {
                newRenders = new BlitableArray<byte>(newIDs.Length, Unity.Collections.Allocator.Persistent);
                for (int i = 0; i < newRenders.Length; i++)
                {
                    if (allRenders.ContainsKey(newIDs[i]))
                    {
                        if (allRenders[newIDs[i]])
                        {
                            newRenders[i] = 1;
                        }
                    }
                }
            }

            public void Dispose()
            {
                newIDs.Dispose();
                newRenders.Dispose();
                oldIDs.Dispose();
            }
        }
}