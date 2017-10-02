// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TextureOps" {
	Properties 
	{ 
	 _MainTex ("", any) = "" {} 
	 _GradientTex("", any) = "" {}
	 _Multiply ("_Multiply", Vector) = (1,1,1,1)
     _TexSizeRecip("_TexSizeRecip", Vector) = (.1,.1,.1,.1)
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f {
		float4 pos : SV_POSITION; 
		half2 uv : TEXCOORD0;
	};
	sampler2D _MainTex;
	sampler2D _GradientTex;
	sampler2D _GradientTex2;
	float4 _Multiply;
	float4 _Multiply2;
	float4 _TexSizeRecip;
	int   _MainIsGrey; 
	int   _TextureBIsGrey;
	int _Saturate;
	int _InvertInput; 
	int _ClampInputUV;
	int _InvertOutput;
	float4 _ScaleOutput;
	float4 _ScaleOutput2;
	int4 m_GeneralInts;
	
/*
	float rand(float2 n)
	{
		return fract(sin(dot(n.xy, float2(12.9898, 78.233)))* 43758.5453);
	}
	*/
	uint wangHash(int _Seed)
	{
		_Seed = (_Seed ^ 61) ^ (_Seed >> 16);
		_Seed *= 9;
		_Seed = _Seed ^ (_Seed >> 4);
		_Seed *= 0x27d4eb2d;
		_Seed = _Seed ^ (_Seed >> 15);
		return _Seed;
	}

	float4 GetTextureMain4(sampler2D _tex, float2 uv)
	{
		if (_ClampInputUV > 0.1f)
			uv = saturate(uv);

		if (_MainIsGrey>0 )
		{
			float r = tex2D(_tex, uv).r;
			if (_InvertInput)
				r = 1.0f - r;

			return float4(r, r, r, 1.0f);
		}
		else
		{
			float4 ret = tex2D(_tex, uv);

			if (_InvertInput)
				ret = 1.0f - ret;

			return tex2D(_tex, uv);
		}
	}
	float4 GetTextureMain4lod(sampler2D _tex, float4 uv)
	{
		if (_MainIsGrey>0)
		{
			float r = tex2Dlod(_tex, uv).r;
			if (_InvertInput)
				r = 1.0f - r;

			return float4(r, r, r, 1.0f);
		}
		else
		{
			float4 ret = tex2Dlod(_tex, uv); 

			if (_InvertInput)
				ret = 1.0f - ret;

			return tex2Dlod(_tex, uv);
		}
	}
	float3 GetNorm(float2 uv, sampler2D _tex)
	{
		float heightMapSizeX = _TexSizeRecip.x;
		float heightMapSizeY = _TexSizeRecip.y;

		float h0 = GetTextureMain4(_tex, uv).x;
		//		float n = tex2D(_MainTex,float2(uv.x,uv.y+1.0*heightMapSizeY)).x;
		float hdown = GetTextureMain4(_tex, float2(uv.x, uv.y - 1.0*heightMapSizeY)).x;
		float hright = GetTextureMain4(_tex, float2(uv.x + 1.0*heightMapSizeX, uv.y)).x;
		//		float w = tex2D(_MainTex,float2(uv.x-1.0*heightMapSizeX,uv.y)).x;                

		float3 right = float3(1, (hright - h0)* _TexSizeRecip.z, 0);
		float3 down = float3(0, (hdown - h0) * _TexSizeRecip.z, 1.0f);

		float3 norm = cross(normalize(down), normalize(right));
		norm = normalize(norm);
		norm.z *= -1.0f;
		
		return norm.xzy;
	}

	float4 GammaCorrection(float4 color, float gamma)
	{
		
		float4 v = 1.0 / gamma;
		return pow(color, v);
	}

	float4 LevelsControlInputRange(float4 color, float minInput, float maxInput)
	{
		
		color -= minInput;
		float range = maxInput - minInput;
		color /= range;

		return color;
	}

	float4 LevelsControlInput(float4 color, float minInput, float gamma, float maxInput)
	{
		return GammaCorrection(LevelsControlInputRange(color, minInput, maxInput), gamma);
	}

	float4 LevelsControlOutputRange(float4 color, float minOutput, float maxOutput)
	{
		
		float range = maxOutput - minOutput;
		color *= range;
		color += minOutput;
		return color;
	
	}
	float4 LevelsControl(float4 color, float minInput, float  gamma, float  maxInput, float  minOutput, float  maxOutput)
	{
		return LevelsControlOutputRange(LevelsControlInput(color, minInput, gamma, maxInput), minOutput, maxOutput);
	}
	float4 ProcessOutput(float4 _col)
	{
		if (_InvertOutput)
			_col.rgb = 1 - _col.rgb;
/*
		float range = _ScaleOutput.w - _ScaleOutput.z;
		range/= _ScaleOutput.y - _ScaleOutput.x;

		_col.rgb -= _ScaleOutput.x;
		_col.rgb *= range;
		_col.rgb += _ScaleOutput.z;
*/
		_col = LevelsControl(_col, _ScaleOutput.x, _ScaleOutput2.x, _ScaleOutput.y, _ScaleOutput.z, _ScaleOutput.w);
//		_col.rgb*=_ScaleOutput;
		if (_Saturate)
			_col= saturate(_col);
		return _col;
	}
	float4 GetTextureB4(sampler2D _tex, float2 uv) 
	{
		if (_TextureBIsGrey>0)
		{
			float r = tex2D(_tex, uv).r;
			return float4(r, r, r, 1.0f);
		}
		else
			return tex2D(_tex, uv);
	}

	v2f vertMult(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex); 
		o.uv = v.texcoord ; 
		return o;
	}
	float4 fragMult(v2f i) : SV_Target
	{
		
		float4 color = GetTextureMain4(_MainTex, i.uv);
		color*= _Multiply;
		//color.b = 100.0f;
		return ProcessOutput(color);
	}

	float4 fragAdd(v2f i) : SV_Target
	{
		float4 color = GetTextureMain4(_MainTex, i.uv);
		color += _Multiply;
		//color.b = 100.0f;
		return ProcessOutput(color);

	} 
	float4 fragMapCylinder(v2f i) : SV_Target
	{
		float v = (i.uv.y-0.5)*2 ;
		//v = sqrt(_Multiply.x*_Multiply.x - v*v);
		v = asin(v*_Multiply.x) + _Multiply.y;
		v*=_Multiply.z;
		float2 uv = float2(i.uv.x,v+0.5);
		float4 color = GetTextureMain4(_MainTex, uv);
		float v2 = -(i.uv.y - 0.5) * 2;
		v2 = asin(v2*_Multiply.x) + _Multiply.y;
		v2 *= _Multiply.z;
		float2 uv2 = float2(i.uv.x, v2 + 0.5);
		color += GetTextureMain4(_MainTex, uv2)*0.3;

		//color.b = 100.0f;
		return ProcessOutput(color);

	}
	float4 fragSetCol(v2f i) : SV_Target
	{
		float4 color = _Multiply;
		color.a = 1.0f;
		return ProcessOutput(color);

	}

		
	float4 fragAnimCurveAxis(v2f i) : SV_Target
	{
		float4 color = (max(GetTextureMain4(_MainTex, float2(i.uv.x*_Multiply.x,0)) , GetTextureB4(_GradientTex, float2(i.uv.y*_Multiply.y,0))));
		return ProcessOutput(color);

	}


	float4 fragAdd2(v2f i) : SV_Target
	{
		float4 color = saturate(GetTextureMain4(_MainTex, i.uv) +GetTextureB4(_GradientTex, i.uv));

		return ProcessOutput(color);
	}

		float4 fragSub2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) - GetTextureB4(_GradientTex, i.uv));
		return ProcessOutput(color);

	}
		float4 fragBlend2(v2f i) : SV_Target
	{
		float4 color = (lerp(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv),_TexSizeRecip.z));
		return ProcessOutput(color);

	}
		float4 fragMult2(v2f i) : SV_Target
	{
		float4 color = (GetTextureMain4(_MainTex, i.uv) * GetTextureB4(_GradientTex, i.uv));
		return ProcessOutput(color);

	}
		float4 fragPow2(v2f i) : SV_Target
	{
		float4 color = (pow(GetTextureMain4(_MainTex, i.uv) , GetTextureB4(_GradientTex, i.uv)));
		return ProcessOutput(color);

	}
		float4 fragMin(v2f i) : SV_Target
	{
		float4 color = min(GetTextureMain4(_MainTex, i.uv) , tex2D(_GradientTex, i.uv));
		return ProcessOutput(color);

	}
		float4 fragMax(v2f i) : SV_Target
	{
		float4 color = (max(GetTextureMain4(_MainTex, i.uv) , tex2D(_GradientTex, i.uv)));
		return ProcessOutput(color);

	}
		float4 fragMin1(v2f i) : SV_Target
	{
		float4 ret = min(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragSrcStepify(v2f i) : SV_Target
	{
		float4 ret = fmod(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragMax1(v2f i) : SV_Target
	{ 
		float4 ret = max(GetTextureMain4(_MainTex, i.uv) , _Multiply.x);
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragStep1(v2f i) : SV_Target
	{
		float4 ret = step(_Multiply.x,GetTextureMain4(_MainTex, i.uv) );
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}
		float4 fragInvert(v2f i) : SV_Target
	{
		float4 ret = 1.0f-(GetTextureMain4(_MainTex, i.uv));
		ret.w = 1.0f;
		return ProcessOutput(ret);

	}

	float4 fragSobel(v2f i) : SV_Target
	{
		float2 delta = _TexSizeRecip.xy*_Multiply.x;

		float4 hr = float4(0, 0, 0, 0);
		float4 vt = float4(0, 0, 0, 0); 

		hr += tex2D(_MainTex, (i.uv + float2(-1.0, -1.0) * delta)) *  1.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0, -1.0) * delta)) * -1.0;
		hr += tex2D(_MainTex, (i.uv + float2(-1.0,  0.0) * delta)) *  2.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) * -2.0;
		hr += tex2D(_MainTex, (i.uv + float2(-1.0,  1.0) * delta)) *  1.0;
//		hr += tex2D(_MainTex, (i.uv + float2(0.0,  1.0) * delta)) *  0.0;
		hr += tex2D(_MainTex, (i.uv + float2(1.0,  1.0) * delta)) * -1.0;

		vt += tex2D(_MainTex, (i.uv + float2(-1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0, -1.0) * delta)) *  2.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0, -1.0) * delta)) *  1.0;
