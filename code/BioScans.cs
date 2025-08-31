using BepInEx.Logging;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFR
{
    // A Bio Scan puzzle.
    //
    internal struct BioScan
    {
        // The in-game persistent ID of this scan puzzle.
        public uint id;
        // The number of scan stages in the puzzle.
        public int numScans;
        // The approximate difficulty level of the puzzle (as rated by us).
        public float difficulty;
        
        public BioScan (uint id, int numScans, float diff)
        {
            this.id = id;
            this.numScans = numScans;
            this.difficulty = diff;
        }

        // Copy the in-game puzzle data to one of our Bio Scans.
        //
        public BioScan (ChainedPuzzleDataBlock puzzle)
        {
            this.id = puzzle.persistentID;
            this.numScans = puzzle.ChainedPuzzle.Count;
            this.difficulty = PuzzleDifficulty (puzzle);
        }

        // Approximate how much a scan stage of a given type contributes to
        // the overall difficulty of the puzzle.
        //
        public static float ScanDifficulty (uint scanType)
            => scanType switch
            {
                3                   => -0.50f,
                5 or 8 or 14        =>  0.25f,
                6 or 20             =>  0.50f,
                (> 24) and (< 30)   =>  0.75f,
                13 or 16 or 32      =>  1.00f,
                15                  =>  1.50f,
                (> 32) and (< 38)   =>  2.00f,
                17 or 18            =>  2.00f,
                40 or 44            =>  2.00f,
                _                   =>  0.00f,
            };

        // Approximate the total difficulty of the puzzle.
        //
        public static float PuzzleDifficulty (ChainedPuzzleDataBlock puzzle)
        {
            float scandiff = 0;
            foreach (var scan in puzzle.ChainedPuzzle)
            {
                scandiff += ScanDifficulty (scan.PuzzleType);
            }
            return scandiff;
        }

        // Check whether a puzzle contains any invalid scans.
        //
        public static bool IsInvalid (ChainedPuzzleDataBlock puzzle)
        {
            foreach (var scan in puzzle.ChainedPuzzle)
            {
                if (scan.PuzzleType is 10 or 11 or 12 or 21 or 22 or 24 or 31 or 38)
                    return true;
            }
            return false;
        }
    }

    // The curated set of all Bio Scan puzzles.
    //
    // Categorizes scans by count. Exports a function to randomly generate
    // a scan in the zone.
    //
    // TODO: Add configurable biases to force scans toward a certain
    //       difficulty level. Among other things, this would enable a
    //       full layout randomizer to generate fresh scans without depending
    //       on a vanilla default.
    //
    internal class SortedBioScans
    {
        private readonly List<BioScan>[] bioscans;
        private readonly Dictionary<uint, int> scanCounts;
        private readonly Dictionary<uint, BioScan> scansById;

        public SortedBioScans (ChainedPuzzleDataBlock[] scans)
        {
            bioscans = new List<BioScan>[15];
            for (int i = 0; i < bioscans.Length; i++)
            {
                bioscans[i] = new List<BioScan> ();
            }

            scanCounts = new Dictionary<uint, int> ();
            scansById = new Dictionary<uint, BioScan> ();

            // Add an empty Bio Scan.
            bioscans[0].Add (new BioScan (0, 1, 0));

            foreach (var scan in scans)
            {
                if (BioScan.IsInvalid (scan))
                    continue;

                var puzzle = new BioScan (scan);

                bioscans[puzzle.numScans].Add (puzzle);

                scanCounts[puzzle.id] = puzzle.numScans;
                scansById[puzzle.id] = puzzle;
            }
        }

        // Create a randomized Bio Scan in a zone.
        //
        // Has a chance of increasing or decreasing the vanilla scan count by
        // a factor of 2 in either direction.
        //
        public float CreateScanInZone
        (
            Random random,
            ExpeditionZoneData zone,
            ExpeditionZoneData original
        )
        {
            var puzzle = original.ChainedPuzzleToEnter;

            int numScans;
            float vanillaDifficulty;

            try
            {
                numScans = scanCounts[puzzle];
                vanillaDifficulty = scansById[puzzle].difficulty;
            }
            catch (Exception)
            {
                // If we fail to find the vanilla puzzle for any reason,
                // assume that it would've had 0 scans.
                numScans = 0;
                vanillaDifficulty = 0;
            }

            numScans = Math.Clamp (numScans + random.Next (5) - 2, 0, 14);

            while (bioscans[numScans].Count == 0) numScans--;
            
            int index = random.Next (bioscans[numScans].Count);
            var scan = bioscans[numScans][index];
            
            zone.ChainedPuzzleToEnter = scan.id;

            return scan.difficulty - vanillaDifficulty;
        }
    }
}
