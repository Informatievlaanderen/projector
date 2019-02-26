#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake
open ``Build-generic``

// The buildserver passes in `BITBUCKET_BUILD_NUMBER` as an integer to version the results
// and `BUILD_DOCKER_REGISTRY` to point to a Docker registry to push the resulting Docker images.

// NpmInstall
// Run an `npm install` to setup Commitizen and Semantic Release.

// DotNetCli
// Checks if the requested .NET Core SDK and runtime version defined in global.json are available.
// We are pedantic about these being the exact versions to have identical builds everywhere.

// Clean
// Make sure we have a clean build directory to start with.

// Restore
// Restore dependencies for debian.8-x64 and win10-x64 using dotnet restore and Paket.

// Build
// Builds the solution in Release mode with the .NET Core SDK and runtime specified in global.json
// It builds it platform-neutral, debian.8-x64 and win10-x64 version.

// Test
// Runs `dotnet test` against the test projects.

// Publish
// Runs a `dotnet publish` for the debian.8-x64 and win10-x64 version as a self-contained application.
// It does this using the Release configuration.

// Pack
// Packs the solution using Paket in Release mode and places the result in the dist folder.
// This is usually used to build documentation NuGet packages.

let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "%s")

Target "Restore" (fun _ -> restore "Be.Vlaanderen.Basisregisters.Projector")
Target "Build" (fun _ -> buildSolution assemblyVersionNumber "Be.Vlaanderen.Basisregisters.Projector")
Target "Test" (fun _ -> testSolution "Be.Vlaanderen.Basisregisters.Projector")
Target "Publish" (fun _ -> publish assemblyVersionNumber "Be.Vlaanderen.Basisregisters.Projector")
Target "Pack" (fun _ -> pack nugetVersionNumber "Be.Vlaanderen.Basisregisters.Projector")

Target "Package" DoNothing

"NpmInstall" ==> "DotNetCli" ==> "Clean" ==> "Restore" ==> "Build" ==> "Test"
"Test" ==> "Publish" ==> "Pack" ==> "Package"

RunTargetOrDefault "Test"