//		vt += tex2D(_MainTex, (i.uv + float2(-1.0,  0.0) * delta)) *  0.0;
//		vt += tex2D(_MainTex, (i.uv + float2(0.0,  0.0) * delta)) *  0.0;
		//vt += tex2D(_MainTex, (i.uv + float2(1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(_MainTex, (i.uv + float2(-1.0,  1.0) * delta)) * -1.0;
		vt += tex2D(_MainTex, (i.uv + float2(0.0,  1.0) * delta)) * -2.0;
		vt += tex2D(_MainTex, (i.uv + float2(1.0,  1.0) * delta)) * -1.0;

		float4 color= sqrt(hr * hr + vt * vt);
		return ProcessOutput(color);
	}
	float4 fragSmooth(v2f i) : SV_Target
	{

		float col = GetTextureMain4(_MainTex, i.uv).r;
		float size = _Multiply.x;
		float dx;
		float dy;


		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = GetTextureMain4(_MainTex, i.uv);
		//				float scaleTex=_InvTexSize;

		float maxLen = sqrt(size*size + size*size);
		float4 sum = 0;
		float sumG = 0.0f;
		for (dx = -size; dx<=size; dx += 1.0f)
		{

			for (dy = -size; dy<=size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
				float dist = length(offset);//sqrt(dot(offset,offset));
				float gauss = (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
				sumG += gauss;
				sum.rgb += col.rgb*gauss;  
			}
		}
		sum.rgb /= sumG;
		sum.a = 1;
		return ProcessOutput(sum);

	}
		float4 fragSrcBlend(v2f i) : SV_Target
	{
		float4 src = GetTextureMain4(_MainTex, i.uv);
		float4 src2 = GetTextureMain4(_GradientTex, i.uv);

		float alpha = src.r;
		float4 ret = src + _Multiply.x*src2*( alpha);
		

		ret.w = 1.0f;
		return ProcessOutput(ret);

	}

		float4 fragPower(v2f i) : SV_Target
	{
		float4 ret= pow(GetTextureMain4(_MainTex, i.uv) , _Multiply);
		ret.w = 1.0f;
		return ProcessOutput(ret);
	}

	float4 fragLevel1(v2f i) : SV_Target
	{
		float4 a = GetTextureMain4(_MainTex, i.uv);
		a *= _Multiply.x;
		a += _Multiply.y;
		a.w = 1.0f;
//		a.r = 1.0f;
		return ProcessOutput(a);
	}
	float4 fragTransform(v2f i) : SV_Target
	{
		float2 uv;
		uv.x = (i.uv.x-0.5)*_Multiply.x*_Multiply2.z - (i.uv.y - 0.5)*_Multiply.y*_Multiply2.z+0.5+ _Multiply2.x;
		uv.y = (i.uv.x - 0.5)*_Multiply.y*_Multiply2.w + (i.uv.y - 0.5)*_Multiply.x*_Multiply2.w +0.5 + _Multiply2.y;

		float4 a = GetTextureMain4(_MainTex, uv);
		return ProcessOutput(a);
	}

	float4 fragSplatter(v2f i) : SV_Target
	{
		float2 uv;
		float4 sum;
		int seed = _Multiply.x;
		float scale;
		float angle;
		float dx;
		float dy;
		float deltaX;
		float deltaY;
		int count = _Multiply.z;
		for (int index = 0; index < count; index++)
		{
			seed = wangHash(seed);
			angle = (seed*1.0 / 4294967295.0f)*30.14159;
			seed = wangHash(seed);
			float srange = _Multiply2.w - _Multiply.y;

			scale= saturate((seed*1.0 / 4294967295.0f)+0.5f)*srange+ _Multiply.y;

			seed = wangHash(seed);
			deltaY = ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;
			seed = wangHash(seed);
			deltaX = ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;
			seed = wangHash(seed);
			float brange = _Multiply2.y - _Multiply2.x;
			float bright = ((seed*1.0 / 4294967295.0f) + 0.5f)*brange + _Multiply2.x;
			dx = cos(angle)*scale;
			dy = sin(angle)*scale;


			uv.x = (i.uv.x - 0.5)*dx - (i.uv.y - 0.5)*dy + 0.5 + deltaX;
			uv.y = (i.uv.x - 0.5)*dy + (i.uv.y - 0.5)*dx + 0.5 + deltaY;
			if(_Multiply2.z==0)
				sum = max(sum, GetTextureMain4(_MainTex, uv)*bright);
			else
				sum += GetTextureMain4(_MainTex, uv);
		}
		return ProcessOutput(sum);
	}
	float4 fragSplatterGrid(v2f i) : SV_Target
	{
		float2 uv;
		float4 sum=0;
		int seed = _Multiply.x;
		float scale;
		float angle;
		float dx;
		float dy;
		float deltaX;
		float deltaY;
		int count = _Multiply.z;
		//for (int index = 0; index < count; index++)
		{
			seed = wangHash(seed);
			angle = (seed*1.0 / 4294967295.0f)*30.14159;
			seed = wangHash(seed);
			scale = ((seed*1.0 / 4294967295.0f) + 0.5f)*_Multiply.y;
			seed = wangHash(seed);
			deltaY = 0;// ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;
			seed = wangHash(seed);
			deltaX = 0;// ((seed*1.0 / 4294967295.0f) - 0.5f)*10.0f;

			dx = cos(angle)*scale; 
			dy = sin(angle)*scale;

//			for (float dx = 0; dx < 1.0f; dx += 0.1f)
			{ 
//				for (float dy = 0; dy < 1.0f; dy += 0.1f )
				{
					float inuvx = i.uv.x;
					float inuvy = i.uv.y;

					
					if (fmod(inuvy*_Multiply.z, 2) < 1)
						inuvx += _Multiply2.z;
					uv.x = (inuvx)* _Multiply.z;//  +deltaX*scale;
					uv.y = (inuvy)*_Multiply.z;// +deltaY*scale;
					seed = wangHash((floor(1+uv.x)+floor(1 + uv.y))*_Multiply.x);
					deltaX =  ((seed*2.0 / 4294967295.0f) - 0.5f)*_Multiply.y;
					uv.x += deltaX;
					seed = wangHash(floor(17 + uv.x) + floor(1937 + uv.y)*_Multiply.x);
					deltaY = ((seed*2.0 / 4294967295.0f) - 0.5f)*_Multiply.y;
					uv.y += deltaY;

					uv.x += _Multiply2.x;
					uv.y += _Multiply2.y;

//					uv.x = (i.uv.x - 0.5)*dx - (i.uv.y - 0.5)*dy + 0.5 + deltaX;
//					uv.y = (i.uv.x - 0.5)*dy + (i.uv.y - 0.5)*dx + 0.5 + deltaY;

					sum += GetTextureMain4(_MainTex, uv);

				}
			}


		}
		return ProcessOutput(sum);
	}
		float4 fragDirWarp(v2f i) : SV_Target
		{
			float2 uv;
			float dist = tex2D(_GradientTex, i.uv).r-0.5f;

//			uv.x = _Multiply.x*dist;
	//		uv.y = _Multiply.y*dist;
			uv.x = cos(dist*(_Multiply.x))*_Multiply.z*0.1f;
			uv.y = sin(dist*(_Multiply.x))*_Multiply.z*0.1f;

			float4 a = GetTextureMain4(_MainTex, i.uv + uv);

			//a.r = 1;
			return ProcessOutput(a);
		}
		float4 fragGradient(v2f i) : SV_Target
	{
		float index = tex2D(_MainTex, i.uv).r;
		float x = index;// +_TexSizeRecip.z;// *_TexSizeRecip.x;
		x = saturate(x);
		float4 color = GetTextureB4(_GradientTex, float2(x,0));
		//float4 color = float4(0,1,0,0);// tex2D(_GradientTex, i.uv);
		color.a = 1.0f;
		return ProcessOutput(color);
	}
	float4 fragClipMin(v2f i) : SV_Target
	{
		float4 tex = GetTextureMain4(_MainTex, i.uv);
		if (tex.r < _Multiply.r)
			tex.r = 0.0f;
		if (tex.g < _Multiply.g)
			tex.g = 0.0f;
		if (tex.b < _Multiply.b)
			tex.b = 0.0f;
		return ProcessOutput(tex);
	}

	float4 fragDistortAbs(v2f i) : SV_Target
	{
		float2 offset = float2(tex2D(_GradientTex, i.uv ).r, tex2D(_GradientTex2, i.uv.yx).r );
		float4 color = GetTextureMain4(_MainTex,offset.rg );
		return ProcessOutput(color);

	}
		float4 fragProbabilityBlend(v2f i) : SV_Target
	{
		float4 color0 = GetTextureMain4(_MainTex, i.uv);
		float4 color1 = GetTextureB4(_GradientTex, i.uv);
		float probability = tex2D(_GradientTex2, i.uv).r;

		float rand = wangHash(i.uv.x*1093+ i.uv.y * 999983)*2.0 / 4294967295.0f;
		float4 color = color0;
		if (probability > rand)
			color = color1;


		return ProcessOutput(color);

	}
		float4 fragRandomEdge(v2f i) : SV_Target
	{
		float4 color = GetTextureMain4(_MainTex, i.uv);
		if (length(color.rgb) < _Multiply2.x)
		{
			float size = _Multiply.x;
			float dx;
			float dy;

			float4 colFinalr = float4(0, 0, 0, 0);
			float4 colStart = GetTextureMain4(_MainTex, i.uv);
			//				float scaleTex=_InvTexSize;

			float maxLen = sqrt(size*size + size*size);
			float4 maxcol = 0;

			for (dx = -size; dx <= size; dx += 1.0f)
			{

				for (dy = -size; dy <= size; dy += 1.0f)
				{
					float2 offset = float2(dx, dy);
					float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
					float4 col = GetTextureMain4lod(_MainTex, float4(i.uv.x + offsetTex.x, i.uv.y + offsetTex.y, 0, 0));
					maxcol = max(maxcol, col);
				}
			}


			float rand = wangHash(i.uv.x * 1093 + i.uv.y * 999983)*2.0 / 4294967295.0f;
			
			if (length(maxcol.rgb)>_Multiply.y && _Multiply.z > rand)
				color = 1;
		}

		return ProcessOutput(color);

	}

	float4 fragDistort(v2f i) : SV_Target
	{
		float4 color;
	float count;
	float blur =  min(20, _TexSizeRecip.w);
	 blur = max(blur, 0.1);
	float maxLen = sqrt(blur * blur + blur * blur);
		for (float dx = -blur; dx <= blur; dx += 1.0f)
		for (float dy = -blur; dy <= blur; dy += 1.0f)
		{
			float2 delta = 0;// float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
			//float2 offset = GetNorm(i.uv, _GradientTex).xy;// float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex, i.uv.yx + delta).r - 0.5);
			float2 offset =  float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex, i.uv.yx + delta).r - 0.5);
   //		offset.g = -offset.g;
		   offset.rg *= _TexSizeRecip.xy;
		   offset.rg *= _Multiply.xy;
		   delta = float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
		   float dist = length(delta);//sqrt(dot(offset,offset));
		   float gauss =  (1 + cos(3.14159*dist / maxLen)) / 2.0*3.14159;
		   
		   color+= GetTextureMain4(_MainTex, i.uv + offset.rg+delta)*gauss; 
		   count += gauss;
		}
		color /= count;
