namespace GTFR;

using BepInEx.Logging;
using GameData;
using GTFR.Genetic;
using Prelude;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.Playables;
using SNetwork;
using GTFO.API;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using System.Collections;
using CellMenu;
using AK.Wwise;
using Il2CppSystem.Xml;

public class GeneticRundown : UnityEngine.MonoBehaviour
{
    // See Note [In Search of Closures] in GTFR.Main.

    private delegate bool TryStartLevel_t(ref bool __result);

    private static TryStartLevel_t OnTryStartLevel;
    public static bool CallOnTryStartLevel(ref bool __result) => OnTryStartLevel(ref __result);

    private static Action OnStartLevelBuild;
    public static void CallOnStartLevelBuild() => OnStartLevelBuild();

    private static Action<CM_PageLoadout> OnUpdateReadyState;

    public static void CallOnUpdateReadyState(CM_PageLoadout __instance) => OnUpdateReadyState(__instance);

    static GeneticRundown()
    {
        OnTryStartLevel = (ref bool __result) =>
        {
            __result = false;
            return false;
        };

        OnStartLevelBuild = () => { };

        OnUpdateReadyState = (page) => { };
    }

    public static void FakeClosurePatch(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<GeneticRundown>();

        harmony.Patch(
            typeof(GS_Lobby).GetMethod("TryStartLevelTrigger"),
            prefix: new HarmonyMethod(typeof(GeneticRundown), "CallOnTryStartLevel")
        );

        harmony.Patch(
            typeof(CM_PageLoadout).GetMethod("UpdateReadyState"),
            postfix: new HarmonyMethod(typeof(GeneticRundown), "CallOnUpdateReadyState")
        );

        harmony.Patch(
            typeof(LevelGeneration.Builder).GetMethod("Build"),
            prefix: new HarmonyMethod(typeof(GeneticRundown), "CallOnStartLevelBuild")
        );
    }

    private enum BuildState
    {
        NeedRebuild,
        Evolving,
        WaitingForChromosome,
        ReadyToDrop,
    }

    public static void CreateGeneticRundown(ManualLogSource log)
    {
        Chromosome chromosome = null;
        var transfer = new ChromosomeTransfer();

        var state = BuildState.NeedRebuild;

        void buildLevel()
        {
            var active = RundownManager.ActiveExpedition;
            var level = LevelLayoutDataBlock.GetBlock(active.LevelLayoutData);

            var random = new Random(active.SessionSeed);

            var result = RunGenetic(log, random, level.Zones.Count);

            chromosome = result;
            NetworkAPI.InvokeFreeSizedEvent("awaitDNA", result.ToBytes());

            state = BuildState.ReadyToDrop;
        }

        void awaitLevel()
        {
            var active = RundownManager.ActiveExpedition;
            var level = LevelLayoutDataBlock.GetBlock(active.LevelLayoutData);

            transfer.Reset();

            log.LogInfo("Waiting for level chromosome...");
            transfer.Wait();
            log.LogInfo("Chromosome received!");

            chromosome = transfer.Result;

            state = BuildState.ReadyToDrop;
        }

        OnTryStartLevel = (ref bool __result) =>
        {
            __result = false;

            if (state is BuildState.ReadyToDrop)
            {
                state = BuildState.NeedRebuild;
                return true;
            }

            if (state is BuildState.NeedRebuild)
            {
                if (SNet.IsMaster)
                {
                    state = BuildState.Evolving;
                    Task.Run(buildLevel);
                }
                else
                {
                    state = BuildState.WaitingForChromosome;
                    Task.Run(awaitLevel);
                }
            }

            return false;
        };

        OnUpdateReadyState = (page) =>
        {
            if (state is BuildState.Evolving or BuildState.WaitingForChromosome)
            {
                page.m_dropButton.gameObject.SetActive(false);
                page.m_changeLoadoutButton.gameObject.SetActive(false);
                page.m_readyButton.gameObject.SetActive(false);
            }

            if (state is BuildState.ReadyToDrop)
            {
                GameStateManager.CurrentState.TryStartLevelTrigger();
            }
        };

        OnStartLevelBuild = () =>
        {
            var active = RundownManager.ActiveExpedition;
            var level = LevelLayoutDataBlock.GetBlock(active.LevelLayoutData);

            var rundown = FromChromosome(chromosome, level.Zones.Count);

            for (int i = 0; i < Math.Min(level.Zones.Count, rundown.Count); i++)
            {
                log.LogInfo($"Zone {i}: {rundown[i].Show()}");
                rundown[i].SetupZone(log, level.Zones[i]);
            }
        };
    }

