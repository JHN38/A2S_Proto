namespace A2S_Proto;

public static class A2S_ProtoHostBuilderExtensions
{
    public static IHostBuilder UseA2S_Proto(this IHostBuilder builder)
    {
        return builder.ConfigureServices((app, services) =>
        {
            services.AddHttpClient<GetServerListCall>();
        });
    }
}