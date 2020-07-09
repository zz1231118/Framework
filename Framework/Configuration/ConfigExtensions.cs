namespace Framework.Configuration
{
    public static class ConfigExtensions
    {
        public static Config SafeWithFallback(this Config config, Config fallback)
        {
            return config == null
                ? fallback
                : ReferenceEquals(config, fallback)
                    ? config
                    : config.WithFallback(fallback);
        }

        public static bool IsNullOrEmpty(this Config config)
        {
            return config == null || config.IsEmpty;
        }
    }
}