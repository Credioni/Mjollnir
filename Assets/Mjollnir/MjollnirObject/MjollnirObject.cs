/*
	Mjollnir.cs 

	Mjollnirobject (Octree)
	Voxel (Octree Node)

	MjollnirObject <=> Meshbuilder & MultiThreading
			|- Voxel <=> Densityfield & MultiThreading

	TOOD
	Voxel Construction to multithreading to mjollnir thread.

	Bugs!
	1. If voxels children share corner with density 0 the surface will have holes! Fixable => Dirty

*/


using System.Collections.Generic;
using UnityEngine;

using static Mjollnir.Isosurface.VoxelUtilities;


namespace Mjollnir{
	namespace Isosurface{

		public partial class MjollnirObject{

			public int DEBUG_CHUNK_SIZE { get; private set; }	= 16;
			public int CHUNK_MIN_SIZE	{ get; private set; }	= 8;
			public int VOXEL_MIN_SIZE	{ get; private set; }	= 2;


			private int node_count;
			private Voxel 	voxeltree;

			private MeshBuilder 	meshbuilder;
			private Densityfield 	densityfield;

			// Creates everything gameObject under rootObject;
			private GameObject		rootObject;
			private MultiThreading	multiThreadingObject;

			/// <summary>
			/// Contains created chunks. Loaded and preloaded chunks
			/// chunkObject string = 'voxel1.id' or 'v1.id-v2.id' or 'v1.id-v2.id-v3.id-v4.id'
			/// </summary>
			private Dictionary <string, GameObject> chunkObject;




			/// <summary>
			/// Creates MjollnirObject
			/// </summary>
			/// <param name="_size"> Sides must be power of 2 </param>
			/// <param name="_pos"></param>
			/// <param name="_multithreadingObject"></param>
            public MjollnirObject( int _size , Vector3 _pos, MultiThreading _multithreadingObject = null )
			{

				//Check that size is created as power of 2
				double power = Mathf.Log(_size, 2);

				if( power % 1 == 0)
				{
					voxeltree = new Voxel(new Bounds(_pos, new Vector3(_size, _size, _size)), this);
					node_count++;
				}
				else
				{
					UnityEngine.Debug.Log("Error while creating MjollnirObject. Size must be power of 2! -" + power);
				}

				chunkObject = new Dictionary<string, GameObject>();

				multiThreadingObject = _multithreadingObject;
			}

			/// <summary>
			/// Assign minimum chunk and voxel size.
			/// </summary>
			/// <param name="_voxel_min"></param>
			/// <param name="_chunk_min"></param>
			public void Init( int _voxel_min, int _chunk_min ){
				CHUNK_MIN_SIZE = _chunk_min;
				VOXEL_MIN_SIZE = _voxel_min;
			}

			/// <summary>
			/// Assign gameobject for chunks.
			/// </summary>
			/// <param name="_rootObject"></param>
			public void AssignGameObject( GameObject _rootObject )
			{
				rootObject = _rootObject;
			}

			/// <summary>
			/// Find a voxel, fast find.
			/// </summary>
			/// <param name="_position"></param>
			/// <param name="_size"></param>
			/// <returns></returns>
			public Voxel Find(Vector3 _position, int _size = 0)
			{
				if ( voxeltree.bounds.Contains(_position) )
				{
					return FindVoxel( voxeltree );
				}
				else
				{
					return null;
				}


				Voxel FindVoxel( Voxel _voxel )
				{

					if ( _size == _voxel.size)
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

						return FindVoxel( _voxel.children[childIndex] );
					}
					else
					{
						return _voxel;
					}
				}
			}

			/// <summary>
			/// Find nodes inside bounds.
			/// </summary>
			/// <param name="_bounds"></param>
			/// <param name="_size"></param>
			/// <returns></returns>
			public List<Voxel> FindNodes( Bounds _bounds, int _size)
			{
				List<Voxel> nodes = new List<Voxel>();

				dFindNodes( voxeltree );

				return nodes;

				void dFindNodes( Voxel _node )
				{
					if ( _bounds.Intersects(_node.bounds) )
					{
						if ( _node.size == _size )
						{
							nodes.Add( _node );
						}
						else if (_node.children != null )
						{
							for (int i = 0; i < 8; i++)
							{
								dFindNodes( _node.children[i] );
							}
						}
						else if (_node.children == null)
						{
							nodes.Add( _node );
						}
					}
				}
			}

