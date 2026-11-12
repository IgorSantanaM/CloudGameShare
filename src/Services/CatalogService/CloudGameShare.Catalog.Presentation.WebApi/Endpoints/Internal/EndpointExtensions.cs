namespace CloudGameShare.Catalog.Presentation.WebApi.Endpoints.Internal;

internal static class EndpointExtensions
{
    public static void UseEndpoints<TMarker>(this WebApplication app)
    {
        var endpointTypes = typeof(TMarker).Assembly.DefinedTypes
            .Where(type => !type.IsAbstract && typeof(IEndpoint).IsAssignableFrom(type));

        foreach (var type in endpointTypes)
        {
            type.GetMethod(nameof(IEndpoint.DefineEndpoints))!.Invoke(null, [app]);
        }
    }
}
