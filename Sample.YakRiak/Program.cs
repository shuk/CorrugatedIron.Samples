using CorrugatedIron;

namespace Sample.YakRiak
{
    class Program
    {
        static void Main(string[] args)
        {
            var cluster = RiakCluster.FromConfig("riakConfig");
            var client = cluster.CreateClient();

            var yak = new YakRiak(client);
            yak.Run();

            cluster.Dispose();
        }
    }
}
