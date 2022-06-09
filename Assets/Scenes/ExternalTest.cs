using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;
using Mjollnir.Isosurface;

//using System.Diagnostics;

public class ExternalTest : MonoBehaviour{

    // Start is called before the first frame update

	//Octree Size
	public 	bool 	MultiThreading = true;
	public 	bool 	Threshold_inf = false;
	public 	int 	SIZE = 32;

	//float.PositiveInfinity
	[Range (0, 50f)]
	public 	float	THRESHOLD 	= 0;
	private float   l_threshold = 0;

	[Range (0, 20)]
	public 	int 	DrawDepth = 1;

	public 	bool 	DrawOctree = false;


	MjollnirObject 	mjollnirObject;
	Mesh 			mesh;
	Densityfield 	densityfield;
	MeshBuilder		meshbuilder;

	double sw;


    void Start(){
		l_threshold = THRESHOLD;

		double whole = Time.realtimeSinceStartup;

		densityfield 	= new Densityfield();

		mjollnirObject = new MjollnirObject( SIZE, gameObject.transform.position, GetComponent<MultiThreading>() );

		if( MultiThreading ){
			mjollnirObject.Construct_MultiThread( ref densityfield );
		}else{
			mjollnirObject.Construct( ref densityfield, THRESHOLD, true );
			Mesh mesh = new Mesh();
			mjollnirObject.GenerateMesh( ref mesh, THRESHOLD);
			GetComponent<MeshFilter>().mesh = mesh;
			UpdateMesh();
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
		if( mjollnirObject != null && DrawOctree ){
			mjollnirObject.Draw(DrawDepth);
			mjollnirObject.DrawExternal();
			//mjollnirObject.voxeltree.DrawExternalNodes();

		}
	}

}
