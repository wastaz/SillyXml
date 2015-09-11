#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

let buildDir = "./build/"
let packagingDir = "./packaging/"

let appReferences = [ "SillyXml/SillyXml.csproj" ]

let version = "0.1.4"

Target "Clean" (fun _ -> 
    CleanDirs [ buildDir; packagingDir; ]
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

Target "CreatePackage" (fun _ ->
    Paket.Pack(fun p -> 
        { p with 
            Version = version
            OutputPath = packagingDir })
)

Target "PublishPackage" (fun _ ->
    Paket.Push(fun p -> { p with WorkingDir = packagingDir })
)

"Clean"
==> "BuildRelease"
==> "CreatePackage"
==> "PublishPackage"

RunTargetOrDefault "BuildRelease"