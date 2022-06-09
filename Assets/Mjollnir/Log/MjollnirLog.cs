using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO
- Timestamps
- Own log window

*/

namespace Mjollnir{
	public static class MjollnirLog{
	    private static List<string> logs;

		static MjollnirLog(){
			logs = new List<string>();
		}

		public static void Add( string _log ){
			logs.Add( _log );
			//UnityEngine.Debug.Log( _log );
		}

		public static List<string> GetLog(){
			return logs;
		}

		public static void PrintAll(){
			for( int i = 0; i < logs.Count; i++ )
				UnityEngine.Debug.Log( logs[i] );
		}
	}
}
