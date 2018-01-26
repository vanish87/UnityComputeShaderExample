using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;


public class SnowConstants
{
    public const float
            PARTICLE_DIAM = .0072f,     //Diameter of each particle; smaller = higher resolution
            FRAMERATE = 1 / 60.0f,          //Frames per second
            CFL = .04f,                 //Adaptive timestep adjustment
            MAX_TIMESTEP = 5e-4f,       //Upper timestep limit
            FLIP_PERCENT = .95f,            //Weight to give FLIP update over PIC (.95)
            CRIT_COMPRESS = 1 - 2.5e-2f,    //Fracture threshold for compression (1-2.5e-2)
            CRIT_STRETCH = 1 + 7.5e-3f, //Fracture threshold for stretching (1+7.5e-3)
            HARDENING = 5.0f,           //How much plastic deformation strengthens material (10)
            DENSITY = 100,              //Density of snow in kg/m^2 (400 for 3d)
            YOUNGS_MODULUS = 1.4e5f,        //Young's modulus (springiness) (1.4e5)
            POISSONS_RATIO = .2f,       //Poisson's ratio (transverse/axial strain ratio) (.2)
            IMPLICIT_RATIO = 0,         //Percentage that should be implicit vs explicit for velocity update
            MAX_IMPLICIT_ITERS = 30,    //Maximum iterations for the conjugate residual
            MAX_IMPLICIT_ERR = 1e4f,        //Maximum allowed error for conjugate residual
            MIN_IMPLICIT_ERR = 1e-4f,   //Minimum allowed error for conjugate residual
            STICKY = .9f,               //Collision stickiness (lower = stickier)
            GRAVITY = -9.8f;


    public const float MU = YOUNGS_MODULUS / (2 + 2 * POISSONS_RATIO);
    public const float LAMBDA = YOUNGS_MODULUS * POISSONS_RATIO / ((1 + POISSONS_RATIO) * (1 - 2 * POISSONS_RATIO));
    public const float EPSILON = HARDENING;
}
public class SnowSimulator : MonoBehaviour
{
    int NUMBER_OF_PARTICLES = 500;

    [SerializeField] private ComputeShader cs;
    ComputeBuffer particle_input_buffer_;
    ComputeBuffer small_particle_input_buffer_;
    ComputeBuffer particle_weight_buffer_;
    ComputeBuffer particle_debug_buffer_;

    MaterialPointParticleData[] particle_input_data_;
    SmallParticleStruct[] small_particle_input_data_;
    ParticleWeight[] particle_weight_data_;
    Matrix4x4[] particle_debug_data_;

    struct MaterialPointParticleData
    {
        public Vector3 position_;
        public Vector3 velocity_;
        public Vector3 acceleration_;

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
        public int3 debug_grid_index_;


        public void PrintInfo()
        {
            Debug.LogFormat("position: {0}", position_);
            Debug.LogFormat("velocity: {0}", velocity_);
            Debug.LogFormat("acceleration: {0}", acceleration_);
            Debug.LogFormat("mass_: {0}", mass_);
            Debug.LogFormat("radius_: {0}", radius_);
            Debug.LogFormat("density_: {0}", density_);
            Debug.LogFormat("volume_: {0}", volume_);
        }

