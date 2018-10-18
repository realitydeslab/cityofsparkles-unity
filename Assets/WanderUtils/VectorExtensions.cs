using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderUtils
{
    public static class VectorExtensions 
    {
        public static Vector3 GroundProjection(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
        
        public static Vector2 GroundProjection2d(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }
    }
}
