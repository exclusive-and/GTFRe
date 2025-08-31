using BepInEx.Logging;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using IL2 = Il2CppSystem.Collections.Generic;
using UnityEngine;
using Player;
using UnityEngine.TestTools;

namespace GTFR
{
    internal class FunnyRundown : MonoBehaviour
    {
        // See Note [In Search of Closures] in GTFR.Main.
        private static event Action OnStartLevelBuild = () => { };
        public static void CallOnStartLevelBuild () => OnStartLevelBuild ();

        private static readonly float[] coverages = { 8f, 16f, 24f, 32f, 40f, 48f, 64f };

        internal static void CreateFunnyRundown
        (
            ManualLogSource log,
            VanillaLevels vanilla,
            SortedBioScans bioscans
        )
        {
            OnStartLevelBuild += () =>
            {
                var active = RundownManager.ActiveExpedition;
                var random = new System.Random (active.SessionSeed);

                var level = LevelLayoutDataBlock.GetBlock (active.LevelLayoutData);
                var originalZones = vanilla.OriginalLayoutOf (level.name).Zones;
                var zones = level.Zones;

                var complexId = active.Expedition.ComplexResourceData;
                var complex = ComplexResourceSetDataBlock.GetBlock (complexId);
                var geomorphs = new Geomorphs (complex);

                var dangerGraph = new DangerGraph (random, zones.Count);
                log.LogInfo ($"Danger Graph: {dangerGraph.ToString ()}");

                DesignFloorPlan (log, random, zones);
                InteriorDecorating (log, random, geomorphs, zones, originalZones);

                foreach (var zone in zones) zone.AliasOverride = level.ZoneAliasStart + (int) zone.LocalIndex;

                var enemyData = EnemyDataBlock.GetAllBlocks ();
                var populationData = EnemyPopulationDataBlock.GetBlock (1);
                var groupData = EnemyGroupDataBlock.GetAllBlocks ();

                var enemies = new SortedEnemies (enemyData, populationData, groupData);

                float funFactor = 0;
                float vanillaEnemyCost = EnemyCostManager.AllowedTotalCost;

                funFactor += InviteGuests (log, random, enemies, dangerGraph, zones);
                funFactor += PlanActivities (log, random, enemies, bioscans, zones, originalZones);

                funFactor += 3.0f * (EnemyCostManager.AllowedTotalCost - vanillaEnemyCost) / 100.0f;

                funFactor /= originalZones.Count;

                DistributeSnacks (funFactor, zones, originalZones);

                log.LogInfo ("A funny rundown awaits you!");
            };
        }

        static void DesignFloorPlan
        (
            ManualLogSource log,
            System.Random random,
            IL2.List<ExpeditionZoneData> zones
        )
        {
            log.LogInfo ("Designing floor plan");

            var connectionCounts = new List<int> ();
            foreach (var zone in zones)
            {
                connectionCounts.Add (0);
            }

            for (int i = 0; i < zones.Count - 1; i++)
            {
                int root = i >= 3 ? random.Next (i - 2, i) : random.Next (i);
                if (connectionCounts[root] >= 2)
                    continue;

                connectionCounts[root]++;
                zones[i].BuildFromLocalIndex = (eLocalZoneIndex) root;

                var coverage = coverages[random.Next (coverages.Length)];
                zones[i].CoverageMinMax = new Vector2 (coverage, coverage);
            }
        }

        static void InteriorDecorating
        (
            ManualLogSource log,
            System.Random random,
            Geomorphs geomorphs,
            IL2.List<ExpeditionZoneData> zones,
            IL2.List<ExpeditionZoneData> originalZones
        )
        {
            log.LogInfo ("Interior decorating");

            for (int i = 0; i < zones.Count - 1; i++)
            {
                zones[i].CustomGeomorph = geomorphs.RandomGeomorph (random, originalZones[i]);

                zones[i].SubComplex = Expedition.SubComplex.All;
            }
        }

        static float InviteGuests
        (
            ManualLogSource log,
            System.Random random,
            SortedEnemies enemies,
            DangerGraph dangerGraph,
            IL2.List<ExpeditionZoneData> zones
        )
        {
            log.LogInfo ("Inviting guests");

            float danger = 0;

            for (int i = 0; i < zones.Count; i++)
            {
                danger += enemies.SpawnEnemiesInZone (log, random, dangerGraph.graph[i], zones[i]);
            }

            return danger;
        }

        static float PlanActivities
        (
            ManualLogSource log,
            System.Random random,
            SortedEnemies enemies,
            SortedBioScans bioscans,
            IL2.List<ExpeditionZoneData> zones,
            IL2.List<ExpeditionZoneData> originals
        )
        {
            log.LogInfo ("Planning activities");

            float danger = 0;

            for (int i = 0; i < zones.Count - 1; i++)
            {
                if (i > 0)
                    danger += bioscans.CreateScanInZone (random, zones[i], originals[i]);

                /*
                // 20% chance of generating a Blood Door with some group of
                // enemies behind it.
                if (random.Next () % 5 == 0)
                {
                    zones[i].ActiveEnemyWave.HasActiveEnemyWave = true;
                    enemies.CreateBloodDoorInZone (random, zones[i]);
                }
                else
                {
                    zones[i].ActiveEnemyWave.HasActiveEnemyWave = false;
                }
                */
            }

            const float scanBaseline = 4 * 0.5f;

            return danger / scanBaseline;
        }

        static void DistributeSnacks
        (
            float danger,
            IL2.List<ExpeditionZoneData> zones,
            IL2.List<ExpeditionZoneData> originals
        )
        {
            for (int i = 0; i < zones.Count; i++)
            {
                zones[i].ToolAmmoMulti      = danger * originals[i].ToolAmmoMulti;
                zones[i].WeaponAmmoMulti    = danger * originals[i].WeaponAmmoMulti;
                zones[i].HealthMulti        = danger * originals[i].HealthMulti;
                zones[i].DisinfectionMulti  = danger * originals[i].DisinfectionMulti;
            }
        }

        public static void FakeClosurePatch (Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<FunnyRundown> ();

            harmony.Patch (
                typeof (LevelGeneration.Builder).GetMethod ("Build"),
                prefix: new HarmonyMethod (typeof (FunnyRundown).GetMethod ("CallOnStartLevelBuild"))
            );
        }
    }
}