        internal void Reset()
        {
            this.position_ = Vector3.zero;
            this.velocity_ = Vector3.zero;
            this.acceleration_ = Vector3.zero;
            this.mass_ = 0;
            this.radius_ = 0;
            this.density_ = 0;
            this.volume_ = 0;
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
    
    public struct SmallParticleStruct
    {
        public Vector3 pos;
        public Vector3 velocity;
        public float mass;


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
        public int3 debug_grid_index_;
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
            { "InitGridData", new CS_Attribute(-1, false)},
            { "ResterizeParticleMassAndVelocityToGrid", new CS_Attribute(-1, false)},
            { "UpdateGridVelocity", new CS_Attribute(-1, false)},
            { "ComputeParticleVolumesAndDensities", new CS_Attribute(-1, false)},
            
        //{ "Particle_NONE", new CS_Attribute(-1, false) },
            //{ "Particle_SOA", new CS_Attribute(-1, true)},
    };
    
    MPMGrid grid_ = new MPMGrid();


    void RandomToFillOneCircle(float Raduis, Vector3 Position)
    {
        float ParticleArea = SnowConstants.PARTICLE_DIAM * SnowConstants.PARTICLE_DIAM;
        float ParticleMass = SnowConstants.DENSITY * ParticleArea * 0.03f;

        float CricleArea = Mathf.PI * Raduis * Raduis / 100000;
        uint NumberOfParticle = (uint)(CricleArea / ParticleArea);

        for (uint i = 0; i < this.particle_input_data_.Length; ++i)
        {
            MaterialPointParticleData it = this.particle_input_data_[i];
            Vector3 rand = new Vector3(UnityEngine.Random.Range(-Raduis, Raduis), UnityEngine.Random.Range(-Raduis, Raduis), 0);
            while (Vector3.Dot(rand, rand) > Raduis * Raduis)
            {
                rand = new Vector3(UnityEngine.Random.Range(-Raduis, Raduis), UnityEngine.Random.Range(-Raduis, Raduis), 0);
            }

            rand = UnityEngine.Random.insideUnitSphere * Raduis;
            it.position_ = rand + Position;
            it.mass_ = ParticleMass;

            this.particle_input_data_[i] = it;
        }


    }

    void RandomToFillCircle(float Raduis, Vector3 Position)
    {
        float ParticleArea = SnowConstants.PARTICLE_DIAM * SnowConstants.PARTICLE_DIAM;
        float ParticleMass = SnowConstants.DENSITY * ParticleArea * 0.03f;

        float CricleArea = Mathf.PI * Raduis * Raduis / 100000;
        uint NumberOfParticle = (uint)(CricleArea / ParticleArea);

        int Count = 0;

        Vector3[] CriclePos =
        { Position , new Vector3(80,50,0) , new Vector3(0,0,0)};


        Vector3[] CricleSpeed =
        { new Vector3(200,-150,0) , new Vector3(0,0,0) , new Vector3(0,0,0) };

        for (uint i = 0; i < this.particle_input_data_.Length; ++i)
        {
            if (i < NumberOfParticle * (Count + 1))
            {
                MaterialPointParticleData it = this.particle_input_data_[i];
                Vector3 rand = new Vector3(UnityEngine.Random.Range(-Raduis, Raduis), UnityEngine.Random.Range(-Raduis, Raduis), 0);
                //rand = UnityEngine.Random.insideUnitSphere * Raduis;
                while (Vector3.Dot(rand,rand) > Raduis * Raduis)
                {
                    rand = new Vector3(UnityEngine.Random.Range(-Raduis, Raduis), UnityEngine.Random.Range(-Raduis, Raduis), 0);
                }
                it.position_ = new Vector3(rand.x + CriclePos[Count].x, rand.y + CriclePos[Count].y, rand.z + CriclePos[Count].z);
                it.velocity_= new Vector3(CricleSpeed[Count].x, CricleSpeed[Count].y, CricleSpeed[Count].z);

                it.mass_ = ParticleMass;
                this.particle_input_data_[i] = it;
            }
            else
            {
                Count++;
            }


        }
    }
    void InitCPUData()
    {
        particle_input_data_ = new MaterialPointParticleData[NUMBER_OF_PARTICLES];
        for (int i = 0; i < particle_input_data_.Length; i++)
        {
            particle_input_data_[i].position_ = new Vector3();
            particle_input_data_[i].velocity_ = new Vector3();
            particle_input_data_[i].acceleration_ = new Vector3();

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
            particle_input_data_[i].debug_grid_index_ = new int3();
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

        particle_debug_data_ = new Matrix4x4[NUMBER_OF_PARTICLES];


        small_particle_input_data_ = new SmallParticleStruct[NUMBER_OF_PARTICLES];
        for (int i = 0; i < small_particle_input_data_.Length; i++)
        {
            small_particle_input_data_[i].pos = UnityEngine.Random.insideUnitCircle;
            small_particle_input_data_[i].velocity = new Vector3();
            small_particle_input_data_[i].force_ = new float3x3();

            small_particle_input_data_[i].velocity_gradient_ = new float3x3();

            //APIC matrix
            //TODO: init them
            small_particle_input_data_[i].B = new float3x3();
            small_particle_input_data_[i].D = new float3x3();

            small_particle_input_data_[i].Fe = new float3x3();
            small_particle_input_data_[i].Fp = new float3x3();

            small_particle_input_data_[i].R = new float3x3();
            small_particle_input_data_[i].S = new float3x3();

            small_particle_input_data_[i].s = new float3x3();
            small_particle_input_data_[i].v = new float3x3();
            small_particle_input_data_[i].d = new float3x3();

            //debug
            small_particle_input_data_[i].debug_grid_index_ = new int3();
        }

        grid_.InitCPUData();
    }

    void InitGPUData()
    {
        int size = Marshal.SizeOf(typeof(MaterialPointParticleData));
        int new_size = Marshal.SizeOf(typeof(Vector3)) * 3 + Marshal.SizeOf(typeof(float)) * 4 + Marshal.SizeOf(typeof(float3x3)) * 11 + Marshal.SizeOf(typeof(int)) * 3;
        Assert.IsTrue(size == new_size);
        Assert.IsTrue(size < 2048);
        Assert.IsTrue(Marshal.SizeOf(typeof(float3x3)) == Marshal.SizeOf(typeof(float)) * 9);

        particle_input_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, size);

        int array_num = ParticleConstants.NUM_OF_ALL_SAMPLES;
        size =  Marshal.SizeOf(typeof(float)) * array_num;
        size += Marshal.SizeOf(typeof(float)) * array_num;
        size += Marshal.SizeOf(typeof(Vector3)) * array_num;
        size += Marshal.SizeOf(typeof(Vector3)) * array_num;
        //size = Marshal.SizeOf(particle_weight_data_[0]);

        particle_weight_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, size);

        particle_debug_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, Marshal.SizeOf(typeof(Matrix4x4)));

