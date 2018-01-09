using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ComputeShaderScript : MonoBehaviour {

    [SerializeField] private ComputeShader cs;

    ComputeBuffer buffer_;

    int number_of_buffer_ = 10000;
    int kernal_index_ = -1;

    [SerializeField] RenderTexture result_;

    Material mat_;

    public struct BufferStruct
    {
        Vector4 pos;
    }
	// Use this for initialization
	void Start () {

        buffer_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(BufferStruct)));
        kernal_index_ = cs.FindKernel("CSMain");

        result_ = new RenderTexture(512,512,24);
        result_.enableRandomWrite = true;
        result_.Create();

        cs.SetTexture(kernal_index_, "Result", result_);
        cs.SetBuffer(kernal_index_, "_buffer", buffer_);
	}
	
	// Update is called once per frame
	void Update () {		

        if(cs != null)
        {
            cs.Dispatch(this.kernal_index_, 512/8, 512/8, 1);
        }
	}

    private void OnRenderObject()
    {
        Graphics.DrawProcedural(MeshTopology.Points, number_of_buffer_);
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);
    }
}
