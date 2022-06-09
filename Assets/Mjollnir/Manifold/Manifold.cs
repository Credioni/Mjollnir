using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;
using Mjollnir.Isosurface;

using QEFSolverMDC;

using static Mjollnir.Manifold_tables;
using static Mjollnir.Isosurface.VoxelUtilities;

namespace Mjollnir{
	public partial class Manifold {

        #region PROCESS
        public static void ProcessObject(ref Voxel _node, ref MeshBuilder _meshbuilder, float _threshold, bool _generate_chunk_edges = false){
			if( _node == null ) return;

			ProcessCell(ref _node, ref _meshbuilder, _threshold, _generate_chunk_edges);
		}

		public static void ProcessCell( ref Voxel _node, ref MeshBuilder _meshbuilder, float _threshold, bool _generate_chunk_edges = false){
			if ( _node != null && !_node.collapsed && _node.children != null )
			{
				for (int i = 0; i < 8; i++){
					ProcessCell(ref _node.children[i], ref _meshbuilder, _threshold, _generate_chunk_edges);
				}

				for (int i = 0; i < 12; i++){
					ProcessFace( new Voxel[]{
								_node.children[CELL2FACE[i, 0]],
								_node.children[CELL2FACE[i, 1]]
							 }, CELL2FACE[i, 2], ref _meshbuilder, _threshold, _generate_chunk_edges
					);
				}

				for (int i = 0; i < 6; i++){
					ProcessEdge(new Voxel[]{
						_node.children[CELL2EDGE[i, 0]],
						_node.children[CELL2EDGE[i, 1]],
						_node.children[CELL2EDGE[i, 2]],
						_node.children[CELL2EDGE[i, 3]]
						}, CELL2EDGE[i, 4], ref _meshbuilder, _threshold, _generate_chunk_edges
					);
				}
			}
		}

		public static void ProcessFace(Voxel[] _node, int _dir, ref MeshBuilder _meshbuilder, float _threshold, bool _generate_chunk_edges = false){
			if( _node[0] == null || _node[1] == null ){ return; }

			if (!_node[0].collapsed || !_node[1].collapsed ){
				for (int i = 0; i < 4; i++){

					ProcessFace(new Voxel[]{
							_node[0].collapsed ? _node[0] : _node[0].children[FACE2FACE[_dir, i, 0]],
							_node[1].collapsed ? _node[1] : _node[1].children[FACE2FACE[_dir, i, 1]],
						}, FACE2FACE[_dir, i, 2], ref _meshbuilder, _threshold, _generate_chunk_edges
					);

				}

				int[,] orders =
				{
					{ 0, 0, 1, 1 },
					{ 0, 1, 0, 1 },
				};

				for (int i = 0; i < 4; i++)
				{
					Voxel[] edge__node = new Voxel[4];

					for (int j = 0; j < 4; j++)
					{
						if (_node[orders[FACE2EDGE[_dir, i, 0], j]].collapsed)
							edge__node[j] = _node[orders[FACE2EDGE[_dir, i, 0], j]];
						else
							edge__node[j] = _node[orders[FACE2EDGE[_dir, i, 0], j]].children[FACE2EDGE[_dir, i, 1 + j]];
					}
					ProcessEdge(edge__node, FACE2EDGE[_dir, i, 5], ref _meshbuilder, _threshold, _generate_chunk_edges);
				}
			}
		}

