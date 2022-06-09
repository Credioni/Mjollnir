/*
	Voxel.cs 

	Mjollnirobject (Octree)
	Voxel (Octree Node)

	MjollnirObject <=> Meshbuilder & MultiThreading
			|- Voxel <=> Densityfield & MultiThreading

	Notes:
	Vertex indexing happens in contouring process
*/

using System.Collections.Generic;
using UnityEngine;

using QEFSolverMDC;

using static Mjollnir.Isosurface.VoxelUtilities;



namespace Mjollnir{
	namespace Isosurface{

		/* Should maybe be internal class -> someday*/
		public class Voxel {

			//Voxel's corners as an bit presentation 0-255
			public int Byte { get; private set; } = 0;

			internal List<Vertex> vertex;

			public Bounds 	bounds	{ get; private set; }
			public int 		size	{ get{return (int)bounds.size.x;} }

			public MjollnirObject 	rootObject	{ get; private set; }
			public Voxel 	root		{ get; private set; }
			public Voxel 	parent		{ get; private set; }
			public Voxel[] 	children	{ get; private set; }

			// Parents child index for this node
			public int 	child_index { get; private set; } = 0;
			// This is where the chunk begins. Used in LOD.
			public bool collapsed	{ get; private set; } = false;
			// If inner surface continues outside of this node it is presented here. Bit operated. Bits 0-11 Edges, 11-17 Faces
			public int	external	{ get; private set; } = 0;



			public Voxel( Bounds _bounds, MjollnirObject _rootObject = null, Voxel _root = null, Voxel _parent = null, int _childIndex = 0){
				rootObject 	= _rootObject;
				root 		= _root != null ? _root:this;
				parent 		= _parent;
				bounds 		= _bounds;

				vertex 		= new List<Vertex>();
				child_index = _childIndex;
			}

			//Assign operations to Multithreading thread.
			public void MultiThreading_Generation( int _size, float _threshold, Densityfield _densityfield, ref MultiThreading _multiThreadingObject ){
				if( size == _size )
				{
					MeshBuilder meshbuilder = new MeshBuilder( "C " + id() );

					OperatorData opData = new OperatorData( new Voxel[1] { this }, meshbuilder, _densityfield, _threshold, new ThreadingJobs[]{ ThreadingJobs.ProcessObject }, true );
					ThreadInfo<OperatorData> threadInfo = new ThreadInfo<OperatorData>( rootObject.Construct_Callback, opData ) ;

					_multiThreadingObject.Assign( threadInfo );
				}
				else if ( !collapsed )
				{
					for( int i = 0; i < 8; i++){
						children[i].MultiThreading_Generation( _size, _threshold, _densityfield, ref _multiThreadingObject );
					}
				}
			}

			/* TODO
			public void Update_LevelOfDetail( Vector3 _position ){
				float distance = bounds.sqrtDistance( _position );

				// 0 1 2 3  4  5  6  7   8
				// 1 2 4 8 16 32 64 128 254

				int power 	  = Mathf.RoundToInt( Mathf.Log( size, 10)/ Mathf.Log(2, 10) );
				int power_min = Mathf.RoundToInt( Mathf.Log(_rootObject.CHUNK_MIN_SIZE, 10)/ Mathf.Log(2, 10) );

				if( power > power_min && children != null ){
					for (int i = 0; i < 8; i++) {

					}
				}else{

				}
			}
			*/

			/// <summary>
			///  Creates current voxel and its childrens to the leaf level.
			///  Also automatically simplifies the octree.
			/// </summary>
			/// <param name="_densityfield"></param>
			/// <param name="_meshbuilder"></param>
			/// <param name="_node_count"></param>
			/// <returns></returns>
			public bool Construct( ref Densityfield _densityfield, ref MeshBuilder _meshbuilder, ref int _node_count ){
				if ( size > rootObject.VOXEL_MIN_SIZE ) {
					Byte 	 = 0;

					//Getting densities from densityfield
					int count_collapsed = 0;

					for (int i = 0; i < 8; i++){
						if ( _densityfield.GetDensity(bounds.center  + PRECENTER_CHILDNODE[i] * size) > 0)
							Byte |= 1 << i;
					}

					/**/
					//Algorithm that should faster surface creation in some level, doesnt work in every case.
					if ( (	(Byte == 0   && _densityfield.GetDensity( bounds.center ) <= 0 ) ||
							(Byte == 255 && _densityfield.GetDensity( bounds.center ) >  0 ) ) &&
						size == rootObject.VOXEL_MIN_SIZE*2  )
					{
						collapsed = true;
						return true;
					}
					
					children = new Voxel[8];

					//Creating childrens 
					for (int i = 0; i < 8; i++) {
						Bounds temp_bounds	= new Bounds ();
						temp_bounds.size	= bounds.size / 2f;
						temp_bounds.center	= bounds.center + PRECENTER_CHILDNODE [i] * size/2f;

						children [i] = new Voxel (temp_bounds, rootObject, root, this, i);

						if( children[i].Construct (	ref _densityfield,
													ref _meshbuilder,
													ref _node_count
												  ) ){
							count_collapsed++;
						}
						else
						{
							_node_count++;
						}

						
						// Getting collapsed edges from child
						int cexternal = children[i].external;

						//Assigning childrens external to correspond parents external
						for (int j = 0; j < 18; j++) {
							int dexternal = NODE_EXTERNAL[i, j];
							int dchild	  = (cexternal >> j) & 1;

							if( dexternal >= 0 ){
								external |= dchild << dexternal;
							}
						}

					}

					//Deleting non needed childrens.
					if(count_collapsed > 7)
					{
						//Assigning byte to this voxel without calculating it from densityfield.
						Byte = children[0].Byte;

						collapsed	= true;
						children	= null;

						return true;
					}else{
						collapsed = false;

						return false;
					}
				}else{
					return ConstructLeaf( ref _densityfield, ref _meshbuilder );
				}

				return false;
			}

