<h1 align="center">
  <img src="https://raw.githubusercontent.com/lizoc/textscript/master/icon.png" height="160" width="160"/>
  <p align="center">TextScript!</p>
  <p align="center" style="font-size: 0.5em">Text templating with .NET comfort</p>
</h1>
<p align="center">
    <a href="https://www.nuget.org/packages/Lizoc.TextScript"><img src="https://img.shields.io/nuget/v/Lizoc.TextScript.svg?style=for-the-badge" alt="NuGet Package"></a>
    <a href="https://www.powershellgallery.com/packages/TextScript"><img src="https://img.shields.io/powershellgallery/v/textscript.svg?style=for-the-badge" alt="PowerShell Gallery"></a>
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge" alt="MIT License"></a>
</p>

**This project is a fork over [Scriban](https://github.com/lunet-io/scriban)**

What does this do?
==================
TextScript is a text templating engine. Designed to be powerful yet easy to learn, TextScript aims to provide the same functionality of liquid and more. TextScript is written entirely in .NET, so there's absolutely no need for ruby, python or a boatload of NodeJS npms! 

TextScript is **<font color="red">forked from</font> [Scriban](https://github.com/lunet-io/scriban)** in late 2018, with a modified parser and enhanced built-in functionality

TextScript is <font color="red">**initially forked from Scriban**</font> in late 2018, and the code base has diverged since. Some primary differences are:

- PowerShell cmdlet with full support for hashtables
- A more terse syntax
- Lesser dependencies
- Built-in LINQ-like functionality

You can [read more](./docs/scriban_compared.md) about a list of differences here.


Show me the money
-----------------
How about some good old templating done right inside PowerShell?

```Powershell
ipmo TextScript
@{ name = 'Partner' } | ConvertFrom-Template -Template 'Howdy {{ name }}!'
# Howdy Partner!
```

Same thing in C#:

```C#
// Parse a template
var template = Template.Parse("Hello {{name}}!");
var result = template.Render(new { Name = "World" }); // => "Hello World!" 
```

TextScript borrows from the very popular [liquid](http://liquidmarkup.org/), which you are encouraged to look at too. In most of the cases, there is no difference at all:

```Powershell
ipmo TextScript
@{
  food = @(
    { name = 'cabbage'; price = '$10'; fullDesc = "Fresh and green" }
    { name = 'banana';  price = '$5';  fullDesc = "Minion's favorite" }
    { name = 'beef';    price = '$50'; fullDesc = "Meatlovers delight"}
  )
} | ConvertFrom-Template @'
<ul id='cart'>
  {{ for item in food }}
    <li>
      <h2>{{ item.name }}</h2>
           Price: {{ item.price }}
           {{ item.full_desc | string.truncate 15 }}
    </li>
  {{ end }}
</ul>
'@
```


Highlights
----------
- Engineered for **speed** and **efficiency**. Near real-time rendering without a dent on the CPU.
- Full access to the lexer. The **full AST** is exposed in .NET. Regex-based parsers are so yesterday!
  - Pin-point the exact location within the template (path, column and line) when an error occurs.
  - **Write an AST to text** with [`Template.ToText`](docs/runtime.md#ast-to-text). You can manipulate templates in memory and re-save them to disk, which is handy for **roundtrip script update scenarios**
- **`liquid` compatibility mode** with the `Template.ParseLiquid` method:
  - While the `liquid` language is less powerful, we know migrating existing code can be a chore. This mode allows for a drop-in replacement for your existing `liquid` templates.
  - With [AST to text](docs/runtime.md#ast-to-text) mode, you can migrate a `liquid` template to TextScript automatically (by using `Template.ParseLiquid`, then `Template.ToText`)
  **Extensible runtime** providing many extensibility points
- [OCD level control over whitespace output](docs/language.md#14-whitespace-control)
- [Full feature language](docs/language.md), including logic flow control such as `if`/`else`, `for`, and `while`, [expressions](docs/language.md#8-expressions) (`x = 1 + 2`), conditions... etc.
- [Function calls and pipes](docs/language.md#88-function-call-expression). Be functional! `myvar | string.capitalize`
  - [Custom functions](docs/language.md#7-functions). Write your own functions directly inside a template, using the `func` statement. 
  - Support for **function pointers and delegates** via the `alias @ directive`
  - Bind [.NET custom functions](docs/runtime.md#imports-functions-from-a-net-class) from the runtime API with [many options](docs/runtime.md#the-scriptobject) for interfacing with .NET objects.
- [Complex objects](docs/language.md#5-objects) 
  - Construct your objects JavaScript/JSON styled: `x = {mymember: 1}`
  - [Arrays too!](docs/language.md#6-arrays) `x = [1,2,3,4]`
- Pass [a block of statements](docs/language.md#98-wrap-function-arg1argn--end) to a function, typically used by the `wrap` statement
- Essential and handy [built-in functions](docs/builtins.md):
  - [`arrays`](docs/builtins.md#array-functions)
  - [`date`](docs/builtins.md#date-functions)
  - [`fs`](docs/builtins.md#fs-functions)
  - [`html`](docs/builtins.md#html-functions)
  - [`maths`](docs/builtins.md#math-functions)
  - [`object`](docs/builtins.md#object-functions)
  - [`regex`](docs/builtins.md#regex-functions)
  - [`string`](docs/builtins.md#string-functions)
  - [`timespan`](docs/builtins.md#timespan-functions)
- [Multi-line statements](docs/language.md#11-code-block) without having to embrace each line by `{{...}}`
- [Safe parser](docs/runtime.md#the-lexer-and-parser) and [safe runtime](docs/runtime.md#safe-runtime), allowing you to control what objects and functions are exposed


Documentation
--------------
Go to the [documentation page](docs/README.md) to get started.


Binaries
--------
Prebuilt binaries are available in the [release tab](./releases). 

To integrate TextScript in your .NET project, reference our NuGet package in your `packages.config` or project file: [![NuGet](https://img.shields.io/nuget/v/Lizoc.TextScript.svg)](https://www.nuget.org/packages/Lizoc.TextScript/)

TextScript can run on the following platforms:

- Windows: `.NET 4.6.2`
- Linux: `NetStandard1.3` 

To install our PowerShell cmdlet:

```powershell
Install-Package TextScript
```

Platform specific implementations for PowerShell:

- Windows Desktop/Server/Core: `.NET 4.6.2`
- Windows Nano: `NETStandard 1.6`
- Linux: `NETStandard 2.0`


Benchmarks
----------
**We aim for real-time rendering**! Compete results are updated in our [benchmarks document](docs/benchmarks.md).


License
-------
See the [LICENSE](./LICENSE) file for licensing information.


Related projects
----------------
We take inspirations from the following repos. For third party licensing info, refer to the [third party license](./THIRD-PARTY-LICENSE.txt) file.

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine
* [Fluid](https://github.com/sebastienros/fluid/) .NET liquid templating engine
* [Nustache](https://github.com/jdiamond/Nustache): Logic-less templates for .NET
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js

