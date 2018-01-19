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

    static private int number_of_buffer_ = 100;

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


                         this.input_data_[i].A = new Matrix4x4(
                             new Vector4(1,4,7, 0),
                             new Vector4(2,5,8, 0),
                             new Vector4(3,6,9, 0),
                             new Vector4(0, 0, 0, 0));
//                          this.input_data_[i].A = new Matrix4x4(
//                              new Vector4(-3.72892f, 2.18524f, -5.94348f, 0),
//                              new Vector4(-9.25222f, 9.61006f, -7.49722f, 0),
//                              new Vector4(1.15526f, -5.82903f, 7.61113f, 0),
//                              new Vector4(0, 0, 0, 0));
            
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

    /*
        R:
        0.980581 -0.196116
        0.196116 0.980581
        S:
        1.56893 2.74563
        2.74563 3.53009
        AToVerify:
        1 2
        3 4
     */

    void VerifyData(Input[] input, Output[] output)
    {
        for (int i = 0; i < input.Length; ++i)
        {
            Matrix4x4 matirx_d = new Matrix4x4(
                new Vector4(output[i].D[0], 0, 0, 0),
                new Vector4(0, output[i].D[1], 0, 0),
                new Vector4(0, 0, output[i].D[2], 0),
                new Vector4(0, 0, 0, 0)
            );

//             Debug.LogFormat("A is \n{0}", input[i].A);
//             Debug.LogFormat("U is \n{0}", output[i].U);
//             Debug.LogFormat("D is {0}", output[i].D.ToString("F4"));
//             Debug.LogFormat("Vt is \n{0}", output[i].Vt);

            /*output[i].Vt = new Matrix4x4(
                new Vector4(1.56893f, 2.74563f, 0, 0),
                new Vector4(2.74563f, 3.53009f, 0, 0),
                new Vector4(0, 0, 0, 0),
                new Vector4(0, 0, 0, 0)
                );*/

            Matrix4x4 delta = output[i].U * matirx_d * output[i].Vt;
            delta.SetRow(0, input[i].A.GetRow(0) - delta.GetRow(0));
            delta.SetRow(1, input[i].A.GetRow(1) - delta.GetRow(1));
            delta.SetRow(2, input[i].A.GetRow(2) - delta.GetRow(2));
            delta.SetRow(3, input[i].A.GetRow(3) - delta.GetRow(3));

            //if (Mathf.Abs(delta.m00) > 0.001f)
            {
                Debug.LogFormat("A is \n{4}\n" +
                    "Retored is \n{5}\n" +
                    "output is \n" +
                    "U:\n{0}\n" +
                    "D:\n{1}\n" +
                    "Vt:\n{2}\n" +
                    "Delta is \n{3}\n", output[i].U, matirx_d, output[i].Vt, delta, input[i].A, output[i].U * matirx_d * output[i].Vt);
            }

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
