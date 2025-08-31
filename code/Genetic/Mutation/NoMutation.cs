
namespace GTFR.Genetic;

public class NoMutation : IMutation
{
    public NoMutation() {}

    public void Mutate(Random random, Chromosome chromosome)
    {
        return;
    }
}
