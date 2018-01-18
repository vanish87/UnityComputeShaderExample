#ifndef MATH_INCLUDED
#define MATH_INCLUDED


#define EPSILON 1e-6f

float2x2 G2(float c, float s)
{
	return float2x2(c, s, -s, c);
}

float3x3 G3_12(float c, float s)
{
	return float3x3( c, s, 0 , 
					-s, c, 0 , 
					 0, 0, 1 );
}


float3x3 G3_23(float c, float s)
{
	return float3x3(1, 0, 0,
					0, c, s,
					0,-s, c);
}


float3x3 G3_13(float c, float s)
{
	return float3x3( c, 0, s,
					 0, 1, 0,
					-s, 0, c);
}

void GetPolarDecomposition2D(float2x2 A, out float2x2 R, out float2x2 S)
{
	float x = A[0][0] + A[1][1];
	float y = A[1][0] - A[0][1];

	float d = sqrt(x*x + y*y);

	float c = 1;
	float s = 0;

	R = G2(c, s);

	float Zero = 0;
	if (abs(d-Zero) > EPSILON)
	{
		d = 1.0f / d;
		R = G2(x * d, -y * d);
	}

	S = transpose(R) * A;
	/*
	PRINT_WARNING("Start GetPolarDecomposition2D");
	PRINT_VAR(A);
	PRINT("To verify:");
	PRINT_VAR(R);
	PRINT_VAR(S);
	float2x2 AToVerify = R * S;
	PRINT_VAR(AToVerify);
	PRINT_WARNING("End GetPolarDecomposition2D");

	const float EPSILON = 1e-4f;
	CHECK_ASSERT(Math::IsFloatEqual(A[0][0], AToVerify[0][0]));
	CHECK_ASSERT(Math::IsFloatEqual(A[0][1], AToVerify[0][1]));
	CHECK_ASSERT(Math::IsFloatEqual(A[1][0], AToVerify[1][0]));
	CHECK_ASSERT(Math::IsFloatEqual(A[1][1], AToVerify[1][1]));*/
}


void GetSVD2D(float2x2 A, out float2x2 U, out float2 D, out float2x2 V)
{
	float2x2 R;
	float2x2 S;

	GetPolarDecomposition2D(A, R, S);

	float c = 1;
	float s = 0;

	if (abs(S[0][1]-0) > EPSILON)
	{
		D[0] = S[0][0];
		D[1] = S[1][1];
	}
	else
	{
		float taw = 0.5f * (S[0][0] - S[1][1]);
		float w = sqrt(taw * taw + S[0][1] * S[0][1]);
		float t = taw > 0 ? S[0][1] / (taw + w) : S[0][1] / (taw - w);

		c = rsqrt(t*t + 1);
		s = -t * c;

		D[0] = c*c *S[0][0] - 2 * c*s*S[0][1] + s*s*S[1][1];
		D[1] = s*s *S[0][0] + 2 * c*s*S[0][1] + c*c*S[1][1];

	}

	if (D[0] < D[1])
	{
		float temp = D[0];
		D[0] = D[1];
		D[1] = temp;

		V = G2(-s, c);
	}
	else
	{
		V = G2(c, s);
	}

	U = R*V;

	//before this variable Vt stores V
	//Vt = transpose(Vt);
	/*
	PRINT_WARNING("Start GetSVD2D");
	PRINT_VAR(A);
	PRINT_VAR(U);
	PRINT_VAR(D);
	PRINT_VAR(Vt);

	PRINT("To verify:");
	float2x2 DMatrix;
	DMatrix[0][0] = D[0];
	DMatrix[1][1] = D[1];
	float2x2 AToVerify = U * DMatrix * Vt;
	PRINT_VAR(AToVerify);
	PRINT_WARNING("End GetSVD2D");

	const float EPSILON = 1e-4f;
	CHECK_ASSERT(Math::IsFloatEqual(A[0][0], AToVerify[0][0]));
	CHECK_ASSERT(Math::IsFloatEqual(A[0][1], AToVerify[0][1]));
	CHECK_ASSERT(Math::IsFloatEqual(A[1][0], AToVerify[1][0]));
	CHECK_ASSERT(Math::IsFloatEqual(A[1][1], AToVerify[1][1]));*/
}


