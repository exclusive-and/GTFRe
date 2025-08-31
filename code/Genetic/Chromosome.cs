namespace GTFR.Genetic;

public class Chromosome
{
    private bool[] genes;

    // Create a new default chromosome of the desired length.

    public Chromosome(int length)
    {
        genes = new bool[length];
    }

    public Chromosome(bool[] genes)
    {
        this.genes = new bool[genes.Length];
        Array.Copy(this.genes, genes, genes.Length);
    }

    // Create a new chromosome by copying an existing one.

    public Chromosome(Chromosome other)
    {
        genes = new bool[other.Length];
        Array.Copy(this.genes, other.genes, other.Length);
    }

    public static Chromosome MakeRandom(Random random, int minLength, int maxLength)
    {
        var length = random.Next(minLength, maxLength);
        var genes = new bool[length];

        for (int i = 0; i < length; i++) {
            genes[i] = random.Next(0, 2) > 0;
        }

        return new Chromosome(genes);
    }

    public string Show()
    {
        var size = (float) genes.Length / 8.0f;
        int magnitude = 0;

        while (size > 1024.0f) {
            magnitude++;
            size /= 1024.0f;
        }

        var suffix = magnitude switch {
            0 => "B",
            1 => "KiB",
            2 => "MiB",
            3 => "GiB",
            4 => "TiB",
            _ => "",
        };

        var a = "";

        a += "--- BEGIN CHROMOSOME ---\n";
        a += "(" + size.ToString("N2") + " " + suffix + ")\n";

        for (int i = 0; i < genes.Length; i += 256) {
            for (int j = 0; j < 256 && i + j < genes.Length; j++) {
                a += genes[i + j] ? '1' : '0';
            }
            a += "\n";
        }

        a += "--- END CHROMOSOME ---";

        return a;
    }

    public bool this[int index] => this.genes[index];

    public bool[] Genes
    {
        get => this.genes;
    }

    public void Overwrite(Chromosome other)
    {
        this.genes = other.genes;
    }

    // Get the length of the chromosome (i.e. the number of bits that make it up).

    public int Length
    {
        get => this.genes.Length;
    }

    public Chromosome Concat(Chromosome other)
    {
        var res = new Chromosome(this.Length + other.Length);
        Array.Copy(this.genes, res.genes, this.Length);
        Array.Copy(other.genes, 0, res.genes, this.Length, other.Length);
        return res;
    }

    public Chromosome Slice(float p, float q)
    {
        var m = (int) Math.Round(p * this.Length);
        var n = (int) Math.Round(q * this.Length);
        return Slice(m, n);
    }

    public Chromosome Slice(int start, int end)
    {
        if (end < start) {
            throw new Exception("invalid slice");
        }
        return SliceN(start, end - start);
    }

    public Chromosome SliceN(int start, int length)
    {
        var res = new Chromosome(length);
        Array.Copy(this.genes, start, res.genes, 0, length);
        return res;
    }

    public byte[] ToBytes()
    {
        var bytes = new byte[genes.Length];

        for (int i = 0; i < genes.Length; i++) {
            bytes[i] = (byte) (genes[i] ? 0xff : 0x00);
        }

        return bytes;
    }

    public static Chromosome FromBytes(byte[] bytes)
    {
        var bits = new bool[bytes.Length];

        for (int i = 0; i < bytes.Length; i++) {
            bits[i] = bytes[i] > 0 ? true : false;
        }

        return new Chromosome(bits);
    }
}