		public static void ProcessEdge(Voxel[] _node, int _dir, ref MeshBuilder _meshbuilder, float _threshold, bool _generate_chunk_edges = false){
			if(_node[0] == null || _node[1] == null || _node[2] == null || _node[3] == null){ return; }

			if (_node[0].collapsed && _node[1].collapsed && _node[2].collapsed && _node[3].collapsed)
			{
				ProcessVoxelVertices(_node, _dir, ref _meshbuilder, _threshold);
			}
			else
			{
				for (int i = 0; i < 2; i++)
				{
					ProcessEdge(new Voxel[]{
						_node[0].collapsed ? _node[0] : _node[0].children[EDGE2EDGE[_dir, i, 0]],
						_node[1].collapsed ? _node[1] : _node[1].children[EDGE2EDGE[_dir, i, 1]],
						_node[2].collapsed ? _node[2] : _node[2].children[EDGE2EDGE[_dir, i, 2]],
						_node[3].collapsed ? _node[3] : _node[3].children[EDGE2EDGE[_dir, i, 3]]
						}, EDGE2EDGE[_dir, i, 4], ref _meshbuilder, _threshold, _generate_chunk_edges
					);
				}
			}
		}

		public static void ProcessVoxelVertices(Voxel[] _node, int _dir, ref MeshBuilder _meshbuilder, float _threshold)
		{

			Vertex[] vertex = new Vertex[4];
			int min_size	= 10000000;
			int[] indices	= { -1, -1, -1, -1 };

			bool flip			= false;
			bool sign_changed	= false;

			for (int i = 0; i < 4; i++)
			{
				//Getting the current edge and its corners
				int edge = EDGE_MASK[_dir, i];

				int c1	= CELL2FACE[edge, 0];
				int c2	= CELL2FACE[edge, 1];

				//Decides which side the triangle is from the corners
				int m1 = (_node[i].Byte >> c1) & 1;
				int m2 = (_node[i].Byte >> c2) & 1;

				if (_node[i].size < min_size  )
				{
					min_size = _node[i].size;
					flip	 = m1 == 0;
					sign_changed = ((m1 == 0 && m2 != 0) || (m1 != 0 && m2 == 0));
				}

				int index = 0;

				//Finding which vertex inside the node is assigned to the edge
				foreach (Vertex v in _node[i].vertex)
				{
					if (v.collapsed_edges[edge] > 0)
					{
						vertex[i] = v;
						break;
					}
					index++;
				}

				if (vertex[i] == null) return;

				string tmp_id = _node[i].id() + "-" + vertex[i].index;

				//TODO PERFORMANCE CHECK THIS
				if ( !_meshbuilder.vertex.ContainsKey(tmp_id) )
					_meshbuilder.vertex[tmp_id] = vertex[i];

			}

			if (sign_changed)
			{
				if (flip)
				{
					_meshbuilder.triangles.Add(_node[0].id() + "-" + vertex[0].index);
					_meshbuilder.triangles.Add(_node[1].id() + "-" + vertex[1].index);
					_meshbuilder.triangles.Add(_node[3].id() + "-" + vertex[3].index);

					_meshbuilder.triangles.Add(_node[0].id() + "-" + vertex[0].index);
					_meshbuilder.triangles.Add(_node[3].id() + "-" + vertex[3].index);
					_meshbuilder.triangles.Add(_node[2].id() + "-" + vertex[2].index);
				}
				else
				{
					_meshbuilder.triangles.Add(_node[0].id() + "-" + vertex[0].index);
					_meshbuilder.triangles.Add(_node[3].id() + "-" + vertex[3].index);
					_meshbuilder.triangles.Add(_node[1].id() + "-" + vertex[1].index);

					_meshbuilder.triangles.Add(_node[0].id() + "-" + vertex[0].index);
					_meshbuilder.triangles.Add(_node[2].id() + "-" + vertex[2].index);
					_meshbuilder.triangles.Add(_node[3].id() + "-" + vertex[3].index);
				}
			}
		}

