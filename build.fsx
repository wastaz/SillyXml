#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

let buildDir = "./build/"

let appReferences = [ "SillyXml/SillyXml.csproj" ]

let version = "0.1"

Target "Clean" (fun _ -> 
    CleanDirs [buildDir]
)

Target "BuildRelease" (fun _ ->
    CreateCSharpAssemblyInfo "./SillyXml/Properties/AssemblyInfo.cs"
         [Attribute.Title "SillyXML"
          Attribute.Description "SillyXML - Immutable class XML serialization for the people!"
          Attribute.Product "SillyXML"
          Attribute.Copyright "Copyright 2015 Fredrik Forssen"
          Attribute.Version version
          Attribute.FileVersion version]
    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)

"Clean"
==> "BuildRelease"

RunTargetOrDefault "BuildRelease"