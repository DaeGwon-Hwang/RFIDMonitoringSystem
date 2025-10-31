@echo off
echo Building projects (msbuild required)...
msbuild RFIDMiddleware.Server\RFIDMiddleware.Server.csproj /p:Configuration=Debug
msbuild RFIDMiddleware.Client\RFIDMiddleware.Client.csproj /p:Configuration=Debug

start "Server" cmd /k "cd /d %~dp0RFIDMiddleware.Server\bin\Debug\net472 && Server.exe"
timeout /t 2 /nobreak >nul
start "Client" cmd /k "cd /d %~dp0RFIDMiddleware.Client\bin\Debug\net472 && Client.exe"