        /*Old
		public static void ProcessVoxelVertices(Voxel[] _node, int _dir, ref MeshBuilder _meshbuilder, float _threshold)
		{
			Vertex[] vertex = new Vertex[4];
			int min_size 	= 10000000;
			int[] indices 	= { -1, -1, -1, -1 };

			bool flip = false;
			bool sign_changed = false;

			for (int i = 0; i < 4; i++)
			{
				int edge = EDGE_MASK[_dir, i];
				int c1 = CELL2FACE[edge, 0];
				int c2 = CELL2FACE[edge, 1];

				int m1 = (_node[i].Byte >> c1) & 1;
				int m2 = (_node[i].Byte >> c2) & 1;

				if (_node[i].size < min_size)
				{
					min_size = _node[i].size;
					flip = m1 == 0;
					sign_changed = ((m1 == 0 && m2 != 0) || (m1 != 0 && m2 == 0));
				}

				int index = 0;

				foreach (Vertex v in _node[i].vertex) {
					if( v.collapsed_edges[edge] > 0 ){
						vertex[i] = v;
						break;
					}
					index++;
				}

				if (vertex[i] == null) return;
				
			}

			if (sign_changed){

				//
				for( int i = 0; i < 4; i++){
					if( !_meshbuilder.vertex.ContainsKey( _node[i].id() ) )
						_meshbuilder.vertex[_node[i].id()] = vertex[i];
				}

				if (!flip){
					_meshbuilder.triangles.Add( _node[0].id() );
					_meshbuilder.triangles.Add( _node[1].id() );
					_meshbuilder.triangles.Add( _node[3].id() );

					_meshbuilder.triangles.Add( _node[0].id() );
					_meshbuilder.triangles.Add( _node[3].id() );
					_meshbuilder.triangles.Add( _node[2].id() );
				}else{
					_meshbuilder.triangles.Add( _node[0].id() );
					_meshbuilder.triangles.Add( _node[3].id() );
					_meshbuilder.triangles.Add( _node[1].id() );

					_meshbuilder.triangles.Add( _node[0].id() );
					_meshbuilder.triangles.Add( _node[2].id() );
					_meshbuilder.triangles.Add( _node[3].id() );
				}
			}
		}
		*/

        #endregion
        #region CLUSTER

        public static void ClusterCellBase(ref Voxel _node, ref MeshBuilder _meshbuilder, float _threshold)
		{
			if (_node.children == null)
				return;

			for (int i = 0; i < 8; i++)
			{
				ClusterCell(ref _node.children[i], ref _meshbuilder, _threshold);
			}
		}

