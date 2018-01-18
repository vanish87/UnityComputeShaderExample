#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED
struct ParticleStruct
{
	float3 position;
	float3 velocity;
	float  mass;
};

float4x4 Identity =
{
	{ 1, 0, 0, 0 },
	{ 0, 1, 0, 0 },
	{ 0, 0, 1, 0 },
	{ 0, 0, 0, 1 }
}; 

float3x3 Identity3x3 =
{
	{ 1, 0, 0},
	{ 0, 1, 0},
	{ 0, 0, 1},
};

#endif