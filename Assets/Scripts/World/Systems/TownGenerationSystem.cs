using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Zoxel.Voxels;

namespace Zoxel.WorldGeneration
{
    [DisableAutoCreation]
    public class TownGenerationSystem : JobComponentSystem
    {
        [BurstCompile]
        struct TownGenerationJob : IJobForEach<WorldGenerationChunk, Chunk, ChunkTerrain, ChunkTown>
        {
            public void Execute(ref WorldGenerationChunk worldGenerationChunk, ref Chunk chunk, ref ChunkTerrain chunkTerrain, ref ChunkTown chunkTown)
            {
                if (worldGenerationChunk.state == 4)
                {
                    worldGenerationChunk.state = 5;
                    int voxelIndex = 0;
                    float3 position = float3.zero;
                    int positionXZ = 0;
                    int height = 0;
                    float3 localPosition = float3.zero;
                    float3 voxelWorldPosition = new float3(
                        chunk.Value.chunkPosition.x * chunk.Value.voxelDimensions.x,
                        chunk.Value.chunkPosition.y * chunk.Value.voxelDimensions.y,
                        chunk.Value.chunkPosition.z * chunk.Value.voxelDimensions.z);

                    for (int i = 0; i < chunkTown.towns.Length; i++)
                    {
                        for (position.x = voxelWorldPosition.x; position.x < voxelWorldPosition.x + chunk.Value.voxelDimensions.x; position.x++)
                        {
                            for (position.y = voxelWorldPosition.y; position.y < voxelWorldPosition.y + chunk.Value.voxelDimensions.y; position.y++)
                            {
                                for (position.z = voxelWorldPosition.z; position.z < voxelWorldPosition.z + chunk.Value.voxelDimensions.z; position.z++)
                                {
                                    localPosition = new float3(position.x - voxelWorldPosition.x,
                                        position.y - voxelWorldPosition.y, position.z - voxelWorldPosition.z);
                                    // if above terrain height, but below 
                                    positionXZ = (int)(localPosition.x * chunk.Value.voxelDimensions.z + localPosition.z);

                                    // if within town range - flatten out town a bit towards the middle

                                    height = chunkTerrain.heights[positionXZ];
                                    // how to smooth here.. use height map there to smooth
                                    BuildTownFloor(chunkTown.towns[i], ref chunk, ref chunkTerrain, height, position, voxelIndex);
                                    BuildWall(chunkTown.towns[i], ref chunk, ref chunkTerrain, height, position, voxelIndex);

                                    // building generation
                                    //BuildTownHall(ref chunk, ref chunkTown, ref chunkTerrain, height, position, voxelIndex);
                                    voxelIndex++;
                                }
                            }
                        }
                    }

                    // use average floor of building
                    for (int i = 0; i < chunkTown.buildings.Length; i++)
                    {
                        //float buildingHeightTotal = 0;
                        //float buildingVoxelsTotal = 0;
                        float maxFloorHeight = 0;
                        voxelIndex = 0;
                        for (position.x = voxelWorldPosition.x; position.x < voxelWorldPosition.x + chunk.Value.voxelDimensions.x; position.x++)
                        {
                            for (position.y = voxelWorldPosition.y; position.y < voxelWorldPosition.y + chunk.Value.voxelDimensions.y; position.y++)
                            {
                                for (position.z = voxelWorldPosition.z; position.z < voxelWorldPosition.z + chunk.Value.voxelDimensions.z; position.z++)
                                {
                                    localPosition = new float3(position.x - voxelWorldPosition.x, position.y - voxelWorldPosition.y, position.z - voxelWorldPosition.z);
                                    positionXZ = (int)(localPosition.x * chunk.Value.voxelDimensions.z + localPosition.z);
                                    height = chunkTerrain.heights[positionXZ];
                                    if (IsInBuildingXZ(chunkTown.buildings[i], position))
                                    {
                                        //buildingHeightTotal += height;
                                        //buildingVoxelsTotal++;
                                        if (height > maxFloorHeight)
                                        {
                                            maxFloorHeight = height;
                                        }
                                    }
                                    voxelIndex++;
                                }
                            }
                        }
                        Building building = chunkTown.buildings[i];
                        building.position = new float3(chunkTown.buildings[i].position.x,
                             maxFloorHeight + 1, chunkTown.buildings[i].position.z);
                        chunkTown.buildings[i] = building;
                        /*float buildingHeight = 0;
                        if (buildingVoxelsTotal != 0)
                        {
                            buildingHeight = (int)math.floor(buildingHeightTotal / buildingVoxelsTotal);
                        }*/
                        voxelIndex = 0;
                        for (position.x = voxelWorldPosition.x; position.x < voxelWorldPosition.x + chunk.Value.voxelDimensions.x; position.x++)
                        {
                            for (position.y = voxelWorldPosition.y; position.y < voxelWorldPosition.y + chunk.Value.voxelDimensions.y; position.y++)
                            {
                                for (position.z = voxelWorldPosition.z; position.z < voxelWorldPosition.z + chunk.Value.voxelDimensions.z; position.z++)
                                {
                                    BuildBuilding(ref chunk, chunkTown.buildings[i], ref chunkTerrain, maxFloorHeight, position, voxelIndex);
                                    voxelIndex++;
                                }
                            }
                        }
                    }
                    chunk.isMapDirty = 1;
                }
            }

