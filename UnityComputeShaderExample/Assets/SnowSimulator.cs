using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class SnowSimulator : MonoBehaviour {

    int NUMBER_OF_PARTICLES = 100;

    [SerializeField] private ComputeShader cs;
    ComputeBuffer particle_input_buffer_;
    ComputeBuffer particle_weight_buffer_;

    MaterialPointParticleData[] particle_input_data_;
    ParticleWeight[] particle_weight_data_;

    struct MaterialPointParticleData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;

        public float mass_;
        public float radius_;

        public float density_;
        public float volume_;

        public float3x3 force_;

        public float3x3 velocity_gradient_;

        //APIC matrix
        //TODO: init them
        public float3x3 B;
        public float3x3 D;

        public float3x3 Fe;
        public float3x3 Fp;

        public float3x3 R;
        public float3x3 S;

        public float3x3 s;
        public float3x3 v;
        public float3x3 d;

        //debug
        public Vector3 debug_grid_index_;


        public void PrintInfo()
        {
            Debug.LogFormat("position: {0}", position);
            Debug.LogFormat("velocity: {0}", velocity);
            Debug.LogFormat("acceleration: {0}", acceleration);
            Debug.LogFormat("mass_: {0}", mass_);
            Debug.LogFormat("radius_: {0}", radius_);
            Debug.LogFormat("density_: {0}", density_);
            Debug.LogFormat("volume_: {0}", volume_);
        }
    }

    struct ParticleWeight
    {
        public float[,,] weight_all_;// = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
        public float[,,] weight_gradient_all_;// = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
        public Vector3[,,] weight_;// = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
        //public Vector3[,,] weight_dev_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
        public Vector3[,,] weight_gradient_;// = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];

        internal void PrintInfo()
        {
            Debug.Log("Weight Data:");
            foreach(var data in weight_all_)
            {
                Debug.LogFormat("{0}", data);
            }
        }
    }

    class CS_Attribute
    {
        public int kernal_id { get; set; }
        public bool use_soa { get; set; }

        public CS_Attribute(int id, bool soa)
        {
            kernal_id = id;
            use_soa = soa;
        }
    };

    Dictionary<string, CS_Attribute> cs_kernal_name_index_map_ = new Dictionary<string, CS_Attribute>()
    {
            //{ "SVD3D", -1},
            //{ "Grid", -1},
            { "InitData", new CS_Attribute(-1, false)},
            { "ResterizeParticleMassToGrid", new CS_Attribute(-1, false)},
            //{ "Particle_NONE", new CS_Attribute(-1, false) },
            //{ "Particle_SOA", new CS_Attribute(-1, true)},
    };
    
    MPMGrid grid_ = new MPMGrid();

    void InitCPUData()
    {
        particle_input_data_ = new MaterialPointParticleData[NUMBER_OF_PARTICLES];
        for (int i = 0; i < particle_input_data_.Length; i++)
        {
            particle_input_data_[i].position = new Vector3();
            particle_input_data_[i].velocity = new Vector3();
            particle_input_data_[i].acceleration = new Vector3();

            particle_input_data_[i].mass_ = 0;
            particle_input_data_[i].radius_ = 0;

            particle_input_data_[i].density_ = 0;
            particle_input_data_[i].volume_ = 0;

            particle_input_data_[i].force_ = new float3x3();

            particle_input_data_[i].velocity_gradient_ = new float3x3();

            //APIC matrix
            //TODO: init them
            particle_input_data_[i].B = new float3x3();
            particle_input_data_[i].D = new float3x3();

            particle_input_data_[i].Fe = new float3x3();
            particle_input_data_[i].Fp = new float3x3();

            particle_input_data_[i].R = new float3x3();
            particle_input_data_[i].S = new float3x3();

            particle_input_data_[i].s = new float3x3();
            particle_input_data_[i].v = new float3x3();
            particle_input_data_[i].d = new float3x3();

            //debug
            particle_input_data_[i].debug_grid_index_ = new Vector3();
        }

        particle_weight_data_ = new ParticleWeight[NUMBER_OF_PARTICLES];
        for (int i = 0; i < particle_weight_data_.Length; i++)
        {
            particle_weight_data_[i].weight_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
            particle_weight_data_[i].weight_gradient_all_ = new float[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
            particle_weight_data_[i].weight_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
            //particle_weight_data_[i].weight_dev_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
            particle_weight_data_[i].weight_gradient_ = new Vector3[ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES, ParticleConstants.NUM_OF_SAMPLES];
        }

        grid_.InitCPUData();
    }

    void InitGPUData()
    {
        int size = Marshal.SizeOf(typeof(MaterialPointParticleData));

        int new_size = Marshal.SizeOf(typeof(Vector3)) * 4 + Marshal.SizeOf(typeof(float)) * 4 + Marshal.SizeOf(typeof(float3x3)) * 11;
        particle_input_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, size);
        int array_num = ParticleConstants.NUM_OF_SAMPLES * ParticleConstants.NUM_OF_SAMPLES * ParticleConstants.NUM_OF_SAMPLES;
        size =  Marshal.SizeOf(typeof(float)) * array_num;
        size += Marshal.SizeOf(typeof(float)) * array_num;
        size += Marshal.SizeOf(typeof(Vector3)) * array_num;
        size += Marshal.SizeOf(typeof(Vector3)) * array_num;
        //size = Marshal.SizeOf(particle_weight_data_[0]);

        particle_weight_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, size);

        foreach (var k in cs_kernal_name_index_map_)
        {
            k.Value.kernal_id = cs.FindKernel(k.Key);
        }

        grid_.InitGPUData();
    }

    void CopyFormCPUtoGPU()
    {
        Assert.IsNotNull(particle_input_data_);
        Assert.IsNotNull(particle_input_buffer_);

        particle_input_buffer_.SetData(particle_input_data_);
        particle_weight_buffer_.SetData(particle_weight_data_);

        //grid_.CopyFormCPUtoGPU();
    }

    void CopyFromGPUToCPU()
    {
        particle_input_buffer_.GetData(particle_input_data_);
        particle_weight_buffer_.GetData(particle_weight_data_);

        //grid_.CopyFromGPUToCPU();
    }

    private void Start()
    {
        StartCoroutine(StartAsync());
    }
    // Use this for initialization
    IEnumerator StartAsync ()
    {
        Assert.IsNotNull(cs);

        this.InitCPUData();
        this.InitGPUData();
        //Random step here
        this.CopyFormCPUtoGPU();

        this.InitData();
        this.ResterizeParticleMassToGrid();

        this.CopyFromGPUToCPU();
        this.PrntInfo();
        yield return 0;
    }

    private void PrntInfo()
    {
        //         foreach(var p in this.particle_output_data_)
        //         {
        //             p.particle_.PrintInfo();
        //         }

        particle_input_data_[0].PrintInfo();
        particle_weight_data_[0].PrintInfo();

        grid_.PrintInfo();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void InitData()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        cs.SetBuffer(id, "_particle_buffer", particle_input_buffer_);
        cs.SetBuffer(id, "_particle_weight_buffer", particle_weight_buffer_);
        cs.Dispatch(id, 512 / 8, 8, 1);
    }

    //execute one time in init step
    void ResterizeParticleMassToGrid()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        cs.SetBuffer(id, "_particle_buffer", particle_input_buffer_);
        cs.SetBuffer(id, "_particle_weight_buffer", particle_weight_buffer_);
        cs.Dispatch(id, 512/8, 8, 1);
    }

    //from The Affine Particle-In-Cell (APIC) method
    //http://www.seas.upenn.edu/~cffjiang/research/mpmcourse/mpmcourse.pdf
//     void ResterizeParticleToGridWithAPIC();
//     void ComputeParticleVelocityWithAPIC();

    void ResterizeParticleToGrid()
    {

    }
//     void ComputeParticleVolumesAndDensities();
//     void ComputeGridForce();
//     void ComputeGridVelocity();
//     void GridCollision();
//     void ComputeParticleDeformationGradient();
//     void ComputeParticleVelocity();
//     void ParticleCollision();
//     void ComputeParticlePosition();
}
