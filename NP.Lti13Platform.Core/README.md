# NP.Lti13Platform.Core

The IMS [Lti Core](https://www.imsglobal.org/spec/lti/v1p3/) spec defines a way that platforms can launch resources that exist in the tool. This project provides an implementation of the spec.

## Features

- Launch the links to the tool
- Handle authentication with the tool
- Create tokens for services

## Getting Started

1. Add the nuget package to your project:

2. Add an implementation of the `ICoreDataService` interface:

```csharp
public class DataService: ICoreDataService
{
    ...
}
```

3. Add the required services.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .WithDefaultPlatformService()
    .WithDefaultTokenService(x =>
    {
        x.Issuer = "https://<mysite>";
    });

builder.Services.AddTransient<ICoreDataService, DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformCore();
```

## ICoreDataService

There is no default `ICoreDataService` implementation to allow each project to store the data how they see fit.

The `ICoreDataService` interface is used to manage the persistance of most of the data involved in LTI communication.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Defaults

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformCore()`.

```csharp
app.UseLti13PlatformCore(config => {
    config.AuthorizationUrl = "/lti13/authorization";
    config.JwksUrl = "/lti13/jwks";
    config.TokenUrl = "/lti13/token";
});
```

### IPlatformService

The `IPlatformService` interface is used to get the platform details to give to the tools.

There is a default implementation of the `IPlatformService` interface that uses a configuration set up on app start. When calling the `WithDefaultPlatformService` method, the configuration can be setup at that time. The Default implementation can be overridden by adding a new implementation of the `IPlatformService` interface and not including the Default.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .WithDefaultPlatformService(x => { /* Set platform data */ });
```

### ITokenService

The `ITokenService` interface is used to get the token details for the tools.

There is a default implementation of the `ITokenService` interface that uses a configuration set up on app start. When calling the `WithDefaultTokenService` method, the configuration can be setup at that time. The Default implementation can be overridden by adding a new implementation of the `ITokenService` interface and not including the Default.

```csharp
builder.Services
    .AddLti13PlatformCore()
    .WithDefaultTokenService(x =>
    {
        x.Issuer = "https://<mysite>"; // This is required to be set when using the default token service.
    });
```

## Configuration

### Platform

The platform information to identify the platform server, contacts, etc.

***

`Guid`

A stable locally unique to the iss identifier for an instance of the tool platform. The value of guid is a case-sensitive string that MUST NOT exceed 255 ASCII characters in length. The use of Universally Unique IDentifier (UUID) defined in [RFC4122](https://www.rfc-editor.org/rfc/rfc4122) is recommended.

***

`ContactEmail`

Administrative contact email for the platform instance.

***

`Description`

Descriptive phrase for the platform instance.

***

`Name`

Name for the platform instance.

***

`Url`

Home HTTPS URL endpoint for the platform instance.

***

`ProductFamilyCode`

Vendor product family code for the type of platform.

***

`Version`

Vendor product version for the platform.

### Token Config

The configuration for handling of tokens between the platform and the tools.

***

`Issuer`

A case-sensitive URL using the HTTPS scheme that contains: scheme, host; and, optionally, port number, and path components; and, no query or fragment components. The issuer identifies the platform to the tools.

***

`TokenAudience`

The value used to validate a token request from a tool. This is used to compare against the 'aud' claim of that JWT token request. If not provided, the token endpoint url will be used as a fallback.

***

`MessageTokenExpirationSeconds` Default: `300`

The expiration time of the lti messages that are sent to the tools.

***

`AccessTokenExpirationSeconds` Default: `3600`

The expiration time of the access tokens handed out to the tools.

## Message Extensions

The LTI specs allow for messages to be extended with custom data. This is handled by adding `Populators` in the setup of the platform. To extend the message, create an interface with the properties that will be used to extend the message, and create a `Populator<T>` to fill those properties when the request for that message is generated.

Multiple populators can be added to the same interface. Multiple interfaces can be added to the same message type. The populator interface properties support the System.Text.Json attributes for serialization.

```csharp
interface ICustomMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

class CustomPopulator: Populator<ICustomMessage>
{
	public override async Task PopulateAsync(ICustomMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        obj.Custom = obj.Custom ?? new Dictionary<string, string>
		{
			{ "key", "value" }
		};
        
        ...
    }
}

builder.Services
    .AddLti13PlatformCore()
    .ExtendLti13Message<ICustomMessage, CustomPopulator>("LtiResourceLinkRequest");
```

## Custom Messgage Types

LTI allows for message types not defined officially in any spec. A custom message type can be defined between tools and platforms. This is handled here by extending a message type name it with the interfaces and populators it needs.

```csharp
interface ICustomMessage
{
    [JsonPropertyName("https://purl.imsglobal.org/spec/lti/claim/custom")]
    public IDictionary<string, string>? Custom { get; set; }
}

class CustomPopulator: Populator<ICustomMessage>
{
	public override async Task PopulateAsync(ICustomMessage obj, MessageScope scope, CancellationToken cancellationToken = default)
    {
        obj.Custom = obj.Custom ?? new Dictionary<string, string>
		{
			{ "key", "value" }
		};
        
        ...
    }
}

builder.Services
    .AddLti13PlatformCore()
    .ExtendLti13Message<ICustomMessage, CustomPopulator>("CustomMessage");
```

## Terminology

`Platform` The platform is the server that is launching the tool. The platform is like a school or company and the tool has the content for the school or comapny to use.

`Tool` The tool is the application that is being launched by the platform. The tool has content to be launched. A relationship needs to be formed before a platform can launch anything in a tool.

`Deployment` A deployment is a specific instance of a tool in a platform. A platform may 'deploy' a tool multiple times (e.g. the district is the platform and has a different deployment of a tool for each school). Even if there is only going to be one instance of a tool in a platform, a single deployment is still required.

`Context` A context is a group of LTI links. In school terminology, a context may be a class or a course. A context may have resources from multiple tools.

`ResourceLink` A resource link is a link to a resource in a tool. The resource link is the object that is launched by the platform. Multiple resource links may be added to a single context.

`Membership` A membership is a relationship between a user and a context. The membership is used to determine what roles a user has in a context.

`User` A user is a person that is using the platform. The user may have a membership multiple contexts.

`Attempt` An attempt defines a user's interaction with a resource link.

`LineItem` A line item like a 'column' in a gradebook. It may or may not be tied to a ResourceLink and a ResourceLink may have 0 or more LineItems associated with it.

`Grade` A grade is a score for a user for a LineItem.

`ServiceToken` A service token is an identifier for a token given to a tool to access a service. It is used to avoid replay attacks and can be removed once the expiration date has elapsed.
