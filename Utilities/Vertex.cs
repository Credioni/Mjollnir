using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QEFSolverMDC;

public class Vertex{
	public Vector3 vertice;
	public Vector3 normal;
	public Vertex parent;
	public Color color;

	public string id;

	public int index;
	public bool reindexed = false;

	//QEF
	public QEFSolver qef;
	public float error;

	//Manifold
	public bool collapsed;			//If the threshold error is higher than this vertex's (qef) error.
	public int[] collapsed_edges;

	public int euler;
	public bool euler_characteristic;

	public int in_cell;


	public int surface_index;
	public int material_index;

	public string info {get{return Information();}}

	public Vertex(){
		qef = null;
		surface_index = -1;
		collapsed = true;
		collapsed_edges = new int[12];
		parent = null;
	}

	public void DrawVertex(){
		GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Cube);
		vertex.name = this.ToString();
		vertex.transform.position = this.vertice;
		vertex.GetComponent< MeshRenderer >().sharedMaterial = new Material(Shader.Find("Diffuse"));
	}

	public string GetCollapsedEdges(int[] d){
		return (d[0] +""+ d[1] +""+ d[2] +""+ d[3] +" "+ d[4] +""+ d[5] +""+ d[6] +""+ d[7] +" "+ d[8] +""+ d[9] +""+ d[10] +""+ d[11]);
	}

	public Vector3 GetPosition()
	{
		return qef.Solve(1e-6f, 4, 1e-6f);
	}

	private string Information()
	{
		string a = GetCollapsedEdges(collapsed_edges);

		return "Vertex: " + index + ", Node: "+ id + ", collapsed_edges = " + a + ", Position = " + (qef.Solve(1e-6f, 4, 1e-6f)) + ", normal: " + normal + ", Error: " + error;
		//return base.ToString();
	}

	public override string ToString()
	{
		string a = GetCollapsedEdges(collapsed_edges);

		return "Vertex : " + index + ", collapsed_edges = " + a + ", Position = " + qef.Solve(1e-6f, 4, 1e-6f) + ", normal: " + normal + ", Error: " + error;
		//return base.ToString();
	}
}








//#
