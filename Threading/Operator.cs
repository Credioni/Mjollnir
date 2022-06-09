/*
	Delegator
*/

using System;
using UnityEngine;

using System.Threading;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;


using Mjollnir;
using Mjollnir.Isosurface;

using static Mjollnir.Isosurface.VoxelUtilities;

namespace Mjollnir{
	partial class MultiThreading{
		internal partial class Operator{

		 // partial private int MAX_THREADS;
			private Thread[] operators;

			//Queue
			private Queue<ThreadInfo<OperatorData>> assign;

			//Finished Calculations
			private Queue<ThreadInfo<OperatorData>> operatedData;

			//Statical
			private int assignedTasks = 0;
			private int finishedTasks = 0;

			public Operator(){
				if( MAX_THREADS == 0 ){ MAX_THREADS = Environment.ProcessorCount; }

				operators 	 = new Thread[ MAX_THREADS ];

				assign 		 = new Queue<ThreadInfo<OperatorData>> ();
				operatedData = new Queue<ThreadInfo<OperatorData>> ();
			}

			public void Assign( ThreadInfo<OperatorData> _threadingData ){
				lock( assign ){
					assign.Enqueue( _threadingData );
				}

				__wakeThreads();
				//__operatorThread();
			}

			public void Update(){
				if( operatedData.Count > 0){
					for (int i = 0; i < operatedData.Count; i++) {
						ThreadInfo<OperatorData> threadInfo = operatedData.Dequeue();
						threadInfo.callback( threadInfo.data );
					}
				}
			}

			private void __wakeThreads(){
				if( assign.Count() > 0 )
				{
					for( int i = 0; i < MAX_THREADS; i++)
					{
						if( operators[0] == null || operators[0].ThreadState != System.Threading.ThreadState.Running  )
						{
							operators[0] = new Thread( new ThreadStart(__operatorThread) );
							operators[0].Name = "MjollnirThread" + i.ToString();
							operators[0].Start();
						}
					}
				}
			}

			private void __operatorThread(){

				while( true )
				{
					ThreadInfo<OperatorData> threadInfo;
					OperatorData 			data;

					if( assign.Count() > 0 ){
						lock( assign )
						{
							threadInfo = assign.Dequeue();
							data = threadInfo.data;
						}
					}else{
						return;
					}

					Stopwatch watch = Stopwatch.StartNew();

					MeshBuilder meshbuilder = data.meshbuilder;


					for(int i = 0; i < data.job.Length; i++)
					{
						if( data.job[i] == ThreadingJobs.ProcessObject )
						{
							Manifold.ProcessObject( ref data.node[0], ref meshbuilder, data.threshold );
						}
						else if(data.job[i] == ThreadingJobs.ProcessFace)
						{
							Manifold.ProcessFace( data.node, data.dir, ref meshbuilder, data.threshold );
						}
						else if (data.job[i] == ThreadingJobs.ProcessEdge)
						{
							Manifold.ProcessEdge( data.node, data.dir, ref meshbuilder, data.threshold);
						}
						else if( data.job[i] == ThreadingJobs.ClusterCellBase )
						{
							Manifold.ClusterCellBase( ref data.node[0], ref meshbuilder, data.threshold);
						}
					}

					watch.Stop();
					/*
					if( watch.ElapsedMilliseconds > 0){
						UnityEngine.Debug.Log( "ID: " + meshbuilder.id + " in " + (watch.ElapsedMilliseconds) + " ms " + (100/(watch.ElapsedMilliseconds)) + " fps");
					}else{
						UnityEngine.Debug.Log( "ID: " + meshbuilder.id + " in " + (watch.ElapsedMilliseconds) + " ms " + ">>100 fps");
					}
					*/

					lock( operatedData ){
						OperatorData tmp_od = new OperatorData( data.node, meshbuilder, data.densityfield, data.threshold, data.job, data.neighbours );
						operatedData.Enqueue( new ThreadInfo<OperatorData>( threadInfo.callback, tmp_od ) );
					}
				}
			}

		}
	}

}
