requires cmake to build the native libary...

mkdir build
cd build
cmake ..
cmake --build .

the .def file is not part of the build... update it with:
cd ZitiNativeApiForDotnetCore
defgen 64 build/x64/_deps/ziti-sdk-c-build/library/Release/ziti.dll

remove the extra files:


build the dll's for packaging:
cmake --build .\ZitiNativeApiForDotnetCore\build\x86 --config Release
cmake --build .\ZitiNativeApiForDotnetCore\build\x64 --config Release


install the formatter:
dotnet tool install -g dotnet-format

How to emit cs errors:
cl /C /EP /I build/x64/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > library/ZitiStatus.cs
- OR -
gcc -nostdinc -E -CC -P -Ibuild/x64/_deps/ziti-sdk-c-src/includes library/sharp-errors.c > library/ZitiStatus.cs