			/// <summary>
			/// Find a neighbours which share edges.
			/// </summary>
			/// <param name="_node"></param>
			/// <returns></returns>
			public List<Voxel> FindNeighbour(Voxel _node)
			{
				if (_node == null) { return null; }

				List<Voxel> neighbours = new List<Voxel>();
				Bounds searchBounds = new Bounds( _node.bounds.center,
				 								  _node.bounds.size + new Vector3(VOXEL_MIN_SIZE, VOXEL_MIN_SIZE, VOXEL_MIN_SIZE) / 2
												);

				neighbours = FindNodes( searchBounds, _node.size);

				//Just read the code
				Voxel[] toreturn = new Voxel[19];
				toreturn[9] = _node;

				int size = Mathf.RoundToInt(_node.size / 2f + VOXEL_MIN_SIZE / 2f);
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

								foreach (Voxel v in neighbours)
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

				List<Voxel> toreturnlist = new List<Voxel>();
				toreturnlist.AddRange(toreturn);
				return toreturnlist;
			}
			
			/// <summary>
			/// Construct mjollnir object.
			/// </summary>
			/// <param name="_densityfield"></param>
			/// <param name="_threshold"></param>
			/// <param name="_cluster"></param>
			/// <param name="_debug"></param>
			public void Construct( ref Densityfield _densityfield, float _threshold, bool _cluster = false, bool _debug = false ){
				double sw 		= Time.realtimeSinceStartup;
				meshbuilder 	= new MeshBuilder();

				// CONSTRUCT
				voxeltree.Construct(ref _densityfield, ref meshbuilder, ref node_count);
				if( _debug) UnityEngine.Debug.Log("Constructed in: " + (Time.realtimeSinceStartup - sw));

				/**/
				// CLUSTER
				if( _cluster ){
					sw = Time.realtimeSinceStartup;
					Manifold.ClusterCellBase(ref voxeltree, ref meshbuilder, _threshold);
					if(_debug) UnityEngine.Debug.Log("ClusterCellBase in: " + (Time.realtimeSinceStartup - sw));
				}

				// PROCESS
				sw = Time.realtimeSinceStartup;
				Manifold.ProcessObject(ref voxeltree, ref meshbuilder, _threshold);
				if( _debug ) UnityEngine.Debug.Log("ProcessObject in: " + (Time.realtimeSinceStartup - sw));
			}

			/// <summary>
			/// Simulate Construct without capable of creating surface.
			/// </summary>
			/// <param name="_densityfield"></param>
			public void SimulateConstruct( Densityfield _densityfield = null ){
				voxeltree.SimulateConstruct( ref node_count, _densityfield );
			}

			/// <summary>
			/// Construct mjollnir object as multithreaded.
			/// </summary>
			/// <param name="_densityfield"></param>
			/// <param name="_threshold"></param>
			public void Construct_MultiThread( ref Densityfield _densityfield, float _threshold = 0f ){

				meshbuilder 	= new MeshBuilder();

				// CONSTRUCT
				voxeltree.Construct(ref _densityfield, ref meshbuilder, ref node_count);

				voxeltree.MultiThreading_Generation( DEBUG_CHUNK_SIZE, _threshold, _densityfield, ref multiThreadingObject );
			}

			/// <summary>
            /// Callback function from threads. 
            ///	Handels chunks and also chunk gaps.
            /// </summary>
            /// <param name="_threadedData"></param>
			public void Construct_Callback( OperatorData _threadedData ){

				Voxel[]		node 		= _threadedData.node;
				MeshBuilder meshBuilder = _threadedData.meshbuilder;
				float 		threshold	= _threadedData.threshold;
				string 		id 			= meshBuilder.id;
				bool 		genNeigh    = _threadedData.neighbours;


				//UnityEngine.Debug.Log(meshBuilder);

				if ( meshBuilder.vertex.Count == 0  )
					return;

				// Creating gameobject if it isnt in memory TODO
				GameObject Chunk;

				if( !chunkObject.ContainsKey(id) ){
					Chunk = new GameObject();

					Chunk.transform.position = new Vector3(0,0,0);
					Chunk.AddComponent< MeshFilter >();
					Chunk.AddComponent< MeshRenderer >();
					Chunk.GetComponent< MeshRenderer >().sharedMaterial = new Material(Shader.Find("Diffuse"));
					//Chunk.GetComponent< MeshRenderer >().sharedMaterial = new Material(Shader.Find("Default-Material"));
					Chunk.name = meshBuilder.id;
					chunkObject.Add(id, Chunk);
				}else{
					Chunk = chunkObject[id];
				}

				Chunk.transform.parent = rootObject.transform;

				// Adding meshdata
				Mesh mesh = Chunk.GetComponent< MeshFilter >().mesh;

				meshBuilder.GenerateMesh( ref mesh, threshold );
				Chunk.GetComponent< MeshFilter >().mesh = mesh;

				//UnityEngine.Debug.Log( meshBuilder );

				//Update neigbouring chunks to match the changed surface
				if( genNeigh ) Update_NeighbourChunksSurface( node[0] );
			}

