# NP.Lti13Platform.AssignmentGradeServices

The IMS [Assignment and Grade Services](https://www.imsglobal.org/spec/lti-ags/v2p0/) spec defines a way that tools can platforms can communicate grades back and forth. This project provides an implementation of the spec.

## Features

- Gets,creates, updates, and deletes line items
- Gets and creates grades

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `ILti13AssignmentGradeDataService` interface:

```csharp
public class DataService: ILti13AssignmentGradeDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformAssignmentGradeServices();

builder.Services.AddTransient<ILti13AssignmentGradeDataService, DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformAssignmentGradeServices();
```

## ILti13AssignmentGradeDataService

There is no default `ILti13AssignmentGradeDataService` implementation to allow each project to store the data how they see fit.

The `ILti13AssignmentGradeDataService` interface is used to manage the persistance of line items and grades.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformAssignmentGradeServices()`.

```csharp
app.UseLti13PlatformAssignmentGradeServices(config => {
    config.LineItemsUrl = "/lti13/{deploymentId}/{contextId}/lineItems"; // {deploymentId} and {contextId} are required
    config.LineItemUrl = "/lti13/{deploymentId}/{contextId}/lineItems/{lineItemId}"; // {deploymentId}, {contextId}, and {lineItemId} are required
    return config;
});
```

### ILti13AssignmentGradeConfigService

The `ILti13AssignmentGradeConfigService` interface is used to get the config for the assignment and grade service. The config is used to tell the tools how to request the members of a context.

There is a default implementation of the `ILti13AssignmentGradeConfigService` interface that uses a configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:AssignmentGradeServices`.
The Default implementation can be overridden by adding a new implementation of the `ILti13AssignmentGradeConfigService` interface.
This may be useful if the service URL is dynamic or needs to be determined at runtime.

```json
{
    "Lti13Platform": {
        "AssignmentGradeServices": {
            "ServiceAddress": "https://<mysite>"
        }
    }
}
```

## Configuration

`ServiceAddress`

The base url used to tell tools where the service is located.
