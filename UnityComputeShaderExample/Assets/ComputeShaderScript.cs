using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

public class ComputeShaderScript : MonoBehaviour {

    [SerializeField] private ComputeShader cs;

    ComputeBuffer buffer_;
    ComputeBuffer buffer_soa_;

    int number_of_buffer_ = 10000;
    int kernal_index_ = -1;
    int kernal_index_soa_ = -1;

    [SerializeField] RenderTexture result_;

    Material mat_;

    public struct BufferStruct
    {
        public Vector4 pos;
        public Vector4 velocity;
    }

    public Vector4[] SOA_Position;
	// Use this for initialization
	void Start () {
        SOA_Position = new Vector4[number_of_buffer_];

        buffer_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(BufferStruct)));
        buffer_soa_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(Vector4)));
        kernal_index_ = cs.FindKernel("CSMain");
        kernal_index_soa_ = cs.FindKernel("CSMain_SOA");

        result_ = new RenderTexture(512,512,24);
        result_.enableRandomWrite = true;
        result_.Create();

        var pData = new BufferStruct[number_of_buffer_];
        for (int i = 0; i < pData.Length; i++)
        {
            pData[i].pos = new Vector4();
            pData[i].velocity = new Vector4();
        }

        buffer_.SetData(pData);
        pData = null;

        var newdata = new Vector4[number_of_buffer_];

        for (int i = 0; i < newdata.Length; i++)
        {
            newdata[i] = new Vector4();
        }

        buffer_soa_.SetData(newdata);

        cs.SetTexture(kernal_index_, "Result", result_);
        cs.SetBuffer(kernal_index_, "_buffer", buffer_);
        cs.SetBuffer(kernal_index_, "_pos_buffer_soa", buffer_soa_);
        cs.SetBuffer(kernal_index_, "_velocity_buffer_soa", buffer_soa_);
    }
	
	// Update is called once per frame
	void Update () {		

        if(cs != null)
        {
            Profiler.BeginSample("AOS");
            cs.Dispatch(this.kernal_index_, 512 / 8, 512 / 8, 1);
            Profiler.EndSample();
            Profiler.BeginSample("SOA");
            cs.Dispatch(this.kernal_index_soa_, 512 / 8, 512 / 8, 1);
            Profiler.EndSample();
        }

	}

    private void OnRenderObject()
    {
        mat_.SetBuffer("_pos_buffer_soa", buffer_soa_);

        Graphics.DrawProcedural(MeshTopology.Points, number_of_buffer_);
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);
    }
}
