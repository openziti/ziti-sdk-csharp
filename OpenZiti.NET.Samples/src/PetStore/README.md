# Pet Store - Consuming an HTTP-based API

This sample demonstrates how to invoke a web request exiting from private networking space towards an HTTP-based
API. This sample will use the swagger "pet store" application and represents consuming an HTTP-based API.

## OpenZiti Concepts Demonstrated

This sample demonstrates some key OpenZiti concepts:

* Application-embedded zero trust client.
* Offloading traffic from an identity. OpenZiti allows you to configure a tunneler to offload traffic towards another.
  This sample offloads traffic from our router to https://localhost:20080 using a `host.v1` config.
* Using an `intercept.v1` to specify a URL should be intercepted by the application.
* Creating a service to combine two configs (the intercept and the host).
* Service Policies to authorize identities to perform `dial` or `bind`.

## Setup

Assuming you have sufficient access to the controller to modify it, you can set three environment variables to
allow the sample to configure itself. To configure, set these environment variables:
* `ZITI_USERNAME` - the username to use. Default: `admin`
* `ZITI_PASSWORD` - the password to use. Default: `admin`
* `ZITI_BASEURL` - the url of the controller to authenticate to.  Default: `localhost:1280`

### Petstore Endpoint

The setup requires a petstore application to be running. It is available as a docker container and also available
online if you prefer to not have to host it yourself. By default, the sample expects you are running it adjacent to
your router (available to the router at `localhost:20080`).

## Running the Sample With Automatic Configuration

To run the sample, you should be able to just run it directly and it will bootstrap the overlay network, presuming you
have configured the environment variables or that your controller exists on the default url and uses the default
username/password.

Example:
```
dotnet run --project OpenZiti.NET.Samples/OpenZiti.NET.Samples.csproj petstore
```

## Running the Sample Without Configuring

You can run the sample without allowing the sample to configure the overlay. To run the sample, you will require an
identity that has access to a service named `weather-demo-svc` or you will need to modify the sample code and change
the referenced service name. Then, when executing the service, pass `noinit` and pass the identity file as shown:
```
dotnet run --project OpenZiti.NET.Samples/OpenZiti.NET.Samples.csproj petstore noinit /some/path/to/an/identity
```
