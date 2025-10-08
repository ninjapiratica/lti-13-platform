# NP.Lti13Platform.DeepLinking

The IMS [Deep Linking](https://www.imsglobal.org/spec/lti-dl/v2p0) spec defines a way that platforms can get content from tools. This project provides an implementation of the spec.

## Features

- Launch the deep linking flow
- Receive the deep linking responses

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `ILti13DeepLinkingDataService` interface:

```csharp
public class DataService: ILti13DeepLinkingDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .WithLti13DeepLinkingDataService<DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformDeepLinking();
```

## ILti13DeepLinkingDataService

There is no default `ILti13DeepLinkingDataService` implementation to allow each project to store the data how they see fit.

The `ILti13DeepLinkingDataService` interface is used to manage the persistance of resource links and other content items.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformDeepLinking()`.

```csharp
app.UseLti13PlatformDeepLinking(config => {
    config.DeepLinkingResponseUrl = "/lti13/deeplinking/{contextId?}"; // {contextId?} is required
    return config;
});
```

### ILti13DeepLinkingHandler

The `ILti13DeepLinkingHandler` interface is used to handle the response from the tool.

***Recommended***:
The default handling of the response is to return a placeholder page. It is strongly recommended to use the Default for development only.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .WithLti13DeepLinkingHandler<Handler>();
```

### ILti13DeepLinkingConfigService

The `ILti13DeepLinkingConfigService` interface is used to get the config for the deep linking service. The config is used to control how deep link requests are made and how the response will be handled.

There is a default implementation of the `ILti13DeepLinkingConfigService` interface that uses a configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:DeepLinking`.
A fallback to the current request scheme and host will be used if no ServiceAddress is configured.

Examples

```json
{
    "Lti13Platform": {
        "DeepLinking": {
            "ServiceAddress": "https://<mysite>",
            ...
        }
    }
}
```

```csharp
builder.Services.Configure<ServicesConfig>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `ILti13DeepLinkingConfigService` interface.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .WithLti13DeepLinkingDataService<DataService>();
```

## Configuration

The configuration for the Deep Linking service tells the tools what kinds of things the platform is looking for and how it will handle the items when they are returned.

***

`AcceptPresentationDocumentTargets` Default: `["embed", "iframe", "window"]`{:csharp}

Defines how the content items will be shown to users (Embedded, Iframe, Window).

***

`AcceptTypes` Default: `["file", "html", "image", "link", "ltiResourceLink"]`{:csharp}

Defines which types of content items the platform is looking for (File, Html, Image, Link, ResourceLink).

***

`AcceptMediaTypes` Default: `["image/*", "text/html"]`{:csharp}

Defines which media types the platform is looking for (image/*, text/html).

***

`AcceptLineItem` Default: `true`{:csharp}

Whether the platform in the context of that deep linking request supports or ignores line items included in LTI Resource Link items. False indicates line items will be ignored. True indicates the platform will create a line item when creating the resource link. If the field is not present, no assumption that can be made about the support of line items.

***

`AcceptMultiple` Default: `true`{:csharp}

Whether the platform allows multiple content items to be submitted in a single response.

***

`AutoCreate` Default: `true`{:csharp}

Whether any content items returned by the tool would be automatically persisted without any option for the user to cancel the operation.

***

`ServiceAddress` Default: `null`{:csharp}

The web address where the deep linking responses will be handled. If not set, the current request scheme and host will be used.

***

`ContentItemTypes` Default Keys: `["file", "html", "image", "link", "ltiResourceLink"]`{:csharp}

A dictionary of type configurations to be used when deserialzing the content items. If not set, the content items will be deserialized as `Dictionary<string, JsonElement>`{:csharp} objects. Common known content items are already added to this dictionary. Additional types can be added.

```csharp
builder.Services.Configure<DeepLinkingConfig>(x =>
{
    x.ContentItemTypes.Add((null, "my.custom.type"), typeof(MyCustomType))
});
```

