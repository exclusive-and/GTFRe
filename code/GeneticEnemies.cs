namespace GTFR;

using BepInEx.Logging;
using GameData;
using GTFR.Genetic;
using IL2 = Il2CppSystem.Collections.Generic;
using Prelude;

public class GeneticEnemies
{
    public record Phenotype(
        double penalty,
        double score,
        eEnemyGroupType group,
        eEnemyRoleDifficulty role,
        eEnemyZoneDistribution distribution,
        float distributionValue
    );

    public record Hunter(double penalty, double score, int persistentID, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.Hunter,
            role,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );

    public static void NextEnemy(ChromosomeReader reader, GeneticZone zone)
    {
        (string, Phenotype)[] phenotypes = new (string, Phenotype)[] {
            ("Sleepers Easy"        , new Sleeper(1.05, 0.00, eEnemyRoleDifficulty.Easy)),
            ("Sleepers Medium"      , new Sleeper(1.05, 0.00, eEnemyRoleDifficulty.Medium)), // Sleepers, giants, chicken
            ("Sleepers Hard"        , new Sleeper(1.08, 0.00, eEnemyRoleDifficulty.Hard)), // Sleepers, giants, chickens, scouts
            ("Hybrids"              , new Hybrid(1.10, 0.00)),
            ("Scouts Easy"          , new Scout(1.08, 0.00, eEnemyRoleDifficulty.Easy)),
            ("Scouts Medium"        , new Scout(1.07, 0.00, eEnemyRoleDifficulty.Medium)),
            ("Scouts Hard"          , new Scout(1.10, 0.00, eEnemyRoleDifficulty.Hard)),
            ("Chargers Easy"        , new Charger(1.05, 0.00, eEnemyRoleDifficulty.Easy)),
            ("Chargers Medium"      , new Charger(1.06, 0.00, eEnemyRoleDifficulty.Medium)),
            ("Charger Giant"        , new Charger(1.10, 0.00, eEnemyRoleDifficulty.Hard)),
            ("Charger Scout"        , new ChargerScout(1.08, 0.00)),
            ("Shadows"              , new Shadow(1.08, 0.00, eEnemyRoleDifficulty.Biss)),
            ("Shadow Giants"        , new Shadow(1.10, 0.00, eEnemyRoleDifficulty.Buss)),
            ("Shadow Scout"         , new ShadowScout(1.10, 0.00)),
            ("Nightmare Strikers"   , new Nightmare(1.05, 0.00, eEnemyRoleDifficulty.Biss)),
            ("Nightmare Shooters"   , new Nightmare(1.06, 0.00, eEnemyRoleDifficulty.Buss)),
            ("Nightmare Scout"      , new NightmareScout(1.17, 0.00)),
            ("Tank"                 , new Tank(1.25, 0.00)),
            ("Mother"               , new Mother(1.20, 0.00)),
            ("P-Mother"             , new PMother(1.30, 0.00)),

            ("BD Easy"              , new Hunter(1.10, 0.00, 30, eEnemyRoleDifficulty.Easy)),
            ("BD Easy"              , new Hunter(1.10, 0.00, 76, eEnemyRoleDifficulty.Easy)),
            ("BD Giant"             , new Hunter(1.12, 0.00, 74, eEnemyRoleDifficulty.Buss)),
            ("BD Hybrid Easy"       , new Hunter(1.13, 0.00, 31, eEnemyRoleDifficulty.Easy)),
            ("BD Hybrid Easy"       , new Hunter(1.13, 0.00, 51, eEnemyRoleDifficulty.Easy)),
            ("BD Hybrid Medium"     , new Hunter(1.14, 0.00, 33, eEnemyRoleDifficulty.Easy)),
            ("BD Chargers Easy"     , new Hunter(1.12, 0.00, 32, eEnemyRoleDifficulty.Easy)),
            ("BD Chargers Easy"     , new Hunter(1.12, 0.00, 72, eEnemyRoleDifficulty.Easy)),
            ("BD Shadows Easy"      , new Hunter(1.13, 0.00, 77, eEnemyRoleDifficulty.Easy)),
            ("BD Shadows Medium"    , new Hunter(1.15, 0.00, 35, eEnemyRoleDifficulty.Easy)),
            ("BD Shadows Medium"    , new Hunter(1.15, 0.00, 78, eEnemyRoleDifficulty.Easy)),
            ("BD Mother"            , new Hunter(1.20, 0.00, 36, eEnemyRoleDifficulty.Hard)),
            ("BD Mother"            , new Hunter(1.20, 0.00, 49, eEnemyRoleDifficulty.Hard)),
            ("BD P-Mother"          , new Hunter(1.30, 0.00, 47, eEnemyRoleDifficulty.MiniBoss)),
            ("BD Tank"              , new Hunter(1.25, 0.00, 46, eEnemyRoleDifficulty.MegaBoss)),
        };

        var asleep = reader.Match(new (string, (string, Phenotype))[] {
            ("00000", phenotypes[ 0]),  // Sleepers Easy
            ("00001", phenotypes[ 1]),  // Sleepers Medium
            ("00010", phenotypes[ 2]),  // Sleepers Hard
            ("00011", phenotypes[ 3]),  // Hybrids
            ("00100", phenotypes[ 4]),  // Scouts Easy
            ("00101", phenotypes[ 5]),  // Scouts Medium
            ("00110", phenotypes[ 6]),  // Scouts Hard
            ("00111", phenotypes[ 7]),  // Chargers Easy
            ("01000", phenotypes[ 8]),  // Chargers Medium
            ("01001", phenotypes[ 9]),  // Charger Giant
            ("01010", phenotypes[10]),  // Charger Scout
            ("01011", phenotypes[11]),  // Shadows
            ("01100", phenotypes[12]),  // Shadow Giant
            ("01101", phenotypes[13]),  // Shadow Scout
            ("01110", phenotypes[14]),  // Nightmare Strikers
            ("01111", phenotypes[15]),  // Nightmare Shooters
            // ("10000", phenotypes[16]),  // Nightmare Scout
            ("10001", phenotypes[17]),  // Tank
            ("10010", phenotypes[18]),  // Mother
            ("10011", phenotypes[19]),  // P-Mother

            ("110000", phenotypes[20]),
            ("110001", phenotypes[21]),
            ("110010", phenotypes[22]),
            ("110011", phenotypes[23]),
            ("110100", phenotypes[24]),
            ("110101", phenotypes[25]),
            ("110110", phenotypes[26]),
            ("110111", phenotypes[27]),
            ("111000", phenotypes[28]),
            ("111001", phenotypes[29]),
            ("111010", phenotypes[30]),
            ("111011", phenotypes[31]),
            ("111100", phenotypes[32]),
            ("111101", phenotypes[33]),
            // ("111110", phenotypes[34]),
        });

        if (asleep is Just<(string, Phenotype)>(var x)) {
            switch (x) {
                case (var name, Hunter hunter):
                    if (zone.bloodDoors.Count < 2) {
                        zone.bloodDoors.Add((name, hunter));
                    }
                    break;
                
                default:
                    zone.enemies.Add(x);
                    break;
            }
        }
    }

