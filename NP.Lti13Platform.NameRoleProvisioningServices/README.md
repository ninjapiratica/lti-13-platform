# NP.Lti13Platform.NameRoleProvisioningServices

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0) spec defines a way that tools can request the names and roles of members of a context. This project provides an implementation of the spec.

## Features

- Returns the members of a context

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `ILti13NameRoleProvisioningDataService` interface:

```csharp
public class DataService: ILti13NameRoleProvisioningDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithLti13NameRoleProvisioningDataService<DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformNameRoleProvisioningServices();
```

## ILti13NameRoleProvisioningDataService

There is no default `ILti13NameRoleProvisioningDataService` implementation to allow each project to store the data how they see fit.

The `ILti13NameRoleProvisioningDataService` interface is used to get the persisted members of a context filtered by multiple parameters.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## OpenAPI Documenatation

Documentation for all endpoints are available through OpenAPI. This is normally configured through Swagger, NSwag or similar.

A 'group' has been given to all the endpoints. This can be used to apply to its own document or add it to an existing document. Below is an example using Swagger. The group name is found by the value in `NP.Lti13Platform.Core.Constants.OpenApi.GroupName`.

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new() { Title = "Public API", Version = "v1" });
    x.SwaggerDoc("v2", new() { Title = "LTI 1.3", Version = "v2" });

    x.DocInclusionPredicate((docName, apiDesc) =>
    {
        return docName == (apiDesc.GroupName ?? string.Empty) || (docName == "v2" && apiDesc.GroupName == NP.Lti13Platform.Core.Constants.OpenApi.GroupName);
    });
});

app.UseLti13PlatformNameRoleProvisioningServices();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "LTI 1.3 API");
});
```

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformNameRoleProvisioningServices()`.

```csharp
app.UseLti13PlatformNameRoleProvisioningServices(config => {
    config.NamesAndRoleProvisioningServicesUrl = "/lti13/{deploymentId}/{contextId}/memberships"; // {deploymentId} and {contextId} are required
    return config;
});
```

### ILti13NameRoleProvisioningConfigService

The `ILti13NameRoleProvisioningService` interface is used to get the config for the name and role provisioning service. The config is used to tell the tools how to request the members of a context.

There is a default implementation of the `ILti13NameRoleProvisioningConfigService` interface that uses a configuration set up on app start.
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

```csharp
builder.Services.Configure<ServicesConfig>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `ILti13NameRoleProvisioningConfigService` interface.
This may be useful if the service URL is dynamic or needs to be determined at runtime.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithLti13NameRoleProvisioningConfigService<ConfigService>();
```

## Configuration

`ServiceAddress`

The base url used to tell tools where the service is located.

## Member Message

The IMS [Name and Role Provisioning Services](https://www.imsglobal.org/spec/lti-nrps/v2p0#message-section) spec defines a way to give tools access to the parts of LTI messages that are specific to members. This project includes the specifics for the core message and known properties defined within the spec. Additional message can be added by calling `ExtendNameRoleProvisioningMessage` on startup. This follows the same pattern as [Populators](../NP.Lti13Platform.Core/README.md#populators) from the core spec. These messages should only contain the user specific message properties of the given message. Multiple populators may be added for the same interface and multiple interfaces may be added for the same <message_type>. Populators must be thread safe or have a Transient Dependency Injection strategy.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformNameRoleProvisioningServices()
    .WithDefaultNameRoleProvisioningService()
    .ExtendNameRoleProvisioningMessage<IMessage, MessagePopulator>("<message_type>");
```