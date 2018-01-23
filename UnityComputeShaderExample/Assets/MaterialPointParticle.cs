using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParticleConstants
{
    public const int NUM_OF_SAMPLES = 4;
}
public class MaterialPointParticle
{
    public Vector3 position = new Vector3(0, 0, 0);
    public Vector3 velocity = new Vector3(0, 0, 0);
    public Vector3 acceleration = new Vector3(0, 0, 0);

    public float mass_;
    public float radius_;

    public float density_;
    public float volume_;

    public Matrix4x4 force_ = new Matrix4x4();

    public Matrix4x4 velocity_gradient_ = new Matrix4x4();

    //APIC matrix
    //TODO: init them
    public Matrix4x4 B;
    public Matrix4x4 D;

    public Matrix4x4 Fe;
    public Matrix4x4 Fp;

    public Matrix4x4 R;
    public Matrix4x4 S;

    public Matrix4x4 s;
    public Matrix4x4 v;
    public Matrix4x4 d;


    //debug
    public Vector3 debug_grid_index_ = new Vector3(0,0,0);
}

public class ParticleWeight
{
    public float[,,] weight_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    public float[,,] weight_gradient_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
    
    public ParticleWeight()
    {
        Debug.LogWarning("Init them to 0!!!!");
    }
}

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