        small_particle_input_buffer_ = new ComputeBuffer(NUMBER_OF_PARTICLES, Marshal.SizeOf(typeof(SmallParticleStruct)));

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
        small_particle_input_buffer_.SetData(small_particle_input_data_);

        grid_.CopyFormCPUtoGPU();
    }

    void CopyFromGPUToCPU()
    {
        particle_input_buffer_.GetData(particle_input_data_);
        particle_debug_buffer_.GetData(particle_debug_data_);

        grid_.CopyFromGPUToCPU();
    }

    private void Start()
    {
        this.InitRendering();

        StartCoroutine(StartAsync());
    }
    // Use this for initialization
    IEnumerator StartAsync ()
    {
        Assert.IsNotNull(cs);

        this.InitCPUData();
        this.InitGPUData();

        //this.RandomToFillCircle(20, new Vector3(50, 0, 0));
        this.RandomToFillOneCircle(20, new Vector3(0, 0, 0));
        //Random step here
        this.CopyFormCPUtoGPU();

        this.VerifyData();

        //this.ResterizeParticleToGrid();
        //this.ComputeParticleVolumesAndDensities();


        //this.CopyFromGPUToCPU();
        //this.PrntInfo();
        yield return 0;
    }

    private void VerifyData()
    {
        foreach(var p in this.particle_input_data_)
        {
            p.Reset();
        }

        this.particle_input_buffer_.GetData(this.particle_input_data_);

        foreach(var p in this.particle_input_data_)
        {
            p.PrintInfo();
        }
    }

    private void PrntInfo()
    {
        foreach(var p in this.particle_input_data_)
        {
            if(p.density_ > 0)
            {
                p.PrintInfo();
            }
        }

        particle_input_data_[0].PrintInfo();
        //particle_weight_data_[0].PrintInfo();


        grid_.PrintInfo();
    }

    // Update is called once per frame
    void Update () {
		
	}

    void SetParticleBuffer(int kernel_id)
    {
        cs.SetBuffer(kernel_id, "_particle_buffer", particle_input_buffer_);
        cs.SetBuffer(kernel_id, "_particle_weight_buffer", particle_weight_buffer_);
        cs.SetBuffer(kernel_id, "_particle_debug_buffer", particle_debug_buffer_);
    }

    void InitData()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        this.SetParticleBuffer(id);
        cs.Dispatch(id, 512 / 8, 8, 1);
    }

    void InitGridData()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        this.grid_.SetGridBuffer(cs, id);
        cs.Dispatch(id, 512 / 8, 8, 1);
    }
    
    void ResterizeParticleMassAndVelocityToGrid()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        this.SetParticleBuffer(id);
        this.grid_.SetGridBuffer(cs, id);
        cs.Dispatch(id, 512/8, 8, 1);
    }

    void UpdateGridVelocity()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        this.SetParticleBuffer(id);
        this.grid_.SetGridBuffer(cs, id);
        cs.Dispatch(id, 512 / 8, 8, 1);
    }

    //from The Affine Particle-In-Cell (APIC) method
    //http://www.seas.upenn.edu/~cffjiang/research/mpmcourse/mpmcourse.pdf
    //     void ResterizeParticleToGridWithAPIC();
    //     void ComputeParticleVelocityWithAPIC();

    void ResterizeParticleToGrid()
    {
        //reset grid
        this.InitGridData();

        this.ResterizeParticleMassAndVelocityToGrid();
        //Velocity part moved to ResterizeParticleMassAndVelocityToGrid

        //last is update velocity
        this.UpdateGridVelocity();



    }
    void ComputeParticleVolumesAndDensities()
    {
        string function_name = System.Reflection.MethodBase.GetCurrentMethod().Name;
        int id = cs_kernal_name_index_map_[function_name].kernal_id;
        this.SetParticleBuffer(id);
        this.grid_.SetGridBuffer(cs, id);
        cs.Dispatch(id, 512 / 8, 8, 1);
    }
    //     void ComputeGridForce();
    //     void ComputeGridVelocity();
    //     void GridCollision();
    //     void ComputeParticleDeformationGradient();
    //     void ComputeParticleVelocity();
    //     void ParticleCollision();
    //     void ComputeParticlePosition();






    //==================================================================================================
    //rendering code


    void InitRendering()
    {
        mat_ = new Material(rs);
    }

    Material mat_;
    [SerializeField] private Shader rs;
    [SerializeField] private float particle_size_ = 0.1f;
    private void OnRenderObject()
    {
        Assert.IsNotNull(mat_);
        mat_.SetPass(0);
        mat_.SetBuffer("_buffer", this.particle_input_buffer_);
        mat_.SetBuffer("_small_buffer", this.small_particle_input_buffer_);
        mat_.SetMatrix("_inv_view_mat", Camera.main.worldToCameraMatrix.inverse);
        mat_.SetFloat("_particle_size", particle_size_);

        Graphics.DrawProcedural(MeshTopology.Points, NUMBER_OF_PARTICLES);
    }
}
