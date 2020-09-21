the .def file is not part of the build... update it with:
defgen.bat 64 ZitiNativeApiForDotnetCore\build\x64\_deps\ziti-sdk-c-build\library\Debug\ziti.dll


build the dll's for packaging:
cmake --build .\ZitiNativeApiForDotnetCore\build\x86 --config Release
cmake --build .\ZitiNativeApiForDotnetCore\build\x64 --config Release