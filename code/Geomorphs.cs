using BepInEx.Logging;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFR
{
    public class Geomorphs
    {
        private readonly List<string> geomorphs;
        private readonly List<string> reactors;

        public Geomorphs (ComplexResourceSetDataBlock complex)
        {
            geomorphs = new List<string> ();
            reactors = new List<string> ();

            foreach (var geo in complex.CustomGeomorphs_Objective_1x1)
            {
                var fab = geo.Prefab;

                if (fab.Contains ("reactor") && !fab.Contains ("tunnel"))
                {
                    reactors.Add (fab);
                }

                if (CheckWhitelist (fab))
                {
                    geomorphs.Add (fab);
                }
            }
        }

        public string? RandomGeomorph
        (
            Random random,
            ExpeditionZoneData zone
        )
        {
            if (zone.CustomGeomorph is null or "")
                return zone.CustomGeomorph;

            if (CheckBlacklist (zone.CustomGeomorph))
                return zone.CustomGeomorph;

            if (zone.CustomGeomorph.Contains ("reactor") && reactors.Count > 0)
                return reactors[random.Next (reactors.Count)];

            if (geomorphs.Count > 0)
                return geomorphs[random.Next (geomorphs.Count)];

            return zone.CustomGeomorph;
        }

        private static readonly string[] whitelist =
        {
            "geo_64x64_tech_node_transition",
            "geo_64x64_tech_node_transition_02_JG",
            "geo_64x64_tech_node_transition_03_JG",
            "geo_64x64_tech_node_transition_04_JG",
            "geo_64x64_tech_node_transition_05_JG",
            "geo_64x64_tech_node_transition_06_JG",
            "geo_64x64_tech_lab_hub_HA_04",
            "geo_64x64_tech_lab_hub_LF_01",
            "geo_64x64_tech_lab_hub_HA_01_R3D1",
            "geo_64x64_tech_lab_hub_SF_02",
            "geo_64x64_tech_lab_hub_HA_01",
            "geo_64x64_tech_destroyed_HA_01",
            "geo_64x64_tech_destroyed_HA_02",
            "geo_64x64_tech_data_center_hub_SF_ConduitVersion",
            "geo_64x64_tech_data_center_hub_SF_01",
            "geo_64x64_tech_data_center_hub_JG_01",
            "geo_64x64_lab_I_HA_01",
            "geo_64x64_lab_I_HA_03",
            "geo_64x64_lab_reactor_HA_01",
            "geo_64x64_lab_reactor_HA_02",
            "geo_64x64_tech_data_center_hub_SF_02_CreativeDesign",
            "geo_64x64_tech_data_center_HA_09_v2_R5B2",
            "geo_64x64_tech_data_center_HA_09",
            "geo_64x64_tech_data_center_HA_08",
            "geo_64x64_tech_data_center_HA_07_v2_R5B2",
            "geo_64x64_tech_data_center_HA_07",
            "geo_64x64_tech_data_center_HA_06",
            "geo_64x64_tech_data_center_HA_05_v3_R5B2",
            "geo_64x64_tech_data_center_HA_05_v2_R5B2",
            "geo_64x64_tech_data_center_HA_05",
            "geo_64x64_tech_data_center_HA_04_V2_R5B2",
            "geo_64x64_tech_data_center_HA_04",
            "geo_64x64_tech_data_center_HA_03",
            "geo_64x64_tech_data_center_HA_02",
            "geo_64x64_tech_data_center_HA_01_v2_R5B2",
            "geo_64x64_tech_data_center_HA_01",
            "geo_64x64_tech_data_center_dead_end_HA_01",
            "geo_64x64_service_floodways_VS_01",
            "geo_64x64_service_floodways_SF_01",
            "geo_64x64_service_floodways_I_HA_02",
            "geo_64x64_service_floodways_I_HA_01_R7D1",
            "geo_64x64_service_floodways_I_HA_01",
            "geo_64x64_service_floodways_hub_SF_02",
            "geo_64x64_service_floodways_hub_SF_01",
            "geo_64x64_service_floodways_hub_HA_03",
            "geo_64x64_service_floodways_hub_HA_02",
            "geo_64x64_service_floodways_hub_HA_01_R7D1",
            "geo_64x64_service_floodways_hub_HA_01",
            "geo_64x64_service_floodways_hub_AW_01_V2",
            "geo_64x64_service_floodways_hub_AW_01",
            "geo_64x64_service_floodways_HA_09",
            "geo_64x64_service_floodways_HA_08",
            "geo_64x64_service_floodways_HA_07",
            "geo_64x64_service_floodways_HA_06_R5A1_2",
            "geo_64x64_service_floodways_HA_06_R5A1_1",
            "geo_64x64_service_floodways_HA_06",
            "geo_64x64_service_floodways_HA_05_R5A1_2",
            "geo_64x64_service_floodways_HA_05_R5A1_1",
            "geo_64x64_service_floodways_HA_05",
            "geo_64x64_service_floodways_HA_04",
            "geo_64x64_service_floodways_HA_03_R5A1_3",
            "geo_64x64_service_floodways_HA_03_R5A1_2",
            "geo_64x64_service_floodways_HA_03_R5A1_1",
            "geo_64x64_service_floodways_HA_03",
            "geo_64x64_lab_I_HA_01",
            "geo_64x64_lab_I_HA_03",
            "geo_64x64_lab_reactor_HA_01",
            "geo_64x64_lab_reactor_HA_02",
            "geo_64x64_mining_challenge_01",
            "geo_64x64_mining_dig_site_AS_01",
            "geo_64x64_mining_dig_site_AS_02",
            "geo_64x64_mining_dig_site_AS_02_v2",
            "geo_64x64_mining_dig_site_AS_04",
            "geo_64x64_mining_dig_site_AS_04_v2",
            "geo_64x64_mining_dig_site_HA_01",
            "geo_64x64_mining_dig_site_HA_01_v2",
            "geo_64x64_mining_dig_site_HA_02",
            "geo_64x64_mining_dig_site_HA_02_R2A1",
            "geo_64x64_mining_dig_site_HA_02_v2",
            "geo_64x64_mining_dig_site_HA_02_v3",
            "geo_64x64_mining_dig_site_HA_02_v4",
            "geo_64x64_mining_dig_site_HA_03",
            "geo_64x64_mining_dig_site_HA_03_v2",
            "geo_64x64_mining_dig_site_HA_03_v3",
            "geo_64x64_mining_dig_site_HA_04",
            "geo_64x64_mining_dig_site_HA_04_v2",
            "geo_64x64_mining_dig_site_HA_05",
            "geo_64x64_mining_dig_site_HA_05_v2",
            "geo_64x64_mining_dig_site_HA_06",
            "geo_64x64_mining_dig_site_HA_06_v2",
            "geo_64x64_mining_dig_site_HA_07",
            "geo_64x64_mining_dig_site_HA_07_v2",
            "geo_64x64_mining_dig_site_HA_07_v3",
            "geo_64x64_mining_dig_site_hub_HA_01",
            "geo_64x64_mining_dig_site_hub_HA_01.R4E1",
            "geo_64x64_mining_dig_site_hub_HA_02",
            "geo_64x64_mining_dig_site_hub_HA_03",
            "geo_64x64_mining_dig_site_hub_SF_01",
            "geo_64x64_mining_dig_site_hub_SF_02",
            "geo_64x64_mining_dig_site_I_HA_01",
            "geo_64x64_mining_dig_site_reactor_tunnel_I_HA_01",
            "geo_64x64_mining_exit_01",
            "geo_64x64_mining_exit_01_tutorial",
            "geo_64x64_mining_exit_02",
            "geo_64x64_mining_portal_HA_01",
            "geo_64x64_mining_HSU_exit_01",
            "geo_64x64_mining_reactor_open_HA_01",
            "geo_64x64_mining_refinery_dead_end_HA_01",
            "geo_64x64_mining_refinery_HA_01",
            "geo_64x64_mining_refinery_HA_02",
            "geo_64x64_mining_refinery_HA_03",
            "geo_64x64_mining_refinery_HA_04",
            "geo_64x64_mining_refinery_HA_04_v2",
            "geo_64x64_mining_refinery_HA_05",
            "geo_64x64_mining_refinery_HA_05_v2",
            "geo_64x64_mining_refinery_HA_06",
            "geo_64x64_mining_refinery_HA_06_v2",
            "geo_64x64_mining_refinery_HA_07",
            "geo_64x64_mining_refinery_HA_08",
            "geo_64x64_mining_refinery_I_HA_01",
            "geo_64x64_mining_refinery_I_HA_01_v2",
            "geo_64x64_mining_refinery_I_HA_02",
            "geo_64x64_mining_refinery_I_HA_03",
            "geo_64x64_mining_refinery_I_HA_04",
            "geo_64x64_mining_refinery_I_HA_05",
            "geo_64x64_mining_refinery_I_HA_06",
            "geo_64x64_mining_refinery_JG_01",
            "geo_64x64_mining_refinery_JG_02",
            "geo_64x64_mining_refinery_L_HA_01",
            "geo_64x64_mining_refinery_X_HA_01",
            "geo_64x64_mining_refinery_X_HA_02",
            "geo_64x64_mining_refinery_X_HA_03",
            "geo_64x64_mining_refinery_X_HA_04",
            "geo_64x64_mining_refinery_X_HA_05",
            "geo_64x64_mining_refinery_X_HA_06",
            "geo_64x64_mining_refinery_X_HA_06_R3B2",
            "geo_64x64_mining_refinery_X_HA_06_v2",
            "geo_64x64_mining_refinery_X_HA_07",
            "geo_64x64_mining_refinery_X_VS_01",
            "geo_64x64_mining_refinery_X_VS_01_v2",
            "geo_64x64_mining_storage_HA_01",
            "geo_64x64_mining_storage_HA_02",
            "geo_64x64_mining_storage_HA_03",
            "geo_64x64_mining_storage_HA_04",
            "geo_64x64_mining_storage_HA_05",
            "geo_64x64_mining_storage_HA_06",
            "geo_64x64_mining_storage_HA_06b",
            "geo_64x64_mining_storage_HA_07",
            "geo_64x64_mining_storage_HA_08",
            "geo_64x64_mining_storage_hub_HA_01",
            "geo_64x64_mining_storage_hub_HA_02",
            "geo_64x64_mining_storage_hub_HA_03",
            "geo_64x64_mining_storage_hub_HA_04",
            "geo_64x64_mining_storage_hub_HA_04_R3B1",
            "geo_64x64_mining_storage_hub_HA_05",
            "geo_64x64_mining_storage_hub_VS_01",
            "geo_64x64_mining_storage_I_HA_01",
            "geo_64x64_mining_storage_I_HA_01_tutorial",
            "geo_64x64_mining_storage_tutorial_VS_01",
            "geo_64x64_mining_storage_tutorial_VS_02",
            "geo_64x64_mining_storage_tutorial_VS_03",
            "geo_64x64_mining_storage_tutorial_VS_04",
            "geo_64x64_mining_storage_vault_HA_01",
            "geo_64x64_mining_storage_vault_v2_HA_01",
            "geo_64x64_mining_storage_vault_v3_HA_01",
            "geo_64x64_service_floodways_dead_end_HA_01",
            "geo_64x64_service_floodways_dead_end_HA_01_R7D1",
            "geo_64x64_service_floodways_HA_01",
            "geo_64x64_service_floodways_HA_01_R5A1_1",
            "geo_64x64_service_floodways_HA_01_R5A1_2",
            "geo_64x64_service_floodways_HA_01_R5A1_3",
            "geo_64x64_service_floodways_HA_01_R5A1_4",
            "geo_64x64_service_floodways_HA_02"
        };

        public static bool CheckWhitelist (string geo)
        {
            return whitelist.Any (e => geo.Contains (e));
        }

        private static readonly string[] blacklist =
        {
            "Mainframe",                                            // For R7D2
            "dead_end",                                             // No End Tile should be rotate out, they are usually objective tiles
            "reactor",                                              // Dont rotate out of reactor please...
            "geo_64x64_Lab_dead_end_room",                          // R7D2 grab key room
            "geo_64x64_mining_refinery_X",                          // R7C2 Gen clusters
            "geo_64x64_mining_reactor_HA_02",                       // R2D1 Reactor Shutdown
            "portal",                                               // Portals
            "exit",                                                 // Extraction tiles
            "geo_64x64_tech_lab_hub_HA_02",                         // I forgor
            "geo_64x64_service_gardens_Lab_HSU_Prep_TestingFunc",   // R7D1 end tile
            "geo_64x64_service_gardens_X_01",                       // R7B1 CollectionCase tile
            "geo_64x64_service_gardens_I_01"                        // R7B1 T-scan end tile
        };

        public static bool CheckBlacklist (string geo)
        {
            return blacklist.Any (e => geo.Contains (e));
        }
    }
}
