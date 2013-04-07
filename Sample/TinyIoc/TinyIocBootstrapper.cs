using CorrugatedIron;
using TinyIoC;

namespace Sample.TinyIoc
{
    public static class TinyIocBootstrapper
    {
        public static TinyIoCContainer Bootstrap()
        {
            // pull the configuration straight out of the app.config file using the appropriate section name
            var cluster = RiakCluster.FromConfig("riakConfig");

            var container = TinyIoCContainer.Current;

            // register the default cluster (single instance)
            container.Register<IRiakEndPoint>(cluster);

            // register the client creator (multiple instance)
            container.Register<IRiakClient>((c, np) => c.Resolve<IRiakEndPoint>().CreateClient());

            return container;
        }
    }
}
