using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class SnowSimulator : MonoBehaviour {

    int NUMBER_OF_PARTICLES = 100;

    [SerializeField] private ComputeShader cs;
    ComputeBuffer input_buffer_;
    Input[] particle_input_data_;
    struct Input
    {
        public MaterialPointParticle particle_;
        public ParticleWeight particle_weight_;
        public ParticleWeightAdvance particle_weight_ad_;
    }
    struct Output
    {
        float dummy;
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
        particle_input_data_ = new Input[NUMBER_OF_PARTICLES];
        for (int i = 0; i < particle_input_data_.Length; i++)
        {
            particle_input_data_[i].particle_ = new MaterialPointParticle();
            particle_input_data_[i].particle_weight_ = new ParticleWeight();
            particle_input_data_[i].particle_weight_ad_ = new ParticleWeightAdvance();
        }
        
        grid_.InitCPUData();
    }

    void InitGPUData()
    {
        input_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, Marshal.SizeOf(typeof(Input)));

        foreach (var k in cs_kernal_name_index_map_)
        {
            k.Value.kernal_id = cs.FindKernel(k.Key);
        }

        grid_.InitGPUData();
    }

    void CopyFormCPUtoGPU()
    {
        Assert.IsNotNull(particle_input_data_);
        Assert.IsNotNull(input_buffer_);

        input_buffer_.SetData(particle_input_data_);

        grid_.CopyFormCPUtoGPU();
    }

    // Use this for initialization
    void Start ()
    {
        Assert.IsNotNull(cs);

        this.InitCPUData();
        this.InitGPUData();
        //Random step here
        //this.CopyFormCPUtoGPU();

        this.InitData();
        this.ResterizeParticleMassToGrid();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitData()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        cs.Dispatch(cs_kernal_name_index_map_[function_name].kernal_id, 512 / 8, 8, 1);
    }

    void ResterizeParticleMassToGrid()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        cs.Dispatch(cs_kernal_name_index_map_[function_name].kernal_id, 512/8, 8, 1);
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
