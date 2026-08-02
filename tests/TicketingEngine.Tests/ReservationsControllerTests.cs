using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Api.Controllers;
using Xunit;

namespace TicketingEngine.Tests;

public class ReservationsControllerTests
{
    [Fact]
    public void Reserve_ShouldUseEventIdRouteParam_WithoutGuidConstraint()
    {
        var reserveAction = typeof(ReservationsController).GetMethod(nameof(ReservationsController.Reserve))!;
        var httpPostAttribute = reserveAction.GetCustomAttribute<HttpPostAttribute>();

        Assert.NotNull(httpPostAttribute);
        Assert.Equal("{eventId}/reserve", httpPostAttribute.Template);

        var eventIdParameter = reserveAction.GetParameters().Single(p => p.Name == "eventId");
        Assert.Equal(typeof(string), eventIdParameter.ParameterType);
        Assert.Null(eventIdParameter.GetCustomAttribute<FromQueryAttribute>());
    }

    [Fact]
    public void GetAvailability_ShouldUseEventIdRouteParam_WithoutGuidConstraint()
    {
        var availabilityAction = typeof(ReservationsController).GetMethod(nameof(ReservationsController.GetAvailability))!;
        var httpGetAttribute = availabilityAction.GetCustomAttribute<HttpGetAttribute>();

        Assert.NotNull(httpGetAttribute);
        Assert.Equal("{eventId}/availability", httpGetAttribute.Template);

        var eventIdParameter = availabilityAction.GetParameters().Single(p => p.Name == "eventId");
        Assert.Equal(typeof(string), eventIdParameter.ParameterType);
        Assert.Null(eventIdParameter.GetCustomAttribute<FromQueryAttribute>());
    }

    [Fact]
    public void Controller_ShouldBeVersioned_WithApiVersion1()
    {
        var controllerType = typeof(ReservationsController);
        var controllerVersion = controllerType.GetCustomAttribute<ApiVersionAttribute>();
        var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(controllerVersion);
        Assert.Equal("1.0", controllerVersion.Versions.Single().ToString());

        Assert.NotNull(routeAttribute);
        Assert.Equal("api/events", routeAttribute.Template);
    }
}
