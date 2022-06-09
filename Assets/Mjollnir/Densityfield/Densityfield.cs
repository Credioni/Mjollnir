/*
	Densityfield.cs 

	Contains everydata from gradient fields and changed enviroment.
*/

using UnityEngine;

/*
using SimplexNoise;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;

using Mjollnir.Isosurface;
*/
using static Mjollnir.Utilities;



namespace Mjollnir
{
	public partial class Densityfield
	{

		private DensityTree densitytree;


		public Densityfield()
		{
            densitytree = new DensityTree( new Bounds(Vector3.zero, Vector3.one) );
			AddDebug();
		}

		public Vector3 GetNormal(Vector3 _v)
		{
			float h = 0.2f;
			return GetNormal(_v, h);
		}

		private Vector3 GetNormal(Vector3 _v, float _h)
		{
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

		public float GetDensity(Vector3 _position)
		{
			float density = 0;

			if ( densitytree.FindDensity(_position, ref density) )
			{
				return density;
			}
			else
			{
				return -glm.Density_Func(_position);
			}

			//return -16.1f + (float)Math.Sqrt(_position.x * _position.x + _position.y * _position.y + _position.z * _position.z );
			//return y;
			
		}

        public void AddDebug()
        {

			int tsize = 16;

			Bounds bounds = new Bounds( Vector3.zero, new Vector3(tsize, tsize, tsize) );

			int size = tsize+1;
			float[,,] densities = new float[size, size, size];

			int dx = tsize / 2;

			for (int i = -dx; i < dx; i++)
			{
				for (int j = -dx; j < dx; j++)
				{
					for (int k = -dx; k < dx; k++)
					{
						float d = 4.1f - Mathf.Sqrt(Mathf.Pow(i, 2) + Mathf.Pow(j, 2) + Mathf.Pow(k, 2));

						//UnityEngine.Debug.Log(i + " " + j + " " + k + " " + d);
						//UnityEngine.Debug.Log(i + dx + " " + j + dx + " " + k + dx + " " + d);
						//UnityEngine.Debug.Log("-----------");


						densities[i + dx, j + dx, k + dx] = d;
					}
				}
			}

			densitytree.AddDebug(bounds, densities);
        }

	}

}
