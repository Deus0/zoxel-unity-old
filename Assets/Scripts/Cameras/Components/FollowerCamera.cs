using Unity.Entities;
using Unity.Mathematics;

namespace Zoxel
{

    public class FollowerCameraComponent : ComponentDataProxy<FollowerCamera> { }

    /// <summary>
    /// Should also make it follow a target character - locked
    /// Or just move to it, over time, then unlock controls
    /// </summary>
    [System.Serializable]
    public struct FollowerCamera : IComponentData
    {
        public int characterID;
        public FollowCameraData Value;

        //public float2 sensitivity;
        //public float2 rotationBoundsX;

        //public float3 originalPosition;

        //public byte isMovement;
        //public float3 movement;

        //public float timePassed;
    }

    [System.Serializable]
    public struct FollowCameraData
    {
        public float2 lerpSpeed;
        public float3 cameraAddition;
        public float3 cameraRotation;
        public float3 targetPosition;
        public quaternion targetRotation;
    }
}