            private bool IsInBuildingXZ(Building building, float3 position)
            {
                /*return (((position.x == -building.dimensions.x && position.z >= -building.dimensions.z && position.z <= building.dimensions.z)
                    || (position.x == building.dimensions.x && position.z >= -building.dimensions.z && position.z <= building.dimensions.z)
                    || (position.z == -building.dimensions.z && position.x >= -building.dimensions.x && position.x <= building.dimensions.x)
                    || (position.z == building.dimensions.z && position.x >= -building.dimensions.x && position.x <= building.dimensions.x)));*/
                return (((position.z >= building.position.z -building.dimensions.z && position.z <= building.position.z + building.dimensions.z)
                    || (position.z >= building.position.z - building.dimensions.z && position.z <= building.position.z + building.dimensions.z)
                    || (position.x >= building.position.x - building.dimensions.x && position.x <= building.position.x + building.dimensions.x)
                    || (position.x >= building.position.x - building.dimensions.x && position.x <= building.position.x + building.dimensions.x)));
            }

            private void BuildBuilding(ref Chunk chunk, Building building, ref ChunkTerrain chunkTerrain,
                float height, float3 position, int voxelIndex)
            {
                int houseHeight = (int)building.dimensions.y;
                int houseWidth = (int)building.dimensions.x;
                int houseDepth = (int)building.dimensions.z;
               // position += building.position;
               // walls
                if (position.y > height && position.y < height + houseHeight
                    && ((position.x == building.position.x - houseWidth && position.z >= building.position.z - houseDepth && position.z <= building.position.z + houseDepth)
                    || (position.x == building.position.x + houseWidth && position.z >= building.position.z - houseDepth && position.z <= building.position.z + houseDepth)
                    || (position.z == building.position.z - houseDepth && position.x >= building.position.x - houseWidth && position.x <= building.position.x + houseWidth)
                    || (position.z == building.position.z + houseDepth && position.x >= building.position.x - houseWidth && position.x <= building.position.x + houseWidth)))
                {
                    if (position.z == building.position.z + houseDepth && position.x >= building.position.x - 1 && position.x <= building.position.x + 1)
                    {
                        chunk.Value.voxels[voxelIndex] = (byte)(0); // door
                    }
                    else
                    {
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].wallID);
                    }
                }
                // roof
                else if ((position.y == height + houseHeight)
                   && (position.x >= building.position.x - houseWidth && position.x <= building.position.x + houseWidth
                   && position.z >= building.position.z - houseDepth && position.z <= building.position.z + houseDepth))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].roofID);
                }
                // floor
                else if ((position.y == height)
                   && (position.x >= building.position.x - houseWidth && position.x <= building.position.x + houseWidth
                   && position.z >= building.position.z - houseDepth && position.z <= building.position.z + houseDepth))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].floorID);
                }
                else if ((position.y > height && position.y < height + houseHeight)
                   && (position.x > building.position.x - houseWidth && position.x < building.position.x + houseWidth
                   && position.z > building.position.z - houseDepth && position.z < building.position.z + houseDepth))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(0);
                }
            }

            private void BuildTownFloor(Town town, ref Chunk chunk, ref ChunkTerrain chunkTerrain,
                float height, float3 position, int voxelIndex)
            {
                if (IsOnTownFloor(height, position, town.dimensions, town.wallThickness))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].stoneID);
                }
            }
            private bool IsOnTownFloor(float height, float3 position, float3 wallSize, float wallThickness)
            {
                return position.y == height // + 1
                    && position.x > -wallSize.x + wallThickness
                    && position.x < wallSize.x - wallThickness
                    && position.z < wallSize.z - wallThickness
                    && position.z > -wallSize.z + wallThickness
                    && noise.cellular2x2x2(position).x * 100 >= 32; //UnityEngine.Random.Range(0, 100) >= 40;
            }

            private void BuildWall(Town town, ref Chunk chunk, ref ChunkTerrain chunkTerrain,
                float height, float3 position, int voxelIndex)
            {
                if (IsPositionInsideWall(town, position, height))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].dirtID);
                }
                if (IsPositionOnOuterWall(town, position, height))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biomes[0].stoneID);
                }
                if (IsPositionInsideGate(town, position, height))
                {
                    chunk.Value.voxels[voxelIndex] = (byte)(0);
                }
            }
            private bool IsPositionInsideGate(Town town, float3 position, float height)
            {
                return position.y > height && position.y < height + town.dimensions.y - 2
                    && (IsZPlusWallInsides(position, new float3(4, 0, town.dimensions.z),
                    town.wallThickness + 1));
            }

            private bool IsPositionInsideWall(Town town, float3 position, float height)
            {
                return position.y > height && position.y < height + town.dimensions.y - 1
                    && (IsZPlusWallInsides(position, town.dimensions, town.wallThickness)
                    || IsZMinusWallInsides(position, town.dimensions, town.wallThickness)
                    || IsXPlusWallInsides(position, town.dimensions, town.wallThickness)
                    || IsXMinusWallInsides(position, town.dimensions, town.wallThickness));
            }

            private bool IsZPlusWallInsides(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x > -wallSize.x - wallThickness
                    && position.x < wallSize.x + wallThickness
                    && position.z > wallSize.z - wallThickness
                    && position.z < wallSize.z + wallThickness);
            }
            private bool IsZMinusWallInsides(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x > -wallSize.x - wallThickness
                    && position.x < wallSize.x + wallThickness
                    && position.z > -wallSize.z - wallThickness
                    && position.z < -wallSize.z + wallThickness);
            }

            private bool IsXPlusWallInsides(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z > -wallSize.z - wallThickness
                    && position.z < wallSize.z + wallThickness
                    && position.x > wallSize.x - wallThickness
                    && position.x < wallSize.x + wallThickness);
            }
            private bool IsXMinusWallInsides(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z > -wallSize.z - wallThickness
                    && position.z < wallSize.z + wallThickness
                    && position.x > -wallSize.x - wallThickness
                    && position.x < -wallSize.x + wallThickness);
            }

            private bool IsPositionOnOuterWall(Town town, float3 position, float height)
            {
                return position.y > height && position.y < height + town.dimensions.y 
                    && (IsZPlusOuterWall(position, town.dimensions, town.wallThickness)
                    || IsZMinusOuterWall(position, town.dimensions, town.wallThickness)
                    || IsXPlusOuterWall(position, town.dimensions, town.wallThickness)
                    || IsXMinusOuterWall(position, town.dimensions, town.wallThickness)
                    || IsZPlusInnerWall(position, town.dimensions, town.wallThickness)
                    || IsZMinusInnerWall(position, town.dimensions, town.wallThickness)
                    || IsXPlusInnerWall(position, town.dimensions, town.wallThickness)
                    || IsXMinusInnerWall(position, town.dimensions, town.wallThickness));
            }

            private bool IsZPlusOuterWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x >= -wallSize.x - wallThickness
                    && position.x <= wallSize.x + wallThickness
                    && position.z == wallSize.z + wallThickness);
            }
            private bool IsZMinusOuterWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x >= -wallSize.x - wallThickness
                    && position.x <= wallSize.x + wallThickness
                    && position.z == -wallSize.z - wallThickness);
            }
            private bool IsXPlusOuterWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z >= -wallSize.z - wallThickness
                    && position.z <= wallSize.z + wallThickness
                    && position.x == wallSize.x + wallThickness);
            }
            private bool IsXMinusOuterWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z >= -wallSize.z - wallThickness
                    && position.z <= wallSize.z + wallThickness
                    && position.x == -wallSize.x - wallThickness);
            }

            private bool IsZPlusInnerWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x >= -wallSize.x + wallThickness
                    && position.x <= wallSize.x - wallThickness
                    && position.z == wallSize.z - wallThickness);
            }
            private bool IsZMinusInnerWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.x >= -wallSize.x + wallThickness
                    && position.x <= wallSize.x - wallThickness
                    && position.z == -wallSize.z + wallThickness);
            }
            private bool IsXPlusInnerWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z >= -wallSize.z + wallThickness
                    && position.z <= wallSize.z - wallThickness
                    && position.x == wallSize.x - wallThickness);
            }
            private bool IsXMinusInnerWall(float3 position, float3 wallSize, float wallThickness)
            {
                return (position.z >= -wallSize.z + wallThickness
                    && position.z <= wallSize.z - wallThickness
                    && position.x == -wallSize.x + wallThickness);
            }
            /*
            if ((position.x >= -chunkTown.wallSize.x - chunkTown.wallThickness && position.x <= chunkTown.wallSize.x + chunkTown.wallThickness))
            {
                // Z+ Wall
                if (position.z == chunkTown.wallSize.z - chunkTown.wallThickness ||
                    position.z == chunkTown.wallSize.z + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
                    }
                }
                else if (position.z > chunkTown.wallSize.z - chunkTown.wallThickness &&
                    position.z < chunkTown.wallSize.z + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
                    }
                }
                // Z- Wall
                if (position.z == -chunkTown.wallSize.z - chunkTown.wallThickness ||
                    position.z == -chunkTown.wallSize.z + chunkTown.wallThickness)
                {
                    if (position.z >= -2 && position.z <= 2)
                    {
                        // gate blocks here
                    }
                    else if (position.y > height && position.y < height + chunkTown.wallSize.y)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
                    }
                }
                else if (position.z > -chunkTown.wallSize.z - chunkTown.wallThickness &&
                    position.z < -chunkTown.wallSize.z + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
                    }
                }
            }

            if ((position.z >= -chunkTown.wallSize.z - chunkTown.wallThickness && position.z <= chunkTown.wallSize.z + chunkTown.wallThickness))
            {
                // X+ Wall
                if (position.x == chunkTown.wallSize.x - chunkTown.wallThickness ||
                    position.x == chunkTown.wallSize.x + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
                    }
                }
                else if (position.x > chunkTown.wallSize.x - chunkTown.wallThickness &&
                    position.x < chunkTown.wallSize.x + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
                    }
                }
                // X- Wall
                if (position.x == -chunkTown.wallSize.x - chunkTown.wallThickness ||
                    position.x == -chunkTown.wallSize.x + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
                    }
                }
                else if (position.x > -chunkTown.wallSize.x - chunkTown.wallThickness &&
                    position.x < -chunkTown.wallSize.x + chunkTown.wallThickness)
                {
                    if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
                    {
                        // check if at gate location
                        chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
                    }
                }
            }*/
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new TownGenerationJob { }.Schedule(this, inputDeps);
        }
    }
}

