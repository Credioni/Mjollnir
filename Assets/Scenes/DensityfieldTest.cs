using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;
using Mjollnir.Isosurface;

public class DensityfieldTest : MonoBehaviour
{

    Densityfield densityfield;


    // Start is called before the first frame update
    void Start()
    {
        densityfield = new Densityfield();
        densityfield.AddDebug();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
