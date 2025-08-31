namespace GTFR.Genetic;

public interface IMutation
{
    public void Mutate(Random random, Chromosome chromosome);
}