			/// <summary>
			/// Construct leaf nodes from densityfield.
			/// </summary>
			/// <param name="_densityfield"></param>
			/// <param name="_meshbuilder"></param>
			/// <returns></returns>
			private bool ConstructLeaf(ref Densityfield _densityfield, ref MeshBuilder _meshbuilder ){

				Byte = 0;
				collapsed = true;

				//Contains the gradient field's values.
				float[] densities = new float[8];

				for (int i = 0; i < 8; i++)
				{
					if ((densities[i] = _densityfield.GetDensity(bounds.center  + PRECENTER_CHILDNODE[i] * size)) > 0)
						Byte |= 1 << i;
				}

				if (Byte == 0 || Byte == 255) return true;


				int edge;
				Vector3 a, b, n, intersection, normal;

				//Creating vertices for node.
				for (int j = 0; j < 4; j++){
					if( manifold_edge_connection_list[ Byte, j, 0] < 0){ break; }

					normal = Vector3.zero;

					vertex.Add(new Vertex());
					vertex[j].qef = new QEFSolver();
					vertex[j].collapsed_edges = new int[12];

					for (int k = 0; k < 7; k++){
						 edge = manifold_edge_connection_list[ Byte, j, k];

						 if(edge >= 0){
							 a = PRECENTER_CHILDNODE[ EDGE_PAIRS[edge, 0] ] * size;
							 b = PRECENTER_CHILDNODE[ EDGE_PAIRS[edge, 1] ] * size;

							 //intersection = bounds.center;
							 intersection	= Intersection(a , b, densities[ EDGE_PAIRS[edge, 0]], densities[ EDGE_PAIRS[edge, 1]]);

							 n = _densityfield.GetNormal(bounds.center + intersection);
							 normal += n;

							 vertex[j].qef.Add(bounds.center + intersection, n);
							 vertex[j].collapsed_edges[edge] = 1;
							 external |= 1 << edge;
						 }
					}

					vertex[j].index 	= _meshbuilder.vertex.Count;
					vertex[j].normal 	= normal.normalized;
					vertex[j].euler 	= 1;
					vertex[j].in_cell 	= child_index;
					vertex[j].euler_characteristic = true;

					vertex[j].id = id() + " v: " + j ;
					vertex[j].qef.Solve(1e-6f, 4, 1e-6f);
					//vertex[j].color = _densityfield.GetSurfaceColor( vertex[j].qef.Solve(1e-6f, 4, 1e-6f));
					vertex[j].error 	= vertex[j].qef.GetError();
					vertex[j].collapsed = true;
				}

				return false;
			}

			/// <summary>
			/// Simulate Construct to finest level - Does not support external, collapsed or surface creation
			/// </summary>
			/// <param name="_node_count"></param>
			/// <param name="_densityfield"></param>
			/// <returns></returns>
			public bool SimulateConstruct( ref int _node_count, Densityfield _densityfield = null ){

				if( _densityfield != null ){
					Byte 	 = 0;

					for (int i = 0; i < 8; i++){
						if ( _densityfield.GetDensity(bounds.center  + PRECENTER_CHILDNODE[i] * size) > 0)
							Byte |= 1 << i;
					}

					if ( (Byte == 0 && _densityfield.GetDensity( bounds.center ) <= 0 ) || (Byte == 255 && _densityfield.GetDensity(bounds.center) > 0) ){
						collapsed = true;
						return true;
					}
				}

				if (size > rootObject.VOXEL_MIN_SIZE ) {
					Byte 	 = 0;

					children = new Voxel[8];

					for (int i = 0; i < 8; i++) {
						Bounds temp_bounds = new Bounds ();
						temp_bounds.size = bounds.size / 2f;
						temp_bounds.center = bounds.center + PRECENTER_CHILDNODE [i] * size/2f;
						children [i] = new Voxel (temp_bounds, rootObject, root, this, i);
						children[i].SimulateConstruct( ref _node_count, _densityfield );
						_node_count++;
					}

					collapsed = false;
					return false;
				}else{
					collapsed = true;
				}

				return false;
			}

