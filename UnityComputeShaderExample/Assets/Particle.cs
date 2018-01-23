using System.Collections;
using System.Collections.Generic;
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
