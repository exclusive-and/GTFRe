using Il2CppSystem.Runtime.Remoting.Messaging;

namespace GTFR.Genetic;

public class OnePointCrossover : ICrossover
{
    private readonly float probability;

    public OnePointCrossover(float probability)
    {
        this.probability = probability;
    }

    public (IEnumerable<Chromosome>, IEnumerable<Chromosome>)
        Cross(
            Random random,
            IEnumerable<Chromosome> progenitors)
    {
        if (probability < random.NextDouble()) {
            return (new List<Chromosome>(), progenitors.Skip(2));
        }

        var parents = progenitors.Take(2).ToList();
        var p1 = parents[0];
        var p2 = parents[1];

        var m = random.Next(0, Math.Min(p1.Length, p2.Length));

        var offspring1 = CreateOffspring(p1, m, p2);
        var offspring2 = CreateOffspring(p2, m, p1);

        return (new List<Chromosome> {offspring1, offspring2}, progenitors.Skip(2));
    }

    private Chromosome CreateOffspring(Chromosome p1, int m, Chromosome p2)
    {
        return p1.Slice(0, m).Concat(p2.Slice(m, p2.Length));
    }
}
