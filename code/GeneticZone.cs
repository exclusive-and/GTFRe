namespace GTFR.Genetic;

using BepInEx.Logging;
using GameData;
using IL2 = Il2CppSystem.Collections.Generic;

public class GeneticZone
{
    public List<(string name, GeneticEnemies.Phenotype)> enemies;

    public List<(string name, GeneticEnemies.Hunter)> bloodDoors;

    public GeneticZone()
    {
        this.enemies = new List<(string, GeneticEnemies.Phenotype)>();
        this.bloodDoors = new List<(string, GeneticEnemies.Hunter)>();
    }

    public string Show()
    {
        var names = new List<string>();
        names.AddRange(enemies.Select(x => x.name));
        names.AddRange(bloodDoors.Select(x => x.name));

        return "[" + string.Join(", ", names) + "]";
    }

    public double Evaluate()
    {
        return EvalEnemies();
    }

    public double EvalEnemies()
    {
        double penalty = 1.0;
        double total = 0.0;

        foreach (var (_, enemy) in enemies) {
            total   += enemy.score;
            penalty *= enemy.penalty;
        }
        
        foreach (var (_, hunter) in bloodDoors) {
            total   += hunter.score;
            penalty *= hunter.penalty;
        }

        return 1.0 / (1.0 + Math.Exp(-penalty));
        // return ;
    }

    public void SetupZone(ManualLogSource log, ExpeditionZoneData zone)
    {
        var sleepers = new IL2.List<EnemySpawningData>();
        foreach (var (_, enemy) in enemies) {
            var sleeper = new EnemySpawningData();
            sleeper.GroupType = enemy.group;
            sleeper.Difficulty = enemy.role;
            sleeper.Distribution = enemy.distribution;
            sleeper.DistributionValue = enemy.distributionValue;
            sleepers.Add(sleeper);
        }
        zone.EnemySpawningInZone = sleepers;

        if (bloodDoors.Count > 1) {
            var (_, hunter) = bloodDoors[0];
            zone.ActiveEnemyWave.HasActiveEnemyWave = true;
            zone.ActiveEnemyWave.EnemyGroupInfrontOfDoor = (uint) hunter.persistentID;
        }

        if (bloodDoors.Count > 2) {
            var (_, hunter) = bloodDoors[1];
            zone.ActiveEnemyWave.EnemyGroupInArea = (uint) hunter.persistentID;
            zone.ActiveEnemyWave.EnemyGroupsInArea = 2;
        }
    }
}
