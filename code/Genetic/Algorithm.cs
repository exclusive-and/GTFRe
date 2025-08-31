namespace GTFR.Genetic;

public sealed class Algorithm
{
    private readonly int MinPopulation;

    private readonly int MaxIterations;

    private readonly int NumElite;

    private readonly IFitness fitness;

    private readonly ICrossover crossover;

    private readonly IMutation mutation;

    private readonly ITermination termination;

    public Algorithm(
        int minPopulation,
        int maxIterations,
        int numElite,
        IFitness fitness,
        ICrossover crossover,
        IMutation mutation,
        ITermination termination)
    {
        this.MinPopulation = minPopulation;
        this.MaxIterations = maxIterations;
        this.NumElite = numElite;
        
        this.fitness = fitness;
        this.crossover = crossover;
        this.mutation = mutation;
        this.termination = termination;
    }

    public IList<Chromosome> Run(Random random, List<Chromosome> initial)
    {
        var chromosomes = new List<Chromosome>(MinPopulation);

        foreach (var x in initial) {
            chromosomes.Add(x);
        }

        for (int i = 0; i < MaxIterations; i++) {
            chromosomes = RunOnce(random, chromosomes).ToList();
            if (termination.IsGoodEnough(chromosomes)) {
                return chromosomes;
            }
        }

        return chromosomes;
    }

    public IList<Chromosome> RunOnce(Random random, IList<Chromosome> progenitors)
    {
        var shuffled = Shuffle(random, progenitors);
        var offspring = Crossovers(random, shuffled);
        Mutations(random, offspring);
        return Reinsertions(random, offspring, progenitors);
    }

    private IList<Chromosome>
        Crossovers(
            Random random,
            IEnumerable<Chromosome> progenitors)
    {
        var offspring = new List<Chromosome>(MinPopulation);

        while (progenitors.Any()) {
            var (children, rest) = crossover.Cross(random, progenitors);
            progenitors = rest;
            offspring.AddRange(children);
        }

        return offspring;
    }

    private void Mutations(Random random, IList<Chromosome> chromosomes)
    {
        foreach (var chromosome in chromosomes) {
            mutation.Mutate(random, chromosome);
        }
    }

    private IList<Chromosome>
        Reinsertions(
            Random random,
            IList<Chromosome> offspring,
            IList<Chromosome> progenitors)
    {
        var elite = progenitors.Take(NumElite);
        var sorted = offspring.Concat(elite).OrderByDescending(fitness.Evaluate);
        return sorted.Take(MinPopulation).ToList();
    }

    private IList<Chromosome> Shuffle(Random random, IList<Chromosome> xs)
    {
        var ys = new List<Chromosome>(xs);

        for (int i = 0; i < ys.Count; i++) {
            var j = random.Next(0, ys.Count);
            var t = ys[i];
            ys[i] = ys[j];
            ys[j] = t;
        }

        return ys;
    }
}
