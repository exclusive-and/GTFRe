namespace GTFR.Genetic;

public interface ICrossover
{
    (IEnumerable<Chromosome>, IEnumerable<Chromosome>)
        Cross(
            Random random,
            IEnumerable<Chromosome> progenitors);
}
