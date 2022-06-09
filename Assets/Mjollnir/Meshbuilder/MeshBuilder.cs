
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;

using Mjollnir;
using Mjollnir.Isosurface;


namespace Mjollnir{
	public class MeshBuilder {

		public string id;

		public Dictionary<string, Vertex> vertex;

		public List<Vector3> vertices;
		public List<Vector3> normals;
		public List<string> triangles;

		public MeshBuilder( string _id = null ){
			if( _id != null ) id = _id;

			vertex 		= new  Dictionary<string, Vertex>();
			triangles 	= new  List<string>();
			vertices 	= new  List<Vector3>();
			normals 	= new  List<Vector3>();
		}

		public void GenerateMesh( ref Mesh _mesh, float _threshold ){
			vertices.Clear();
			normals.Clear();

			Dictionary<string, int> triangle_ID2Index = new Dictionary<string, int>();

			foreach( var item in vertex ){
				Vertex vx = item.Value;
				triangle_ID2Index[ item.Key ] = vertices.Count;

				while( vx.parent != null && vx.parent.error < _threshold )
					vx = vx.parent;

				vertices.Add( vx.qef.Solve(1e-6f, 4, 1e-6f) );
				normals.Add( vx.normal );

			}

			List<int> triangles_return = new List<int>();

			for( int i = 0; i < triangles.Count; i++){
				triangles_return.Add( triangle_ID2Index[ triangles[i] ] );
			}

			_mesh.vertices 	= vertices.ToArray();
			_mesh.normals	= normals.ToArray();
			_mesh.triangles = triangles_return.ToArray();
		}

		public void GenerateMeshTesting( ref Mesh _mesh, float _threshold ){
			/*
			vertices.Clear();
			normals.Clear();
			triangles.Clear();

			for(int i = 0; i < vertex.Count; i++){
				Vertex vx = vertex[i];

				vertices.Add( vx.qef.Solve(1e-6f, 4, 1e-6f) );
				normals.Add( vx.normal );
				triangles.Add( triangles.Count );
			}

			_mesh.vertices 	= vertices.ToArray();
			_mesh.normals	= normals.ToArray();
			_mesh.triangles = triangles.ToArray();
			*/
		}

		//OLD CALL
		public void GenerateVertexBuffer( ref Voxel _tree ){
			if ( _tree.collapsed ){
				for (int i = 0; i < 8; i++){
					GenerateVertexBuffer( ref _tree.children[i] );
				}
			}

			if ( _tree.vertex == null || _tree.vertex.Count == 0)
				return;

			for (int i = 0; i < _tree.vertex.Count; i++){
				_tree.vertex[i].index = vertices.Count;
				vertices.Add( _tree.vertex[i].vertice );
			}
		}

		public override string ToString()
		{
			return ( "ID: " + id + ", Vertex: " + vertex.Count + ", Normals: " + normals.Count + ", Triangles: " + triangles.Count + " | " + triangles.Count%3 );
		}
	}
}
