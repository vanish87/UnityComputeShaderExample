using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class ComputeShaderScript : MonoBehaviour {

    static int number_of_buffer_ = 10000;


    [SerializeField] private ComputeShader cs;
    [SerializeField] private Shader rs;

    ComputeBuffer buffer_;

    struct SOA_Buffer
    {
        public string name;
        public int size;

        public SOA_Buffer(string buffer_name, int strip_size)
        {
            name = buffer_name;
            size = strip_size;
        }
    }

    Dictionary<SOA_Buffer, ComputeBuffer> cs_buffer_soa_ = new Dictionary<SOA_Buffer, ComputeBuffer>()
    {
        { new SOA_Buffer("_buffer_pos"      , Marshal.SizeOf(typeof(Vector3))), null },
        { new SOA_Buffer("_buffer_velocity" , Marshal.SizeOf(typeof(Vector3))), null },
        { new SOA_Buffer("_buffer_mass"     , Marshal.SizeOf(typeof(float)))  , null },
    };    

    Dictionary<string, int> cs_kernal_name_index_map_ = new Dictionary<string, int>()
    {
            //{ "SVD3D", -1},
            //{ "Grid", -1},
            { "Particle", -1},
            { "Particle_SOA", -1},
    };

    Material mat_;

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct ParticleStruct
    {
        public Vector3 pos;
        public Vector3 velocity;
        public float   mass;
    }
    
	// Use this for initialization
	void Start () {
        buffer_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(ParticleStruct)));

        foreach(var buffer in cs_buffer_soa_.Keys.ToList())
        {
            cs_buffer_soa_[buffer] = new ComputeBuffer(number_of_buffer_, buffer.size);
        }

        foreach(var k in cs_kernal_name_index_map_.Keys.ToList())
        {
            cs_kernal_name_index_map_[k] = cs.FindKernel(k);
        }

        var pData = new ParticleStruct[number_of_buffer_];
        for (int i = 0; i < pData.Length; i++)
        {
            pData[i].pos = Random.insideUnitCircle;
            pData[i].velocity = new Vector3();
        }

        buffer_.SetData(pData);
        pData = null;
        
        mat_ = new Material(rs);
    }
	
	// Update is called once per frame
	void Update () {		

        if(cs != null)
        {
            foreach (var k in cs_kernal_name_index_map_)
            {
                if (k.Key == "Particle")
                {
                    cs.SetBuffer(k.Value, "_buffer", buffer_);

                    Profiler.BeginSample(k.Key);
                    cs.Dispatch(k.Value, 512 / 8, 512 / 8, 1);
                    Profiler.EndSample();
                }
                else
                {
                    foreach (var buffer in cs_buffer_soa_)
                    {
                        cs.SetBuffer(k.Value, buffer.Key.name, buffer.Value);
                    }

                    Profiler.BeginSample(k.Key);
                    cs.Dispatch(k.Value, 512 / 8, 512 / 8, 1);
                    Profiler.EndSample();
                }
            }
            
        }

	}

    private void OnRenderObject()
    {
        Assert.IsNotNull(mat_);
        mat_.SetPass(0);
        mat_.SetBuffer("_buffer", buffer_);

        Graphics.DrawProcedural(MeshTopology.Points, number_of_buffer_);
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);
    }
}
