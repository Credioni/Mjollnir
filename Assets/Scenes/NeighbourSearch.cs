using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;
using Mjollnir.Isosurface;

//using System.Diagnostics;

public class NeighbourSearch : MonoBehaviour{

    // Start is called before the first frame update

	public GameObject findThis;
	//Octree Size
	public 	bool 	MultiThreading = true;
	public 	int 	SIZE = 32;


	[Range (0, 20)]
	public 	int 	DrawDepth = 1;
	public 	bool 	DrawOctree = true;
	public  bool	print_founded = true;


	MjollnirObject 	mjollnirObject;
	Mesh 			mesh;
	Densityfield 	densityfield;
	MeshBuilder		meshbuilder;

	
	double sw;
	Voxel finded;
	Voxel l_finded;
	List<Voxel> foundedNeighbours;

	double deltatime;

    void Start(){
		double whole = Time.realtimeSinceStartup;
		deltatime = Time.realtimeSinceStartup;

		densityfield 	= new Densityfield();

		mjollnirObject 	= new MjollnirObject( SIZE, gameObject.transform.position );
		mjollnirObject.SimulateConstruct( densityfield );

		//mjollnirObject.voxeltree.PrintAllExtern();
		finded 		= mjollnirObject.Find( findThis.transform.position, 2);
		l_finded 	= finded;
		foundedNeighbours = mjollnirObject.FindNeighbour( finded );

		UnityEngine.Debug.Log("Whole time in: " + (Time.realtimeSinceStartup - whole));
    }


    // Update is called once per frame
    void Update(){

		if( Time.realtimeSinceStartup - deltatime > 0.25f ){

			finded 				= mjollnirObject.Find( findThis.transform.position, 2);

			if( finded != null && l_finded != finded ){
				foundedNeighbours 	= mjollnirObject.FindNeighbour( finded );
			}

			if( print_founded && finded != null && l_finded != finded)
			{
				UnityEngine.Debug.Log("Founded Neighbours");
				for ( int i = 0; i < foundedNeighbours.Count; i++)
				{
					UnityEngine.Debug.Log( i + "-" + foundedNeighbours[i]);
				}
			}

			l_finded = finded;
			deltatime = Time.realtimeSinceStartup;
		}
    }

	void OnDrawGizmos(){
		if( mjollnirObject != null && DrawOctree ){
			mjollnirObject.Draw(DrawDepth);
		}

		if( finded != null ){
			finded.ToGizmosCube( Color.red );
		}

		if( foundedNeighbours != null ){
			foreach (Voxel v in foundedNeighbours) {
				if( v != null)
					v.ToGizmosWireCube( Color.green);
			}
		}
	}

}
