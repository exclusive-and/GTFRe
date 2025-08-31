
namespace GTFR.Genetic;

public class FlipMutation : IMutation
{
    private float probability;

    public FlipMutation(float probability)
    {
        this.probability = probability;
    }

    public void Mutate(Random random, Chromosome chromosome)
    {
        if (this.probability < random.NextDouble()) {
            return;
        }

        var m = random.Next(0, chromosome.Length - 2);
        var n = random.Next(m, chromosome.Length - 1);

        for (int i = m; i < n; i++) {
            chromosome.Genes[i] ^= true;
        }
    }
}
