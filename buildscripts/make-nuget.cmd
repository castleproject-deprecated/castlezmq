@ECHO OFF

mkdir ..\build\Package\
..\.nuget\Nuget.exe pack Castle.Zmq.nuspec -OutputDirectory ..\build\Package\