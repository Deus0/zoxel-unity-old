using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AnimatorSystem
{
    /*public class BakedRenderSystem : JobComponentSystem
    {
        [BurstCompile]
        struct UpdateDataJob : IJobParallelFor
        {
            [WriteOnly] public NativeArray<Matrix4x4> matrices;
            [WriteOnly] public NativeArray<float> yPositions;
            //[ReadOnly] public ComponentDataArray<StateMachineUnit> units;

            public void Execute(int index)
            {
                //matrices[index] = units[index].Matrix;
                //yPositions[index] = units[index].YPos;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (injectRendererDatas.Length != rendererData.Length)
            {
                DestroyRendererArrays();
                CreateRendererArrays(injectRendererDatas.Length);
                rendererData = new RendererData(injectRendererDatas.Length);
            }
            var handle = new UpdateDataJob { matrices = matrices, yPositions = yPositions, units = injectRendererDatas.stateDatas }.Schedule(injectRendererDatas.Length, 64, inputDeps);
            handle.Complete();
            matrices.CopyTo(rendererData.matrices);
            yPositions.CopyTo(rendererData.yPositions);

            //for (int i = 0; i < injectRendererDatas.Length; i += 1023)
            {
                //var len = Mathf.Min(injectRendererDatas.Length - i, 1023);
                //Array.Copy(rendererData.matrices, i, tempMatrices, 0, len);
                //Array.Copy(rendererData.yPositions, i, tempYPositions, 0, len);
                //materialPropertyBlock.SetFloatArray("_YPos", tempYPositions);
                for (int j = 0; j < StateGraph.rendererData.SubMeshCount; j++)
                {
                    Graphics.DrawMeshInstanced(StateGraph.rendererData.Mesh, j, StateGraph.rendererData.Materials[j], tempMatrices, len, materialPropertyBlock, StateGraph.rendererData.ShadowCastingMode, StateGraph.rendererData.ReceivesShadows);
                }
            }

            // for all entities spawned
            //Graphics.DrawMeshInstanced(StateGraph.rendererData.Mesh, j, StateGraph.rendererData.Materials[j], tempMatrices, len, materialPropertyBlock, StateGraph.rendererData.ShadowCastingMode, StateGraph.rendererData.ReceivesShadows);

            return new JobHandle();
        }
    }*/
}
