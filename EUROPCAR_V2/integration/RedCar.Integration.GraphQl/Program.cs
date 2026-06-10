using RedCar.Integration.GraphQl.Graph;
using RedCar.Integration.GraphQl.Services;
using RedCar.Shared.Auth;
using RedCar.Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MicroserviciosGatewaySettings>(
    builder.Configuration.GetSection(MicroserviciosGatewaySettings.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(nameof(MsHttpGateway));
builder.Services.AddScoped<MsHttpGateway>();

builder.Services.AddRedCarJwt(builder.Configuration);

builder.Services
    .AddGraphQLServer()
    .AddQueryType<BookingQuery>()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = builder.Environment.IsDevelopment());

builder.Services.AddRedCarMassTransit(builder.Configuration, "integration-graphql");

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/graphql");
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/info", () => Results.Ok(new
{
    service = "RedCar.Integration.GraphQl",
    role = "Unified GraphQL gateway over existing MS REST APIs",
    graphql = "/graphql"
}));

app.Run();

public partial class Program;