			/// <summary>
			/// Creates gaps and corners between chunks.
			/// </summary>
			/// <param name="_node"></param>
			/// <param name="_threshold"></param>
			private void Update_NeighbourChunksSurface( Voxel _node, float _threshold = 0f ){

				/*
				 * These numbers contains the indexes around founded voxels neighbours.
				 *	Which are handmade to correspond right order for surface creation.
				 */

				int [,] neighbours_external = new int[18,4]{
					/* Edges  0-11 */
					{  5,  6,  8,  9 },
					{  6,  7,  9, 10 },
					{  8,  9, 11, 12 },
					{  9, 10, 12, 13 },

					{  1,  8,  2,  9 },
					{  8, 15,  9, 16 },
					{  2,  9,  3, 10 },
					{  9, 16, 10, 17 },

					{  0,  2,  6,  9 },
					{  6, 14,  9, 16 },
					{  2,  9,  4, 12 },
					{  9, 16, 12, 19 },

					/* Faces 12-17*/

					{  2,  9, -1, -1 },
					{  9, 16, -1, -1 },

					{  6,  9, -1, -1 },
					{  9, 12, -1, -1 },

					{  8,  9, -1, -1 },
					{  9, 10, -1, -1 }
				};

				List<Voxel> neighbours = FindNeighbour( _node );

				//Faces - Creating gaps between chunks.

				for ( int i = 12; i < 18; i++){
					if( ((_node.external >> i) & 1) == 1 ){

						//Initializing node for surface-algrotihm
						// contains current node and a selected neighbour node in a tmp_node.
						Voxel[] tmp_nodes = new Voxel[2];

						int[] nindex = new int[2];

						string tmp_id = "G ";

						//Finding neighbour node to tmp_node.
						for( int j = 0; j < 2; j++){
							//Node index in founded neighbours face node
							nindex[j] = neighbours_external[i,j];

							if( neighbours[ nindex[j] ] == null )
								break;

							tmp_nodes[j] = neighbours[ nindex[j] ];
							tmp_id += tmp_nodes[j].id() + (j == 0 ? "-" : "") ;

						}

						if (tmp_nodes[0] == null || tmp_nodes[1] == null)
							continue;

						//Selectin direction
						int dir;

						if( i == 12 || i == 13)
						{
							dir = 0;
						}else if (i == 14 || i == 15)
						{
							dir = 1;
						}
						else
						{
							dir = 2;
						}

						//Initializing Threading
						MeshBuilder meshbuilder = new MeshBuilder(tmp_id);

						OperatorData opData = new OperatorData( tmp_nodes, meshbuilder, densityfield,
																_threshold, new ThreadingJobs[] { ThreadingJobs.ProcessFace}, false, dir );

						ThreadInfo<OperatorData> threadInfo = new ThreadInfo<OperatorData>( Construct_Callback, opData ) ;
						multiThreadingObject.Assign(threadInfo);
	
					}
				}
				

				//Edges - Creating corners between chunks.
				for (int i = 0; i < 11; i++)
				{
					if (((_node.external >> i) & 1) == 1)
					{
						string tmp_id = "X ";
						Voxel[] tmp_nodes = new Voxel[4];
						ThreadingJobs[] operatorJobs = new ThreadingJobs[] { ThreadingJobs.ProcessEdge };

						// Finding nodes for processing
						for (int j = 0; j < 4; j++)
						{
							//Neighbours index
							int nindex = neighbours_external[i, j];

							if (neighbours[nindex] == null)
								break;

							tmp_nodes[j] = neighbours[nindex];
							tmp_id += tmp_nodes[j].id() + (j < 3 ? "-" : "");
						}

						if (tmp_nodes[0] == null || 
							tmp_nodes[1] == null || 
							tmp_nodes[2] == null || 
							tmp_nodes[3] == null
							)
						{
							continue;
						}


						//Selectin direction
						int dir;

						if (i < 4)
						{
							dir = 0;
						}
						else if ( 4 <= i && i < 8 )
						{
							dir = 1;
						}
						else
						{
							dir = 2;
						}

						//Initializing Threading
						MeshBuilder meshbuilder = new MeshBuilder(tmp_id);

						OperatorData opData = new OperatorData(tmp_nodes, meshbuilder, densityfield, _threshold, operatorJobs, false, dir);
						ThreadInfo<OperatorData> threadInfo = new ThreadInfo<OperatorData>(Construct_Callback, opData);
						multiThreadingObject.Assign(threadInfo);
					}
				}
			}
			
