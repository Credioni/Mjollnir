using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
using SimplexNoise;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
*/
using Mjollnir.Isosurface;


using static Mjollnir.Utilities;


namespace Mjollnir{
	public partial class OldDensityfield {

		//Perlin perlin;
		public Texture2D image;
		public Color[] colors;
		public Color[,] colormap;
		//Noise2D heightMapBuilder;
		public static AnimationCurve heightcurve;

		private static float[,] heights;

		//Noise General
		[SerializeField] int _octaveCount = 2;
		[SerializeField] float _frecuency = 2;
		[SerializeField] float _persistence = 0.5f;

		//Spherical
		public Gradient _gradient;
		float _west = -180;
		float _east = 180;
		float _north = -90;
		float _south = 90;

		private static int NoiseMapSizeX = 512;
		private static int NoiseMapSizeY = 256;


		public OldDensityfield(){

		}

		public Color GetSurfaceColor(Vector3 _v){

			int u = Mathf.RoundToInt(GetLatitude(_v) * NoiseMapSizeX);
			int v = Mathf.RoundToInt(GetLongitude(_v) * NoiseMapSizeY);

			int tf = v * NoiseMapSizeX + u;

			if(tf >= NoiseMapSizeX * NoiseMapSizeY){
				tf = NoiseMapSizeX * NoiseMapSizeY -1;
			}else if(tf < 0){
				tf = 0;
			}
			float asd = GetLongitude(_v);

			return colors[tf];
		}

		public static float GetLatitude(Vector3 _v){
			float Lat = Mathf.Atan2(_v.z, _v.x ) + Mathf.PI;
			// +Mathf.PI is used to rotates latitude to match unity's sphere texture placement.

			if( Lat < 0){
					Lat = (Mathf.PI * 2 + Lat ) / ( 2 * Mathf.PI);
			}else{
				Lat /= ( 2 * Mathf.PI);
			}

			if(Lat > 1){
				Lat = 1;
			}else if(Lat < 0){
				Lat = 0;
			}

			return Lat;
		}

		public static float GetLongitude(Vector3 _v){
			float Lon = Mathf.Atan(_v.y  / Mathf.Sqrt(_v.x * _v.x + _v.z * _v.z)) +  Mathf.PI/ 2f;

			Lon /= Mathf.PI;

			if(Lon > 1){
				Lon = 1;
			}else if(Lon < 0){
				Lon = 0;
			}

			return Lon;
		}


		private static float GetSphericalHeight(Vector3 _pos){

			int u = Mathf.RoundToInt(GetLatitude(_pos) * NoiseMapSizeX);
			int v = Mathf.RoundToInt(GetLongitude(_pos) * NoiseMapSizeY);

			if (u < 0)
				u = 0;
			if (v < 0)
				v = 0;
			if (u >= NoiseMapSizeX)
				u = NoiseMapSizeX -1;
			if (v >= NoiseMapSizeY)
				v = NoiseMapSizeY -1;

			float height = heights [u, v];
			//height = height > 0.55f ? 0.55f: height;
			return height * 10f;
		}

		public Vector3 GetNormal(Vector3 _v){
			float h = 0.2f;
			return GetNormal(_v, h);
		}

		public Vector3 GetNormal(Vector3 _v, float _h){
			float h = _h;

			float dxp = GetDensity(new Vector3(_v.x + h, _v.y, _v.z));
			float dxm = GetDensity(new Vector3(_v.x - h, _v.y, _v.z));
			float dyp = GetDensity(new Vector3(_v.x, _v.y + h, _v.z));
			float dym = GetDensity(new Vector3(_v.x, _v.y - h, _v.z));
			float dzm = GetDensity(new Vector3(_v.x, _v.y, _v.z - h));
			float dzp = GetDensity(new Vector3(_v.x, _v.y, _v.z + h));
			//Vector3 gradient = new Vector3(map[x + 1, y] - map[x - 1, y], map[x, y + 1] - map[x, y - 1]);
			Vector3 gradient = new Vector3(dxp - dxm, dyp - dym, dzp - dzm);

			gradient.Normalize();
			//if(gradient == new Vector3(0,0,0)){Debug.Log("HueHue");}
			return gradient;
		}

		public float InterpolationProcent(float _a, float _b){
			float mu;
			if(_a * _b > 0){
				mu = 2 * (_a - _b)/(_a + _b);
			} else{
				mu = (_a / (_a - _b));
			}
			return mu;
		}

		public Vector3 Interpolation(Vector3 p1, Vector3 p2, float d1, float d2){
			return p1 + (-d1) * (p2 - p1) / (d2 - d1);
		}

		public bool GetEdge(Bounds _bounds, int _edge){
			float d0 = GetDensity(_bounds.center  + PRECENTER_CHILDNODE[TEdgePairs[_edge, 0]] * _bounds.size.x);
			float d1 = GetDensity(_bounds.center  + PRECENTER_CHILDNODE[TEdgePairs[_edge, 1]] * _bounds.size.x);

			if( (d0 >= 0 && 0 <= d1 && d1 != d0 ) || (d1 >= 0 && 0 <= d0 && d1 != d0 )){
				return true;
			}else{
				return false;
			}
		}

		public float GetDensity(Vector3 _v){
			return GetDensity(_v.x, _v.y, _v.z);
		}

		public float GetDensity(float x, float y, float z){

			//return -16.1f + (float)Math.Sqrt(x * x + y * y + z * z);
			//return y;
			return glm.Density_Func(new Vector3(x,y,z));
			
		}

		public int GetByte(Bounds _bounds){
			int corners = 0;
			for (int i = 0; i < 8; i++)
			{
				if ((GetDensity(_bounds.center  + PRECENTER_CHILDNODE[i] * _bounds.size.x)) > 0)
					corners |= 1 << i;
			}
			return corners;
		}

		public byte GetByte(float[] _d){
			BitArray asd = new BitArray (new byte[]{ 0 });

			if (_d[0] > 0f) {
				asd [0] = true;
			}
			if (_d[1] > 0f) {
				asd [1] = true;
			}
			if (_d[2] > 0f) {
				asd [2] = true;
			}
			if (_d[3] > 0f) {
				asd [3] = true;
			}
			if (_d[4] > 0f) {
				asd [4] = true;
			}
			if (_d[5] > 0f) {
				asd [5] = true;
			}
			if (_d[6] > 0f) {
				asd [6] = true;
			}
			if (_d[7] > 0f) {
				asd [7] = true;
			}

			byte[] bytes = new byte[1];
			asd.CopyTo(bytes, 0);
			return bytes[0];
		}

		public override string ToString()
		{
			return ("True");
		}
	}
}
