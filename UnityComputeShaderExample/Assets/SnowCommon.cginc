#ifndef SNOW_COMMON_INCLUDED
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define SNOW_COMMON_INCLUDED

static const int number_of_samples_ = 4;

const static float
PARTICLE_DIAM = .0072f,		//Diameter of each particle; smaller = higher resolution
FRAMERATE = 1 / 60.0f,			//Frames per second
CFL = .04f,					//Adaptive timestep adjustment
MAX_TIMESTEP = 5e-4f,		//Upper timestep limit
FLIP_PERCENT = .95f,			//Weight to give FLIP update over PIC (.95)
CRIT_COMPRESS = 1 - 2.5e-2f,	//Fracture threshold for compression (1-2.5e-2)
CRIT_STRETCH = 1 + 7.5e-3f,	//Fracture threshold for stretching (1+7.5e-3)
HARDENING = 5.0f,			//How much plastic deformation strengthens material (10)
DENSITY = 100,				//Density of snow in kg/m^2 (400 for 3d)
YOUNGS_MODULUS = 1.4e5f,		//Young's modulus (springiness) (1.4e5)
POISSONS_RATIO = .2f,		//Poisson's ratio (transverse/axial strain ratio) (.2)
IMPLICIT_RATIO = 0,			//Percentage that should be implicit vs explicit for velocity update
MAX_IMPLICIT_ITERS = 30,	//Maximum iterations for the conjugate residual
MAX_IMPLICIT_ERR = 1e4f,		//Maximum allowed error for conjugate residual
MIN_IMPLICIT_ERR = 1e-4f,	//Minimum allowed error for conjugate residual
STICKY = .9f,				//Collision stickiness (lower = stickier)
GRAVITY = -9.8f;


static const float MU = YOUNGS_MODULUS / (2 + 2 * POISSONS_RATIO);
static const float LAMBDA = YOUNGS_MODULUS*POISSONS_RATIO / ((1 + POISSONS_RATIO)*(1 - 2 * POISSONS_RATIO));
static const float EPSILON = HARDENING;

static const int VOXEL_CELL_SIZE = 10;
static const int VOXEL_GRID_SIZE = 32;

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

	float3x3 B;
	float3x3 D;

	float3x3 Fe;
	float3x3 Fp;

	float3x3 R;
	float3x3 S;

	float3x3 s;
	float3x3 v;
	float3x3 d;


	int3 debug_grid_index_;	
};


struct SmallParticleStruct
{
	float3 position;
	float3 velocity;
	float  mass;

	float3x3 force_;

	float3x3 velocity_gradient_;

	float3x3 B;
	float3x3 D;

	float3x3 Fe;
	float3x3 Fp;

	float3x3 R;
	float3x3 S;

	float3x3 s;
	float3x3 v;
	float3x3 d;

	int3 debug_grid_index_;
};

void Reset(inout SnowParticleStruct p)
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
	float3 weight_[number_of_samples_][number_of_samples_][number_of_samples_];
	//Debug only
	//float3 weight_dev_[number_of_samples_][number_of_samples_][number_of_samples_];
	float3 weight_gradient_[number_of_samples_][number_of_samples_][number_of_samples_];
};

void Reset(inout ParticleWeight w)
{
	w.weight_all_ = (float[number_of_samples_][number_of_samples_][number_of_samples_])0;
	w.weight_gradient_all_ = (float[number_of_samples_][number_of_samples_][number_of_samples_])0;


	w.weight_ = (float3[number_of_samples_][number_of_samples_][number_of_samples_])0;
	w.weight_gradient_ = (float3[number_of_samples_][number_of_samples_][number_of_samples_])0;
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

void Reset(inout Cell cell)
{
	cell.mass_ = 0;
	cell.momentum_ = (float3)0;
	cell.velocity_ = (float3)0;
	cell.velocity_new_ = (float3)0;
	cell.force_ = (float3)0;

	cell.is_active_ = false;
}
inline void Identify(inout float3x3 mat)
{
	mat = (float3x3)0;
	mat[0][0] = mat[1][1] = mat[2][2] = 1;
}

#endif