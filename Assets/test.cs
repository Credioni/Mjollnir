using UnityEngine;

using System.Collections;
using System.Runtime.InteropServices;
using System.IO;


public class test : MonoBehaviour
{
	[DllImport("test")]
	private static extern int print ();

	[DllImport("test")]
	private static extern void change();

	[DllImport("test")]
	private static extern void terminate();

    void Start(){
		UnityEngine.Debug.Log("Hello  print " + print ());
		change ();
		UnityEngine.Debug.Log("Hello  print " + print ());
		UnityEngine.Debug.Log("Hello  print " + print ());
		terminate();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
