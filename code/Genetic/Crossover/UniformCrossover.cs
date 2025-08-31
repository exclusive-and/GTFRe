namespace GTFR.Genetic;

public class UniformCrossover : ICrossover
{
    private readonly float probability;

    public UniformCrossover(float probability)
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

        var offspring1 = new Chromosome(p1);
        var offspring2 = new Chromosome(p2);

        for (int i = 0; i < Math.Min(p1.Length, p2.Length); i++) {
            var (x, y) = random.Next(0, 2) > 0 ? (p2[i], p1[i]) : (p1[i], p2[i]);
            offspring1.Genes[i] = x;
            offspring2.Genes[i] = y;
        }

        return (new List<Chromosome> {offspring1, offspring2}, progenitors.Skip(2));
    }
}
