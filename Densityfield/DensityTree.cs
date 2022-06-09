using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Mjollnir.Utilities;



namespace Mjollnir
{
    public partial class Densityfield
    {
        private class DensityTree
        {

            private DensityTreeNode rootNode;

            public Bounds bounds { get { return rootNode.bounds; } private set { } }

            public int MIN_SIZE { get; private set; } = 1;

            public DensityTree(Bounds _bounds)
            {
                rootNode = new DensityTreeNode( this, _bounds );
            }

            public void AddDebug( Bounds _bounds, float[,,] _densities )
            {
                rootNode.Add( _bounds, _densities );
            }

            public bool FindDensity( Vector3 _pos, ref float _density )
            {

                if ( rootNode.FindDensity( _pos, ref _density) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        private class DensityTreeNode
        {
            float[,,] densities;

            public Bounds bounds;

            public int size { get { return (int)this.bounds.size.x; } }

            public DensityTree root { get; private set; }
            public DensityTreeNode      parent { get; private set; }
            public DensityTreeNode[]    children { get; private set; }

            public int child_index { get; private set; }

            public DensityTreeNode(DensityTree _root, Bounds _bounds, DensityTreeNode _parent = null, int _child_index = 0)
            {
                root = _root;
                parent = _parent;
                bounds = _bounds;

                child_index = _child_index;
            }

            private bool Subdivide()
            {
                if (bounds.size.x > root.MIN_SIZE &&
                     children == null
                   )
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Bounds temp_bounds = new Bounds();
                        temp_bounds.size = bounds.size / 2f;
                        temp_bounds.center = bounds.center + PRECENTER_CHILDNODE[i] * size / 2f;

                        children[i] = new DensityTreeNode(root, temp_bounds, this, i);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool SubdivideAll()
            {
                if (Subdivide())
                {
                    for (int i = 0; i < 8; i++)
                    {
                        children[i].SubdivideAll();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Add densities to volumetric object.
            /// </summary>
            /// <param name="_bounds"></param>
            /// <param name="_densities"></param>
            /// <param name="_fill">Does densities be filling or caving</param>
            public void Add( Bounds _bounds, float[,,] _densities, bool _fill = false )
            {
                bounds = _bounds;
                int dsize = Mathf.RoundToInt( Mathf.Pow( _densities.Length, 1/3f) );

                if (_fill)
                {
                    for (int i = 0; i < dsize; i++)
                    {
                        for (int j = 0; j < dsize; j++)
                        {
                            for (int k = 0; k < dsize; k++)
                            {

                            }
                        }
                    }
                }
                else
                {

                }

                densities = _densities;
            }

             public bool FindDensity( Vector3 _pos, ref float _density )
             {
                if ( bounds.Contains(_pos) && densities != null )
                {
                    Vector3 localPos = _pos - (bounds.center - bounds.size / 2);

                    int x = Mathf.FloorToInt(localPos.x);
                    int y = Mathf.FloorToInt(localPos.y);
                    int z = Mathf.FloorToInt(localPos.z);

                    _density = densities[x,y,z];
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public string id()
            {
                if (parent != null)
                {
                    return parent.id() + child_index.ToString();
                }
                else
                {
                    return child_index.ToString();
                }
            }

        }

    }
}