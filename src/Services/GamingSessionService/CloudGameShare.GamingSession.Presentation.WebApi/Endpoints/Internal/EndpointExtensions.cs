namespace CloudGameShare.GamingSession.Presentation.WebApi.Endpoints.Internal;

internal static class EndpointExtensions
{
    public static void UseEndpoints<TMarker>(this WebApplication app)
    {
        foreach (var type in typeof(TMarker).Assembly.DefinedTypes.Where(type =>
            !type.IsAbstract && typeof(IEndpoint).IsAssignableFrom(type)))
        {
            type.GetMethod(nameof(IEndpoint.DefineEndpoints))!.Invoke(null, [app]);
        }
    }
}
