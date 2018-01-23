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
    Input[] input_data_;

    ComputeBuffer output_buffer_;
    Output[] output_data_;

    //int kernal_id_;

    const int VOXEL_GRID_SIZE = 32;
    const int NUMBER_OF_CELLS = VOXEL_GRID_SIZE * VOXEL_GRID_SIZE * VOXEL_GRID_SIZE;

    int test_count_ = 0;
    int error_count_ = 0;

    float time_total_ = 0;
    float time_count_ = 0;
    
    public class Cell
    {        
		public Cell()
        {
            this.Reset();
        }

        float mass_;
        Vector3 momentum_ = new Vector3();
        Vector3 velocity_ = new Vector3();
        Vector3 velocity_new_ = new Vector3();
        Vector3 force_ = new Vector3();

        bool is_active_;

        //void PrintInfo();
        void Reset()
        {
            mass_ = 0;
            momentum_.Set(0, 0, 0);
            velocity_.Set(0, 0, 0);
            velocity_new_.Set(0, 0, 0);
            force_.Set(0, 0, 0);

            is_active_ = false;
        }
    };
    struct Input
    {
        public Cell cell;
    }

    struct Output
    {
        float dummy;
    }

    // Use this for initialization
    public void InitCPUData() {

        this.input_data_= new Input[NUMBER_OF_CELLS];
        for (int i = 0; i < this.input_data_.Length; i++)
        {
            //generator some random matrix
            this.input_data_[i].cell = new Cell();

        }
    }

    public void InitGPUData()
    {
        //this.kernal_id_ = cs.FindKernel("ComputeGrid");
        input_buffer_ = new ComputeBuffer(NUMBER_OF_CELLS, Marshal.SizeOf(typeof(Input)));
    }

    public void CopyFormCPUtoGPU()
    {
        input_buffer_.SetData(input_data_);
    }

    /*void RunCS ()
    {
        time_count_++;
        if (cs != null)
        {
            cs.SetBuffer(this.kernal_id, "_input_buffer", input_buffer_);
            cs.SetBuffer(this.kernal_id, "_output_buffer", output_buffer_);

            float time = Time.realtimeSinceStartup;
            Profiler.BeginSample(this.GetType().Name);
            cs.Dispatch(this.kernal_id, NUMBER_OF_CELLS / 8, NUMBER_OF_CELLS / 8, 1);
            Profiler.EndSample();

            time_total_ += Time.realtimeSinceStartup - time;

            output_buffer_.GetData(output_data_);

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

    void VerifyData(Input[] input, Output[] output)
    {
        for (int i = 0; i < input.Length; ++i)
        {
           
        }
    }

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
}
