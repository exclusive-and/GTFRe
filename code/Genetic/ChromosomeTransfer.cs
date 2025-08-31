namespace GTFR.Genetic;

using BepInEx.Logging;
using GTFO.API;
using SNetwork;

public class ChromosomeTransfer
{
    private TransferImpl transferImpl;
    private Action<ulong, byte[]> await;

    public ChromosomeTransfer()
    {
        transferImpl = new TransferImpl();

        await = (_, data) =>
        {
            transferImpl.transferTask.SetResult(Chromosome.FromBytes(data));
        };

        NetworkAPI.RegisterFreeSizedEvent("awaitDNA", await);
    }

    private class TransferImpl
    {
        public TaskCompletionSource<Chromosome> transferTask;

        public TransferImpl()
        {
            transferTask = new TaskCompletionSource<Chromosome>();
        }
    }

    public Chromosome Result
    {
        get => transferImpl.transferTask.Task.Result;
    }

    public void Wait()
    {
        transferImpl.transferTask.Task.Wait();
    }

    public void Reset()
    {
        transferImpl.transferTask = new TaskCompletionSource<Chromosome>();
    }
}
