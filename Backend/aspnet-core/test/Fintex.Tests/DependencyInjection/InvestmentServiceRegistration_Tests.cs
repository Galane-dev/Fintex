using Abp.Application.Services;
using Abp.Dependency;
using Fintex;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Fintex.Tests.DependencyInjection;

public class InvestmentServiceRegistration_Tests : FintexTestBase
{
    [Fact]
    public void Should_Register_All_Custom_Investment_Services()
    {
        var assembly = typeof(FintexApplicationModule).Assembly;
        var serviceTypes = assembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                type.IsPublic &&
                !type.IsGenericTypeDefinition &&
                type.Namespace != null &&
                type.Namespace.StartsWith("Fintex.Investments", StringComparison.Ordinal) &&
                (typeof(ITransientDependency).IsAssignableFrom(type) ||
                 typeof(IApplicationService).IsAssignableFrom(type)) &&
                (type.Name.EndsWith("Service", StringComparison.Ordinal) ||
                 type.Name.EndsWith("AppService", StringComparison.Ordinal)))
            .OrderBy(type => type.FullName)
            .ToList();

        serviceTypes.Count.ShouldBeGreaterThan(0);

        foreach (var serviceType in serviceTypes)
        {
            LocalIocManager.IsRegistered(serviceType)
                .ShouldBeTrue($"Expected {serviceType.FullName} to be registered in the IoC container.");
        }
    }
}
