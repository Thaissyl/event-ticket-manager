namespace EventTickets.API.Endpoints;

public static class EndpointExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapEvents();
        app.MapAuth();
        app.MapOrders();
        app.MapTicketTierEndpoints();
        app.MapCartEndpoints();
        app.MapPaymentEndpoints();
        app.MapTicketEndpoints();
        app.MapCheckinEndpoints();
        app.MapAnalyticsEndpoints();
        app.MapAdminEndpoints();
    }
}