		public static void ClusterCell(ref Voxel _node, ref MeshBuilder _meshbuilder, float _threshold){
			 if (_node.children == null)
				 return;

			 for (int i = 0; i < 8; i++)
			 {
				 ClusterCell(ref _node.children[i], ref _meshbuilder, _threshold);
			 }


			 int surface_index = 0;
			 //List<Vertex> surface_indeces = new List<Vertex>();
			 List<Vertex> collected_vertices = new List<Vertex>();
			 List<Vertex> new_vertices = new List<Vertex>();

			 for (int i = 0; i < 12; i++){
				 ClusterFace(new Voxel[]{
					 	_node.children[CELL2FACE[i, 0]],
						_node.children[CELL2FACE[i, 1]]
				 }, CELL2FACE[i, 2], ref surface_index, collected_vertices);
			 }


			 for (int i = 0; i < 6; i++){
				 ClusterEdge( new Voxel[] {
					  _node.children[CELL2EDGE[i, 0]],
					  _node.children[CELL2EDGE[i, 1]],
					  _node.children[CELL2EDGE[i, 2]],
					  _node.children[CELL2EDGE[i, 3]]
				  }, CELL2EDGE[i, 4], ref surface_index, collected_vertices);
			 }

			 int highest_index = surface_index == -1 ? 0 : surface_index;

			 foreach (Voxel n in _node.children){

				 foreach (Vertex v in n.vertex)
				 {
					 if (v == null)
						 continue;
					 if (v.surface_index == -1)
					 {
						 v.surface_index = highest_index++;
						 collected_vertices.Add(v);
					 }
				 }
			 }

			 if (collected_vertices.Count > 0){

				 for (int i = 0; i <= highest_index; i++)
				 {
					 QEFSolver qef = new QEFSolver();
					 Vector3 normal = new Vector3(0,0,0);
					 //Color color = collected_vertices.Count > 0 ? collected_vertices[0].color : new Color(0,0,0,0);
					 int count = 0;
					 int[] edges = new int[12];
					 int euler = 0;
					 int e = 0;
					 foreach (Vertex v in collected_vertices){
						 if (v.surface_index == i){
							 /* Calculate ei(Sv) */
							 for (int k = 0; k < 3; k++)
							 {
								 int edge = ExternalEdges[v.in_cell, k];
								 edges[edge] += v.collapsed_edges[edge];
							 }
							 /* Calculate e(Svk) */
							 for (int k = 0; k < 9; k++)
							 {
								 int edge = TInternalEdges[v.in_cell, k];
								 e += v.collapsed_edges[edge];
							 }

							 qef.Add(ref v.qef.data);
							 normal += v.normal;
							// color = Color.Lerp(color, v.color, .5f);
							 euler += v.euler;
							 count++;
						 }
					 }

					 if (count == 0)
					 {
						 continue;
					 }

					 /*
					 https://en.wikipedia.org/wiki/Euler_characteristic
					 */

					 bool euler_characteristic = true;
					 for (int f = 0; f < 6 && euler_characteristic; f++)
					 {
						 int intersections = 0;

						 for (int ei = 0; ei < 4; ei++){
							 intersections += edges[FACES[f, ei]];
						 }

						 if (!(intersections == 0 || intersections == 2))
							 euler_characteristic = false;
					 }

					 //If the new surface isnt manifold => return
					 if(!euler_characteristic){continue;}

					 Vertex new_vertex = new Vertex();
					 normal /= (float)count;
					 normal.Normalize();
					 new_vertex.normal = normal;
					 new_vertex.qef = qef;
					 new_vertex.collapsed_edges = edges;
					 new_vertex.euler = euler - e / 4;

					 new_vertex.in_cell 				= _node.child_index;
					 new_vertex.euler_characteristic 	= euler_characteristic;

					 new_vertices.Add(new_vertex);

					 qef.Solve(1e-6f, 4, 1e-6f);
					 float err = qef.GetError();
					 new_vertex.collapsed = err <= _threshold && euler_characteristic;
					 new_vertex.error = err;

					 foreach (Vertex v in collected_vertices)
					 {
						 if (v.surface_index == i){

							 if (v != new_vertex)
								 v.parent = new_vertex;
							 else
								 v.parent = null;
						 }
					 }
				 }
			 }
			 else
			 {
				 return;
			 }

			  _node.vertex = new_vertices;

			  /*
			 bool collapse_this = true;

			 foreach (Vertex v in _node.vertex) {
				 if(!v.collapsed){
					 collapse_this = false;
				 }
			 }

			 for (int i = 0; i < 8; i++) {
			 	if(!_node.children[i].collapsed){
					collapse_this = false;
				}
			 }

			 _node.collapsed = collapse_this;
			 */
		}

		public static void ClusterFace(Voxel[] _node, int direction, ref int surface_index, List<Vertex> collected_vertices)
		{

			 if (!_node[0].collapsed || !_node[1].collapsed)
			 {


				 for (int i = 0; i < 4; i++)
				 {
					ClusterFace(new Voxel[]{
							_node[0].collapsed ? _node[0] : _node[0].children[FACE2FACE[direction, i, 0]],
							_node[1].collapsed ? _node[1] : _node[1].children[FACE2FACE[direction, i, 1]],
						}, FACE2FACE[direction, i, 2], ref surface_index, collected_vertices
					);
				 }
			 }

			 int[,] orders =
				 {
					 { 0, 0, 1, 1 },
					 { 0, 1, 0, 1 },
				 };

			 for (int i = 0; i < 4; i++)
			 {
				 Voxel[] edge__node = new Voxel[4];

				 for (int j = 0; j < 4; j++)
				 {
					 if (_node[orders[FACE2EDGE[direction, i, 0], j]] == null)
						 continue;
					 if (_node[orders[FACE2EDGE[direction, i, 0], j]].collapsed)
						 edge__node[j] = _node[orders[FACE2EDGE[direction, i, 0], j]];
					 else
						 edge__node[j] = _node[orders[FACE2EDGE[direction, i, 0], j]].children[FACE2EDGE[direction, i, 1 + j]];
				 }

				 ClusterEdge(edge__node, FACE2EDGE[direction, i, 5], ref surface_index, collected_vertices);
			 }
		}

