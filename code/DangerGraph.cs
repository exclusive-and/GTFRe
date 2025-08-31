using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFR
{
    internal class DangerGraph : Object
    {
        private static readonly float[] factors =
            { 0, 2, 3, 5, 8, 13, 21 }; //, 34, 55, 89, 144 };

        public readonly float[] graph;

        public DangerGraph (Random random, int numZones)
        {
            graph = new float[numZones];

            float total = 0;

            for (int x = 0; x < numZones; x++)
            {
                var factor = factors[random.Next (factors.Length)];

                total += factor;
                graph[x] = factor;
            }

            for (int x = 0; x < numZones; x++)
            {
                graph[x] /= factors[factors.Length - 1];
            }
        }

        public override string ToString () => String.Join (" ", graph);

        // Experimental
        private static readonly float[][] presets =
        {
            new float[] { 2, 5, 11, 3, 8, 11 },
            new float[] { 5, 5, 5, 5, 5, 5 },
        };
    }
}
