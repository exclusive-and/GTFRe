namespace GTFR;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using GameData;
using GTFR.Genetic;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

[BepInPlugin ("GTFRevolution", "GTFRevolution", "1.1.3")]
public class Main : BasePlugin
{
    public override void Load() => HahaInternal.HahaFunny(Log);
}

internal class HahaInternal : MonoBehaviour
{
    // See Note [In Search of Closures] below.

    // HahaFunny does a bit of preamble needed before things can really
    // get going.
    // 
    // See HahaSerious for the real action.
    public static void HahaFunny(ManualLogSource log)
    {
        log.LogInfo("Haha Funny Main >:3");
        FakeClosurePatch(log);
        HahaSerious(log);
    }

    // Note [In Search of Closures]
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // This note describes a workaround for C#'s weak HOF types.
    //
    // Ideally closures could be used as patches, so patches can capture
    // state and parameters without any misdirection, as in:
    //
    //      delegate void ExamplePatch ();
    //      
    //      ExamplePatch MkExamplePatch (PatchState state)
    //      { ... }
    //
    //      public void ApplyExamplePatch (MethodInfo original)
    //      {
    //          Harmony harmony = new Harmony ("example");
    //          
    //          var patchState = new PatchState ();
    //          harmony.Patch (original, MkExamplePatch (state));
    //      }
    //
    // Unfortunately Harmony patches must be static methods, so this lovely
    // ideal is presently impossible. In view of these limitations, this
    // code does the next best thing: using C# events to emulate closures.
    //
    // More precisely, the MkExamplePatch function is replaced with a
    // static event, such as OnCallOriginal. The closure returned by
    // MkExamplePatch can then be spoofed by a subscriber to OnCallEvent.
    // The patch is then simply a static method that calls OnCallOriginal.
    //
    // The above example using closures can be translated like so:
    //
    //      private static event Action OnCallOriginal = () => { };
    //      public static void CallOnCallOriginal () => OnCallOriginal ();
    //
    //      static void MkExamplePatch (PatchState state)
    //      {
    //          OnCallOriginal += () => { ... };
    //      }
    //
    //      public void FakeClosurePatch (MethodInfo original)
    //      {
    //          Harmony harmony = new Harmony ("example");
    //          harmony.Patch (original, CallOnCallOriginal);
    //
    //          var patchState = new PatchState ();
    //          MkExamplePatch (patchState);
    //      }

    private static event Action OnStartGame = () => { };
    public static void CallOnStartGame() => OnStartGame();

    // HahaSerious is the REAL main function.
    static void HahaSerious(ManualLogSource log)
    {
        OnStartGame += () =>
        {
            var config = new Configuration();

            log.LogInfo("Haha Serious Main >:(");

            /*
            var ingameLayouts = LevelLayoutDataBlock.GetAllBlocks();
            var vanillaLayouts = new VanillaLevels(ingameLayouts);

            var unsortedEnemies = EnemyGroupDataBlock.GetAllBlocks();

            var unsortedScans = ChainedPuzzleDataBlock.GetAllBlocks();
            var scans = new SortedBioScans(unsortedScans);

            FunnyResources.UncapResourcePacks();
            FunnyRundown.CreateFunnyRundown(log, vanillaLayouts, scans);
            */


            GeneticRundown.CreateGeneticRundown(log);
        };
    }

    // Small patch to randomize build seed. This doesn't capture any state,
    // so it can just be done in-place.
    public static void CallOnSetActiveExpedition
    (
        ref pActiveExpedition expPackage,
        ref ExpeditionInTierData expTierData
    )
    {
        var random = new System.Random(expPackage.sessionSeed);
        expTierData.Seeds.BuildSeed = random.Next(100000000, int.MaxValue);
    }

    // Patch in all the fake closures with Harmony.
    public static void FakeClosurePatch(ManualLogSource log)
    {
        ClassInjector.RegisterTypeInIl2Cpp<HahaInternal>();

        var harmony = new Harmony("gtfr:e");

        harmony.Patch(
            typeof(StartMainGame).GetMethod("Start"),
            postfix: new HarmonyMethod(typeof(HahaInternal).GetMethod("CallOnStartGame"))
        );
            
        harmony.Patch(
            typeof(RundownManager).GetMethod("SetActiveExpedition"),
            prefix: new HarmonyMethod(typeof(HahaInternal).GetMethod("CallOnSetActiveExpedition"))
        );

        GeneticRundown.FakeClosurePatch(harmony);
        // FunnyResources.FakeClosurePatch(harmony);
        // FunnyRundown.FakeClosurePatch(harmony);
    }
}
