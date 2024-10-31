# NP.Lti13Platform.NameRoleProvisioningServices

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0) spec defines a way that tools can request the names and roles of members of a context. This project provides an implementation of the spec.

## Features

- Returns the members of a context

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `INameRoleProvisioningDataService` interface:

```csharp
public class DataService: INameRoleProvisioningDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithDefaultNameRoleProvisioningService();

builder.Services.AddTransient<INameRoleProvisioningDataService, DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformNameRoleProvisioningServices();
```

## INameRoleProvisioningDataService

There is no default `INameRoleProvisioningDataService` implementation to allow each project to store the data how they see fit.

The `INameRoleProvisioningDataService` interface is used to get the persisted members of a context filtered by multiple parameters.

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

### INameRoleProvisioningService

The `INameRoleProvisioningService` interface is used to get the config for the name and role provisioning service. The config is used to tell the tools how to request the members of a context.

There is a default implementation of the `INameRoleProvisioningService` interface that uses a configuration set up on app start. When calling the `WithDefaultNameRoleProvisioningService` method, the configuration can be setup at that time. A fallback to the current request scheme and host will be used if no ServiceEndpoint is configured. The Default implementation can be overridden by adding a new implementation of the `INameRoleProvisioningService` interface and not including the Default. This may be useful if the service URL is dynamic or needs to be determined at runtime.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithDefaultNameRoleProvisioningService(x => { x.ServiceAddress = new Uri("https://<mysite>") });
```

## Member Message

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0#message-section) spec defines a way to give tools access to the parts of LTI messages that are specific to members. This project includes the specifics for the core message and known properties defined within the spec. Additional message can be added by calling `ExtendNameRoleProvisioningMessage` on startup. This follows the same pattern as [Populators](../NP.Lti13Platform.Core/README.md#populators) from the core spec. These messages should only contain the user specific message properties of the given message. Multiple populators may be added for the same interface and multiple interfaces may be added for the same <message_type>.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithDefaultNameRoleProvisioningService()
    .ExtendNameRoleProvisioningMessage<IMessage, MessagePopulator>("<message_type>");
```