    public static Chromosome RunGenetic(ManualLogSource log, Random random, int numZones)
    {
        const int minPopulation = 16384;
        const int maxIterations = 64;
        const int numElites = 1024;
        const float threshold = 100.0f;

        var crossover = new OnePointCrossover(0.75f);
        var mutation = new MultiMutation(
            new IMutation[] {
                new FlipMutation(0.09f),
                new CopyMutation(0.05f),
            });

        var fitness = new GeneticRundown.Fitness(numZones);
        var termination = new GeneticRundown.Termination(0.875f * numZones, fitness);
            
        var algo = new Algorithm(minPopulation, maxIterations, numElites, fitness, crossover, mutation, termination);

        var initial = new List<Chromosome>(minPopulation);

        for (int i = 0; i < minPopulation; i++) {
            initial.Add(Chromosome.MakeRandom(random, 8, 32));
        }

        var final = algo.Run(random, initial)
                .OrderByDescending(fitness.Evaluate)
                .ToList();
        
        var best = final[0];

        log.LogInfo("");
        log.LogInfo("\n" + best.Show());
        log.LogInfo("");
        log.LogInfo($"Max final fitness: {fitness.Evaluate(best)}");

        return best;
    }

    public class Fitness : IFitness
    {
        private int numZones;

        public Fitness(int numZones)
        {
            this.numZones = numZones;
        }

        public double Evaluate(Chromosome chromosome)
        {
            double bonus = 1.0;
            double total = 0.0;

            var zones = GeneticRundown.FromChromosome(chromosome, numZones);

            for (int i = 0; i < Math.Min(numZones, zones.Count); i++) {
                if (zones[i].enemies.Count > 0) {
                    bonus *= 1.00;
                }
                total += zones[i].Evaluate();
            }

            return bonus * total;
        }
    }

    public class Termination : ITermination
    {
        private float Threshold;

        private IFitness fitness;

        public Termination(float threshold, IFitness fitness)
        {
            this.Threshold = threshold;
            this.fitness = fitness;
        }

        public bool IsGoodEnough(IList<Chromosome> chromosomes)
        {
            return fitness.Evaluate(chromosomes[0]) > Threshold;
        }
    }

    private struct pChromosome
    {
        public Il2CppStructArray<bool> genes;

        public pChromosome(Chromosome chromosome)
        {
            this.genes = chromosome.Genes;
        }
    }

    public enum Allele
    {
        Nothing,
        Command,    // Prefix 00
        Enemy,      // Prefix 01
    }

    public static Maybe<Allele> NextAllele(ChromosomeReader reader) => reader.Next(1) switch {
        Nothing<string>    => new Nothing<Allele>(),
        Just<string>("0")  => new Just<Allele>(Allele.Command),
        Just<string>("1")  => new Just<Allele>(Allele.Enemy),
        _                  => new Just<Allele>(Allele.Nothing),
    };

    public static List<GeneticZone> FromChromosome(Chromosome chromosome, int numZones)
    {
        var reader = new ChromosomeReader(chromosome);

        var zones = new GeneticZone[numZones];

        for (int i = 0; i < numZones; i++) {
            zones[i] = new GeneticZone();
        }

        int zone = 0;

        while (NextAllele(reader) is Just<Allele>(var allele)) {
            switch (allele) {
                case Allele.Command:
                    if (numZones <= ++zone) {
                        return zones.ToList();
                    }
                    break;
                
                case Allele.Enemy:
                    GeneticEnemies.NextEnemy(reader, zones[zone]);
                    break;
                
                default:
                    break;
            }
        }

        return zones.ToList();
    }
}
