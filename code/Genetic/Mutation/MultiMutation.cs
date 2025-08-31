
namespace GTFR.Genetic;

public class MultiMutation : IMutation
{
    private IEnumerable<IMutation> mutations;

    public MultiMutation(IEnumerable<IMutation> mutations)
    {
        this.mutations = mutations;
    }

    public void Mutate(Random random, Chromosome chromosome)
    {
        foreach (var mutation in mutations) {
            mutation.Mutate(random, chromosome);
        }
    }
}
