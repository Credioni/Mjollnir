using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mjollnir;

namespace Mjollnir{
	namespace Isosurface{

		public class Chunk
		{



			public MjollnirObject 	rootObject;
			public Chunk 			root;
			public Chunk 			parent;						// Because everybody has parents and childrens
			public Chunk[] 			children;

			public Chunk(){

			}
		}
	}
}