		public static void ClusterEdge(Voxel[] _node, int direction, ref int surface_index, List<Vertex> collected_vertices)
		{
			 if (_node[0].collapsed && _node[1].collapsed && _node[2].collapsed && _node[3].collapsed){
				 ClusterIndexes(_node, direction, ref surface_index, collected_vertices);
			 }
			 else
			 {
				 for (int i = 0; i < 2; i++)
				 {
					 Voxel[] edge__node = new Voxel[4];

					 for (int j = 0; j < 4; j++)
					 {
						 if (_node[j] == null)
							 continue;
						 if (_node[j].collapsed)
							 edge__node[j] = _node[j];
						 else
							 edge__node[j] = _node[j].children[EDGE2EDGE[direction, i, j]];
					 }

					 ClusterEdge(edge__node, EDGE2EDGE[direction, i, 4], ref surface_index, collected_vertices);
				 }
			 }
		}

		public static void ClusterIndexes(Voxel[] _node, int direction, ref int max_surface_index, List<Vertex> collected_vertices)
		{
			Vertex[] vertices = new Vertex[4];
			int v_count = 0;
			int node_count = 0;
			int tmp_edge;

			for (int i = 0; i < 4; i++)
			{
				if (_node[i] == null)
				continue;

				node_count++;


				int edge = EDGE_MASK[direction, i];
				int c1 = CELL2FACE[edge, 0];
				int c2 = CELL2FACE[edge, 1];

				int m1 = (_node[i].Byte >> c1) & 1;
				int m2 = (_node[i].Byte >> c2) & 1;


				//find the vertex index

				int index = 0;
				bool skip = false;

				for (int j = 0; j < 4; j++){
					if(manifold_edge_connection_list[_node[i].Byte, j, 0] == -1){
						if (!((m1 == 0 && m2 != 0) || (m1 != 0 && m2 == 0))){
							skip = true;
							break;
						}
					}

					for (int k = 0; k < 7; k++){
						tmp_edge = manifold_edge_connection_list[_node[i].Byte, j, k];
						if(tmp_edge == edge){
							j = 5;
							break;
						}
					}
				}

				if (!skip && index < _node[i].vertex.Count)
				{
					vertices[i] = _node[i].vertex[index];
					while (vertices[i].parent != null)
						vertices[i] = vertices[i].parent;

					v_count++;
				}
			}

			if (v_count == 0) return;

			int surface_index = -1;

			for (int i = 0; i < 4; i++){
				if (vertices[i] == null) continue;

			 	Vertex v = vertices[i];

				if (v.surface_index != -1)
				{
					if (surface_index != -1 && surface_index != v.surface_index)
					{
						AssignSurface(collected_vertices, v.surface_index, surface_index);
					}else if (surface_index == -1){
						surface_index = v.surface_index;
					}
				}
			}

			if (surface_index == -1) surface_index = max_surface_index++;

			for (int i = 0; i < 4; i++)
			{
				Vertex v = vertices[i];
				if (v == null) continue;

				if (v.surface_index == -1)
				{
					collected_vertices.Add(v);
				}

				v.surface_index = surface_index;

			}
		}

		private static void AssignSurface(List<Vertex> vertices, int from, int to)
		{
			 foreach (Vertex v in vertices)
			 {
				 if (v != null && v.surface_index == from)
					 v.surface_index = to;
			 }
		}

		#endregion //CLUSTER
	}
}































//#
