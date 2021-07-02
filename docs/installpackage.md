# Installing Our Package

Our package name is currently `EducationExtension`.

Our top level namespace is currently `Extension`.

Our GPR source is `https://nuget.pkg.github.com/dotnet-interactive-learning/index.json`.

## From GitHub Package Registry (GPR) NuGet Source

1. Install NuGet CLI. See [Nuget CLI Reference](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference). 

2. From the terminal, type `nuget` to verify you have installed sucessfully.

1. Get a GitHub personal access token.
    
    1. Github settings > Developer settings > Personal access tokens
    2. Generate new token
    3. Add a note for your token, e.g. "GPRread"
    4. Enable `read:packages` scope
    5. Copy your PAT
    
3. `nuget source add -Name [mysourcename] -Source [GPR source] -Username [Github username] -Password [your PAT]`.

4. Check that the source was added using `dotnet nuget source list`.

5. To check that the authentication is correct, run `#r "nuget: [packagename], [version number or *-* if latest]"` in a notebook.

## From .nupkg files

This might be useful after manually downloading from our Github page or from a Github Actions artifact.

Prerequisites

0. `nuget source add -Name [mysourcename] -Source "C:\packages"`

1. Check that your NuGet.Config has the local package source at bottom of the list. To find your nuget config, see [here](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior).

Then

0. If the same version is already installed, go to `C:\Users\[your user]\.nuget\packages\EducationExtension` and delete that version

1. Move the .nupkg to a well known local directory, e.g. `C:\packages`

2. To verify, run `#r "nuget: [packagename], [version number or *-* if latest]"` in a notebook.

## From a local build

1. `dotnet pack` in the solution directory.

2. A nupkg will be located at `\Extension\Debug` 

3. Follow "From .nupkg files"

It might be useful to have a command that does all the things, such as

> `rm -r -Force "C:\Users\[your user]\.nuget\packages\EducationExtension" ; rm -Force "C:\packages\EducationExtension.0.0.1.nupkg" ; copy "C:\dev\intern2021\src\Extension\bin\Debug\EducationExtension.0.0.1.nupkg" "C:\packages"`

This command deletes already installed EducationExtension versions. Deletes the nupkg file in the local package source. Then moves the newly packed nupkg to the lcoal package source.
