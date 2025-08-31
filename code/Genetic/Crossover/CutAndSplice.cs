namespace GTFR.Genetic;

public class CutAndSplice : ICrossover
{
    private readonly float probability;

    public CutAndSplice(float probability)
    {
        this.probability = probability;
    }

    public (IEnumerable<Chromosome>, IEnumerable<Chromosome>)
        Cross(
            Random random,
            IEnumerable<Chromosome> progenitors)
    {
        if (random.NextDouble() > probability) {
            return (new List<Chromosome>(), progenitors.Skip(2));
        }

        var parents = progenitors.Take(2).ToList();
        var p1 = parents[0];
        var p2 = parents[1];

        var m = random.Next(1, p1.Length) + 1;
        var n = random.Next(1, p2.Length) + 1;

        var offspring1 = CreateOffspring(p1, m, p2, n);
        var offspring2 = CreateOffspring(p2, n, p1, m);

        return (new List<Chromosome> {offspring1, offspring2}, progenitors.Skip(2));
    }

    private static Chromosome CreateOffspring(Chromosome p1, int m, Chromosome p2, int n)
    {
        return p1.Slice(0, m).Concat(p2.Slice(n, p2.Length));
    }
}
