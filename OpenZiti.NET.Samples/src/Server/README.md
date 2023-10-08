# Server - A Server/Client

This sample demonstrates embedding zero trust in an application __completely__. The client uses application-embedded
zero trust, __AND__ the server uses application-embedded zero trust as well. It is a very simple implementation
consisting of a server that accepts a single connection and reads from it. After reading a line, the server will
return the line back to the client in an 'echo' or 'reflect' server way but it will add "Thanks for sending me:" to the
response.

For example, if the client sends `Hello, server` (shown below), the server will reply and the client will output:
```
Hello, server
done sending. moving to read response
Read:
Hi hosted-demo-svc-client. Thanks for sending me: Hello, server
```

## OpenZiti Concepts Demonstrated

This sample demonstrates some key OpenZiti concepts:

* Application-embedded zero trust server.
* Application-embedded zero trust client.
* Offloading traffic from an identity. OpenZiti allows you to configure a tunneler to offload traffic towards another.
  This sample offloads traffic from a router to https://wttr.in using a `host.v1` config.
* Using an `intercept.v1` to specify a URL should be intercepted by the application.
* Creating a service to combine two configs (the intercept and the host).
* Service Policies to authorize identities to perform `dial` or `bind`.

## Setup

Assuming you have sufficient access to the controller to modify it, you can set three environment variables to
allow the sample to configure itself. To configure, set these environment variables:
* `ZITI_USERNAME` - the username to use. Default: `admin`
* `ZITI_PASSWORD` - the password to use. Default: `admin`
* `ZITI_BASEURL` - the url of the controller to authenticate to.  Default: `localhost:1280`

## Running the Sample

Generally the samples are runnable using `dotnet run`. However, this sample has two programs that need to run: the host
and the client. This ends up conflicting with `dotnet run` since `dotnet run` tries to compile the project, before
running it. That'd be fine if it didn't try to overwrite the existing binary, but it seems that `dotnet run` doesn't 
make any attempt to determine if the resultant binary needs to be recompiled and it just tries to compile it. This ends
up causing `dotnet run` to output this warning:
```
warning MSB3026: Could not copy ...OpenZiti.NET.dll" to "bin\Debug\net6.0\OpenZiti.NET.dll". 
Beginning retry 3 in 1000ms. The process cannot access the file ... because it is being used by another process. 
The fi le is locked by: "OpenZiti.NET.Samples (10052)"
```

The only way to work around this is to first **build** the sample, then execute the executable directly, instead of
using `dotnet run`. That is why this sample has example commands that look slightly different.

You can either choose to build the entire solution: `dotnet build .\Ziti.NuGet.sln`, or you can build just the
samples using: `dotnet build openziti.NET.Samples/OpenZiti.NET.Samples.csproj`. This will create an executable in
the `Debug` folder. For example, from windows this executable is created (and referenced below):
```
./OpenZiti.NET.Samples/bin/Debug/net6.0/OpenZiti.NET.Samples.exe
```

## Running the Sample With Automatic Configuration

To run the sample, you should be able to just run it directly and it will bootstrap the overlay network, presuming you
have configured the environment variables or that your controller exists on the default url and uses the default
username/password.

Example:
```
./OpenZiti.NET.Samples/bin/Debug/net6.0/OpenZiti.NET.Samples.exe hosted
```

## Running the Sample Without Configuring

You can run the sample without allowing the sample to configure the overlay. To run the sample, you will require an
identity that has access to a service named `weather-demo-svc` or you will need to modify the sample code and change
the referenced service name. Then, when executing the service, pass `noinit` and pass the identity file as shown:
```
./OpenZiti.NET.Samples/bin/Debug/net6.0/OpenZiti.NET.Samples.exe hosted noinit /some/path/to/an/identity
```
