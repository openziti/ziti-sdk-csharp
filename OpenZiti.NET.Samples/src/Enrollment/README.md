# Enrollment - Enrolling an Identity

This sample demonstrates how to enroll a token to produce a strong identity. The strong identity is what enables
a client to authenticate to an OpenZiti overlay network. 

When creating an identity, you can specify the "Enrollment" type. This sample makes a "one time token" (OTT). This
token will be in the form of a JWT. After creating the identity with an OTT enrollment, you can fetch the identity
details and write this OTT to a file. That is what this file demonstrates.

If you have administrative access to the controller, you can create the identity in the controller and then enroll it.
If not, you can just use this sample to enroll the identity.

## OpenZiti Concepts Demonstrated

This sample really only demonstrates a single key OpenZiti concepts: enrollment.

## Setup

Assuming you have sufficient access to the controller to modify it, you can set three environment variables to
allow the sample to configure itself. To configure, set these environment variables:
* `ZITI_USERNAME` - the username to use. Default: `admin`
* `ZITI_PASSWORD` - the password to use. Default: `admin`
* `ZITI_BASEURL` - the url of the controller to authenticate to.  Default: `localhost:1280`

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
