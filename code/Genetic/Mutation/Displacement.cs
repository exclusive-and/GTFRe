
namespace GTFR.Genetic;

public class Displacement : IMutation
{
    private float probability;

    public Displacement(float probability)
    {
        this.probability = probability;
    }

    public void Mutate(Random random, Chromosome chromosome)
    {
        if (probability < random.NextDouble()) {
            return;
        }

        var i = random.Next(0, chromosome.Length - 2);
        var j = random.Next(i, chromosome.Length - 1);

        var p = chromosome.Slice(0, i);
        var q = chromosome.Slice(i, j);
        var r = chromosome.Slice(j, chromosome.Length);

        var s = p.Concat(r);
        var k = random.Next(0, s.Length);

        var t = s.Slice(0, k);
        var u = s.Slice(k, s.Length);

        chromosome.Overwrite(t.Concat(q).Concat(u));
    }
}