/*
if ((position.x >= -chunkTown.wallSize.x - chunkTown.wallThickness && position.x <= chunkTown.wallSize.x + chunkTown.wallThickness))
{
    // Z+ Wall
    if (position.z == chunkTown.wallSize.z - chunkTown.wallThickness || 
        position.z == chunkTown.wallSize.z + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
        }
    }
    else if (position.z > chunkTown.wallSize.z - chunkTown.wallThickness &&
        position.z < chunkTown.wallSize.z + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
        }
    }
    // Z- Wall
    if (position.z == -chunkTown.wallSize.z - chunkTown.wallThickness ||
        position.z == -chunkTown.wallSize.z + chunkTown.wallThickness)
    {
        if (position.z >= -2 && position.z <= 2)
        {
            // gate blocks here
        }
        else if (position.y > height && position.y < height + chunkTown.wallSize.y)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
        }
    }
    else if (position.z > -chunkTown.wallSize.z - chunkTown.wallThickness &&
        position.z < -chunkTown.wallSize.z + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
        }
    }
}

if ((position.z >= -chunkTown.wallSize.z - chunkTown.wallThickness && position.z <= chunkTown.wallSize.z + chunkTown.wallThickness))
{
    // X+ Wall
    if (position.x == chunkTown.wallSize.x - chunkTown.wallThickness ||
        position.x == chunkTown.wallSize.x + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
        }
    }
    else if (position.x > chunkTown.wallSize.x - chunkTown.wallThickness &&
        position.x < chunkTown.wallSize.x + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
        }
    }
    // X- Wall
    if (position.x == -chunkTown.wallSize.x - chunkTown.wallThickness ||
        position.x == -chunkTown.wallSize.x + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.stoneID);
        }
    }
    else if (position.x > -chunkTown.wallSize.x - chunkTown.wallThickness &&
        position.x < -chunkTown.wallSize.x + chunkTown.wallThickness)
    {
        if (position.y > height && position.y < height + chunkTown.wallSize.y - 1)
        {
            // check if at gate location
            chunk.Value.voxels[voxelIndex] = (byte)(chunkTerrain.biome.dirtID);
        }
    }
}
*/
