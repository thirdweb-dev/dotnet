@echo off
REM Thirdweb Build Script
REM Usage: tw.bat [command]

REM Check if command was provided
if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="clean-api" goto clean-api
if "%1"=="generate-api" goto generate-api
if "%1"=="build" goto build
if "%1"=="clean" goto clean
if "%1"=="restore" goto restore
if "%1"=="test" goto test
if "%1"=="pack" goto pack
if "%1"=="run" goto run
if "%1"=="lint" goto lint
if "%1"=="fix" goto fix
goto help

:clean-api
echo Cleaning generated API files...
if exist "Thirdweb\Thirdweb.Api\GeneratedClient.cs" (
    echo Removing generated client file...
    del /q "Thirdweb\Thirdweb.Api\GeneratedClient.cs"
) else (
    echo No generated client file to clean
)
echo Clean completed!
goto end

:generate-api
echo Generating Thirdweb API client with NSwag...

REM Check if NSwag is installed
nswag version >nul 2>&1
if errorlevel 1 (
    echo NSwag CLI is not installed. Installing via dotnet tool...
    dotnet tool install --global NSwag.ConsoleCore
    if errorlevel 1 (
        echo Failed to install NSwag CLI
        exit /b 1
    )
)

REM Generate API client using NSwag
echo Running NSwag to generate client...
nswag run nswag.json
if errorlevel 1 (
    echo Failed to generate API client
    exit /b 1
)

echo API client generation complete!
exit /b 0

:build
echo Building solution...
REM First generate the API if it doesn't exist
if not exist "Thirdweb\Thirdweb.Api\GeneratedClient.cs" (
    echo API client not found, generating it first...
    call :generate-api
    if errorlevel 1 goto end
)
dotnet build
goto end

:clean
echo Cleaning solution...
dotnet clean
goto end

:restore
echo Restoring packages...
dotnet restore
goto end

:test
echo Running tests...
dotnet test
goto end

:pack
echo Creating NuGet packages...
REM Ensure API is generated before packing
if not exist "Thirdweb\Thirdweb.Api\GeneratedClient.cs" (
    echo API client not found, generating it first...
    call :generate-api
    if errorlevel 1 goto end
)
dotnet build --configuration Release
dotnet pack --configuration Release
goto end

:run
echo Running console application...
dotnet run --project Thirdweb.Console
goto end

:lint
echo Checking code formatting with CSharpier...
csharpier --help >nul 2>&1
if errorlevel 1 (
    echo CSharpier is not installed. Install it with: dotnet tool install -g csharpier
    goto end
)
csharpier check .
if errorlevel 1 (
    echo Code formatting issues found! Run 'tw fix' to automatically fix them.
    goto end
)
echo Code formatting is correct!
goto end

:fix
echo Fixing code formatting with CSharpier...
csharpier --help >nul 2>&1
if errorlevel 1 (
    echo CSharpier is not installed. Install it with: dotnet tool install -g csharpier
    goto end
)
csharpier format .
if errorlevel 1 (
    echo CSharpier formatting failed!
    goto end
)
echo Code formatting completed!
goto end

:help
echo Available commands:
echo   build          - Generate API (if needed) and build the solution
echo   clean          - Clean build artifacts
echo   restore        - Restore NuGet packages
echo   test           - Run tests
echo   pack           - Generate API (if needed) and create NuGet package
echo   run            - Run the console application
echo   generate-api   - Generate API client from OpenAPI spec
echo   clean-api      - Clean generated API files
echo   lint           - Check code formatting (dry run)
echo   fix            - Fix code formatting issues
echo   help           - Show this help message
echo.
echo Usage: tw.bat [command]
goto end

:end
