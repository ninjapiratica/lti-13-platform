# NP.Lti13Platform

NP.Lti13Platform is a .NET 8 project that provides an implementation of an LTI 1.3 platform. This project is a wrapper for the other LTI 1.3 projects. For specific information regarding any of the specific specs, please see their respective projects.

## Features

- LTI 1.3 Core (Launch)
- Deep Linking
- Assignment and Grade Services
- Name and Role Provisioning Services

## Getting Started

1. Add the nuget package to your project:

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
    .AddLti13Platform()
    .WithLti13DataService<DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13Platform();
```

## IDataService

There is no default `IDataService` implementation to allow each project to store the data how they see fit.

The `IDataService` interface is a combination of all data services required for all the specs of the LTI 1.3 platform. Each service can be individually overridden instead of implementing the entire data service in a single service. 

```diff
builder.Services
    .AddLti13Platform()
-    .WithLti13DataService<DataService>();
+    .WithLti13CoreDataService<CoreDataService>()
+    .WithLti13DeepLinkingDataService<DeepLinkingDataService>()
+    .WithLti13AssignmentGradeDataService<AssignmentGradeDataService>()
+    .WithLti13NameRoleProvisioningDataService<NameRoleProvisioningDataService>();
```

All of the internal services are transient and therefore the data services may be added at any scope (Transient, Scoped, Singleton).

## OpenAPI Documenatation

Documentation for all endpoints are available through OpenAPI. This is normally configured through Swagger, NSwag or similar.

To avoid adding these endpoints to a consumer's normal api documents, no 'group' has been given to the endpoints. This can be configured and added to the documents you choose. Below is an example using Swagger.

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new() { Title = "Public API", Version = "v1" });
    x.SwaggerDoc("v2", new() { Title = "LTI 1.3", Version = "v2" });

    x.DocInclusionPredicate((docName, apiDesc) =>
    {
        return docName == (apiDesc.GroupName ?? string.Empty) || (docName == "v2" && apiDesc.GroupName == "group_name");
    });
});

app.UseLti13Platform(openAPIGroupName: "group_name");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "LTI 1.3 API");
});
```

## Defaults

Many of the specs have default implementations that use a static configuration on startup. If you can't configure the services at startup you can add your own implementation of the services.

```diff
builder.Services
    .AddLti13Platform()
    .WithLti13DataService<DataService>()
+    .WithLti13TokenConfigService<TokenService>()
+    .WithLti13PlatformService<PlatformService>()
+    .WithLti13DeepLinkingConfigService<DeepLinkingConfigService>()
+    .WithLti13DeepLinkingHandler<DeepLinkingHandler>()
+	 .WithLti13AssignmentGradeConfigService<AssignmentGradeConfigService>()
+    .WithLti13NameRoleProvisioningConfigService<NameRoleProvisioningConfigService>();
```