			/// <summary>
			/// Get external faces and edges in a table.
			/// </summary>
			/// <returns></returns>
			public int[] GetExternal(){
				int[] toreturn = new int[18];

				for (int i = 0; i < 18; i++) {
					toreturn[i] = (external >> i) & 1;
				}

				return toreturn;
			}

			public string Info()
			{
				return ToString() + InfoExternal() + " Vertex: " + vertex.Count;
			}

			/// <summary>
			/// Human-Readable form for external.
			/// </summary>
			/// <returns></returns>
			private string InfoExternal()
			{
				string toreturn = "E";

				for (int i = 0; i < 18; i++)
				{
					if(i == 12)
					{
						toreturn += " F";
					}

					if( i < 12 && (i % 4) == 0 && i != 0)
					{
						toreturn += " ";
					}else if ( i >= 12 && (i % 3) == 0 && i != 0)
					{
						toreturn += " ";
					}

					if (( (external >> i) & 1) == 1)
					{
						toreturn += "1";
					}
					else
					{
						toreturn += "0";
					}
				}

				return (toreturn + " ");
			}

			/// <summary>
			/// Only basic information. Id, byte and bounds.
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return "Voxel: " + id() + ", Byte: " + Byte +  ", Bounds: " + bounds;
			}

			#region GIZMOS TOCHANGE TO MJOLLNIROBJECT AS RECURSIVE CALL


			public void ToGizmosCube( Color _color ){
				Gizmos.color = _color;
				Gizmos.DrawCube( bounds.center, bounds.size );
			}

			public void ToGizmosWireCube( Color _color ){
				Gizmos.color = _color;
				Gizmos.DrawWireCube( bounds.center, bounds.size * 0.99f);
			}

			public void DrawNodes( int? _depth = null, bool _byte = false ){
				if( _depth != null && _depth >= 0 ){
					if( Byte != 0 && Byte != 255 && Byte != null && _byte ){
						Gizmos.color = new Color(0,1,1,1);
						Gizmos.DrawWireCube( bounds.center, bounds.size * 0.95f ) ;
					}else{
						Gizmos.color = new Color(0,0,1,1);
						Gizmos.DrawWireCube( bounds.center, bounds.size);
					}

					if ( children != null ) {
						for ( int i = 0; i < 8; i++ ) {
								children[i].DrawNodes( _depth -1 );
						}
					}
				}
			}

			public void DrawChunks(){
				if( collapsed || rootObject.CHUNK_MIN_SIZE == size ){
					Gizmos.color = new Color(0,1,1,1);
					Gizmos.DrawWireCube( bounds.center, bounds.size * 0.95f ) ;
				}else{
					Gizmos.color = new Color(0,0,1,1);
					Gizmos.DrawWireCube( bounds.center, bounds.size);
				}

				if ( children != null ) {
					for (int i = 0; i < 8; i++) {
						children[i].DrawChunks();
					}
				}

			}



			public void DrawPhysicalVoxelNodes(GameObject _parent){
				if( collapsed && Byte != 0 && Byte != 255){
					GameObject voxelObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					voxelObject.name = this.ToString();
					if( vertex.Count == 1){
						voxelObject.transform.position = vertex[0].qef.Solve(1e-6f, 4, 1e-6f);
					}else{
						voxelObject.transform.position = bounds.center;
					}

					if(_parent != null){voxelObject.transform.parent = _parent.transform;}
					//voxelObject.GetComponent< MeshRenderer >().sharedMaterial = new Material(Shader.Find("Diffuse"));
				}else{
					if (children != null) {
						for (int i = 0; i < 8; i++) {
							children[i].DrawPhysicalVoxelNodes(_parent);
						}
					}
				}
			}

			public void DrawVertexNodes(){

				if(this.vertex.Count > 0){
					Gizmos.color = new Color(0,1,1,.5f);
					Gizmos.DrawCube(bounds.center, new Vector3(.5f,.5f,.5f));
				}

				if (children != null) {
					for (int i = 0; i < 8; i++) {
						children[i].DrawVertexNodes();
					}
				}
			}

            #endregion
            #region Debug

			/// <summary>
			/// Creates unique id for every octree node. Based of child index.
			/// </summary>
			/// <returns></returns>
            public string id(){
				if( parent != null ){
					return parent.id() + child_index.ToString();
				}else{
					return child_index.ToString();
				}
			}

			public void PrintAllExtern(){
				if( children != null){
					Debug.Log( id() + " " + ExternalToString() );

					for (int i = 0; i < 8; i++) {
						children[i].PrintAllExtern();
					}
				}
			}

			public string ExternalToString()
			{
				if( external > 0){
					string tmp = "{ ";

					for (int i = 0; i < 18; i++){
						tmp += (external >> i) & 1;
						if( i == 11) tmp += " ";

					}

					return tmp + " }";
				}else{
					return "{ 0 }";
				}
			}

            #endregion //Debug
        }


    }
}
