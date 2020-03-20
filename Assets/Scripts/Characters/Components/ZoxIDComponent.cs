using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Zoxel
{
    public class ZoxIDComponent : ComponentDataProxy<ZoxID> { }

    [System.Serializable]
    public struct ZoxID : IComponentData
    {
        public int id;
        public int clanID;
        public int creatorID;
        public override bool Equals(object obj)
        {
            if (!(obj is ZoxID))
                return false;
            ZoxID e = (ZoxID)obj;
            return e.id == id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}