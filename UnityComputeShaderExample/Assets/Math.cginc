#ifndef MATH_INCLUDED
#define MATH_INCLUDED


#define EPSILON 1e-6f

float2x2 G2(float c, float s)
{
	return float2x2(c, s, -s, c);
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


void GetSVD2D(float2x2 A, out float2x2 U, out float2 D, out float2x2 Vt)
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

		c = 1.0 / sqrt(t*t + 1);
		s = -t * c;

		D[0] = c*c *S[0][0] - 2 * c*s*S[0][1] + s*s*S[1][1];
		D[1] = s*s *S[0][0] + 2 * c*s*S[0][1] + c*c*S[1][1];

	}

	if (D[0] < D[1])
	{
		float temp = D[0];
		D[0] = D[1];
		D[1] = temp;

		Vt = G2(-s, c);
	}
	else
	{
		Vt = G2(c, s);
	}

	U = R*Vt;

	//before this variable Vt stores V
	Vt = transpose(Vt);
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


#endif