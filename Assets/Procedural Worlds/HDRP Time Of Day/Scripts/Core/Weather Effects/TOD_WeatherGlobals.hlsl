//UNITY_SHADER_NO_UPGRADE
#ifndef TOD_WEATHER_GLOBALS_INCLUDED
#define TOD_WEATHER_GLOBALS_INCLUDED

//Rain Data
float4 _PW_RainDataA;
float4 _PW_RainDataB;
Texture2D _PW_RainMap;

//Sand Data
float4 _PW_SandDataA;
Texture2D _PW_SandAlbedoMap;
Texture2D _PW_SandNormalMap;
Texture2D _PW_SandMaskMap;

//Snow Data
#if !defined(_SNOW_ON)
float4 _PW_SnowDataA;
float4 _PW_SnowDataB;
Texture2D _PW_SnowAlbedoMap;
Texture2D _PW_SnowNormalMap;
Texture2D _PW_SnowMaskMap;
float4 _PW_SnowColor;
#endif

float4 _WorldSpaceLightPos0;

#ifdef MATERIAL_QUALITY_HIGH
#define PARALLAX_ENABLED
#define PARALLAX_SEARCH_STEPS
#define AFFINE_STEPS 3
#elif MATERIAL_QUALITY_MEDIUM
#define PARALLAX_ENABLED
#endif

#ifndef MAX_STEPS
	#define MAX_STEPS 100
#endif

void ApplyParallaxWeather_float(Texture2D Heightmap, SamplerState samp, float4 HeightMapChannel, float2 uv, float3 viewDir, int steps, float parallaxStrength, out float2 parallaxUVs)
{
	#ifdef PARALLAX_ENABLED
	float4 deriv = float4(ddx(uv), ddy(uv));
	UNITY_BRANCH
	if(steps > 1)
	{
		viewDir = normalize(viewDir);
		viewDir.xy /= (viewDir.z + 0.42);
		float2 uvOffset = 0;
		float stepSize = 1.0 / min(steps, MAX_STEPS);
		float2 uvDelta = viewDir.xy * (stepSize * parallaxStrength);
		float stepHeight = 1;
		float surfaceHeight = dot(HeightMapChannel, SAMPLE_TEXTURE2D(Heightmap, samp, uv));


		float2 prevUVOffset = uvOffset;
		float prevStepHeight = stepHeight;
		float prevSurfaceHeight = surfaceHeight;


		for(int i = 1; i < min(steps, MAX_STEPS) && stepHeight > surfaceHeight; i++)
		{
			prevUVOffset = uvOffset;
			prevStepHeight = stepHeight;
			prevSurfaceHeight = surfaceHeight;

			uvOffset -= uvDelta;
			stepHeight -= stepSize;
			surfaceHeight = dot(HeightMapChannel, SAMPLE_TEXTURE2D_GRAD(Heightmap, samp, uv + uvOffset, deriv.xy, deriv.zw));

		}

		#ifdef PARALLAX_SEARCH_STEPS
		for (int j = 0; j < AFFINE_STEPS; j++) 
		{
			uvDelta *= 0.5;
			stepSize *= 0.5;

			if (stepHeight < surfaceHeight) {
				uvOffset += uvDelta;
				stepHeight += stepSize;
			}
			else {
				uvOffset -= uvDelta;
				stepHeight -= stepSize;
			}
			surfaceHeight = dot(HeightMapChannel, SAMPLE_TEXTURE2D_GRAD(Heightmap, samp, uv + uvOffset, deriv.xy, deriv.zw));
		}
		#else
		float prevDifference = prevStepHeight - prevSurfaceHeight;
		float difference = surfaceHeight - stepHeight;
		float t = prevDifference / (prevDifference + difference);
		uvOffset = prevUVOffset - uvDelta * t;
		#endif

		parallaxUVs = uv + uvOffset;
	}
	else
	{
		parallaxUVs = uv;
	}
    
	#else
	parallaxUVs = uv;
	#endif
}



void getRainGlobals_float(out float4 rainDataA, out float4 rainDataB)
{
	rainDataA = _PW_RainDataA;
	rainDataB = _PW_RainDataB;
}


//Sand Data
void getSandGlobals_float(out float4 sandDataA)
{
	sandDataA = _PW_SandDataA;
}

