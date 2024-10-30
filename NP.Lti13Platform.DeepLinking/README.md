# NP.Lti13Platform.DeepLinking

The IMS [Deep Linking](https://www.imsglobal.org/spec/lti-dl/v2p0) spec defines a way that platforms can get content from tools. This project provides an implementation of the spec.

## Features

- Launch the deep linking flow
- Receive the deep linking responses

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `IDeepLinkingDataService` interface:

```csharp
public class DataService: IDeepLinkingDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .AddDefaultDeepLinkingService();

builder.Services.AddTransient<IDeepLinkingDataService, DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformDeepLinking();
```

## IDeepLinkingDataService

There is no default `IDeepLinkingDataService` implementation to allow each project to store the data how they see fit.

The `IDeepLinkingDataService` interface is used to manage the persistance of resource links and other content items.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformDeepLinking()`.

```csharp
app.UseLti13PlatformDeepLinking(config => {
    config.DeepLinkingResponseUrl = "/lti13/deeplinking/{contextId?}"; // {contextId?} is required
});
```

### IDeepLinkingService

The `IDeepLinkingService` interface is used to get the config for the deep linking service as well as handle the response from the tool. The config is used to control how deep link requests are made and how the response will be handled.

There is a default implementation of the `IDeepLinkingService` interface that uses a configuration set up on app start. When calling the `AddDefaultDeepLinkingService` method, the configuration can be setup at that time. A fallback to the current request scheme and host will be used if no ServiceAddress is configured. The Default implementation can be overridden by adding a new implementation of the `IDeepLinkingService` interface and not including the Default.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .AddDefaultDeepLinkingService(x => { /* Update config as needed */ });
```

***Recommended***:

Then default handling of the response is to return it as an 200 OK response with the response body being a JSON representation of the content items returned from the tool. It is strongly recommended to use the Default for development only.

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

`ContentItemTypes` Default: `[]`{:csharp}

A dictionary of type configurations to be used when deserialzing the content items. If not set, the content items will be deserialized as `Dictionary<string, JsonElement>`{:csharp} objects. A convenience method to add the known content items to this dictionary is provided.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .AddLti13PlatformDeepLinking()
    .AddDefaultDeepLinkingService(x => 
    {
        x.AddDefaultContentItemMapping();
    });
```