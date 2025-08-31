
namespace GTFR.Genetic;

public class NoCrossover : ICrossover
{
    public NoCrossover() {}

    public (IEnumerable<Chromosome>, IEnumerable<Chromosome>)
        Cross(
            Random random,
            IEnumerable<Chromosome> progenitors)
    {
        return (progenitors, new List<Chromosome>());
    }
}
