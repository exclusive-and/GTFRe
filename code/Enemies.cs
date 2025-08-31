using GameData;
using IL2 = Il2CppSystem.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFab.MultiplayerModels;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using BepInEx.Logging;

namespace GTFR
{
    internal class SortedEnemies
    {
        // Enemy group sampler constants.
        private const int numSamplesPerArea = 15;
        private const int numGroupsPerArea = 5;
        private const float maxAreaScore = 12.0f;

        struct Sample
        {
            public EnemyGroupDataBlock[] groups;
            public float danger;

            public Sample
            (
                Random random,
                int numGroups,
                List<EnemyGroupDataBlock> groups,
                float[,,] scoresByKind
            )
            {
                this.groups = new EnemyGroupDataBlock[numGroups];

                this.danger = 0;
                for (int i = 0; i < numGroups; i++)
                {
                    var group = groups[random.Next (groups.Count)];

                    foreach (var constituent in group.Roles)
                    {
                        var role = (int) constituent.Role;
                        var diff = (int) group.Difficulty;

                        this.danger += DistributionFactor (constituent.Distribution)
                                    * scoresByKind[(int) group.Type, role, diff];
                    }

                    this.groups[i] = group;
                }
            }
        }

        // Base game enemy type constants.
        private const int numGroupTypes = 7;
        private const int numRoleTypes = 11;
        private const int numDifficultyTypes = 8;

        // Groups to draw candidates from.
        private readonly List<EnemyGroupDataBlock> sleeperGroups;
        private readonly List<EnemyGroupDataBlock> hunterGroups;

        private readonly List<uint> canSpawnBigScary;
        private readonly List<uint> canSpawnScout;
        private readonly List<uint> canSpawnUltraBoss;

        // Scores sorted by groups and their constituents.
        private readonly float[,,] scoresByKind;

        public SortedEnemies
        (
            Il2CppArrayBase<EnemyDataBlock> enemyData,
            EnemyPopulationDataBlock populationData,
            Il2CppArrayBase<EnemyGroupDataBlock> groupData
        )
        {
            // Sort enemy populations by their role and difficulty.

            var sortedPopulations = new List<EnemyRoleData>[numRoleTypes, numDifficultyTypes];

            for (int role = 0; role < numRoleTypes; role++)
            {
                for (int diff = 0; diff < numDifficultyTypes; diff++)
                {
                    sortedPopulations[role, diff] = new List<EnemyRoleData> ();
                }
            }

            foreach (var population in populationData.RoleDatas)
            {
                if (population is null)
                    continue;

                var role = (int) population.Role;
                var diff = (int) population.Difficulty;
                sortedPopulations[role, diff].Add (population);
            }

            // Calculate scores for each group based on its type,
            // its difficulty, and the roles of its constituent populations.

            sleeperGroups = new List<EnemyGroupDataBlock> ();
            hunterGroups = new List<EnemyGroupDataBlock> ();
            scoresByKind = new float[numGroupTypes, numRoleTypes, numDifficultyTypes];

            canSpawnBigScary = new List<uint> ();
            canSpawnScout = new List<uint> ();
            canSpawnUltraBoss = new List<uint> ();

            foreach (var group in groupData)
            {
                // Don't add squid blocks since they're super broken
                if (group.persistentID is 41 or 69)
                    continue;

                int numPopulations = 0;

                // Calculate the scores of this group's constituent populations.
                foreach (var constituent in group.Roles)
                {
                    var role = (int) constituent.Role;
                    var diff = (int) group.Difficulty;

                    var populations = sortedPopulations[role, diff];

                    if (populations.Count == 0)
                        continue;

                    float total = 0;

                    foreach (var population in populations)
                    {
                        switch (population.Enemy)
                        {
                            case 20: // Scout
                            case 40: // Shadow scout
                            case 41: // Charger scout
                            case 54: // Scout zoomer (wtf is that??)
                            case 56: // Nightmare scout
                                canSpawnScout.Add (group.persistentID);
                                break;

                            case 36: // Mother
                            case 37: // P-mother
                                canSpawnBigScary.Add (group.persistentID);
                                break;

                            case 29: // Tank
                                canSpawnBigScary.Add (group.persistentID);
                                break;

                            case 47: // Pablo
                                canSpawnUltraBoss.Add (group.persistentID);
                                break;
                        }

                        var numEnemies = group.MaxScore / population.Cost;
                        var enemy = EnemyDataBlock.GetBlock (population.Enemy);
                        var balance = EnemyBalancingDataBlock.GetBlock (enemy.BalancingDataId);
                        var health = balance.Health.HealthMax / balance.Health.WeakspotDamageMulti;
                        var score = health * numEnemies * DistributionFactor (constituent.Distribution);

                        total += score;
                    }

                    scoresByKind[(int) group.Type, role, diff] = (float) Math.Sqrt (total / populations.Count ());
                    numPopulations++;
                }

                // Don't add groups that have no valid populations.
                if (numPopulations == 0)
                    continue;

                // Add the group to the appropriate list for its type.
                switch (group.Type)
                {
                    case eEnemyGroupType.Hunter:
                        hunterGroups.Add (group);
                        break;
                    case eEnemyGroupType.Patrol:
                    case eEnemyGroupType.Awake:
                        break;
                    default:
                        sleeperGroups.Add (group);
                        break;
                }
            }
        }

