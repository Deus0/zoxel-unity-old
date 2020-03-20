using UnityEngine;
using System.Collections;
using Unity.Mathematics;

namespace Zoxel
{

    public static class QuaternionHelpers
    {
        public static quaternion slerp(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                dt = -dt;
                q2.value = -q2.value;
            }

            if (dt < 0.9995f)
            {
                float angle = math.acos(dt);
                float s = math.rsqrt(1.0f - dt * dt);    // 1.0f / sin(angle)
                float w1 = math.sin(angle * (1.0f - t)) * s;
                float w2 = math.sin(angle * t) * s;
                return math.quaternion(q1.value * w1 + q2.value * w2);
            }
            else
            {
                // if the angle is small, use linear interpolation
                return nlerp(q1, q2, t);
            }
        }

        public static quaternion nlerp(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                q2.value = -q2.value;
            }

            return math.normalize(math.quaternion(math.lerp(q1.value, q2.value, t)));
        }

        public static quaternion slerpSafe(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                dt = -dt;
                q2.value = -q2.value;
            }

            if (dt < 0.9995f)
            {
                float angle = math.acos(dt);
                float s = math.rsqrt(1.0f - dt * dt);    // 1.0f / sin(angle)
                float w1 = math.sin(angle * (1.0f - t)) * s;
                float w2 = math.sin(angle * t) * s;
                return math.quaternion(q1.value * w1 + q2.value * w2);
            }
            else
            {
                // if the angle is small, use linear interpolation
                return nlerpSafe(q1, q2, t);
            }
        }

        public static quaternion nlerpSafe(quaternion q1, quaternion q2, float t)
        {
            float dt = math.dot(q1, q2);
            if (dt < 0.0f)
            {
                q2.value = -q2.value;
            }

            return math.normalizesafe(math.quaternion(math.lerp(q1.value, q2.value, t)));
        }
    }
}