void getSandMaterial_float(in float3 worldPosition, float3 viewDir, float steps, SamplerState samp, out float4 sandAlbedo, out float3 sandNormal, out float4 sandMask)
{
	float2 sandUVs = worldPosition.xz / _PW_SandDataA.a;

	#ifndef MATERIAL_QUALITY_LOW
		#ifdef MATERIAL_QUALITY_HIGH
			float2 newSandUVs;
			ApplyParallaxWeather_float(_PW_SandMaskMap, samp, float4(0,0,1,0), sandUVs, viewDir, steps, 0.1, newSandUVs);
					
			sandAlbedo = SAMPLE_TEXTURE2D(_PW_SandAlbedoMap, samp, newSandUVs);
			sandNormal = UnpackNormal(SAMPLE_TEXTURE2D(_PW_SandNormalMap, samp, newSandUVs));
			sandMask = SAMPLE_TEXTURE2D(_PW_SandMaskMap, samp, newSandUVs);
		#else
			sandAlbedo = SAMPLE_TEXTURE2D(_PW_SandAlbedoMap, samp, sandUVs);
			sandNormal = UnpackNormal(SAMPLE_TEXTURE2D(_PW_SandNormalMap, samp, sandUVs));
			sandMask = SAMPLE_TEXTURE2D(_PW_SandMaskMap, samp, sandUVs);
		#endif
	
	#else
		sandAlbedo = float4(0.125,0.066125,0.0,0);
		sandNormal = float3(0,0,1);
		sandMask = float4(0,1,0,0.0f);
	#endif

}

//Snow Data
void getSnowGlobals_float(out float4 snowDataA, out float4 snowDataB)
{
	snowDataA = _PW_SnowDataA;
	snowDataB = _PW_SnowDataB;
}

void getSnowMaterial_float(in float3 worldPosition, float3 viewDir, float steps, SamplerState samp, out float4 snowAlbedo, out float3 snowNormal, out float4 snowMask)
{
	float2 snowUVs = worldPosition.xz / _PW_SnowDataB.r;

	#ifndef MATERIAL_QUALITY_LOW
		#ifdef MATERIAL_QUALITY_HIGH
			float2 newSnowUVs;
			ApplyParallaxWeather_float(_PW_SnowMaskMap, samp, float4(0,0,1,0), snowUVs, viewDir, steps, 0.1, newSnowUVs);
			
			snowAlbedo = SAMPLE_TEXTURE2D(_PW_SnowAlbedoMap, samp, newSnowUVs) * _PW_SnowColor;
			snowNormal = UnpackNormal(SAMPLE_TEXTURE2D(_PW_SnowNormalMap, samp, newSnowUVs));
			snowMask = SAMPLE_TEXTURE2D(_PW_SnowMaskMap, samp, newSnowUVs);
		#else
			snowAlbedo = SAMPLE_TEXTURE2D(_PW_SnowAlbedoMap, samp, snowUVs) * _PW_SnowColor;
			snowNormal = UnpackNormal(SAMPLE_TEXTURE2D(_PW_SnowNormalMap, samp, snowUVs));
			snowMask = SAMPLE_TEXTURE2D(_PW_SnowMaskMap, samp, snowUVs);
		#endif

	#else
		snowAlbedo = float4(0.9,0.9,0.9,0) * _PW_SnowColor;
		snowNormal = float3(0,0,1);
		snowMask = float4(0,1,0,0.5);
	#endif
}


float generateRain(float pointSample, float speed, float offset)
{
	float time = frac((_Time.y * speed) + offset);
	float rainGen = sin(lerp(0, -20, time) * pointSample);
	float timeMask = sin(time * 3.1415);

	return saturate(rainGen * timeMask);
}

float sampleRain(float2 uv, float4 pointData)
{
	float rainA = generateRain(pointData.r, _PW_RainDataB.r, 0);
	float rainB = generateRain(pointData.g, _PW_RainDataB.r, 0.5);
	float rainC = generateRain(pointData.b, _PW_RainDataB.r, 0.75);
	float rainD = generateRain(pointData.a, _PW_RainDataB.r, 0.25);

	return saturate(rainA + rainB + rainC + rainD);
}

void getRainNormals_float(in float3 worldPosition, SamplerState samp, out float3 rainNormal)
{	
	
	float2 rainCenterUVs = worldPosition.xz / _PW_RainDataB.a;
	float4 centerRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, samp, rainCenterUVs);

	float2 rainXDerivUVs = (worldPosition.xz / _PW_RainDataB.a) + float2(0.001, 0);
	float4 xDerivRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, samp, rainXDerivUVs);

    float2 rainYDerivUVs = (worldPosition.xz / _PW_RainDataB.a) + float2(0, 0.001);
	float4 yDerivRainMap = SAMPLE_TEXTURE2D(_PW_RainMap, samp, rainYDerivUVs);

	//Sample center rain
	float centerRain = sampleRain(rainCenterUVs, centerRainMap);
	float xDerivRain = sampleRain(rainXDerivUVs, xDerivRainMap);
	float yDerivRain = sampleRain(rainYDerivUVs, yDerivRainMap);

	float xDeriv = centerRain - xDerivRain;
	float yDeriv = centerRain - yDerivRain;

	float3 xVector = float3(xDeriv, 1, 0);
	float3 yVector = float3(0, 1, yDeriv);

	rainNormal =  cross(xVector, yVector);

}

