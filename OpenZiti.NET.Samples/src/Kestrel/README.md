![Ziggy using the sdk-csharp](https://raw.githubusercontent.com/openziti/branding/main/images/banners/CSharp.jpg)

# Zitified Kestrel Sample

This sample demonstrates how to use the OpenZiti SDK to host a Kestrel-based ASP.NET Core web API over the Ziti network.

The sample includes two REST API endpoints:
- **WeatherForecast** - Returns sample weather data
- **MetricItems** - CRUD API for sensor metrics with in-memory database

Example metric data:
```json
{
  "id": 1,
  "sensorGuid": "abcd1234",
  "name": "temperature",
  "value": 72
}
```

## OpenZiti Concepts Demonstrated

* **Application-embedded zero trust server** - No ports open, no firewall rules needed
* **Native .NET integration** - Seamlessly integrates with ASP.NET Core middleware pipeline
* **Identity-based access control** - Only authorized Ziti identities can connect

## Prerequisites

1. A running OpenZiti network
2. A Ziti identity with bind permissions for your service
3. .NET 8.0 SDK

## Configuration

Update `appsettings.json` with your Ziti configuration:

```json
{
  "Ziti": {
    "IdentityPath": "path/to/your/identity.json",
    "ServiceName": "your-service-name",
    "LogLevel": "INFO",
    "Terminator": ""
  }
}
```

Configuration options:
- **IdentityPath** (required) - Path to your Ziti identity JSON file
- **ServiceName** (required) - Name of the Ziti service to bind
- **LogLevel** (optional) - Ziti SDK log level (NONE, ERROR, WARN, INFO, DEBUG, VERBOSE, TRACE)
- **Terminator** (optional) - Custom terminator name (leave empty for auto-generated)

## Running the Sample

```bash
cd OpenZiti.NET.Samples/src/Kestrel
dotnet run
```

The application will:
1. Initialize the Ziti SDK
2. Bind to your configured Ziti service
3. Start listening for connections from authorized Ziti clients
4. Host Swagger UI at `/swagger` (in development mode)

## Testing the API

Once running, you can test the endpoints from a Ziti client:

### Weather Forecast
```bash
GET /WeatherForecast
```

### Metric Items
```bash
# Get all metrics
GET /api/MetricItemsController

# Get specific metric
GET /api/MetricItemsController/1

# Create metric
POST /api/MetricItemsController
Content-Type: application/json

{
  "sensorGuid": "sensor-123",
  "name": "temperature",
  "value": 72
}

# Update metric
PUT /api/MetricItemsController/1
Content-Type: application/json

{
  "id": 1,
  "sensorGuid": "sensor-123",
  "name": "temperature",
  "value": 75
}

# Delete metric
DELETE /api/MetricItemsController/1
```

## Architecture

### Key Components

**ZitiConnectionListenerFactory.cs**
- Implements `IConnectionListenerFactory` to provide Ziti-based connections to Kestrel
- Reads configuration from `appsettings.json`
- Initializes Ziti context and binds to service
- Accepts incoming connections from Ziti clients
- Proper logging and error handling

**ServiceCollectionExtensions.cs**
- Extension method `UseZitiTransport()` to enable Ziti transport in Kestrel
- Registers `ZitiConnectionListenerFactory` as the connection listener

**Program.cs**
- Standard ASP.NET Core minimal hosting setup
- Calls `UseZitiTransport()` to enable Ziti networking
- Configures controllers, Entity Framework, and Swagger

### How It Works

1. When the application starts, `UseZitiTransport()` registers the custom `ZitiConnectionListenerFactory`
2. Kestrel calls `BindAsync()` which initializes the Ziti SDK and binds to the service
3. `AcceptAsync()` is called repeatedly to accept incoming connections
4. Each Ziti client connection is wrapped in a `SocketConnectionContext`
5. Kestrel processes the HTTP request through the normal ASP.NET Core pipeline
6. Response flows back through Ziti to the client

### Benefits of This Approach

- **Zero code changes** to controllers or business logic
- **Full middleware support** - authentication, logging, CORS, etc. all work normally
- **Transparent security** - Ziti handles encryption and identity verification
- **No exposed ports** - Service only accessible via Ziti overlay
- **Works with existing tooling** - Swagger, OpenAPI, etc.
