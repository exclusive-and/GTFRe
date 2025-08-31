using BepInEx.Logging;
using GameData;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GTFR
{
    internal class FunnyResources : MonoBehaviour
    {
        // See Note [In Search of Closures] in GTFR.Main.

        private delegate void GiveAmmoEvent
            (PlayerAmmoStorage playerStorage, ref AmmoType ammoType);

        private static event GiveAmmoEvent OnGiveAmmo =
            (PlayerAmmoStorage playerStorage, ref AmmoType ammoType) => { };

        public static void CallOnGiveAmmo
        (
            PlayerAmmoStorage __instance,
            ref AmmoType ammoType,
            ref int bulletCount
        ) => OnGiveAmmo (__instance, ref ammoType);

        private delegate void UpdateAmmoPackEvent (ref int bulletCount);

        private static event UpdateAmmoPackEvent OnUpdateAmmoPack =
            (ref int bulletCount) => { };

        public static void CallOnUpdateAmmoPack
        (
            PlayerAmmoStorage __instance,
            ref AmmoType ammoType,
            ref int bulletCount,
            ref float __result
        ) => OnUpdateAmmoPack (ref bulletCount);

        public static void UncapResourcePacks ()
        {
            var expBalance = ExpeditionBalanceDataBlock.GetBlock (1);
            expBalance.ResourcePackSizes[0] *= 2;
            expBalance.ResourcePackSizes[1] *= 2;
            expBalance.ResourcePackSizes[2] *= 2;
            expBalance.ResourcePackSizes.Add (2.4f);
            expBalance.ResourcePackSizes.Add (3f);
            expBalance.ResourcePackSizes.Add (3.2f);
            expBalance.ResourcePackSizes.Add (4.0f);

            OnGiveAmmo += (PlayerAmmoStorage playerStorage, ref AmmoType ammoType) =>
            {
                var ammoPack = playerStorage.m_ammoStorage[(int) ammoType];
                var current = ammoPack.AmmoInPack;

                OnUpdateAmmoPack = (ref int bulletCount) =>
                {
                    var ammoCost = (float) bulletCount * ammoPack.CostOfBullet;
                    ammoPack.AmmoInPack = current + ammoCost;
                    playerStorage.UpdateAllAmmoUI ();
                };
            };
        }

        public static void FakeClosurePatch (Harmony harmony)
        {
            ClassInjector.RegisterTypeInIl2Cpp<FunnyResources> ();

            harmony.Patch (
                typeof (PlayerAmmoStorage).GetMethod ("UpdateBulletsInPack"),
                prefix: new HarmonyMethod (typeof (FunnyResources).GetMethod ("CallOnGiveAmmo"))
            );

            harmony.Patch (
                typeof (PlayerAmmoStorage).GetMethod ("UpdateBulletsInPack"),
                postfix: new HarmonyMethod (typeof (FunnyResources).GetMethod ("CallOnUpdateAmmoPack"))
            );
        }
    }
}
