using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ParticleConstants
{
    public const int NUM_OF_SAMPLES = 4;
}
[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct float3x3
{
    public float m00; public float m01; public float m02;
    public float m10; public float m11; public float m12;
    public float m20; public float m21; public float m22;
}

/*
[StructLayout(LayoutKind.Sequential, Pack = 0)]
public class MaterialPointParticleData
{
    public Vector3 position = new Vector3();
    public Vector3 velocity = new Vector3();
    public Vector3 acceleration = new Vector3();

    public float mass_ = 0;
    public float radius_ = 0;

    public float density_ = 0;
    public float volume_ = 0;

    public Matrix4x4 force_ = new Matrix4x4();

    public Matrix4x4 velocity_gradient_ = new Matrix4x4();

    //APIC matrix
    //TODO: init them
    public Matrix4x4 B = new Matrix4x4();
    public Matrix4x4 D = new Matrix4x4();

    public Matrix4x4 Fe = new Matrix4x4();
    public Matrix4x4 Fp = new Matrix4x4();

    public Matrix4x4 R = new Matrix4x4();
    public Matrix4x4 S = new Matrix4x4();

    public Matrix4x4 s = new Matrix4x4();
    public Matrix4x4 v = new Matrix4x4();
    public Matrix4x4 d = new Matrix4x4();
    
    //debug
    public Vector3 debug_grid_index_ = new Vector3();
}
public class MaterialPointParticle
{
    MaterialPointParticleData data_ = new MaterialPointParticleData();

    static public int GetSize()
    {
        return Marshal.SizeOf(typeof(MaterialPointParticleData));
    }

    internal void PrintInfo()
    {
//         Debug.LogFormat("position: {0}", position);
//         Debug.LogFormat("velocity: {0}", velocity);
//         Debug.LogFormat("acceleration: {0}", acceleration);
//         Debug.LogFormat("mass_: {0}"    , mass_);
//         Debug.LogFormat("radius_: {0}"  , radius_);
//         Debug.LogFormat("density_: {0}" , density_);
//         Debug.LogFormat("volume_: {0}"  , volume_);
    }
}*/
/*

public class ParticleWeight
{
    public float[,,] weight_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    public float[,,] weight_gradient_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    
    public ParticleWeight()
    {
        Debug.LogWarning("Init them to 0!!!!");
    }
}*/

public class ParticleWeightAdvance
{
    public Vector3[,,] weight_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    //public Vector3[,,] weight_dev_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    public Vector3[,,] weight_gradient_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];

    public ParticleWeightAdvance()
    {
        Debug.LogWarning("Init them to 0!!!!");
    }
}
