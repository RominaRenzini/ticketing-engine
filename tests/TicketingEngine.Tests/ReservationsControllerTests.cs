using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Api.Controllers;
using Xunit;

namespace TicketingEngine.Tests;

public class ReservationsControllerTests
{
    [Fact]
    public void Controller_ShouldUseQueryStringVersioningAndAvoidEventIdInRoute()
    {
        var controllerType = typeof(ReservationsController);
        var controllerVersion = controllerType.GetCustomAttribute<ApiVersionAttribute>();
        var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
        var reserveAction = controllerType.GetMethod(nameof(ReservationsController.Reserve))!;
        var httpPostAttribute = reserveAction.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(controllerVersion);
        Assert.Equal("1.0", controllerVersion.Versions.Single().ToString());

        Assert.NotNull(routeAttribute);
        Assert.Equal("api/events", routeAttribute.Template);

        Assert.NotNull(httpPostAttribute);
        Assert.Equal("reserve", httpPostAttribute.Template);

        var eventIdParameter = reserveAction.GetParameters().Single(parameter => parameter.Name == "eventId");
        Assert.NotNull(eventIdParameter.GetCustomAttribute<FromQueryAttribute>());
        Assert.Null(eventIdParameter.GetCustomAttribute<FromRouteAttribute>());
    }
}
