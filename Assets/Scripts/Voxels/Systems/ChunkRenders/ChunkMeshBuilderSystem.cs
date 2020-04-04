using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Zoxel.Voxels
{

	/// <summary>
	/// Todo:
	///		cull sides differently depending on mesh
	/// 
	/// After this:
	/// Make Chunk get and set voxel data directly to the arrays?
	/// 
	/// </summary>
    [DisableAutoCreation, UpdateAfter(typeof(ChunkSideCullingSystem))]
	public class ChunkMeshBuilderSystem : JobComponentSystem
    {
        [BurstCompile]
		struct ChunkMeshBuilderJob : IJobForEach<ChunkRendererBuilder, ChunkRenderer, ChunkSides> //IJobForEach<ChunkRenderer>
		{
            public void Execute(ref ChunkRendererBuilder chunkRendererBuilder, ref ChunkRenderer chunk, ref ChunkSides chunkSides)   //Entity entity, int index,  
			{
				if (chunkRendererBuilder.state == 2)
                {
                    if (chunkSides.sidesUp.Length != 0)
                    {
                        chunkRendererBuilder.state = 3;
                        GenerateCubeMesh(ref chunk, ref chunkSides);
                    }
                }
			}

            private void GenerateCubeMesh(ref ChunkRenderer chunk, ref ChunkSides chunkSides)
            {
                // used in builder
                chunk.buildPointer = new BuildPointer();
                int voxelArrayIndex = 0;
                int sideIndex;
                int voxelMetaIndex;
                float3 position = new float3(0, 0, 0);
               // float3 whiteColor = new float3(1, 1, 1);
                for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                {
                    for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                    {
                        for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                        {
                            // each voxelmeta has a different uv map chunk.uvIndexes
                            voxelMetaIndex = ((int)chunk.Value.voxels[voxelArrayIndex]) - 1;
                            for (sideIndex = 0; sideIndex <= 5; sideIndex++)
                            {
                                // if sides allow it, then add vertex side of model!
                                if ((sideIndex == 0 && chunkSides.sidesUp[voxelArrayIndex] != 0)
                                    || (sideIndex == 1 && chunkSides.sidesDown[voxelArrayIndex] != 0)
                                    || (sideIndex == 2 && chunkSides.sidesLeft[voxelArrayIndex] != 0)
                                    || (sideIndex == 3 && chunkSides.sidesRight[voxelArrayIndex] != 0)
                                    || (sideIndex == 4 && chunkSides.sidesBack[voxelArrayIndex] != 0)
                                    || (sideIndex == 5 && chunkSides.sidesForward[voxelArrayIndex] != 0))
                                {
                                    BuildVoxelSide(ref chunk, sideIndex, voxelMetaIndex, position);
                                    if (chunk.buildPointer.vertIndex + 4 >= chunk.vertices.Length ||
                                        chunk.buildPointer.triangleIndex + 6 >= chunk.triangles.Length)
                                    {
                                        //chunk.hasBuiltMeshData = 1;
                                        return;
                                    }
                                }
                            }
                            voxelArrayIndex++;
                        }
                    }
                }
            }

            private void BuildVoxelSide(ref ChunkRenderer chunk, int sideIndex, int voxelMetaIndex, float3 position)
            {
                int arrayIndex;
                // draw side!
                for (arrayIndex = 0; arrayIndex < 4; arrayIndex++)
                {
                    var vert = chunk.vertices[arrayIndex + chunk.buildPointer.vertIndex];
                    // verts
                    // get cube verts
                    // use an array for model indexes in chunkRender

                    // multiply by world scale
                    if (chunk.hasWeights == 0)
                    {
                        //UnityEngine.Debug.LogError("Building Voxel for: " + arrayIndex);
                        //chunk.vertices[arrayIndex + chunk.buildPointer.vertIndex] =
                        float3 newPosition = (cubeVertices[sideIndex * 4 + arrayIndex] + position);
                        newPosition = new float3(newPosition.x * chunk.Value.worldScale.x,newPosition .y * chunk.Value.worldScale.y,
                                        newPosition.z * chunk.Value.worldScale.z);
                        //  noise.cnoise(position / 100)));
                        /*float3 noiseInputPosition = ((new float3(chunk.Value.chunkPosition.x * chunk.Value.voxelDimensions.x,
                            chunk.Value.chunkPosition.y * chunk.Value.voxelDimensions.y, chunk.Value.chunkPosition.z * chunk.Value.voxelDimensions.z))
                            + newPosition) * 0.001f;
                        float3 additionalNoise = new float3(noise.snoise(noiseInputPosition),
                            noise.snoise(noiseInputPosition - 32f),
                            noise.snoise(noiseInputPosition + 32f)) * 5f;*/
                        //  + additionalNoise
                        vert.position = newPosition;
                    }
                    else
                    {
                        //  this is for skeletons?
                        vert.position = (cubeVertices[sideIndex * 4 + arrayIndex] + position - chunk.Value.voxelDimensions.ToFloat3() / 2f);
                        vert.position =
                            new float3(vert.position.x * chunk.Value.worldScale.x,vert.position.y * chunk.Value.worldScale.y,
                                        vert.position.z * chunk.Value.worldScale.z);
                            //+ 10f * (new float3(noise.cnoise(position), noise.cnoise(position * 100),
                              //  noise.cnoise(position / 100)));
                    }


                    // colors
                    if (voxelMetaIndex < chunk.voxelColors.Length)
                    {
                        vert.color = chunk.voxelColors[voxelMetaIndex];
                    }
                    else
                    {
                        vert.color = new float3(1, 1, 1);
                    }

                    // UV maps
                    /*int uvMapIndex = voxelMetaIndex * 24 + sideIndex * 4 + arrayIndex;
                    if (uvMapIndex < chunk.uvMaps.Length)
                    {
                        chunk.uvs[arrayIndex + chunk.buildPointer.vertIndex] = chunk.uvMaps[uvMapIndex];
                    }
                    else
                    {
                        chunk.uvs[arrayIndex + chunk.buildPointer.vertIndex] = new float2(0, 0);
                    }*/
                    int uvMapIndex = sideIndex * 4 + arrayIndex; //  24 + 
                    if (voxelMetaIndex < chunk.uvMaps.Length)
                    {
                        if (uvMapIndex < chunk.uvMaps[voxelMetaIndex].uvs.Length)
                        {
                            vert.uv = chunk.uvMaps[voxelMetaIndex].uvs[uvMapIndex];
                        }
                        else
                        {
                            vert.uv = new float2(0, 0);
                        }
                    }
                    else
                    {
                        vert.uv = new float2(0, 0);
                    }
                    chunk.vertices[arrayIndex + chunk.buildPointer.vertIndex] = vert;
                }

                // triangle points! (6 indexes instead of 4 verrts)
                for (arrayIndex = 0; arrayIndex < 6; arrayIndex++)
                {
                    chunk.triangles[arrayIndex + chunk.buildPointer.triangleIndex] =
                        cubeTriangles[sideIndex * 6 + arrayIndex] + chunk.buildPointer.vertIndex;
                }

                // update index
                chunk.buildPointer.vertIndex += 4;
                chunk.buildPointer.triangleIndex += 6;
            }

            private void GenerateSmoothMesh(ref ChunkRenderer chunk)
            {
                /*int maxVertices = 0;
                int maxTriangles = 0;
                int maxColors = 0;
                int maxUVs = 0;
                float3 position = new float3(0, 0, 0);
                int voxelMetaIndex;
                int voxelArrayIndex = 0;
                for (position.x = 0; position.x < chunk.Value.voxelDimensions.x; position.x++)
                {
                    for (position.y = 0; position.y < chunk.Value.voxelDimensions.y; position.y++)
                    {
                        for (position.z = 0; position.z < chunk.Value.voxelDimensions.z; position.z++)
                        {
                            voxelMetaIndex = ((int)chunk.Value.voxels[voxelArrayIndex]) - 1;   // each voxelmeta has a different uv map chunk.uvIndexes
                            if (voxelMetaIndex != 0)
                            {
                                // solid, do something!
                                // first just draw all as solid! unless edge!
                                int cubeIndex = 0; // generate from 8 points?
                                int edgeIndex = MarchingCubesTables.EdgeTable[cubeIndex];
                                int[] row = MarchingCubesTables.TriangleTable[cubeIndex];

                                float3[] newVerts = GenerateVertexList(points, edgeIndex);

                            }
                            voxelArrayIndex++;
                        }
                    }
                }
                chunk.blittableColors = maxColors;
                chunk.blittableVertices = maxVertices;
                chunk.blittableUVs = maxUVs;
                chunk.blittableTriangles = maxTriangles;*/
            }

            private float3[] GenerateVertexList(MarchingPoint[] points, int edgeIndex, float _isolevel)
            {
                float3[] newPoints = new float3[12];
                for (int i = 0; i < 12; i++)
                {
                    if ((edgeIndex & (1 << i)) != 0)
                    {
                        int[] edgePair = MarchingCubesTables.EdgeIndexTable[i];
                        int edge1 = edgePair[0];
                        int edge2 = edgePair[1];

                        MarchingPoint point1 = points[edge1];
                        MarchingPoint point2 = points[edge2];

                        newPoints[i] = VertexInterpolate(point1.localPosition, point2.localPosition, point1.density, point2.density, _isolevel);
                    }
                }

                return newPoints;
            }
            private float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float _isolevel)
            {
                if (math.abs(_isolevel - v1) < 0.000001f)
                {
                    return p1;
                }
                if (math.abs(_isolevel - v2) < 0.000001f)
                {
                    return p2;
                }
                if (math.abs(v1 - v2) < 0.000001f)
                {
                    return p1;
                }

                float mu = (_isolevel - v1) / (v2 - v1);

                float3 p = p1 + mu * (p2 - p1);

                return p;
            }



            #region Statics
            public static readonly float3[] cubeVertices = new float3[]
			{
				// up
				new float3(0, 1, 0),
				new float3(1, 1, 0),
				new float3(1, 1, 1),
				new float3(0, 1, 1),
				// down
				new float3(0, 0, 0),
				new float3(1, 0, 0),
				new float3(1, 0, 1),
				new float3(0, 0, 1),
				// left
				new float3(0, 0, 0),
				new float3(0, 1, 0),
				new float3(0, 1, 1),
				new float3(0, 0, 1),
				// right
				new float3(1, 0, 0),
				new float3(1, 1, 0),
				new float3(1, 1, 1),
				new float3(1, 0, 1),
				// forward
				new float3(0, 0, 0),
				new float3(1, 0, 0),
				new float3(1, 1, 0),
				new float3(0, 1, 0),
				// back
				new float3(0, 0, 1),
				new float3(1, 0, 1),
				new float3(1, 1, 1),
				new float3(0, 1, 1)
			};

			public static readonly int[] cubeTriangles = new int[]
			{
				3, 1, 0, 3, 2, 1,

				0, 2, 3, 0, 1, 2,

				3, 1, 0, 3, 2, 1,

				0, 2, 3, 0, 1, 2,

				3, 1, 0, 3, 2, 1,

				0, 2, 3, 0, 1, 2
			};
			#endregion


        }

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            return new ChunkMeshBuilderJob { }.Schedule(this, inputDeps);
		}
	}

    public struct MarchingPoint
    {
        public float3 localPosition;
        public float density;

        public MarchingPoint(float3 localPosition, float density)
        {
            this.localPosition = localPosition;
            this.density = density;
        }
    }

    public static class MarchingCubesTables
    {
        public static readonly int[][] EdgeIndexTable =
        {
            new[] {0, 1},
            new[] {1, 2},
            new[] {2, 3},
            new[] {3, 0},
            new[] {4, 5},
            new[] {5, 6},
            new[] {6, 7},
            new[] {7, 4},
            new[] {0, 4},
            new[] {1, 5},
            new[] {2, 6},
            new[] {3, 7}
        };

        public static readonly int[] EdgeTable =
        {
            0x0, 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
            0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
            0x190, 0x99, 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
            0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
            0x230, 0x339, 0x33, 0x13a, 0x636, 0x73f, 0x435, 0x53c,
            0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
            0x3a0, 0x2a9, 0x1a3, 0xaa, 0x7a6, 0x6af, 0x5a5, 0x4ac,
            0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
            0x460, 0x569, 0x663, 0x76a, 0x66, 0x16f, 0x265, 0x36c,
            0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
            0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff, 0x3f5, 0x2fc,
            0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
            0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55, 0x15c,
            0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
            0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc,
            0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
            0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
            0xcc, 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
            0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
            0x15c, 0x55, 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
            0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
            0x2fc, 0x3f5, 0xff, 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
            0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
            0x36c, 0x265, 0x16f, 0x66, 0x76a, 0x663, 0x569, 0x460,
            0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
            0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa, 0x1a3, 0x2a9, 0x3a0,
            0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
            0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33, 0x339, 0x230,
            0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
            0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99, 0x190,
            0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
            0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
        };

        public static readonly int[][] TriangleTable =
        {
            new int[] { },
            new[] {0, 8, 3},
            new[] {0, 1, 9},
            new[] {1, 8, 3, 9, 8, 1},
            new[] {1, 2, 10},
            new[] {0, 8, 3, 1, 2, 10},
            new[] {9, 2, 10, 0, 2, 9},
            new[] {2, 8, 3, 2, 10, 8, 10, 9, 8},
            new[] {3, 11, 2},
            new[] {0, 11, 2, 8, 11, 0},
            new[] {1, 9, 0, 2, 3, 11},
            new[] {1, 11, 2, 1, 9, 11, 9, 8, 11},
            new[] {3, 10, 1, 11, 10, 3},
            new[] {0, 10, 1, 0, 8, 10, 8, 11, 10},
            new[] {3, 9, 0, 3, 11, 9, 11, 10, 9},
            new[] {9, 8, 10, 10, 8, 11},
            new[] {4, 7, 8},
            new[] {4, 3, 0, 7, 3, 4},
            new[] {0, 1, 9, 8, 4, 7},
            new[] {4, 1, 9, 4, 7, 1, 7, 3, 1},
            new[] {1, 2, 10, 8, 4, 7},
            new[] {3, 4, 7, 3, 0, 4, 1, 2, 10},
            new[] {9, 2, 10, 9, 0, 2, 8, 4, 7},
            new[] {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4},
            new[] {8, 4, 7, 3, 11, 2},
            new[] {11, 4, 7, 11, 2, 4, 2, 0, 4},
            new[] {9, 0, 1, 8, 4, 7, 2, 3, 11},
            new[] {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1},
            new[] {3, 10, 1, 3, 11, 10, 7, 8, 4},
            new[] {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4},
            new[] {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3},
            new[] {4, 7, 11, 4, 11, 9, 9, 11, 10},
            new[] {9, 5, 4},
            new[] {9, 5, 4, 0, 8, 3},
            new[] {0, 5, 4, 1, 5, 0},
            new[] {8, 5, 4, 8, 3, 5, 3, 1, 5},
            new[] {1, 2, 10, 9, 5, 4},
            new[] {3, 0, 8, 1, 2, 10, 4, 9, 5},
            new[] {5, 2, 10, 5, 4, 2, 4, 0, 2},
            new[] {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8},
            new[] {9, 5, 4, 2, 3, 11},
            new[] {0, 11, 2, 0, 8, 11, 4, 9, 5},
            new[] {0, 5, 4, 0, 1, 5, 2, 3, 11},
            new[] {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5},
            new[] {10, 3, 11, 10, 1, 3, 9, 5, 4},
            new[] {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10},
            new[] {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3},
            new[] {5, 4, 8, 5, 8, 10, 10, 8, 11},
            new[] {9, 7, 8, 5, 7, 9},
            new[] {9, 3, 0, 9, 5, 3, 5, 7, 3},
            new[] {0, 7, 8, 0, 1, 7, 1, 5, 7},
            new[] {1, 5, 3, 3, 5, 7},
            new[] {9, 7, 8, 9, 5, 7, 10, 1, 2},
            new[] {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3},
            new[] {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2},
            new[] {2, 10, 5, 2, 5, 3, 3, 5, 7},
            new[] {7, 9, 5, 7, 8, 9, 3, 11, 2},
            new[] {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11},
            new[] {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7},
            new[] {11, 2, 1, 11, 1, 7, 7, 1, 5},
            new[] {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11},
            new[] {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0},
            new[] {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0},
            new[] {11, 10, 5, 7, 11, 5},
            new[] {10, 6, 5},
            new[] {0, 8, 3, 5, 10, 6},
            new[] {9, 0, 1, 5, 10, 6},
            new[] {1, 8, 3, 1, 9, 8, 5, 10, 6},
            new[] {1, 6, 5, 2, 6, 1},
            new[] {1, 6, 5, 1, 2, 6, 3, 0, 8},
            new[] {9, 6, 5, 9, 0, 6, 0, 2, 6},
            new[] {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8},
            new[] {2, 3, 11, 10, 6, 5},
            new[] {11, 0, 8, 11, 2, 0, 10, 6, 5},
            new[] {0, 1, 9, 2, 3, 11, 5, 10, 6},
            new[] {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11},
            new[] {6, 3, 11, 6, 5, 3, 5, 1, 3},
            new[] {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6},
            new[] {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9},
            new[] {6, 5, 9, 6, 9, 11, 11, 9, 8},
            new[] {5, 10, 6, 4, 7, 8},
            new[] {4, 3, 0, 4, 7, 3, 6, 5, 10},
            new[] {1, 9, 0, 5, 10, 6, 8, 4, 7},
            new[] {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4},
            new[] {6, 1, 2, 6, 5, 1, 4, 7, 8},
            new[] {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7},
            new[] {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6},
            new[] {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9},
            new[] {3, 11, 2, 7, 8, 4, 10, 6, 5},
            new[] {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11},
            new[] {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6},
            new[] {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6},
            new[] {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6},
            new[] {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11},
            new[] {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7},
            new[] {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9},
            new[] {10, 4, 9, 6, 4, 10},
            new[] {4, 10, 6, 4, 9, 10, 0, 8, 3},
            new[] {10, 0, 1, 10, 6, 0, 6, 4, 0},
            new[] {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10},
            new[] {1, 4, 9, 1, 2, 4, 2, 6, 4},
            new[] {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4},
            new[] {0, 2, 4, 4, 2, 6},
            new[] {8, 3, 2, 8, 2, 4, 4, 2, 6},
            new[] {10, 4, 9, 10, 6, 4, 11, 2, 3},
            new[] {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6},
            new[] {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10},
            new[] {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1},
            new[] {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3},
            new[] {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1},
            new[] {3, 11, 6, 3, 6, 0, 0, 6, 4},
            new[] {6, 4, 8, 11, 6, 8},
            new[] {7, 10, 6, 7, 8, 10, 8, 9, 10},
            new[] {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10},
            new[] {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0},
            new[] {10, 6, 7, 10, 7, 1, 1, 7, 3},
            new[] {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7},
            new[] {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9},
            new[] {7, 8, 0, 7, 0, 6, 6, 0, 2},
            new[] {7, 3, 2, 6, 7, 2},
            new[] {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7},
            new[] {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7},
            new[] {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11},
            new[] {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1},
            new[] {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6},
            new[] {0, 9, 1, 11, 6, 7},
            new[] {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0},
            new[] {7, 11, 6},
            new[] {7, 6, 11},
            new[] {3, 0, 8, 11, 7, 6},
            new[] {0, 1, 9, 11, 7, 6},
            new[] {8, 1, 9, 8, 3, 1, 11, 7, 6},
            new[] {10, 1, 2, 6, 11, 7},
            new[] {1, 2, 10, 3, 0, 8, 6, 11, 7},
            new[] {2, 9, 0, 2, 10, 9, 6, 11, 7},
            new[] {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8},
            new[] {7, 2, 3, 6, 2, 7},
            new[] {7, 0, 8, 7, 6, 0, 6, 2, 0},
            new[] {2, 7, 6, 2, 3, 7, 0, 1, 9},
            new[] {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6},
            new[] {10, 7, 6, 10, 1, 7, 1, 3, 7},
            new[] {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8},
            new[] {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7},
            new[] {7, 6, 10, 7, 10, 8, 8, 10, 9},
            new[] {6, 8, 4, 11, 8, 6},
            new[] {3, 6, 11, 3, 0, 6, 0, 4, 6},
            new[] {8, 6, 11, 8, 4, 6, 9, 0, 1},
            new[] {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6},
            new[] {6, 8, 4, 6, 11, 8, 2, 10, 1},
            new[] {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6},
            new[] {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9},
            new[] {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3},
            new[] {8, 2, 3, 8, 4, 2, 4, 6, 2},
            new[] {0, 4, 2, 4, 6, 2},
            new[] {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8},
            new[] {1, 9, 4, 1, 4, 2, 2, 4, 6},
            new[] {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1},
            new[] {10, 1, 0, 10, 0, 6, 6, 0, 4},
            new[] {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3},
            new[] {10, 9, 4, 6, 10, 4},
            new[] {4, 9, 5, 7, 6, 11},
            new[] {0, 8, 3, 4, 9, 5, 11, 7, 6},
            new[] {5, 0, 1, 5, 4, 0, 7, 6, 11},
            new[] {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5},
            new[] {9, 5, 4, 10, 1, 2, 7, 6, 11},
            new[] {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5},
            new[] {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2},
            new[] {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6},
            new[] {7, 2, 3, 7, 6, 2, 5, 4, 9},
            new[] {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7},
            new[] {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0},
            new[] {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8},
            new[] {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7},
            new[] {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4},
            new[] {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10},
            new[] {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10},
            new[] {6, 9, 5, 6, 11, 9, 11, 8, 9},
            new[] {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5},
            new[] {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11},
            new[] {6, 11, 3, 6, 3, 5, 5, 3, 1},
            new[] {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6},
            new[] {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10},
            new[] {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5},
            new[] {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3},
            new[] {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2},
            new[] {9, 5, 6, 9, 6, 0, 0, 6, 2},
            new[] {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8},
            new[] {1, 5, 6, 2, 1, 6},
            new[] {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6},
            new[] {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0},
            new[] {0, 3, 8, 5, 6, 10},
            new[] {10, 5, 6},
            new[] {11, 5, 10, 7, 5, 11},
            new[] {11, 5, 10, 11, 7, 5, 8, 3, 0},
            new[] {5, 11, 7, 5, 10, 11, 1, 9, 0},
            new[] {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1},
            new[] {11, 1, 2, 11, 7, 1, 7, 5, 1},
            new[] {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11},
            new[] {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7},
            new[] {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2},
            new[] {2, 5, 10, 2, 3, 5, 3, 7, 5},
            new[] {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5},
            new[] {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2},
            new[] {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2},
            new[] {1, 3, 5, 3, 7, 5},
            new[] {0, 8, 7, 0, 7, 1, 1, 7, 5},
            new[] {9, 0, 3, 9, 3, 5, 5, 3, 7},
            new[] {9, 8, 7, 5, 9, 7},
            new[] {5, 8, 4, 5, 10, 8, 10, 11, 8},
            new[] {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0},
            new[] {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5},
            new[] {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4},
            new[] {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8},
            new[] {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11},
            new[] {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5},
            new[] {9, 4, 5, 2, 11, 3},
            new[] {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4},
            new[] {5, 10, 2, 5, 2, 4, 4, 2, 0},
            new[] {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9},
            new[] {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2},
            new[] {8, 4, 5, 8, 5, 3, 3, 5, 1},
            new[] {0, 4, 5, 1, 0, 5},
            new[] {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5},
            new[] {9, 4, 5},
            new[] {4, 11, 7, 4, 9, 11, 9, 10, 11},
            new[] {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11},
            new[] {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11},
            new[] {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4},
            new[] {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2},
            new[] {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3},
            new[] {11, 7, 4, 11, 4, 2, 2, 4, 0},
            new[] {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4},
            new[] {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9},
            new[] {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7},
            new[] {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10},
            new[] {1, 10, 2, 8, 7, 4},
            new[] {4, 9, 1, 4, 1, 7, 7, 1, 3},
            new[] {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1},
            new[] {4, 0, 3, 7, 4, 3},
            new[] {4, 8, 7},
            new[] {9, 10, 8, 10, 11, 8},
            new[] {3, 0, 9, 3, 9, 11, 11, 9, 10},
            new[] {0, 1, 10, 0, 10, 8, 8, 10, 11},
            new[] {3, 1, 10, 11, 3, 10},
            new[] {1, 2, 11, 1, 11, 9, 9, 11, 8},
            new[] {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9},
            new[] {0, 2, 11, 8, 0, 11},
            new[] {3, 2, 11},
            new[] {2, 3, 8, 2, 8, 10, 10, 8, 9},
            new[] {9, 10, 2, 0, 9, 2},
            new[] {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8},
            new[] {1, 10, 2},
            new[] {1, 3, 8, 9, 1, 8},
            new[] {0, 9, 1},
            new[] {0, 3, 8},
            new int[] { }
        };
    }
}
//vodel.GetColor());
//chunk.vertices[arrayIndex + chunk.maxVertices] = (cubeVertices[sideIndex, arrayIndex] + position) * voxelScale;
//chunk.uvs[arrayIndex + chunk.maxUVs] 
//chunk.uvs[arrayIndex + chunk.maxUVs] = uvAddition + 0.25f * chunk.uvIndexes[uvMapIndex];
//chunk.uvs[arrayIndex + chunk.maxUVs] = uvAddition + 0.25f * cubeUvs[sideIndex * 4 + arrayIndex];
//chunk.colors[arrayIndex + chunk.maxVertices] = whiteColor;= cubeUvs[sideIndex, arrayIndex];