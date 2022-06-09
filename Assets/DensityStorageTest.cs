using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityStorageTest : MonoBehaviour
{

    public Octree<float> storage;

    public uint size = 4;

    public int depth = 2;


    // Start is called before the first frame update
    void Start()
    {
        storage = new Octree<float>( transform.position, size);
        storage.SubdivideAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        if(storage != null)
        {
            storage.GizmosDrawDepth(depth);
        }
    }
}