        private static float DistributionFactor (eEnemyRoleDistribution distrib)
            => distrib switch
            {
                eEnemyRoleDistribution.None         => 0f,
                eEnemyRoleDistribution.Force_One    => 1f,
                eEnemyRoleDistribution.Rel_05       => .05f,
                eEnemyRoleDistribution.Rel_10       => .1f,
                eEnemyRoleDistribution.Rel_15       => .15f,
                eEnemyRoleDistribution.Rel_25       => .25f,
                eEnemyRoleDistribution.Rel_50       => .5f,
                eEnemyRoleDistribution.Rel_75       => .75f,
                eEnemyRoleDistribution.Rel_100      => 1f,

                _ => throw new Exception ($"Got invalid relative distribution: {distrib}")
            };

        public float SpawnEnemiesInZone (ManualLogSource log, Random random, float factor, ExpeditionZoneData zone)
        {
            if (factor == 0)
            {
                zone.EnemySpawningInZone = null;
                return 0;
            }

            int coverage = (int) zone.CoverageMinMax.x / 8;

            // Determine the target difficulty of the zone from its
            // difficulty factor.
            float maxZoneScore = coverage * maxAreaScore;
            float target = factor * maxZoneScore;

            // Generate sample candidates, and calculate how difficult
            // each one would be. Note that the candidates will have different
            // numbers of groups, so that there can be more zone variety.

            int numSamples = numSamplesPerArea * coverage;
            var samples = new Sample[numSamples];

            int numGroups = 1;
            int maxGroups = numGroupsPerArea * coverage;

            for (int sample = 0; sample < numSamples; sample++)
            {
                samples[sample] = new Sample (random, numGroups, sleeperGroups, scoresByKind);

                numGroups %= maxGroups;
                numGroups += 1;
            }

            // Find the best candidate from the samples, and use that.

            var sortedSamples = samples
                .OrderBy (sample => Math.Abs (sample.danger - target))
                .ToList ();
            var candidate = sortedSamples[random.Next (5)];

            // Generate the appropriate spawn groups in the zone.

            var sleepers = new IL2.List<EnemySpawningData> ();

            foreach (var group in candidate.groups)
            {
                var sleeper = new EnemySpawningData ();
                sleeper.Difficulty = group.Difficulty;
                sleeper.GroupType = group.Type;

                if (canSpawnBigScary.Contains (group.persistentID))
                {
                    sleeper.Distribution = eEnemyZoneDistribution.Force_One;
                    sleeper.DistributionValue = 1.0f;
                }
                else if (canSpawnScout.Contains (group.persistentID))
                {
                    sleeper.Distribution = eEnemyZoneDistribution.Force_One;
                    sleeper.DistributionValue = 1.0f;
                }
                else
                {
                    sleeper.Distribution = eEnemyZoneDistribution.Rel_Value;
                    sleeper.DistributionValue = (float) Math.Sqrt (factor);
                }

                sleepers.Add (sleeper);
            }

            log.LogInfo ($"Zone {zone.AliasOverride}: danger: {(int) (100f * candidate.danger / maxZoneScore)}%");

            zone.EnemySpawningInZone = sleepers;

            // Calculate a danger factor as a percentage of baseline score.
            return candidate.danger / (0.2f * maxZoneScore);
        }

        // Create a population-randomized Blood Door in the provided zone.
        //
        // Weighted so that higher-difficulty populations spawn fewer groups.
        //
        public void CreateBloodDoorInZone (Random random, ExpeditionZoneData zone)
        {
            // zone.ActiveEnemyWave.EnemyGroupInfrontOfDoor = infront.persistentID;
            // zone.ActiveEnemyWave.EnemyGroupInArea = inrear.persistentID;

            // zone.ActiveEnemyWave.EnemyGroupsInArea = numGroups;
        }
    }
}
