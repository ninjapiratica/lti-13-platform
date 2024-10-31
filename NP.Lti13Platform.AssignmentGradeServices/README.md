# NP.Lti13Platform.AssignmentGradeServices

The IMS [Assignment and Grade Services](https://www.imsglobal.org/spec/lti-ags/v2p0/) spec defines a way that tools can platforms can communicate grades back and forth. This project provides an implementation of the spec.

## Features

- Gets,creates, updates, and deletes line items
- Gets and creates grades

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `IAssignmentGradeDataService` interface:

```csharp
public class DataService: IAssignmentGradeDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformAssignmentGradeServices()
    .WithDefaultAssignmentGradeService();

builder.Services.AddTransient<IAssignmentGradeDataService, DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformAssignmentGradeServices();
```

## IAssignmentGradeDataService

There is no default `IAssignmentGradeDataService` implementation to allow each project to store the data how they see fit.

The `IAssignmentGradeDataService` interface is used to manage the persistance of line items and grades.

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

### IAssignmentGradeService

The `IAssignmentGradeService` interface is used to get the config for the assignment and grade service. The config is used to tell the tools how to request the members of a context.

There is a default implementation of the `IAssignmentGradeService` interface that uses a configuration set up on app start. When calling the `WithDefaultAssignmentGradeService` method, the configuration can be setup at that time. A fallback to the current request scheme and host will be used if no ServiceEndpoint is configured. The Default implementation can be overridden by adding a new implementation of the `INameRoleProvisioningService` interface and not including the Default. This may be useful if the service URL is dynamic or needs to be determined at runtime.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformAssignmentGradeServices()
    .WithDefaultAssignmentGradeService(x => { x.ServiceAddress = new Uri("https://<mysite>") });
```