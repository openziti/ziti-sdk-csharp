# Appetizer - reflect server

This sample demonstrates how to write data to a stream and receive data. It uses the https://openziti.io/appetizer
demonstration environment which is capable of genearting an identity for you, to be used with this sample. It
utilizes the reflect server functionality to return whatever data you send to the server back to you. The reflect
server also is tied to an LLM which is attempting to classify your input. If you use something that seems "hateful"
you will receive an error message indicating your input was not accepted. The server is written in go and the 
source for the server is available at https://github.com/openziti-test-kitchen/appetizer.

## OpenZiti Concepts Demonstrated

This sample demonstrates some key OpenZiti concepts:

* Application-embedded zero trust client written in C# to a golang-based application embedded zero trust server.
* Sending data in a line-delimeted protocol to a server deployed out in the cloud, then receiving and displaying
  the response.
* Directly dialing a service by service name, in this case it is named "reflectService"
* Creating a a strong identity by enrolling a one-time use token provided by the appetizer server.

## Setup

This sample is unlike others. It relies on an instance of the appetizer demo to be deployed. It's easiest to use
the instance OpenZiti has deployed by going to https://appetizer.openziti.io/. You'll see a screen asking you to
enter something unique, like your email address. There's a button to click named "Add to OpenZiti". After you click
the button, you'll be able to save/download a one-time use token.

To run the sample, simply provide the path to that token as a argument to the program.

## Running the Sample

After downloading the .jwt, you should be able to just run it directly.

Example:
```
dotnet run --project OpenZiti.NET.Samples/OpenZiti.NET.Samples.csproj appetizer-reflect
```
