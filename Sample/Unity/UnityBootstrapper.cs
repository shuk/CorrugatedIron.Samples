using CorrugatedIron;
using Microsoft.Practices.Unity;

namespace Sample.Unity
{
    public static class UnityBootstrapper
    {
        public static IUnityContainer Bootstrap()
        {
            // pull the configuration straight out of the app.config file using the appropriate section name
            var cluster = RiakCluster.FromConfig("riakConfig");

            var container = new UnityContainer();

            // register the default cluster (single instance)
            container.RegisterInstance<IRiakEndPoint>(cluster);

            // register the client creator (multiple instance)
            container.RegisterType<IRiakClient>(new InjectionFactory(c => c.Resolve<IRiakEndPoint>().CreateClient()));

            return container;
        }
    }
}
