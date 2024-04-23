![Ziggy using the sdk-csharp](https://raw.githubusercontent.com/openziti/branding/main/images/banners/CSharp.jpg)

# Zitified Kestrel Sample

This sample demonstrates how to use the OpenZiti SDK to secure a Kestrel server. 
In this case, the server is a simple REST API listening in the Ziti Overlay, that returns some metric data. This data looks like this:
```
{
  "sensorguid": "abcd1234",
  "name": "temp",
  "value": "6"
}
```

## OpenZiti Concepts Demonstrated

This sample demonstrates some key OpenZiti concepts:
* Application-embedded zero trust server.
* Availability to natively integrate with the DotNet Core ecosystem.
* Offloading traffic from an identity.

## Running the Sample

To run the sample, you should be able to just run it directly and it will bootstrap the overlay network. The program expects to have your identity saved into the `C:\OpenZiti\CSharp-RestApi-Server.json` file. This location can be changed in the file `DelegatedZitiConnectionListenerFactory.cs`. You can run the code as:
```
dotnet run --project OpenZiti.NET.Samples.Kestrel/ZitiRestServerCSharp.csproj
```

## Code Walkthrough

There're a few key components in this sample:
* `DelegatedZitiConnectionListenerFactory.cs` - This is the main entry point of the application. It sets up the Kestrel server and the Ziti SDK using the identity provided.
* `ServiceCollectionExtension.cs` - This file contains the extension method to overide the default `IConnectionListenerFactory` with the `DelegatedZitiConnectionListenerFactory`.
