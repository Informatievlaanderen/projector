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

let assemblyVersionNumber = (sprintf "%s.0")
let nugetVersionNumber = (sprintf "%s")

let build = buildSolution assemblyVersionNumber
let publish = publishSolution assemblyVersionNumber
let pack = packSolution nugetVersionNumber

// Library ------------------------------------------------------------------------

Target "Lib_Build" (fun _ -> build "Be.Vlaanderen.Basisregisters.Projector")
Target "Lib_Test" (fun _ -> testSolution "Be.Vlaanderen.Basisregisters.Projector")

Target "Lib_Publish" (fun _ -> publish "Be.Vlaanderen.Basisregisters.Projector")
Target "Lib_Pack" (fun _ -> pack "Be.Vlaanderen.Basisregisters.Projector")

// --------------------------------------------------------------------------------

Target "PublishLibrary" DoNothing
Target "PublishAll" DoNothing

Target "PackageMyGet" DoNothing
Target "PackageAll" DoNothing

// Publish ends up with artifacts in the build folder
"NpmInstall" ==> "DotNetCli" ==> "Clean" ==> "Restore" ==> "Lib_Build" ==> "Lib_Test" ==> "Lib_Publish" ==> "PublishLibrary"
"PublishLibrary" ==> "PublishAll"

// Package ends up with local NuGet packages
"PublishLibrary" ==> "Lib_Pack" ==> "PackageMyGet"
"PackageMyGet" ==> "PackageAll"

RunTargetOrDefault "Lib_Test"
