using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Particle
{
    public SpatialInfomation data_;

    public class SpatialInfomation
    {
        public Vector3 location;//physics location that differs from render element's
        public Vector3 velocity;
        public Vector3 acceleration;
        public SpatialInfomation()
        {
            location = new Vector3(0, 0, 0);
            velocity = new Vector3(0, 0, 0);
            acceleration = new Vector3(0, 0, 0);
            mass = 1;
            radius = 1;
        }

        public float mass;
        public float radius;
    };


}
[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct float3x3
{
    public float m00; public float m01; public float m02;
    public float m10; public float m11; public float m12;
    public float m20; public float m21; public float m22;
}
public struct int3
{
    public int x;
    public int y;
    public int z;
}