/*
		offset = float2(tex2D(_GradientTex, i.uv*2.0).r - 0.5, tex2D(_GradientTex, i.uv*2.0 + float2(0.5, 0.5)).r - 0.5);
//		offset.g = -offset.g;
		offset.rg *= _TexSizeRecip.xy;
		offset.rg *= _TexSizeRecip.z*0.5;
		 color += (GetTextureMain4(_MainTex, i.uv + offset.rg));
		 offset = float2(tex2D(_GradientTex, i.uv*4.0).r - 0.5, tex2D(_GradientTex, i.uv*4.0 + float2(0.5, 0.5)).r - 0.5);
		 //		offset.g = -offset.g;
		 offset.rg *= _TexSizeRecip.xy;
		 offset.rg *= _TexSizeRecip.z*0.25;
		 color += (color + GetTextureMain4(_MainTex, i.uv + offset.rg));
		 color *= 1.0 / 3.0;
*/
//		color.rgb=GetNorm(i.uv, _GradientTex).xyz*.5+.5; 
		return ProcessOutput(color);
	}

	float4 fragEdgeDist(v2f i) : SV_Target
	{
		float size = _Multiply.x;//50.0f;
		float size2 = _Multiply.x;//50.0f;
		float dx;
		float dy;
		float distFinalr = size + 1000;

		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = GetTextureMain4(_MainTex, i.uv);// tex2D(_MainTex, i.uv);
//		if (_Invert>0)
//			colStart = 1 - colStart;

		for (dx = -size; dx<=size; dx += 1.0f)
		{
			
			for (dy = -size2; dy<=size2; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = GetTextureMain4(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
//				if (_Invert>0)
//					col = 1 - col;
				if (col.r>_Multiply.y)
				{
					float dist = length(offset);//sqrt(dot(offset,offset));
												//if(col.r>0.9)
					if (dist<distFinalr)
					{
						colStart = col;
						distFinalr = dist; 
					}
				}
			}
		}
		if (distFinalr < size + 100)
		{
			if(m_GeneralInts.x==0)
				colStart = 1.0f - saturate(distFinalr / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));
			else
			{
				//leave colstart the colour of the pixel we found
			}

		}
		else
			colStart = 0.0;

//		if (colStart.r<0.2)
//			colStart = 0;
//		if (_Invert>0)
//			colStart = 1 - colStart;
		//colStart = 0.1;
		return ProcessOutput(colStart);

	}
		float4 fragEdgeDistDir(v2f i) : SV_Target
	{
		float size = _Multiply.x;//50.0f;
	float size2 = _Multiply.x;//50.0f;
	float dx;
	float dy;

	float4 colFinalr = float4(0,0,0,0);
	float4 colStart = tex2D(_MainTex, i.uv);
	//		if (_Invert>0)
	//			colStart = 1 - colStart;
	float2 delta = 0;// float2(dx*_TexSizeRecip.x, dy*_TexSizeRecip.y) / 3;
	float2 offsetDir = float2(tex2D(_GradientTex, i.uv + delta).r - 0.5, tex2D(_GradientTex2, i.uv.yx + delta).r - 0.5);
	int len = length(offsetDir);
	offsetDir = normalize(offsetDir);
//	size = size*len;

	float distFinalr = size + 1000;

	float recipDist = 1.0 / size;//_Dist; 

	for (dx = 0; dx<size; dx += 1)
	{

	//	for (dy = -size2; dy<size2; dy += 1.0f)
		{
			float2 offset = offsetDir*dx;// float2(dx, dy);
			float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
			float4 col = tex2Dlod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
			//				if (_Invert>0)
			//					col = 1 - col;
			if (col.r>_Multiply.y)
			{
				float dist = length(offset);//sqrt(dot(offset,offset));
											//if(col.r>0.9)
				if (dist<distFinalr)
				{
					colStart = col;
					distFinalr = dist;
				}
			}
		}
	}
	if (distFinalr < size + 100)
	{
//		if (m_GeneralInts.x == 0)
			colStart = 1.0f - saturate(distFinalr / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));

	}
	else
		colStart = 0.0;

	//		if (colStart.r<0.2)
	//			colStart = 0;
	//		if (_Invert>0)
	//			colStart = 1 - colStart;

	return ProcessOutput(colStart);

	}
	float4 fragBlackEdge(v2f i) : SV_Target
	{
		float size = _Multiply.r;//50.0f;
		float dx;
		float dy;
		float distFinalr = size + 1000;

		float recipDist = 1.0 / size;//_Dist;

		float4 colFinalr = float4(0,0,0,0);
		float4 colStart = tex2D(_MainTex, i.uv);
		//		if (_Invert>0)
		//			colStart = 1 - colStart;

		for (dx = -size; dx<=size; dx += 1.0f)
		{
			for (dy = -size; dy<=size; dy += 1.0f)
			{
				float2 offset = float2(dx,dy);
				float2 offsetTex = offset*_TexSizeRecip.xy;//=float2(dx/1024.0,dy/1024.0);
				float4 col = tex2Dlod(_MainTex, float4(i.uv.x + offsetTex.x,i.uv.y + offsetTex.y,0,0));
				//				if (_Invert>0)
				//					col = 1 - col;
				if ( length(colStart-col)>_Multiply.y)
				{
					float dist = length(offset);//sqrt(dot(offset,offset));
												//if(col.r>0.9)
					if (dist<distFinalr)
					{
//						colStart = col;
						distFinalr = dist;
					}
				}
			}
		}
		if (distFinalr < size + 100)
		{
				colStart = 0;// 1.0f - saturate(distFinalr / size);//distFinalr/size;//colFinalr*distFinalr;//(1-saturate(  sqrt(distUse)*(colFinal.a) ));
		}

		//		if (colStart.r<0.2)
		//			colStart = 0;
		//		if (_Invert>0)
		//			colStart = 1 - colStart;

		return ProcessOutput(colStart);

	}
	float4 fragCopyR(v2f i) : SV_Target
	{
		float4 col =  tex2D(_MainTex, i.uv).r;
		col *= _Multiply.x;
		//col.g = 0.5f; 
		col.a = 1;
		//col.b = 0;
		return col;// ProcessOutput(col);
	}

	float4 fragCopy(v2f i) : SV_Target
	{
		float4 col = tex2D(_MainTex, i.uv);
		//col.g = 0.5f; 
		col.a = 1;
		//col.b = 0;
		return col;
	}
	float4 fragCopyColAndAlpha(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv);
		float alpha=GetTextureB4(_GradientTex,i.uv).r;
		
		col.a = alpha;
		//col.b = 0;
		return col;
	}
	float4 fragCopyColRGBA(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv);
		return col = ProcessOutput(col);
		
	}
	float4 fragCopyRAndAlpha(v2f i) : SV_Target
	{
		float4 col = GetTextureMain4(_MainTex,i.uv).r;
		float alpha = GetTextureB4(_GradientTex,i.uv).r;
		col.g = 0;
		col.b = 0;

		col.a = alpha;
		//col.b = 0;
		return col;
	}
	StructuredBuffer<uint4> _Histogram;

	float4 fragHistogram(v2f i) : SV_Target
	{
		float remapI = i.uv.x * 511.0;
		uint index = floor(remapI);
		float delta = frac(remapI);
		int _Channel = 0;
		float v1 = _Histogram[index][_Channel];
		float v2 = _Histogram[min(index + 1, 511)][_Channel];
		float h = v1;// *(1.0 - delta) + v2 * delta;
		uint y = (uint)round(i.uv.y * _Multiply.y);
		float4 color =  float4(0.1, 0.1, 0.1, 1.0);
		

		float fill = step(y, h);
		color = fill;// lerp(color, float4(1, 1, 1, 1), fill);
		color.a = 1;
		return color;


	}

		

		float4 fragNormal(v2f i) : SV_Target
	{

		float heightMapSizeX = _TexSizeRecip.x;
		float heightMapSizeY = _TexSizeRecip.y;

		float h0 = GetTextureMain4(_MainTex,i.uv).x;
		//		float n = tex2D(_MainTex,float2(i.uv.x,i.uv.y+1.0*heightMapSizeY)).x;
				float hdown = GetTextureMain4(_MainTex,float2(i.uv.x,i.uv.y - 1.0*heightMapSizeY)).x;
				float hright = GetTextureMain4(_MainTex,float2(i.uv.x + 1.0*heightMapSizeX,i.uv.y)).x;
				//		float w = tex2D(_MainTex,float2(i.uv.x-1.0*heightMapSizeX,i.uv.y)).x;                

						float3 right = float3(1, (hright - h0)* _TexSizeRecip.z, 0);
						float3 down = float3(0, (hdown - h0) * _TexSizeRecip.z, 1.0f);

						float3 norm = cross(normalize(down),normalize(right));
						norm = normalize(norm);
						norm.z *= -1.0f;

						norm = norm*0.5 + 0.5f; 

						float4 color;
						color.xzy = norm;

						color.a = 1.0f;

						return ProcessOutput(color);
						
	}
		ENDCG
		SubShader {
		Pass{ //0
			 ZTest Always Cull Off ZWrite Off

			 CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMult
			 ENDCG
		}
			Pass{//1
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragGradient
			ENDCG
		}
			Pass{//2
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDistort
			ENDCG
		}

			Pass{//3
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragNormal
			ENDCG
		}
			Pass{//4
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAdd
			ENDCG
		}
			Pass{//5
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAdd2
			ENDCG
		}
			Pass{//6
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMult2
			ENDCG
		}
			Pass{//7
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPow2
			ENDCG
		}
			Pass{//8
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMin
			ENDCG
		}
			Pass{//9
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragMax
			ENDCG
		}
			Pass{//10
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragClipMin
			ENDCG
		}
			Pass{//11
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragBlend2
			ENDCG
		}
			Pass{//12
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSub2
			ENDCG
		}
			Pass{//13
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragLevel1
				ENDCG
		}
			Pass{//14
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
	#pragma vertex vertMult
	#pragma fragment fragTransform
				ENDCG
		}
			Pass{//15
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDirWarp
				ENDCG
		}
			Pass{//16
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragPower
				ENDCG
		}
			Pass{//17
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMin1
				ENDCG
		}
			Pass{//18
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMax1
				ENDCG
		}
			Pass{//19
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragCopyR 
					ENDCG
		}
			Pass{//20
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragCopy
					ENDCG
		}
			Pass{//21
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragStep1
			ENDCG
		}
			Pass{//22
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragInvert
					ENDCG
		}
			Pass{//23
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragSrcBlend
					ENDCG
		}
			Pass{//24
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragSrcStepify
					ENDCG
		}
			Pass{//25
					ZTest Always Cull Off ZWrite Off

					CGPROGRAM
		#pragma vertex vertMult
		#pragma fragment fragEdgeDist
					ENDCG
		}
	Pass{//26
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSmooth
			ENDCG
		}
	Pass{//27
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragBlackEdge
			ENDCG
		}
	Pass{//28
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyColAndAlpha
			ENDCG
		}
	Pass{//29
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragEdgeDistDir
				ENDCG
			}
	Pass{//30
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSplatter
			ENDCG
		}//
				Pass{//31
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSplatterGrid
				ENDCG
			}//
				Pass{//32
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragSobel
				ENDCG
			}//
				Pass{//33
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragAnimCurveAxis
				ENDCG
			}//
				Pass{//34
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragDistortAbs
				ENDCG
			}//
				Pass{//35
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragMapCylinder
				ENDCG
			}//
				Pass{//36
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyRAndAlpha
				ENDCG
			}
		Pass{//37
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragHistogram
				ENDCG
			}
		Pass{//38
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragProbabilityBlend
		ENDCG
				}
	Pass
	{//39
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragRandomEdge
		ENDCG
	}
	Pass
	{//40
		ZTest Always Cull Off ZWrite Off

		CGPROGRAM
#pragma vertex vertMult
#pragma fragment fragCopyColRGBA
		ENDCG
	}
	}
	

	Fallback off
}
