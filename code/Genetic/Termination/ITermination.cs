namespace GTFR.Genetic;

public interface ITermination
{
    public bool IsGoodEnough(IList<Chromosome> chromosomes);
}
