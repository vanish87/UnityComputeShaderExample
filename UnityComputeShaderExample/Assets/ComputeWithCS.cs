using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class ComputeWithCS : MonoBehaviour {
    
    [SerializeField] private ComputeShader cs;
    ComputeBuffer input_buffer_;
    Input[] input_data_;

    ComputeBuffer output_buffer_;
    Output[] output_data_;

    int kernal_id;

    static private int number_of_buffer_ = 1;

    struct Input
    {
        public Matrix4x4 A;
    }

    struct Output
    {
        public Matrix4x4 U;
        public Vector4 D;
        public Matrix4x4 Vt;
    }

    // Use this for initialization
    void Start () {
        input_buffer_  = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(Input)));
        output_buffer_ = new ComputeBuffer(number_of_buffer_, Marshal.SizeOf(typeof(Output)));

        this.input_data_= new Input[number_of_buffer_];
        for (int i = 0; i < this.input_data_.Length; i++)
        {
            //generator some random matrix
            this.input_data_[i].A = new Matrix4x4(
                new Vector4(Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), 0), 
                new Vector4(Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), 0), 
                new Vector4(Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), Random.Range(-10.0f, 10), 0),
                new Vector4(0, 0, 0, 0));
        }

        input_buffer_.SetData(this.input_data_);

        this.output_data_ = new Output[number_of_buffer_];

        this.kernal_id = cs.FindKernel("ComputeSVD3D");


        this.RunCS();
    }

    // Update is called once per frame
    private void Update()
    {
        //this.RunCS();
    }

    void RunCS () {		

        if(cs != null)
        {
            cs.SetBuffer(this.kernal_id, "_input_buffer", input_buffer_);
            cs.SetBuffer(this.kernal_id, "_output_buffer", output_buffer_);

            Profiler.BeginSample("SVD3D");
            cs.Dispatch(this.kernal_id, 512 / 8, 512 / 8, 1);
            Profiler.EndSample();

            output_buffer_.GetData(output_data_);

            VerifyData(input_data_, output_data_);
        }

	}

    void VerifyData(Input[] input, Output[] output)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            Matrix4x4 matirx_d = new Matrix4x4(
                new Vector4( output[i].D[0], 0, 0, 0),
                new Vector4(0, output[i].D[1] , 0, 0),
                new Vector4(0, 0, output[i].D[2] , 0),
                new Vector4(0, 0, 0, 0)
            );

            Debug.LogFormat("A is \n{0}", input[i].A);
            Debug.LogFormat("U is \n{0}", output[i].U);
            Debug.LogFormat("D is {0}", output[i].D);
            Debug.LogFormat("Vt is \n{0}", output[i].Vt);

            //Assert.IsTrue(input[i].A == output[i].U * matirx_d * output[i].Vt);
        }
    }

    private void OnRenderObject()
    {
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);
    }
}
