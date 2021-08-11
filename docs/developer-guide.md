# Developer Instructions


## Tools

You will need to install the latest [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) for your platform.

This project supports Visual Studio. Any version, including the free Community Edition, should be sufficient, as long as you install Visual Studio support for .NET development.

This project also supports using Visual Studio Code. Install the C# extension to get started.

---

## Build and test (command line)

Navigate to the root directory of our solution, which is `src`. Then, use the .NET CLI to build or test our package:

```
dotnet build
```
or
```
dotnet test
```

---

## Using a local build of our package in a notebook

1. Add the build output directory as a package source to your NuGet config

    1. Check out [Configuring NuGet Behavior](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior). Editing the User level NuGet config is typically a good starting point.

    2. Under `<packageSources>` add something like

        ```
        <add key="LocalJourneyBuild" value="directory_to_your_project_root\src\Microsoft.DotNet.Interactive.Journey\bin\Debug" />
        ```

2. Build and pack

3. In a notebook, use your local build using

    ```
    #r "nuget: Interactive.Journey, your_local_build_version"
    ```

4. If you rebuild with changes and want to use the new one, you will need to delete the previous installation of any locally built package with the same version.

    1. NuGet packages are installed in the global-packages folder, whose location can be found [here](https://docs.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders)

    2. Delete our package.


---

## More Information

Our package is powered by .NET Interactive. More information can be found [here](https://github.com/dotnet/interactive).