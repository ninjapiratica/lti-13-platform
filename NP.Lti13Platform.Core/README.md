# NP.Lti13Platform.Core

The IMS [LTI 1.3 Core](https://www.imsglobal.org/spec/lti/v1p3/) spec defines the base functionality for LTI 1.3. This project provides an implementation of the spec.

## Features

- LTI 1.3 Launch
- Authentication
- Token issuance
- JWKS endpoint

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
builder.Services.AddLti13PlatformCore<DataService>();
```

4. Setup the routing for the LTI 1.3 platform endpoints:

```csharp
app.UseLti13PlatformCore();
```

## ICoreDataService

There is no default `ICoreDataService` implementation to allow each project to store the data how they see fit.

The `ICoreDataService` interface is used to manage the persistence of tools, service tokens, and keys.

All of the internal services are transient and therefore the data service may be added at any scope (Transient, Scoped, Singleton).

## Message Handlers

LTI platforms and tools communicate via messages. LTI messages are handled by implementations of `IMessageHandler`.

To add a custom message handler:

```csharp
builder.Services
    .AddLti13PlatformCore<DataService>()
    .WithMessageHandler<CustomMessageHandler>();
```

### IMessageHandler

The `IMessageHandler` interface defines the contract for handling LTI messages. Implementations receive details about the launch request and return a message result.

#### Interface Overview

```csharp
public interface IMessageHandler
{
    Task<MessageResult> HandleMessageAsync(
        string loginHint, 
        string? ltiMessageHint, 
        Tool tool, 
        string nonce, 
        CancellationToken cancellationToken = default);
}
```

#### Parameters

- **loginHint**: A unique identifier provided by the platform to correlate the login request with the user. Contains information about the user initiating the launch.
- **ltiMessageHint**: An optional hint provided by the platform to help identify the specific LTI message or context. May be null.
- **tool**: The tool configuration used to validate and process the LTI message. Contains the tool's client ID, name, and other configuration details.
- **nonce**: A unique, random string used to prevent replay attacks.
- **cancellationToken**: A token that can be used to cancel the asynchronous operation.

#### Return Value

The method returns a `Task<MessageResult>`, which is an abstract base class with three possible derived types:

- **SuccessResult**: Indicates successful message handling. Contains the LTI message object that was processed.
  ```csharp
  return new MessageResult.SuccessResult(new LtiResourceLinkRequestMessage { /* ... */ });
  ```

- **ErrorResult**: Indicates that message handling failed. Contains an error message describing the failure.
  ```csharp
  return new MessageResult.ErrorResult("Invalid user context");
  ```

- **NoneResult**: Indicates no content or outcome is returned. Used when the loginHint and ltiMessageHint were not sufficient to determine a message to return. The two hints may be for a different message.
  ```csharp
  return new MessageResult.NoneResult();
  ```

#### Example Implementation

```csharp
public class CustomMessageHandler : IMessageHandler
{
    private readonly ICustomDataService _dataService;
    private readonly ILogger<CustomMessageHandler> _logger;

    public CustomMessageHandler(ICustomDataService dataService, ILogger<CustomMessageHandler> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<MessageResult> HandleMessageAsync(
        string loginHint, 
        string? ltiMessageHint, 
        Tool tool, 
        string nonce, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse the ltiMessageHint to determine the type of message being handled
            var messageType = ExtractMessageTypeFromHint(ltiMessageHint);

            // This handler only processes messages of a specific type, so if the message type is not recognized, return NoneResult
            if (string.IsNullOrEmpty(messageType))
            {
                return new MessageResult.NoneResult();
            }

            // Parse the loginHint to extract user information
            var userId = ExtractUserIdFromHint(loginHint);
            
            // Retrieve user and context data
            var user = await _dataService.GetUserAsync(userId, cancellationToken);
            if (user == null)
            {
                return new MessageResult.ErrorResult("User not found");
            }

            // Build your custom message
            var message = new CustomLtiMessage
            {
                UserId = user.Id,
                UserEmail = user.Email,
                // ... other message properties
            };

            return new MessageResult.SuccessResult(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling LTI message");
            return new MessageResult.ErrorResult($"Failed to handle message: {ex.Message}");
        }
    }

    private string ExtractUserIdFromHint(string loginHint)
    {
        // Implement your logic to extract user ID from the login hint
        // This depends on how your platform encodes the hint
        return loginHint;
    }

    private string ExtractMessageTypeFromHint(string ltiMessageHint)
    {
        // Implement your logic to extract the message type from the login hint
        // This depends on how your message handler encodes the hint
        return ltiMessageHint;
    }
}
```

## Defaults

### LtiResourceLinkMessageHandler

The core LTI spec includes a message for LTI Resource Links. This project includes a default handler for those messages.
For handling LTI Resource Link launches, this default handler requires an implementation of `IResourceLinkMessageDataService`.

```csharp
public class ResourceLinkDataService: IResourceLinkMessageDataService
{
    ...
}
```

Add it with:

```csharp
builder.Services
    .AddLti13PlatformCore<DataService>()
    .WithDefaultLtiResourceLinkMessageHandler<ResourceLinkDataService>();
```

#### Extensions

The core spec allows for extensions to the LTI Resource Link message. Extensions can be added by implementing `ILtiResourceLinkMessageExtension`.

```csharp
builder.Services
    .AddLti13PlatformCore<DataService>()
    .WithResourceLinkMessageExtension<CustomExtension>();
```

### Routing

Default routes are provided for all endpoints. Routes can be configured when calling `UseLti13PlatformCore()`.

```csharp
app.UseLti13PlatformCore(config => {
    config.JwksUrl = "/lti13/jwks/{clientId}"; // {clientId} is required
    config.TokenUrl = "/lti13/token"; // No parameters required
    config.AuthenticationUrl = "/lti13/auth"; // No parameters required
    return config;
});
```

### IPlatformService

The `IPlatformService` interface is used to get the platform configuration. The config is used to tell tools about the platform.

There is a default implementation of the `IPlatformService` interface that uses a configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:Platform`.

Examples:

```json
{
    "Lti13Platform": {
        "Platform": {
            "Guid": "server-id",
            ...
        }
    }
}
```

OR

```csharp
builder.Services.Configure<Platform>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `IPlatformService` interface.

```csharp
builder.Services
    .AddLti13PlatformCore<DataService>()
    .WithPlatformService<CustomPlatformService>();
```

### ITokenConfigService

The `ITokenConfigService` interface is used to get the token configuration. The config is used to issue tokens for service calls.

There is a default implementation of the `ITokenConfigService` interface that uses a configuration set up on app start.
It will be configured using the [`IOptions`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration) pattern and configuration.
The configuration path for the service is `Lti13Platform:Token`.

***Important***: The `Issuer` is required for the default token service to load.

Examples:

```json
{
    "Lti13Platform": {
        "Token": {
            "Issuer": "https://<mysite>",
            ...
        }
    }
}
```

OR

```csharp
builder.Services.Configure<TokenConfig>(x => { });
```

The Default implementation can be overridden by adding a new implementation of the `ITokenConfigService` interface.

```csharp
builder.Services
    .AddLti13PlatformCore<DataService>()
    .WithTokenConfigService<TokenService>();
```

## Configuration

### Platform Configuration

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

### Token Configuration

The configuration for handling of tokens between the platform and the tools.

***

`Issuer`

A case-sensitive URL using the HTTPS scheme that contains: scheme, host; and, optionally, port number, and path components; and, no query or fragment components. The issuer identifies the platform to the tools. An issuer is required.

***

`TokenAudience`

The value used to validate a token request from a tool. This is used to compare against the 'aud' claim of that JWT token request. If not provided, the token endpoint url will be used as a fallback.

***

`MessageTokenExpirationSeconds` Default: `300`

The expiration time of the lti messages that are sent to the tools.

***

`AccessTokenExpirationSeconds` Default: `3600`

The expiration time of the access tokens handed out to the tools.

## OpenAPI Documentation

Documentation for all endpoints are configured using `Microsoft.AspNetCore.OpenApi`. There is a convenience method to add a new document for the LTI 1.3 endpoints.

```csharp
using NP.Lti13Platform.Core;

...
builder.Services.AddLti13OpenApi("lti");
```

To add the endpoints to an existing document, the GroupName and the `AddLti13Authorization` extension method can be used.

```csharp
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = (description) => description.GroupName == NP.Lti13Platform.Core.Lti13OpenApi.GroupName;
    options.AddLti13Authorization();
});
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