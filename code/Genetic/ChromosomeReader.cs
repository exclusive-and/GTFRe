namespace GTFR.Genetic;

using Prelude;

public class ChromosomeReader
{
    private readonly Chromosome chromosome;
    private int i;

    public ChromosomeReader(Chromosome chromosome)
    {
        this.chromosome = chromosome;
        this.i = 0;
    }

    public bool IsDone() => !HasRemaining();

    public bool HasRemaining() => HasRemaining(0);

    public bool HasRemaining(int n) => i + n < chromosome.Length;

    public Maybe<string> Next(int n)
    {
        if (!HasRemaining(n)) {
            return new Nothing<string>();
        }

        var a = "";

        for (int j = 0; j < n; j++) {
            a += chromosome[i++] ? '1' : '0';
        }

        return new Just<string>(a);
    }

    public Maybe<T> Match<T>(IEnumerable<(string, T)> alts)
    {
        var a = "";

        Func<(string, T), bool> isMatch =
            (x) => x switch {
                (var pat, _) => pat.StartsWith(a)
            };

        while (true) {
            if (!HasRemaining()) {
                return new Nothing<T>();
            }

            a += chromosome[i++] ? '1' : '0';

            var xs = alts.Where(isMatch);

            if (!xs.Any()) {
                return new Nothing<T>();
            }

            foreach (var (pat, r) in xs) {
                if (pat == a) {
                    return new Just<T>(r);
                }
            }
        }
    }
}
