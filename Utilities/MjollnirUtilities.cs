using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mjollnir
{
    public static class Utilites
    {

        public static Vector3 Intersection(Vector3 p1, Vector3 p2, float d1, float d2)
        {
            float d0 = -d1 / (d2 - d1);

            Vector3 p0 = p1 + (p2 - p1) * d0;

            return p0;
        }

        public static Vector3 TrilinearIntersection( Bounds _bounds, float[] _densities )
        {

            List<Vector3> interpolations = new List<Vector3>();

            //Arranging 
            int count = -1;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Vector3 pa = _bounds.center - _bounds.size / 2;
                    Vector3 pb = _bounds.center - _bounds.size / 2 + new Vector3( _bounds.size.x, 0, 0 );

                    float d1 = _densities[count++];
                    float d2 = _densities[count++];

                    interpolations.Add( Intersection(pa, pb, d1, d2) );
                }
            }

            Vector3 p0 = interpolations[0];
            Vector3 p1 = interpolations[1];

            Vector3 p2 = interpolations[0];
            Vector3 p3 = interpolations[1];

            Vector3 pA = 1 / 2 * (p1 - p0);

            Vector3 pB = 1 / 2 * (p3 - p2);

            Vector3 intersection = 1 / 2 * (pB - pA);


            return intersection;
        }
    }
}

