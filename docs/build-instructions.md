Build Instructions
==================
If you want to build from source:

```batch
build configure
build debug *
build release *
```

You need to do this on a Windows PC. Tested on Windows 10. The build script will download the .NET Core SDK if required.


Adding custom functions
-----------------------
If you have modified any files in `src\Lizoc.TextScript\Source\Lizoc\TextScript\Functions`, you will need to update the signature bindings file and a unit test file. Perform the build steps above after you have made your modifications, then:

```batch
ren .\src\Lizoc.TextScript\Source\Lizoc\TextScript\Runtime\CustomFunctions.Generated.cs .\src\Lizoc.TextScript\Source\Lizoc\TextScript\Runtime\CustomFunctions.Generated.cs.bak
.\working\bin\TextScriptCodeGen\Debug\net462\TextScriptCodeGen.exe .\working\bin\Lizoc.TextScript\Debug\net462\Lizoc.TextScript.dll .\src\Lizoc.TextScript\Source\Lizoc\TextScript\Runtime\CustomFunctions.Generated.cs
```

Open up `src\Lizoc.TextScript.Tests\Resource\400-builtins\400-builtins.out.txt` with your text editor and update the list of expected built-in functions (arranged in alphabetical order).

Then perform the build steps at the top again.