void Zerochasing(inout float3x3 U, inout float3x3 A, inout float3x3 V)
{
	float3x3 G = G3_23(A[0][1], A[0][2]);
	A = A * G;
	U = transpose(G) * U;

	G = G3_23(A[0][1], A[0][2]);
	A = transpose(G) * A;
	V = transpose(G) * V;

	G = G3_23(A[1][1], A[2][1]);
	A = transpose(G) * A;
	U = U * G;
}

void Bidiagonalize(inout float3x3 U, inout float3x3 A, inout float3x3 V)
{
	float3x3 G = G3_23(A[1][0],A[2][1]);
	A = transpose(G) * A;
	U = U * G;

	Zerochasing(U, A, V);
}

float FrobeniusNorm(float3x3 input)
{
	float ret = 0;
	for (int i = 0; i < 3; ++i)
	{
		for (int j = 0; j < 3; ++j)
		{
			ret += input[i][j] * input[i][j];
		}
	}

	return ret;
}

/*void FlipSign(int index, inout float3x3 mat, inout float3 sigma)
{
	mat[0][index] = -mat[0][index];
	mat[1][index] = -mat[1][index];
	mat[2][index] = -mat[2][index];
	sigma[index]  = -sigma[index];
}*/

void FlipSignColumn(inout float3x3 mat, int col)
{
	mat[0][col] = -mat[0][col];
	mat[1][col] = -mat[1][col];
	mat[2][col] = -mat[2][col];
}

inline void Swap(inout float a, inout float b)
{
	float temp = a;
	a = b;
	b = temp;
}


inline void Swap(inout float3 a, inout float3 b)
{
	float3 temp = a;
	a = b;
	b = temp;
}

inline void SwapColumn(inout float3x3 a, int col_a, inout float3x3 b, int col_b)
{
	float3 temp = float3(a[0][col_a], a[1][col_a], a[2][col_a]);
	a[0][col_a] = b[0][col_b];
	a[1][col_a] = b[1][col_b];
	a[2][col_a] = b[2][col_b];

	b[0][col_b] = temp[0];
	b[1][col_b] = temp[1];
	b[2][col_b] = temp[2];
}

void SortWithTopLeftSub(float3x3 U, float3 sigma, float3x3 V)
{
	if (abs(sigma[1]) >= abs(sigma[2]))
	{
		if (sigma[1] < 0)
		{
			FlipSignColumn(U, 1); sigma[1] = -sigma[1];
			FlipSignColumn(U, 2); sigma[2] = -sigma[2];
			//FlipSign(1, U, sigma);
			//FlipSign(2, U, sigma);
		}
		return;
	}
	if (sigma[2] < 0)
	{
		FlipSignColumn(U, 1); sigma[1] = -sigma[1];
		FlipSignColumn(U, 2); sigma[2] = -sigma[2];
	}
	Swap(sigma[1],sigma[2]);
	SwapColumn(U, 1, U, 2);
	SwapColumn(V, 1, V, 2);

	if (sigma[1] > sigma[0])
	{
		Swap(sigma[0], sigma[1]);;
		SwapColumn(U, 0, U, 1);
		SwapColumn(V, 0, V, 1);
	}
	else
	{
		FlipSignColumn(U, 2);
		FlipSignColumn(V, 2);
	}
}

void SortWithBotRightSub(float3x3 U, float3 sigma, float3x3 V)
{
	if (abs(sigma[0]) >= abs(sigma[1]))
	{
		if (sigma[0] < 0)
		{
			FlipSignColumn(U, 0); sigma[0] = -sigma[0];
			FlipSignColumn(U, 2); sigma[2] = -sigma[2];
			//FlipSign(0, U, sigma);
			//FlipSign(2, U, sigma);
		}
		return;
	}
	Swap(sigma[0], sigma[1]);
	SwapColumn(U, 0, U, 1);
	SwapColumn(V, 0, V, 1);

	if (abs(sigma[1]) < abs(sigma[2]))
	{
		Swap(sigma[1], sigma[2]);;
		SwapColumn(U, 1, U, 2);
		SwapColumn(V, 1, V, 2);
	}
	else
	{
		FlipSignColumn(U, 2);
		FlipSignColumn(V, 2);
	}

	if (sigma[1] < 0)
	{
		FlipSignColumn(U, 1); sigma[1] = -sigma[1];
		FlipSignColumn(U, 2); sigma[2] = -sigma[2];
		//FlipSign(1, U, sigma);
		//FlipSign(2, U, sigma);
	}
}

