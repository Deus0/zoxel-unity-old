using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Zoxel {
    
    [Serializable]
    public struct int3 : IEquatable<int3> {
        public int x;
        public int y;
        public int z;

        public int3(int x_, int y_, int z_) {
            x = x_; y = y_; z = z_;
        }

        public int3(float3 newOne) {
            x = (int) newOne.x;
            y = (int) newOne.y; 
            z = (int) newOne.z;
        }

        public override string ToString() {
            return "" + x + "," + y + "," + z;
        }

        public float3 ToFloat3()
        {
            return new float3(x,y,z);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + x.GetHashCode();
                hash = hash * 23 + y.GetHashCode();
                hash = hash * 23 + z.GetHashCode();
                /*hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();*/
                return hash;
            }
            /*int result = (int) (x ^ (x >>> 32));
            result = 31 * result + (int) (y ^ (y >>> 32));
            result = 31 * result + (int) (z ^ (z >>> 32));
            return result;*/
            //return 256 * x + 128 * y + 64 * z; // Or something like that
        }

        public override bool Equals(object obj)
        {
            return obj is int3 && Equals((int3) obj);
        }
        public static bool operator ==(int3 a, int3 b)
            => a.x == b.x && a.z == b.z && a.y == b.y;
        public static bool operator !=(int3 a, int3 b)
            => a.x != b.x || a.z != b.z || a.y != b.y;

        public bool Equals(int3 p)
        {
            return x == p.x && y == p.y && z == p.z;
        }
        public static int3 operator +(int3 a, int3 b)
            => new int3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static int3 operator -(int3 a, int3 b)
            => new int3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static int3 operator *(int3 a, int b)
            => new int3(a.x * b, a.y * b, a.z * b);
        public static int3 operator /(int3 a, int b)
            => new int3(a.x / b, a.y / b, a.z / b);

        public static int3 Zero(){ 
            return new int3(0, 0, 0); 
        }
        public static int3 Up(){ 
            return new int3(0, 1, 0); 
        }
        public static int3 Down(){ 
            return new int3(0, -1, 0); 
        }
        public static int3 Left(){ 
            return new int3(-1, 0, 0); 
        }
        public static int3 Right(){ 
            return new int3(1, 0, 0); 
        }
        public static int3 Forward(){ 
            return new int3(0, 0, 1); 
        }
        public static int3 Back(){ 
            return new int3(0, 0, -1); 
        }
    }
}