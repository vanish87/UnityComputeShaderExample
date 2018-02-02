using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

unsafe public class ComputeWithDefinedStruct : MonoBehaviour
{

    [SerializeField] private ComputeShader cs;
    ComputeBuffer input_buffer_;
    Input[] input_data_;

    ComputeBuffer output_buffer_;
    Output[] output_data_;

    int kernal_id;

    static private int number_of_buffer_ = 100;
    float range = 1000;

    int test_count_ = 0;
    int error_count_ = 0;

    float time_total_ = 0;
    float time_count_ = 0;
    float current_time_ = 0;

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    unsafe struct Input
    {
        public float3x3 A;
        public float3x3 B;
        public float3x3 C;

        public fixed float D[64];
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    unsafe struct Output
    {
        public float3x3 A;
        public float3x3 B;
        public float3x3 C;
        public Matrix4x4 matrix;//good

        public fixed float D[64];
        //public float[] array; //error
        //public float[,] 2darray; //error

    }

    // Use this for initialization
    void Start()
    {        
        int size = Marshal.SizeOf(typeof(Input));// + Marshal.SizeOf(typeof(float)) * 4;
        Assert.IsTrue(size == Marshal.SizeOf(typeof(float3x3)) * 3 + Marshal.SizeOf(typeof(float)) * 64);
        input_buffer_ = new ComputeBuffer(number_of_buffer_, size);
        output_buffer_ = new ComputeBuffer(number_of_buffer_, size);

        this.input_data_ = new Input[number_of_buffer_];
        this.RandomInputAndSetToBuffer();

        this.output_data_ = new Output[number_of_buffer_];
        for (int i = 0; i < this.output_data_.Length; i++)
        {
            //generator some random matrix
            this.output_data_[i].A = new float3x3();
            this.output_data_[i].B = new float3x3();
            this.output_data_[i].C = new float3x3();

            fixed (Output* p = &this.output_data_[i])
            {
                p->D[0] = 10;
            }
        }

        this.kernal_id = cs.FindKernel("DefinedStruct");


        this.RunCS();
    }

    // Update is called once per frame
    private void Update()
    {
        //this.RunCS();
    }

    void RandomInputAndSetToBuffer()
    {
        test_count_ += this.input_data_.Length;
        for (int i = 0; i < this.input_data_.Length; i++)
        {
            //generator some random matrix
            this.input_data_[i].A = new float3x3();
            this.input_data_[i].B = new float3x3();
            this.input_data_[i].C = new float3x3();

            this.input_data_[i].A.m00 = 1;
            this.input_data_[i].B.m00 = 2;
            this.input_data_[i].C.m00 = 3;

            fixed (Input* p = &this.input_data_[i])
            {
                p->D[0] = 10;
            }
        }
        input_buffer_.SetData(input_data_);
    }

    void RunCS()
    {
        time_count_++;
        this.RandomInputAndSetToBuffer();

        if (cs != null)
        {
            cs.SetBuffer(this.kernal_id, "_input_buffer", input_buffer_);
            cs.SetBuffer(this.kernal_id, "_output_buffer", output_buffer_);

            float time = Time.realtimeSinceStartup;
            Profiler.BeginSample("DefinedStruct");
            cs.Dispatch(this.kernal_id, number_of_buffer_ / 8, number_of_buffer_ / 8, 1);
            Profiler.EndSample();

            current_time_ = Time.realtimeSinceStartup - time;
            time_total_ += current_time_;

            output_buffer_.GetData(output_data_);

            VerifyData(input_data_, output_data_);
        }

    }

    private void VerifyData(Input[] input_data_, Output[] output_data_)
    {
        Debug.LogFormat("{0}, {1}, {2}", output_data_[0].A.m00, output_data_[0].B.m00, output_data_[0].C.m00);

        fixed (Output* p = &this.output_data_[0])
        {
            Debug.LogFormat("{0}", p->D[0]);
        }
    }

    private void OnRenderObject()
    {
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);

        string s = System.String.Format("input tested: {0}\nerror ratio {1:P4}\ntime average: {2:F5} ms\nCurrent Time:{3:F5}",
                                        test_count_, (error_count_ * 1.0f / test_count_), time_total_ / time_count_ * 1000, current_time_ * 1000f);
        GUI.TextArea(new Rect(100, 100, 300, 100), s);
    }
}
