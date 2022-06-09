using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Mjollnir.Isosurface.VoxelUtilities;

public partial class Octree<T>
{

	public OctreeNode children { get; private set; }

	public Bounds bounds { get; private set; }


	public int MIN_SIZE { get; private set; } = 1;



	public Octree( Vector3 _location , uint _size = 1 )
    {
		children	= new OctreeNode( _location, _size, this);
	}

	/// <summary>
	/// Find a node.
	/// </summary>
	/// <param name="_position"></param>
	/// <param name="_size"></param>
	/// <returns></returns>
	public OctreeNode Find(Vector3 _position, int _size = 0)
	{
		if (children.bounds.Contains(_position))
		{
			return FindVoxel(children);
		}
		else
		{
			return null;
		}


		OctreeNode FindVoxel(OctreeNode _voxel)
		{

			if (_size == _voxel.size)
			{
				return _voxel;
			}
			else if (_voxel.children != null)
			{
				int childIndex = 0;

				for (int i = 0; i < 3; i++)
				{
					if (_position[i] > _voxel.bounds.center[i])
						childIndex |= 1 << (2 - i);
				}

				return FindVoxel(_voxel.children[childIndex]);
			}
			else
			{
				return _voxel;
			}
		}
	}

	/// <summary>
	/// Add T-type object to octree
	/// </summary>
	/// <param name="_object"></param>
	/// <param name="_position"></param>
	/// <returns> Type of changes done to octree while adding object </returns>
	public int Add( T _object, Vector3 _position )
	{
		int changes = 0;

		//Checking if inside the octree

		if( !bounds.Contains(_position) )
		{
			Growth(_position);
		}


		//children.Add( T _object, Vector3 _position );


		return changes;
	}

	/// <summary>
	/// Find nodes inside bounds.
	/// </summary>
	/// <param name="_bounds"></param>
	/// <param name="_size"></param>
	/// <returns></returns>
	public List<OctreeNode> FindNodes(Bounds _bounds, int _size)
	{
		List<OctreeNode> nodes = new List<OctreeNode>();

		dFindNodes(children);

		return nodes;

		void dFindNodes(OctreeNode _node)
		{
			if (_bounds.Intersects(_node.bounds))
			{
				if (_node.size == _size)
				{
					nodes.Add(_node);
				}
				else if (_node.children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						dFindNodes(_node.children[i]);
					}
				}
				else if (_node.children == null)
				{
					nodes.Add(_node);
				}
			}
		}
	}

	/// <summary>
	/// Find a neighbours which share edges.
	/// </summary>
	/// <param name="_node"></param>
	/// <returns></returns>
	public List<OctreeNode> FindNeighbour(OctreeNode _node)
	{
		if (_node == null) { return null; }

		List<OctreeNode> neighbours = new List<OctreeNode>();
		Bounds searchBounds = new Bounds(_node.bounds.center,
										   _node.bounds.size + new Vector3(MIN_SIZE, MIN_SIZE, MIN_SIZE) / 2
										);

		neighbours = FindNodes(searchBounds, _node.size);

		//Just read the code
		OctreeNode[] toreturn = new OctreeNode[19];
		toreturn[9] = _node;

		int size = Mathf.RoundToInt(_node.size / 2f + MIN_SIZE / 2f);
		int loop_index = 0;

		//HueHueHue
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				for (int z = -1; z <= 1; z++)
				{

					if ((x == 0 || y == 0 || z == 0))
					{
						Vector3 point = searchBounds.center + new Vector3(size * x, size * y, size * z);

						foreach (OctreeNode v in neighbours)
						{
							if (v.bounds.Contains(point))
							{
								toreturn[loop_index] = v;
								neighbours.Remove(v);
								break;
							}
						}
						loop_index++;
					}

				}
			}
		}

		List<OctreeNode> toreturnlist = new List<OctreeNode>();
		toreturnlist.AddRange(toreturn);
		return toreturnlist;
	}


    #region MANAGEMENT

	private bool Growth( Vector3 _direction)
	{
		Vector3 direction = (children.bounds.center - _direction);

		int growX = direction.x >= 0 ? 1 : -1;
		int growY = direction.y >= 0 ? 1 : -1;
		int growZ = direction.z >= 0 ? 1 : -1;

		// Getting the size which is needed to octree to contain the point.
		int osizeX = (int)Mathf.RoundToInt(Mathf.Log(direction.x) / Mathf.Log(bounds.size.x)) - (int)children.octree_size;
		int osizeY = (int)Mathf.RoundToInt(Mathf.Log(direction.y) / Mathf.Log(bounds.size.x)) - (int)children.octree_size;
		int osizeZ = (int)Mathf.RoundToInt(Mathf.Log(direction.z) / Mathf.Log(bounds.size.x)) - (int)children.octree_size;

		Vector3 dirX = Vector3.zero;
		Vector3 dirY = Vector3.zero;
		Vector3 dirZ = Vector3.zero;

		if ( osizeX > 0 )
		{
			dirX = growX * new Vector3(children.bounds.size.x, 0, 0) / 2f;
		}else if ( osizeY > 0)
		{
			dirY = growY * new Vector3(0, children.bounds.size.y, 0) / 2f;
		}
		else if ( osizeZ > 0)
		{
			dirZ = growZ * new Vector3(0, 0, children.bounds.size.z) / 2f;
		}

		Vector3 new_center = children.bounds.center + (dirX + dirY + dirZ ) / 2f;

		OctreeNode tmp_child = new OctreeNode(new_center, children.octree_size + 1, this);
		tmp_child.Subdivide();

		// Getting child_index for older childern 
		int childIndex = 0;

		for (int i = 0; i < 3; i++)
		{
			if (new_center[i] > children.bounds.center[i])
				childIndex |= 1 << (2 - i);
		}

		tmp_child.children[childIndex] = children;

		children = tmp_child;

		return true;
	}

    #endregion


    #region FUNCTIONS - DEBUG

    public void SubdivideAll()
	{
		children.SubdivideAll();
	}


	#endregion

	#region FUNCTIONS - DEBUG - GIZMOS

	/// <summary>
	/// Draw nodes to desired depth
	/// </summary>
	/// <param name="_depth"></param>
	public void GizmosDrawDepth( int? depth = null)
	{

		DrawNodes(children, depth);

		void DrawNodes( OctreeNode _node, int? _depth = null )
		{
			if ( _depth == null || _depth >= 0 )
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube( _node.bounds.center, _node.bounds.size );

				if (_node.children != null)
				{
					for (int i = 0; i < 8; i++)
					{
						DrawNodes(_node.children[i], _depth - 1);
					}
				}
			}
		}
	}

	#endregion
}
