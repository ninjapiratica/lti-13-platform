# NP.Lti13Platform

NP.Lti13Platform is a .NET 8 project that provides an implementation of an LTI 1.3 platform. This project is a wrapper for the other LTI 1.3 projects. For specific information regarding any of the specific specs, please see their respective projects.

## Features

- LTI 1.3 Core (Launch)
- Deep Linking
- Assignment and Grade Services
- Name and Role Provisioning Services

## Getting Started

1. Add the nuget package to your project:

```bash
nuget install NP.Lti13Platform
```

2. Add an implementation of the `IDataService` interface:

```csharp
public class DataService: IDataService
{
    ...
}
```

3. Add the required services (most configurations are optional, the required configurations are shown):
**For information regarding configurations, please see the individual projects.*

```csharp
builder.Services
    .AddLti13PlatformWithDefaults(x => { x.Issuer = "https://<site>.com"; })
    .AddDataService<DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13Platform();
```

## IDataService

There is no default IDataService implementation to allow each project to store the data how they see fit.

The IDataService interface is a combination of all data services required for all the specs of the LTI 1.3 platform. Each service can be individually overridden instead of implementing the entire data service in a single service. 

```diff
builder.Services
+    .AddLti13PlatformWithDefaults(x => { x.Issuer = "https://<site>.com"; });
-    .AddLti13PlatformWithDefaults(x => { x.Issuer = "https://<site>.com"; })
-    .AddDataService<DataService>();

+ builder.Services.AddTransient<ICoreDataService, CustomCoreDataService>();
+ builder.Services.AddTransient<IDeepLinkingDataService, CustomDeepLinkingDataService>();
+ builder.Services.AddTransient<INameRoleProvisioningDataService, CustomNameRoleProvisioningDataService>();
+ builder.Services.AddTransient<IAssignmentGradeDataService, CustomAssignmentGradeDataService>();
```

All of the internal services are transient and therefore the data services may be added at any scope (Transient, Scoped, Singleton).