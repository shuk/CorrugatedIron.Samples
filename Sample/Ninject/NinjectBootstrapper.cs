using CorrugatedIron;
using Ninject;

namespace Sample.Ninject
{
    public static class NinjectBootstrapper
    {
        public static IKernel Bootstrap()
        {
            // pull the configuration straight out of the app.config file using the appropriate section name
            var cluster = RiakCluster.FromConfig("riakConfig");
            var container = new StandardKernel();

            // register the default cluster (single instance)
            container.Bind<IRiakEndPoint>().ToConstant(cluster);

            // register the client creator (multiple instance)
            container.Bind<IRiakClient>().ToMethod(ctx => container.Get<IRiakEndPoint>().CreateClient());

            return container;
        }
    }
}
