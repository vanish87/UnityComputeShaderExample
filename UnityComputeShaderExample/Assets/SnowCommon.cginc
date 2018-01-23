#ifndef SNOW_COMMON_INCLUDED
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define SNOW_COMMON_INCLUDED

static const int number_of_samples_ = 4;

struct SnowParticleStruct
{
	float3 position_;//physics location that differs from render element's
	float3 velocity_;
	float3 acceleration_;

	float mass_;
	float radius_;

	float density_;
	float volume_;

	float3x3 force_;

	float3x3 velocity_gradient_;

	int3 debug_grid_index_;

	float3x3 D;

	float3x3 Fe;
	float3x3 Fp;

	float3x3 R;
	float3x3 S;

	float3x3 s;
	float3x3 v;
	float3x3 d;

	
	
};

void Reset(SnowParticleStruct p)
{
	p.position_ = (float3)0;
	p.velocity_ = (float3)0;
	p.acceleration_ = (float3)0;

	p.mass_ = 0;
	p.radius_ = 0;

	p.density_ = 0;
	p.volume_ = 0;

	p.force_ = (float3x3)0;

	p.velocity_gradient_ = (float3x3)0;

	p.debug_grid_index_ = (int3)0;

	p.D = (float3x3)0;

	p.Fe = (float3x3)0;
	p.Fp = (float3x3)0;

	p.R = (float3x3)0;
	p.S = (float3x3)0;

	p.s = (float3x3)0;
	p.v = (float3x3)0;
	p.d = (float3x3)0;
}

struct ParticleWeight
{
	float weight_all_[number_of_samples_][number_of_samples_][number_of_samples_];
	float weight_gradient_all_[number_of_samples_][number_of_samples_][number_of_samples_];
};

struct ParticleWeightAdvance
{
	float3 weight_[number_of_samples_][number_of_samples_][number_of_samples_];
	//Debug only
	//float3 weight_dev_[number_of_samples_][number_of_samples_][number_of_samples_];
	float3 weight_gradient_[number_of_samples_][number_of_samples_][number_of_samples_];
};

void Reset(ParticleWeight w)
{
	w.weight_all_ = (float[number_of_samples_][number_of_samples_][number_of_samples_])0;
}

struct Cell
{
	float mass_;
	float3 momentum_;
	float3 velocity_;
	float3 velocity_new_;
	float3 force_;

	bool is_active_;
};

inline void Identify(inout float3x3 mat)
{
	mat = (float3x3)0;
	mat[0][0] = mat[1][1] = mat[2][2] = 1;
}

#endif