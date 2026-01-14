# NP.Lti13Platform.AssignmentGradeServices

The IMS [Assignment and Grade Services](https://www.imsglobal.org/spec/lti-ags/v2p0/) spec defines a way that tools and platforms can communicate grades back and forth. This project provides an implementation of the spec.

## Features

- Creates, retrieves, updates, and deletes line items
- Creates and retrieves grades
- Manages grade submissions and results

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `IAssignmentGradeDataService` interface:

```csharp
public class AssignmentGradeDataService: IAssignmentGradeDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformAssignmentGradeServices<AssignmentGradeDataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformAssignmentGradeServices();
```

## IAssignmentGradeDataService

There is no default `IAssignmentGradeDataService` implementation to allow each project to store the data how they see fit.

The `IAssignmentGradeDataService` interface is used to manage the persistence of line items and grades in the platform.

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

### IAssignmentGradeConfigService

The `IAssignmentGradeConfigService` interface is used to get the configuration for the assignment and grade service. The config is used to tell tools where to submit and retrieve grades.

There is a default implementation of the `IAssignmentGradeConfigService` interface that uses configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:AssignmentGradeServices`.

Examples:

```json
{
    "Lti13Platform": {
        "AssignmentGradeServices": {
            "ServiceAddress": "https://<mysite>"
        }
    }
}
```

OR

```csharp
builder.Services.Configure<ServicesConfig>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `IAssignmentGradeConfigService` interface.
This may be useful if the service URL is dynamic or needs to be determined at runtime.

```csharp
builder.Services
    .AddLti13PlatformCore<CoreDataService>()
    .AddPlatformAssignmentGradeServices<AssignmentGradeDataService>()
    .WithAssignmentGradeConfigService<CustomConfigService>();
```

## Configuration

`ServiceAddress`

The base URL used to tell tools where the service is located.