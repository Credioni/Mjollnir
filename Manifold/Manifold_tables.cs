using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using QEFSolverMDC;

namespace Mjollnir{
	public static class Manifold_tables {

        // written by credion 8/2018

        #region Manifold_Process

        public static int[,] CELL2FACE =
        {
            { 0, 4, 0 }, { 1, 5, 0 }, { 2, 6, 0 }, { 3 ,7, 0 },	// x-axis
            { 0, 2, 1 }, { 1, 3, 1 }, { 4, 6, 1 }, { 5, 7, 1 },	// y-axis
            { 0, 1, 2 }, { 2, 3, 2 }, { 4, 5, 2 }, { 6, 7, 2 }	// z-axis
        };

        public static int[,] CELL2EDGE = {
            { 0, 1, 2, 3, 0 }, { 4, 5, 6, 7, 0 },
            { 0, 4, 1, 5, 1 }, { 2, 6, 3, 7, 1 },
            { 0, 2, 4, 6, 2 }, { 1, 3, 5, 7, 2 }
        };

        public static int[, ,] FACE2FACE = {
            { { 4, 0, 0}, { 5, 1, 0}, { 6, 2, 0}, { 7, 3, 0} },
            { { 2, 0, 1}, { 6, 4, 1}, { 3, 1, 1}, { 7, 5, 1} },
            { { 1, 0, 2}, { 3, 2, 2}, { 5, 4, 2}, { 7, 6, 2} }
        };

        public static int[, ,] FACE2EDGE = {
            { { 1, 4, 0, 5, 1, 1}, { 1, 6, 2, 7, 3, 1}, { 0, 4, 6, 0, 2, 2}, { 0, 5, 7, 1, 3, 2} },
            { { 0, 2, 3, 0, 1, 0}, { 0, 6, 7, 4, 5, 0}, { 1, 2, 0, 6, 4, 2}, { 1, 3, 1, 7, 5, 2} },
            { { 1, 1, 0, 3, 2, 0}, { 1, 5, 4, 7, 6, 0}, { 0, 1, 5, 0, 4, 1}, { 0, 3, 7, 2, 6, 1} }
        };

        public static int[, ,] EDGE2EDGE = {
            { { 3, 2, 1, 0, 0}, { 7, 6, 5, 4, 0} },
            { { 5, 1, 4, 0, 1}, { 7, 3, 6, 2, 1} },
            { { 6, 4, 2, 0, 2}, { 7, 5, 3, 1, 2} }
        };

		public static int[,] EDGE_MASK = {
			{ 3, 2, 1, 0 },
			{ 7, 5, 6, 4 },
			{ 11, 10, 9, 8 }
		};


        #endregion //Manifold_Process
        #region Euler_characteristic

        public static int[,] ExternalEdges = {
            { 0, 8, 4 },
            { 1, 8, 5 },
            { 2, 9, 4 },
            { 3, 9, 5 },

            { 0, 10, 6 },
            { 10, 1, 7 },
            { 2, 11, 6 },
            { 11, 3, 7 }
        };

        public static int[,] TInternalEdges = {
            { 1, 2, 3, 5, 6, 7, 9, 10, 11 },
            { 0, 2, 3, 4, 6, 7, 9, 10, 11 },
            { 0, 1, 3, 5, 6, 7, 8, 10, 11 },
            { 0, 1, 2, 4, 6, 7, 8, 10, 11 },
            { 1, 2, 3, 4, 5, 7, 8, 9, 11 },
            { 0, 2, 3, 4, 5, 6, 8, 9, 11 },
            { 0, 1, 3, 4, 5, 7, 8, 9, 10 },
            { 0, 1, 2, 4, 5, 6, 8, 9, 10 }
        };

        #endregion //Euler_characteristic

    }
}
