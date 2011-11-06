using System.Configuration;

namespace Sample.SessionStateProvider
{
    class RiakSessionStateConfiguration : ConfigurationSection
    {
        public static RiakSessionStateConfiguration LoadFromConfig(string sectionName)
        {
            return (RiakSessionStateConfiguration)ConfigurationManager.GetSection(sectionName);
        }

        [ConfigurationProperty("timeout_ms", IsDefaultCollection = true, IsRequired = true)]
        public int TimeoutInMilliseconds
        {
            get { return (int) this["timeout_in_ms"]; }
            set { this["timeout_in_ms"] = value;  }
        }
    }
}
