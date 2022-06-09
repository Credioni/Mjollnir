using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;
using Mjollnir.Isosurface;

//using System.Diagnostics;

	/* TODO
	 * 
	 * -Densityfield node for saving
	 * -Chunk loader etc.
	 * -> LOD
	 * -Materials
	 */

public class Benchmark : MonoBehaviour{

    // Start is called before the first frame update

	
	public 	bool 	MultiThreading = true;
	public 	bool 	Threshold_inf = false;
	//Octree Size
	public int 	SIZE = 32;

	//float.PositiveInfinity
	[Range (0, 50f)]
	public 	float	THRESHOLD 	= 0;
	private float   l_threshold = 0;

	[Range (0, 20)]
	public 	int 	DrawDepth = 1;

	public 	bool 	DrawOctree = false;
	public  bool	DrawLeafNodes = false;

	public	bool	DrawPhysicalNodes	= false;
	public	bool	DrawVertexNodes		= false;


	MjollnirObject 	mjollnirObject;
	Mesh 			mesh;
	Densityfield 	densityfield;
	MeshBuilder		meshbuilder;

	double sw;


    void Start(){

		Vector3 a = new Vector3(0, 0, 0);
		Vector3 b = new Vector3(1, 0, 0);

		Vector3 p = new Vector3(0.5f, 0, 0);

		float d1 = -3f;
		float d2 = 1f;

		float df = d2 - d1;

		float d = (a - b).magnitude;
		float dx = (a - p).magnitude;

		float x1 = dx/d * df + d1;


		//UnityEngine.Debug.Log(Mathf.FloorToInt(2.0f) );
		//UnityEngine.Debug.Log(x1);

		/**/
		l_threshold = THRESHOLD;

		double whole = Time.realtimeSinceStartup;

		densityfield = new Densityfield();

		mjollnirObject = new MjollnirObject( SIZE, gameObject.transform.position, GetComponent<MultiThreading>() );
		mjollnirObject.AssignGameObject(transform.gameObject);

		if( MultiThreading ){
			mjollnirObject.Construct_MultiThread( ref densityfield );
		}else{
			mjollnirObject.Construct( ref densityfield, THRESHOLD, true );
			Mesh mesh = new Mesh();
			mjollnirObject.GenerateMesh( ref mesh, THRESHOLD);
			GetComponent<MeshFilter>().mesh = mesh;
			UpdateMesh();
		}

		if (DrawPhysicalNodes)
		{
			mjollnirObject.DrawPhysicalNodes();
		}

		if (DrawVertexNodes)
		{
			mjollnirObject.DrawVertexNodes();
		}

		UnityEngine.Debug.Log("Whole time in: " + (Time.realtimeSinceStartup - whole));

		//mjollnirObject.voxeltree.PrintAllExtern();
		
    }


	void UpdateMesh(){
		if(THRESHOLD != l_threshold){
			/*
			Mesh mesh = new Mesh();
			mjollnirObject.Generate( ref mesh, THRESHOLD);
			GetComponent<MeshFilter>().mesh = mesh;

			l_threshold = (Threshold_inf ? float.PositiveInfinity : THRESHOLD);
			*/
		}
	}

    // Update is called once per frame
    void Update(){
		if(THRESHOLD != l_threshold){
			UpdateMesh();
			l_threshold = THRESHOLD;
		}
    }

	void OnDrawGizmos(){
		if( mjollnirObject != null ){
			if (DrawOctree)
			{
				mjollnirObject.Draw(DrawDepth);
			}

			if (DrawLeafNodes)
			{
				mjollnirObject.DrawLeafNodes();
			}

			//mjollnirObject.DrawExternal();
			//mjollnirObject.voxeltree.DrawExternalNodes();

		}
	}

}




















//
