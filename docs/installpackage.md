# Installing Our Package

Our package name is currently `EducationExtension`.

Our top level namespace is currently `Extension`.

Our Github Package Registry source is `https://nuget.pkg.github.com/dotnet-interactive-learning/index.json`.

---

## From GitHub Package Registry (GPR) NuGet Source

Prerequisites:

1. Install [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0). This will come with `dotnet nuget` command. 

2. Get a GitHub personal access token.
    
    1. Github settings > Developer settings > Personal access tokens
    2. Generate new token
    3. Add a note for your token, e.g. "GPRread"
    4. Enable `read:packages` scope
    5. Copy your PAT

Adding a authenticated nuget source using CLI:

1. `dotnet nuget add source [GPR source] -n [mysourcename] -u [Github username] -p [your PAT]`

2. Check that the source was added using `dotnet nuget list source`.

3. To check that the authentication is correct, run `#r "nuget: [packagename], [version number or *-* if latest]"` in a new notebook.

---

## From .nupkg files

This might be useful after manually downloading from our Github page or from a Github Actions artifact.

Prerequisites

0. Prepare a well-known local directory as a local package source, e.g. `C:\packages`

1. `dotnet nuget add source "C:\packages" -n [mysourcename]`

2. Check that your NuGet.Config has the local package source at top of the list. To find your nuget config, see [here](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior).

Then

0. If the same version is already installed, go to `C:\Users\[your user]\.nuget\packages\EducationExtension` and delete that version

1. Move the .nupkg to a well known local directory, e.g. `C:\packages`

2. To verify, run `#r "nuget: [packagename], [version number or *-* if latest]"` in a new notebook.

---

## From a local build

1. `dotnet pack` in the solution directory.

2. A nupkg will be located at `[solution directory]\Extension\Debug` 

3. Follow "From .nupkg files"

---

## Tips and Tricks

After installing the package, if you wish to reinstall a different build of the package that has the same version, be sure to delete the existing installation. This can be found at `C:\Users\[your user]\.nuget\packages\EducationExtension`.

In the case that you have a local package source that is higher priority than the online package source, if you wish to reinstall a package from an online source, be sure to delete the *.nupkg of the same version in your local source.

Depending on your workflow, it might be useful to have a command, such as

> For bash/powershell: `rm -r "C:\Users\[your user]\.nuget\packages\EducationExtension" ; rm "C:\packages\EducationExtension.0.0.1.nupkg" ; copy "C:\dev\intern2021\src\Extension\bin\Debug\EducationExtension.0.0.1.nupkg" "C:\packages"`

or 

> For windows command prompt: `rmdir /s /q "C:\Users\[your user]\.nuget\packages\EducationExtension" ; del /q "C:\packages\EducationExtension.0.0.1.nupkg" ; copy "C:\dev\intern2021\src\Extension\bin\Debug\EducationExtension.0.0.1.nupkg" "C:\packages"`

This command 
1. deletes already installed EducationExtension versions
2. deletes the nupkg file in the local package source
3. moves the newly packed nupkg in your repo to the local package source

Adapt for your own uses. Bonus points: set a shell alias for this command.
