using Autofac;
using CorrugatedIron;

namespace Sample.Autofac
{
    public static class AutofacBootstrapper
    {
        public static IContainer Bootstrap()
        {
            // pull the configuration straight out of the app.config file using the appropriate section name
            var cluster = RiakCluster.FromConfig("riakConfig");

            var builder = new ContainerBuilder();

            // register the default cluster (single instance)
            builder.RegisterInstance(cluster).As<IRiakEndPoint>().SingleInstance();

            // register the client creator (multiple instance)
            builder.Register(c => c.Resolve<IRiakEndPoint>().CreateClient()).As<IRiakClient>();

            return builder.Build();
        }
    }
}
