# OpenZiti.NET.Samples

This project contains sample applications that will work with an OpenZiti overlay.

You will probably want to have the `ziti` CLI available as it's handy to use for exploring the OpenZiti configuration.
If you do not have the `ziti` CLI, you can quickly install it by running a single 
powershell command:
```
iex(iwr -Uri https://get.openziti.io/quick/getZiti.ps1)
```

See the README files in the subdirectories of the [src](./src) directory for more information about each sample.

| Sample                                       | Description                                                                                                                                               |
|----------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| [OpenAPI PetStore](./src/PetStore/README.md) | Illustrates how to invoke an HTTP-based API securely                                                                                                      |
| [Weather](./src/Weather/README.md)           | Illustrates how to make an http-based request and output the content to the screen using wttr.in                                                          |
| ["reflect" server](./src/Server/README.md)   | Illustrates how to use OpenZiti as a server __AND__ a client. Demonstrates true application-embedded zero trust! Starts a server that responds to clients |
| [Enrollment](./src/Enrollment/README.md)     | A simple sample demonstrating how to enroll an OpenZiti Identity                                                                                          |

## "noinit"

If you are not able to modify the overlay network because you don't have access or credentials to the controller, you
can still execute the sample. You will need an identity that is properly configured. See the README for the sample for
specific details. When executing the sample you will need to pass the sample to execute, as well as pass the "noinit"
parameter in position two, and the identity file to use in position 3. Example:
```
dotnet run --project OpenZiti.NET.Samples/OpenZiti.NET.Samples.csproj weather noinit /some/path/to/an/identity
```