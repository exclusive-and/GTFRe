namespace GTFR.Genetic;

public class Fitness : IFitness
{
    private readonly Func<Chromosome, double> func;

    public Fitness(Func<Chromosome, double> func)
    {
        this.func = func;
    }

    public double Evaluate(Chromosome chromosome) => func(chromosome);
}