float heightLerp(float transitionPhase, float height, float contrast)
{
	float alpha = saturate((height - 1) + (transitionPhase* 2));
	return saturate(lerp(1 - contrast,contrast + 1,alpha));
}

float3 normalStrength(float3 In, float Strength)
{
	return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

float3 normalBlend(float3 A, float3 B)
{
	return normalize(float3(A.rg + B.rg, A.b * B.b));
}

void SampleSand_float(float3 worldPosition, float3 worldNormal, float3 viewDir, float steps, SamplerState samp, float4 i_Albedo, float3 i_Normal, float4 i_Mask, out float4 o_Albedo, out float3 o_Normal, out float4 o_Mask)
{
	#if _TOD_SAND_OFF
		o_Albedo = i_Albedo;
		o_Normal = i_Normal;
		o_Mask = i_Mask;
	#else
		o_Albedo = float4(0,0,0,0);
		o_Normal = float3(0,0,0);
		o_Mask = float4(0,0,0,0);
		
		float4 sandAlbedo;
		float4 sandMask;
		float3 sandNormal;
		getSandMaterial_float(worldPosition, viewDir, steps, samp, sandAlbedo, sandNormal, sandMask);
		float4 sandDataA = _PW_SandDataA;
		float heightAlpha = heightLerp(sandDataA.r * 0.5, 1 - i_Mask.b, 1);
		heightAlpha = (i_Mask.b == 0 || i_Mask.b == 1) ? sandDataA.r : heightAlpha;

		o_Albedo = lerp(i_Albedo, sandAlbedo, heightAlpha);
		#ifndef MATERIAL_QUALITY_LOW
		o_Normal = lerp(i_Normal, sandNormal, heightAlpha);
		#else
		o_Normal = lerp(i_Normal, normalStrength(i_Normal, 0.75), heightAlpha);
		#endif

		o_Mask = lerp(i_Mask, sandMask, heightAlpha);

		float worldHeightMask = smoothstep(-sandDataA.b, sandDataA.b, worldPosition.g - sandDataA.g);
		worldHeightMask = (1 - worldHeightMask) * saturate(dot(worldNormal, float3(0,1,0)));

		o_Albedo = lerp(i_Albedo, o_Albedo, worldHeightMask);
		o_Normal = lerp(i_Normal, o_Normal, worldHeightMask);
		o_Mask = lerp(i_Mask, o_Mask, worldHeightMask);
	#endif
	
}

void SampleSnow_float(float3 worldPosition, float3 worldNormal, float3 viewDir, float steps, SamplerState samp, float4 i_Albedo, float3 i_Normal, float4 i_Mask, out float4 o_Albedo, out float3 o_Normal, out float4 o_Mask)
{
	#if _TOD_SNOW_OFF
		o_Albedo = i_Albedo;
		o_Normal = i_Normal;
		o_Mask = i_Mask;
	#else
		o_Albedo = float4(0,0,0,0);
		o_Normal = float3(0,0,0);
		o_Mask = float4(0,0,0,0);
		
		float4 snowAlbedo;
		float4 snowMask;
		float3 snowNormal;
		getSnowMaterial_float(worldPosition, viewDir, steps, samp, snowAlbedo, snowNormal, snowMask);
		float4 snowDataA = _PW_SnowDataA;
		float4 snowDataB = _PW_SnowDataB;
		float heightAlpha = heightLerp(snowDataA.r * 0.5, 1 - i_Mask.b, 1);
		heightAlpha = (i_Mask.b == 0 || i_Mask.b == 1) ? snowDataA.r : heightAlpha;

		o_Albedo = lerp(i_Albedo, snowAlbedo, heightAlpha);
		#ifndef MATERIAL_QUALITY_LOW
		o_Normal = lerp(i_Normal, snowNormal, heightAlpha);
		#else
		o_Normal = lerp(i_Normal, normalStrength(i_Normal, 0.75), heightAlpha);
		#endif

		o_Mask = lerp(i_Mask, snowMask, heightAlpha);

		float worldHeightMask = smoothstep(-snowDataB.g, snowDataB.g, worldPosition.g - snowDataA.b);
		worldHeightMask = (worldHeightMask) * saturate(dot(worldNormal, float3(0,1,0)));

		o_Albedo = lerp(i_Albedo, o_Albedo, worldHeightMask);
		o_Normal = lerp(i_Normal, o_Normal, worldHeightMask);
		o_Mask = lerp(i_Mask, o_Mask, worldHeightMask);
	#endif
	
}



void SampleRain_float(float3 worldPosition, float3 worldNormal, SamplerState samp, float4 i_Albedo, float3 i_Normal, float4 i_Mask, out float4 o_Albedo, out float3 o_Normal, out float4 o_Mask)
{
	#if _TOD_RAIN_OFF
		o_Albedo = i_Albedo;
		o_Normal = i_Normal;
		o_Mask = i_Mask;
	#else
		o_Albedo = float4(0,0,0,0);
		o_Normal = float3(0,0,0);
		o_Mask = float4(0,0,0,0);
			
		float4 rainDataA = _PW_RainDataA;
		float4 rainDataB = _PW_RainDataB;
		float3 rainNormal;
		#ifndef MATERIAL_QUALITY_LOW
			float heightAlpha = heightLerp(rainDataA.r * 0.5, 1 - i_Mask.b, 2);
			heightAlpha = (i_Mask.b == 0 || i_Mask.b == 1) ? rainDataA.r : heightAlpha;
			float normalScale = lerp(0, 0.6, rainDataA.r);
			getRainNormals_float(worldPosition, samp, rainNormal);
			rainNormal = normalStrength(rainNormal, normalScale);
			//float3 editedNormal = normalStrength(i_Normal, 0.3);
			float3 editedNormal = lerp(i_Normal, normalStrength(i_Normal, 0.1), heightAlpha);
			rainNormal = normalBlend(editedNormal, rainNormal);
		#else
			rainNormal = normalStrength(i_Normal, lerp(1, 0.6, rainDataA.r));
		#endif
		o_Mask = i_Mask;

		float3 CameraToWorldDirection = _WorldSpaceCameraPos.xyz - worldPosition;
		float squareMag = dot(CameraToWorldDirection, CameraToWorldDirection);
		float nearCameraDistance = 1 - saturate(squareMag * 0.00001);
	
		//Rain Smoothness
		o_Mask.a = lerp(i_Mask.a, (rainDataB.b - 0.25), rainDataA.r * nearCameraDistance);
			
		float worldHeightMask = saturate(dot(worldNormal, float3(0,1,0)));
		o_Albedo = lerp(i_Albedo, i_Albedo * rainDataB.g, rainDataA.r);
			
		o_Albedo = lerp(i_Albedo, o_Albedo, worldHeightMask);
		o_Normal = lerp(i_Normal, normalize(rainNormal), worldHeightMask * rainDataA.r);
		o_Mask = lerp(i_Mask, o_Mask, worldHeightMask);
	#endif
	
}


void SampleWeatherVegetation_float(float3 worldPosition, float3 worldNormal, float4 i_Albedo, float4 i_Mask, out float4 o_Albedo, out float4 o_Mask)
{
	float4 sandDataA = _PW_SandDataA;
	float4 snowDataA = _PW_SnowDataA;
	float4 snowDataB = _PW_SnowDataB;
	float4 rainDataA = _PW_RainDataA;
	float4 rainDataB = _PW_RainDataB;
	
	float4 sandAlbedo = float4(0.125,0.066125,0.0,0);
	float4 snowAlbedo = float4(0.8,0.8,0.8,0) * _PW_SnowColor;

	float worldHeightMaskSand = smoothstep(-sandDataA.b, sandDataA.b, worldPosition.g - sandDataA.g);
	worldHeightMaskSand = (1 - worldHeightMaskSand) * saturate(dot(lerp(worldNormal, float3(0,1,0), 0.3), float3(0,1,0)));

	float heightAlphaSnow = heightLerp(snowDataA.r * 0.5, 1 - i_Mask.b, 2);
	
	float worldHeightMaskSnow = smoothstep(-snowDataB.g, snowDataB.g, worldPosition.g - snowDataA.b);
	worldHeightMaskSnow = worldHeightMaskSnow * saturate(dot(lerp(worldNormal, float3(0,1,0), 0.3), float3(0,1,0)));
	
	o_Albedo = lerp(i_Albedo, sandAlbedo, worldHeightMaskSand * sandDataA.r);

	o_Albedo = lerp(i_Albedo, snowAlbedo, heightAlphaSnow);
	o_Albedo = lerp(i_Albedo, o_Albedo, worldHeightMaskSnow * snowDataA.r);
	
	
	o_Mask = i_Mask;
	o_Mask.a = saturate(lerp(o_Mask.a, 0.8, rainDataA.r));
	
}

void getPivot_float(out float3 pivot)
{
	pivot = UNITY_MATRIX_M._m03_m13_m23 + _WorldSpaceCameraPos;
}

#endif