void SolveReducedTopLeft(float3x3 B, float3x3 U, float3 sigma, float3x3 V)
{
	float s3 = B[2][2];
	//float2x2 u = G2(1, 0);
	//float2x2 v = G2(1, 0);

	float2x2 top_left = float2x2(B[0][0],B[0][1],  B[1][0],B[1][1]);

	float2x2 A2 = top_left;
	float2x2 U2;
	float2 D2;
	float2x2 V2;
	GetSVD2D(A2, U2, D2, V2);

	float3x3 u3 = G3_12(U2[0][0], U2[0][1]);
	float3x3 v3 = G3_12(V2[0][0], V2[0][1]);

	U = U * u3;
	V = V * v3;
	sigma = float3(D2,s3);
}


void SolveReducedBotRight(float3x3 B, float3x3 U, float3 sigma, float3x3 V)
{
	float s1 = B[0][0];
	//float2x2 u = G2(1, 0);
	//float2x2 v = G2(1, 0);

	float2x2 bot_right = float2x2(B[1][1], B[1][2], B[2][1], B[2][2]);

	float2x2 A2 = bot_right;
	float2x2 U2;
	float2 D2;
	float2x2 V2;
	GetSVD2D(A2, U2, D2, V2);

	float3x3 u3 = G3_12(U2[0][0], U2[0][1]);
	float3x3 v3 = G3_12(V2[0][0], V2[0][1]);

	U = U * u3;
	V = V * v3;
	sigma = float3(s1, D2);
}

void PostProcess(float3x3 B, inout float3x3 U, inout float3x3 V, float3 alpha, float2 beta, out float3 sigma, float tao)
{
	if (abs(beta[1]) <= tao)
	{
		SolveReducedTopLeft(B, U, sigma, V);
		SortWithTopLeftSub(U, sigma, V);
	}
	else if (abs(beta[0]) <= tao)
	{
		SolveReducedBotRight(B, U, sigma, V);
		SortWithBotRightSub(U, sigma, V);
	}
	else if (abs(alpha[2]) <= tao)
	{
		float3x3 G_ = G3_23(B[1][2], B[2][2]);
		B = transpose(G_) * B;
		U = U * G_;

		SolveReducedTopLeft(B, U, sigma, V);
		SortWithTopLeftSub(U, sigma, V);
	}
	else if (abs(alpha[0]) <= tao)
	{
		float3x3 G_ = G3_12(B[0][1], B[1][1]);
		B = transpose(G_) * B;
		U = U * G_;

		G_ = G3_13(B[0][2], B[2][2]);
		B = transpose(G_) * B;
		U = U * G_;

		SolveReducedBotRight(B, U, sigma, V);
		SortWithBotRightSub(U, sigma, V);
	}
}


void GetSVD3D(float3x3 A, out float3x3 U, out float3 D, out float3x3 V)
{
	float3x3 B = A;
	U = Identity3x3;
	V = Identity3x3;

	Bidiagonalize(U, B, V);

	float3 alpha = float3(B[0][0], B[1][1], B[2][2]);
	float2 beta  = float2(B[0][1], B[1][2]);
	float2 gamma = float2(alpha[0] * beta[0], alpha[1] * beta[1]);

	float tol = 128 * EPSILON;
	float tao = tol * max(0.5 * FrobeniusNorm(B), 1.0);

	while (abs(alpha[0]) > tao && abs(alpha[1]) > tao && abs(alpha[2]) > tao &&
		   abs(beta[0])  > tao && abs(beta[1]) > tao)
	{
		float a1 = alpha[1] * alpha[1] + beta[0] * beta[0];
		float a2 = alpha[2] * alpha[2] + beta[1] * beta[1];
		float b1 = gamma[1];

		float d = (a1 - a2) * 0.5;
		float mu = (b1 * b1) / (abs(d) + sqrt(d*d + b1*b1));
		//copy sign from d to mu
		float d_sign = sign(d);
		mu = d_sign * abs(mu);

		float3x3 G = G3_12(alpha[0] * alpha[0] - mu, gamma[0]);
		B = B * G;
		V = V * G;

		Zerochasing(U, B, V);

		alpha = float3(B[0][0], B[1][1], B[2][2]);
		beta = float2(B[0][1], B[1][2]); 
		gamma = float2(alpha[0] * beta[0], alpha[1] * beta[1]);
	}

	PostProcess(B, U, V, alpha, beta, D, tao);
}

#endif