    private record Sleeper(double penalty, double score, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.Hibernate,
            role,
            eEnemyZoneDistribution.Rel_Value,
            1.0f
        );

    private record Scout(double penalty, double score, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureDetect,
            role,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );
    
    private record Hybrid(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureSneak,
            eEnemyRoleDifficulty.Biss,
            eEnemyZoneDistribution.Rel_Value,
            1.0f
        );
    
    private record Charger(double penalty, double score, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.Detect,
            role,
            eEnemyZoneDistribution.Rel_Value,
            1.0f
        );
    
    private record ChargerScout(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureDetect,
            eEnemyRoleDifficulty.MiniBoss,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );
    
    private record Shadow(double penalty, double score, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.Hibernate,
            role,
            eEnemyZoneDistribution.Rel_Value,
            1.0f
        );

    private record ShadowScout(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureDetect,
            eEnemyRoleDifficulty.Boss,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );
    
    private record Nightmare(double penalty, double score, eEnemyRoleDifficulty role):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.Detect,
            role,
            eEnemyZoneDistribution.Rel_Value,
            1.0f
        );

    private record NightmareScout(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureDetect,
            eEnemyRoleDifficulty.Buss,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );

    private record Tank(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureSneak,
            eEnemyRoleDifficulty.MegaBoss,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );

    private record Mother(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureSneak,
            eEnemyRoleDifficulty.Hard,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );

    private record PMother(double penalty, double score):
        Phenotype(
            penalty,
            score,
            eEnemyGroupType.PureSneak,
            eEnemyRoleDifficulty.MiniBoss,
            eEnemyZoneDistribution.Force_One,
            1.0f
        );
}
