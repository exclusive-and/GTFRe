
namespace GTFR.Genetic;

public class CopyMutation : IMutation
{
    private float probability;

    public CopyMutation(float probability)
    {
        this.probability = probability;
    }

    public void Mutate(Random random, Chromosome chromosome)
    {
        if (probability < random.NextDouble()) {
            return;
        }

        var m = random.Next(0, chromosome.Length - 2);
        var n = random.Next(m, chromosome.Length - 1);

        var p = chromosome.Slice(0, m);
        var q = chromosome.Slice(m, n);
        var r = chromosome.Slice(n, chromosome.Length);

        chromosome.Overwrite(p.Concat(q).Concat(q).Concat(r));
    }
}
