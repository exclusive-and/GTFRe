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

public class GeneticRundown : UnityEngine.MonoBehaviour
{
    public static void FakeClosurePatch(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<GeneticRundown>();

        harmony.Patch(
            typeof(LevelGeneration.Builder).GetMethod("Build"),
            prefix: new HarmonyMethod(typeof(GeneticRundown).GetMethod("CallOnStartLevelBuild"))
        );
    }

    // See Note [In Search of Closures] in GTFR.Main.
    private static event Action OnStartLevelBuild = () => { };
    public static void CallOnStartLevelBuild() => OnStartLevelBuild();

    public static void CreateGeneticRundown(ManualLogSource log, ChromosomeTransfer transfer)
    {
        OnStartLevelBuild += () =>
        {
            var active = RundownManager.ActiveExpedition;
            var random = new System.Random (active.SessionSeed);
            var level = LevelLayoutDataBlock.GetBlock (active.LevelLayoutData);

            transfer.Reset();

            if (SNet.IsMaster)
            {
                var chromosome = RunGenetic(log, random, level.Zones.Count);
    
                var rundown = GeneticRundown.FromChromosome(chromosome, level.Zones.Count);

                for (int i = 0; i < Math.Min(level.Zones.Count, rundown.Count); i++) {
                    log.LogInfo($"Zone {i}: {rundown[i].Show()}");
                    rundown[i].SetupZone(log, level.Zones[i]);
                }

                NetworkAPI.InvokeFreeSizedEvent("awaitDNA", chromosome.ToBytes());
            }
            else
            {
                log.LogInfo("Waiting for level chromosome...");
                transfer.Wait();
                log.LogInfo("Chromosome received!");
    
                var rundown = GeneticRundown.FromChromosome(transfer.Result, level.Zones.Count);

                for (int i = 0; i < Math.Min(level.Zones.Count, rundown.Count); i++) {
                    // log.LogInfo($"Zone {i}: {rundown[i].Show()}");
                    rundown[i].SetupZone(log, level.Zones[i]);
                }
            }
        };
    }

    public static Chromosome RunGenetic(ManualLogSource log, Random random, int numZones)
    {
        const int minPopulation = 16384;
        const int maxIterations = 64;
        const int numElites = 1024;
        const float threshold = 20.0f;

        var crossover = new OnePointCrossover(0.75f);
        var mutation = new MultiMutation(
            new IMutation[] {
                new FlipMutation(0.09f),
                new CopyMutation(0.05f),
            });

        var fitness = new GeneticRundown.Fitness(numZones);
        var termination = new GeneticRundown.Termination(0.85f * (float) numZones, fitness);
            
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
