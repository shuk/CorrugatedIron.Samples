using CorrugatedIron;
using StructureMap;

namespace Sample.Structuremap
{
    public static class StructuremapBootstrapper
    {
        public static IContainer Bootstrap()
        {
            // pull the configuration straight out of the app.config file using the appropriate section name
            var cluster = RiakCluster.FromConfig("riakConfig");

            var container = new Container(expr =>
                {
                    // register the default cluster (single instance)
                    expr.For<IRiakEndPoint>().Singleton().Add(cluster);

                    // register the client creator (multiple instance)
                    expr.For<IRiakClient>().Use(ctx => ctx.GetInstance<IRiakEndPoint>().CreateClient());
                });

            return container;
        }
    }
}
