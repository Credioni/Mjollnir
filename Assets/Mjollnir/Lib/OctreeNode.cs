using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Mjollnir.Isosurface.VoxelUtilities;


public partial class Octree<T>
{
    public class OctreeNode
    {

        public Bounds bounds;

        public int size { get { return (int)this.bounds.size.x; } }

        public uint octree_size { get; private set; }

        public Octree<T> root { get; private set; }
        public OctreeNode parent { get; private set; }
        public OctreeNode[] children { get; private set; }


        public uint child_index { get; private set; }


        public OctreeNode( Vector3 _location, uint _size, Octree<T> _root)
        {
            root    = _root;
            parent  = null;
            octree_size = _size;
            child_index = 0;

            uint tsize = (uint)Mathf.Pow(2, _size);
            bounds = new Bounds( _location, new Vector3(tsize, tsize, tsize) );

 
        }

        private OctreeNode( Bounds _bounds, Octree<T> _root, OctreeNode _parent, uint _child_index )
        {
            root = _root;
            parent = _parent;
            bounds = _bounds;

            octree_size = (uint)parent.octree_size / 2;

            child_index = _child_index;
        }

        public bool Subdivide()
        {
            if (bounds.size.x > root.MIN_SIZE && children == null )
            {
                children = new OctreeNode[8];

                for (uint i = 0; i < 8; i++)
                {
                    Bounds temp_bounds  = new Bounds();
                    temp_bounds.size    = bounds.size / 2f;
                    temp_bounds.center  = bounds.center + PRECENTER_CHILDNODE[i] * size / 2f;

                    children[i] = new OctreeNode(temp_bounds, root, this, i);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SubdivideAll()
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

        public List<uint> id()
        {
            if (parent != null)
            {
                List<uint> tmp = parent.id();

                tmp.Insert(0, child_index);

                return tmp;

            }
            else
            {
                return new List<uint>() { child_index };
            }
        }

    }

}