using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class MPMGrid
{
    //[SerializeField] private ComputeShader cs;
    ComputeBuffer input_buffer_;

    ComputeBuffer debug_buffer_;
    Cell[] output_data_;

    //int kernal_id_;

    const int VOXEL_GRID_SIZE = 32;
    const int NUMBER_OF_CELLS = VOXEL_GRID_SIZE * VOXEL_GRID_SIZE * VOXEL_GRID_SIZE;

    int test_count_ = 0;
    int error_count_ = 0;

    float time_total_ = 0;
    float time_count_ = 0;
        
    public struct Cell
    {        
        public float mass_;
        public Vector3 momentum_;
        public Vector3 velocity_;
        public Vector3 velocity_new_;
        public Vector3 force_;

        public bool is_active_;

        //void PrintInfo();
        public void Reset()
        {
            mass_ = 0;
            momentum_.Set(0, 0, 0);
            velocity_.Set(0, 0, 0);
            velocity_new_.Set(0, 0, 0);
            force_.Set(0, 0, 0);

            is_active_ = false;
        }

        internal void PrintInfo()
        {
            Debug.LogFormat("mass_: {0}", mass_);
            Debug.LogFormat("momentum_: {0}", momentum_.ToString("F5"));
            Debug.LogFormat("velocity_: {0}", velocity_.ToString("F5"));
            Debug.LogFormat("velocity_new_: {0}", velocity_new_.ToString("F5"));
            Debug.LogFormat("force_: {0}", force_.ToString("F5"));
            Debug.LogFormat("is_active_: {0}", is_active_);
        }
    };
    
    // Use this for initialization
    public void InitCPUData()
    {
        this.output_data_ = new Cell[NUMBER_OF_CELLS];
        for (int i = 0; i < this.output_data_.Length; i++)
        {
            //generator some random matrix
            this.output_data_[i] = new Cell();

            this.output_data_[i].momentum_ = new Vector3();
            this.output_data_[i].velocity_ = new Vector3();
            this.output_data_[i].velocity_new_ = new Vector3();
            this.output_data_[i].force_ = new Vector3();

            this.output_data_[i].Reset();
        }
    }

    public void InitGPUData()
    {
        int size = Marshal.SizeOf(typeof(Cell));
        //this.kernal_id_ = cs.FindKernel("ComputeGrid");
        input_buffer_ = new ComputeBuffer(NUMBER_OF_CELLS, size);
        debug_buffer_ = new ComputeBuffer(NUMBER_OF_CELLS, size);
    }

    public void CopyFormCPUtoGPU()
    {

    }

    public void CopyFromGPUToCPU()
    {
        input_buffer_.GetData(output_data_);
    }

    public void SetGridBuffer(ComputeShader cs, int kernel_id)
    {
        Assert.IsNotNull(cs);
        Assert.IsNotNull(input_buffer_);
        Assert.IsNotNull(debug_buffer_);

        cs.SetBuffer(kernel_id, "_grid_buffer", input_buffer_);
        cs.SetBuffer(kernel_id, "_grid_debug_buffer", debug_buffer_);
    }

    /*void RunCS ()
    {
        time_count_++;
        if (cs != null)
        {
            cs.SetBuffer(this.kernal_id, "_input_buffer", input_buffer_);
            cs.SetBuffer(this.kernal_id, "_output_buffer", debug_buffer_);

            float time = Time.realtimeSinceStartup;
            Profiler.BeginSample(this.GetType().Name);
            cs.Dispatch(this.kernal_id, NUMBER_OF_CELLS / 8, NUMBER_OF_CELLS / 8, 1);
            Profiler.EndSample();

            time_total_ += Time.realtimeSinceStartup - time;

            debug_buffer_.GetData(output_data_);

            VerifyData(input_data_, output_data_);
        }

	}*/

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
     

    private void OnRenderObject()
    {
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512 , 512), result_);

        string s = System.String.Format("input tested: {0}\nerror ratio {1:P4}\ntime average: {2:F5} ms", 
                                        test_count_, (error_count_ * 1.0f / test_count_), time_total_/time_count_ * 1000);
        GUI.TextArea(new Rect(100, 100, 300, 100), s);
    }

    internal void PrintInfo()
    {
        foreach(var d in output_data_)
        {
            if(d.is_active_)
            {
                d.PrintInfo();
            }
        }
        return;
    }
}