			/// <summary>
			/// Creates mesh from meshbuilder.
			/// </summary>
			/// <param name="_mesh"></param>
			/// <param name="_threshold"></param>
			public void GenerateMesh(ref Mesh _mesh, float _threshold)
			{
				_mesh = new Mesh();
				meshbuilder.GenerateMesh(ref _mesh, _threshold);
			}

			#region GIZMOS

			/// <summary>
			/// Draw nodes to desired depth
			/// </summary>
			/// <param name="_depth"></param>
			public void Draw( int? _depth = null ){
				voxeltree.DrawNodes( _depth );
			}

			/// <summary>
			/// Draw leaf nodes
			/// </summary>
			public void DrawLeafNodes()
			{
				DrawNode(voxeltree);

				void DrawNode(Voxel _node)
				{
					if ( _node.size == VOXEL_MIN_SIZE )
					{
						Gizmos.color = new Color(0, 0, 1, 1);
						Gizmos.DrawWireCube( _node.bounds.center, _node.bounds.size);
					}

					if ( _node.children != null)
					{
						for (int i = 0; i < 8; i++)
						{
							DrawNode(_node.children[i]);
						}
					}
				}
			}

			/// <summary>
			/// Draw lines where surface is continuing from chunk. 
			/// </summary>
			/// <param name="_depth"></param>
			public void DrawExternal( int _depth = 100000 ){

				DrawExternal_Recursive( voxeltree, _depth );

				void DrawExternal_Recursive( Voxel _node, int _depthX ){
					if( _node == null) return;

					for (int i = 0; i < 12; i++){
						if( ((_node.external >> i) & 1) == 1 ){
							Gizmos.color = Color.white;
							Gizmos.DrawLine( _node.bounds.center,  _node.size * EDGE_DIR[i] + _node.bounds.center );
						}
					}

					for (int i = 12; i < 18; i++){
						if( ((_node.external >> i) & 1) == 1 ){
							Gizmos.color = Color.white;
							Gizmos.DrawLine( _node.bounds.center,  _node.size * FACE_DIR[i - 12] + _node.bounds.center );
						}
					}

				}
			}

			/// <summary>
			/// Draw gameobjects to screen to correspond voxel nodes.
			/// </summary>
			/// <param name="_collapsed"></param>
			public void DrawPhysicalNodes( bool _collapsed = true )
			{
				DrawNode(voxeltree);

				void DrawNode( Voxel _node)
				{
					if( _node.collapsed == _collapsed && _node.vertex.Count > 0 )
					{
						GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
						cube.transform.position = _node.bounds.center;
						cube.transform.name		= _node.Info();
					}

					if (_node.children != null)
					{
						for (int i = 0; i < 8; i++)
						{
							DrawNode(_node.children[i]);
						}
					}
				}
				
			}

			/// <summary>
			/// Draw every vertex.
			/// </summary>
			/// <param name="_collapsed"></param>
			public void DrawVertexNodes(bool _collapsed = true)
			{
				DrawVertex(voxeltree);

				void DrawVertex(Voxel _node)
				{
					if ( _collapsed == _node.collapsed )
					{
						foreach( Vertex v in _node.vertex)
						{
							GameObject pvertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							pvertex.transform.position	= v.GetPosition();
							pvertex.transform.name		= v.id;
						}
					}


					if (_node.children != null)
					{
						for (int i = 0; i < 8; i++)
						{
							DrawVertex(_node.children[i]);
						}
					}
				}

			}

			#endregion //GIZMOS
		}
	}
}
