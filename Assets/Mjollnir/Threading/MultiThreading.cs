using System;
using UnityEngine;

using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


using Mjollnir;
using Mjollnir.Isosurface;

namespace Mjollnir{

	public enum ThreadingJobs{
		ProcessObject,
		ProcessFace,
		ProcessEdge,

		ClusterCellBase,
	}

	public struct ThreadInfo<T>{
		public readonly Action<T> callback;
		public  		T 		  data;

		public ThreadInfo( Action<T> _callback, T _data ){
			callback  = _callback;
			data 	  = _data;
		}
	}

	public struct OperatorData{
		public Voxel[]	node;
		public float	threshold;
		public bool		neighbours;
		public int		dir;

		public MeshBuilder  meshbuilder;
		public Densityfield densityfield;

		public ThreadingJobs[] job;

		public OperatorData(  Voxel[] _node, MeshBuilder _meshbuilder, Densityfield _densityfield, float _treshold, ThreadingJobs[] _job, bool _neighbours = false, int _dir = 0 ){
			node 		 = _node;
			threshold 	 = _treshold;
			densityfield = _densityfield;
			job 		 = _job;
			meshbuilder  = _meshbuilder;
			neighbours   = _neighbours;
			dir			 = _dir;
		}
	}

	public partial class MultiThreading : MonoBehaviour{

		private Operator testing;

		public MultiThreading(){
			testing = new Operator();
		}

		public void Assign( ThreadInfo<OperatorData> _threadingData ){
			testing.Assign( _threadingData );
		}

		void Start(){
			//testing = new Operator();
		}

		void Update(){
			testing.Update();
		}

	}
}
