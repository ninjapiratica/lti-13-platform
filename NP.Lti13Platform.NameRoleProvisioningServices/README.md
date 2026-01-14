# NP.Lti13Platform.NameRoleProvisioningServices

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0) spec defines a way that tools can request the names and roles of members of a context. This project provides an implementation of the spec.

## Features

- Returns the members of a context
- Supports membership filters and pagination
- Retrieves user information and roles

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `INameRoleProvisioningDataService` interface:

```csharp
public class NameRoleProvisioningDataService: INameRoleProvisioningDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformNameRoleProvisioningServices<NameRoleProvisioningDataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformNameRoleProvisioningServices();
```

## INameRoleProvisioningDataService

There is no default `INameRoleProvisioningDataService` implementation to allow each project to store the data how they see fit.

The `INameRoleProvisioningDataService` interface is used to retrieve members of a context with support for filtering by role, status, and pagination.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformNameRoleProvisioningServices()`.

```csharp
app.UseLti13PlatformNameRoleProvisioningServices(config => {
    config.NamesAndRoleProvisioningServicesUrl = "/lti13/{deploymentId}/{contextId}/memberships"; // {deploymentId} and {contextId} are required
    return config;
});
```

### INameRoleProvisioningConfigService

The `INameRoleProvisioningConfigService` interface is used to get the configuration for the name and role provisioning service. The config is used to tell tools how to request the members of a context.

There is a default implementation of the `INameRoleProvisioningConfigService` interface that uses configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:NameRoleProvisioningServices`.

Examples:

```json
{
    "Lti13Platform": {
        "NameRoleProvisioningServices": {
            "ServiceAddress": "https://<mysite>"
        }
    }
}
```

OR

```csharp
builder.Services.Configure<ServicesConfig>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `INameRoleProvisioningConfigService` interface.
This may be useful if the service URL is dynamic or needs to be determined at runtime.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformNameRoleProvisioningServices<NameRoleProvisioningDataService>()
    .WithNameRoleProvisioningConfigService<CustomConfigService>();
```

### INameRoleProvisioningServicesMessageExtension

The `INameRoleProvisioningServicesMessageExtension` interface allows for adding custom extensions to name and role provisioning service messages.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformNameRoleProvisioningServices<NameRoleProvisioningDataService>()
    .WithNameRoleProvisioningServicesMessageExtension<CustomExtension>();
```

## Configuration

`ServiceAddress`

The base URL used to tell tools where the service is located.

***

`SupportMembershipDifferences` Default: `true`

Boolean indicating if the service supports membership differences. If `true`, it is expected the 'asOfDate' parameter in the `GetMemberships` data service will be used. If historical membership is not supported, this value should be set to `false`. 

## Message Extensions

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0#message-section) spec defines member-specific claims within LTI messages. This project provides support for accessing these member-specific claims in resource link messages. Additional extensions can be added by implementing `INameRoleProvisioningServicesMessageExtension`.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformNameRoleProvisioningServices<NameRoleProvisioningDataService>()
    .WithNameRoleProvisioningServicesMessageExtension<CustomMessageExtension>();
```