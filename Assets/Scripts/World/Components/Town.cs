using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{
    [System.Serializable]
    public struct Building
    {
        public float3 position;
        public float3 dimensions;
        public int characterID;
    }
    [System.Serializable]
    public struct Town
    {
        public float3 position;
        public float3 dimensions;
        public float wallThickness;
    }
    [System.Serializable]
    public struct ChunkTown : IComponentData
    {
        /*public float3 centrePosition;
        public float3 wallSize;
        public float wallThickness;*/
        public BlitableArray<Town> towns;
        public BlitableArray<Building> buildings;

        public bool IsPointInsideOf(float3 point)
        {
            //return false;
            /*UnityEngine.Debug.LogError("Point: " + point + " --- " + centrePosition + " --- " + wallSize + " --- " + wallThickness + ": " +
                ((point.x >= centrePosition.x - wallSize.x - wallThickness &&
                point.x <= centrePosition.x + wallSize.x + wallThickness &&
                point.z >= centrePosition.z - wallSize.z - wallThickness &&
                point.z <= centrePosition.z + wallSize.z + wallThickness)));*/
            for (int i = 0; i < towns.Length; i++)
            {
                if (point.x >= towns[i].position.x - towns[i].dimensions.x - towns[i].wallThickness &&
                    point.x <= towns[i].position.x + towns[i].dimensions.x + towns[i].wallThickness &&
                    point.z >= towns[i].position.z - towns[i].dimensions.z - towns[i].wallThickness &&
                    point.z <= towns[i].position.z + towns[i].dimensions.z + towns[i].wallThickness)
                    return true;
            }
            return false;
        }

        /*public bool IsPointInsideOfWalls(float3 point)
        {
            return (point.x >= centrePosition.x - wallSize.x + wallThickness + 1&&
                point.x <= centrePosition.x + wallSize.x - wallThickness - 1 &&
                point.z >= centrePosition.z - wallSize.z + wallThickness + 1 &&
                point.z <= centrePosition.z + wallSize.z - wallThickness - 1);
        }*/
    }

}
