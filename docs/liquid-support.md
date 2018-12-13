Liquid Compatibility
====================
TextScript supports all [core liquid syntax](https://shopify.github.io/liquid/) types, operators, tags and filters.

- [Known issues](#known-issues)
- [Types](#supported-types)
- [Operators](#supported-operators)
- [Tags](#supported-tags)
  - [Variable and properties accessors](#variable-and-properties-accessors)
  - [`comment` tag](#comment-tag)
  - [`raw` tag](#raw-tag)
  - [`assign` tag](#assign-tag)
  - [`if` tag](#if-tag)
  - [`unless` tag](#unless-tag)
  - [`case` and `when` tags](#case-and-when-tags)
  - [`for` tag](#for-tag)
  - [`tablerow` tag](#tablerow-tag)
  - [`capture` tag](#capture-tag)
  - [Pipe calls](#pipe-calls)
- [Filters](#supported-filters)

Known issues
------------
Bear in mind that there is no strictly formalized liquid syntax. There are many custom tag implementations out there that freely decide on their arguments.

For example, we support passing arguments to tags and filters like this:

```liquid
{{ "this is a string" | function "arg1" 15 16 }}
{% custom_tag "arg1" 15 16 %}
```

But liquid implementations exists that choose their own syntax for specifying arguments:

```liquid
{% avatar user=author size=24 %}
```

We find multiple versions of liquid implementations that have really different syntax, and it is really not a good idea to support them all. 

As a consequence, **the liquid parser implemented in TextScript cannot parse any custom liquid tags/filters that are using custom arguments parsing**
but only regular arguments (strings, numbers, variables, variable properties) separated by spaces.

[:top:](#liquid-support)


Supported types
---------------
Liquid types are translated to the same types in TextScript:

- [string](language.md#31-strings)
- [number](language.md#32-numbers)
- [boolean](language.md#33-boolean)
- [array](language.md#6-arrays) -- except that you can create an array directly in TextScript!

The `nil` value (which can't be expressed in liquid) is equivalent to the expression [`null`](language.md#34-null) in TextScript.

In addition to liquid, TextScript supports the creation of an [`object`](language.md#5-objects)

[:top:](#liquid-support)


Supported operators
-------------------
Liquid supports only conditional expressions and they directly translate to [conditionnal expressions](language.md#85-conditional-expressions) in TextScript.

In addition, TextScript supports:

- [binary operators](language.md#84-arithmetic-expressions)
- [unary operators](language.md#86-unary-expressions)
- [range `1..x` expressions](language.md#87-range-expressions)
- [The null coalescing operator `??`](language.md#88-the-null-coalescing-operator-)

[:top:](#liquid-support)


Supported tags
--------------
This is a list of "how to do this in TextScript" compared to liquid.

> **NOTE**
> All the following examples are using the feature [**Ast to text**](runtime.md#ast-to-text) that
> allows you to translate liquid templates to TextScript automatically.

[:top:](#liquid-support)

### Variable and properties

| Liquid                           | TextScript
|----------------------------------|-----------------------------------
| `{% assign variable = 1 %}`      | `{{ variable = 1 }}`
| `{{ variable }}`                 | `{{ variable }}`
| `{{ my-handle }}`                | `{{ this["my-handle"] }}`
| `{{ page.title }}`               | `{{ page.title }}`
| `{% assign for = 5 %}`           | `{{ (for) = 5 }}` (keywords needs to be bracketed in TextScript)
| `{{ for }}`                      | `{{ (for) }}`
| `{{ products[0].title }}`        | `{{ products[0].title }}`
| `{{ product.empty? }}`           | `{{ product.empty? }}`

[:top:](#liquid-support)

### `comment` tag

Liquid `comment`/`endcomment` tags in TextScript are code block `{{` ... `}}` embracing a [multiline comments `##`](language.md#2-comments)

> **liquid**
```liquid
This is plain {% comment %}This is comment {% with ## some tag %} and comment{% endcomment %}
```
> **textscript**
```textscript
This is plain {{## This is comment {% with \#\# some tag %\} and comment ##}}
```

[:top:](#liquid-support)

### `raw` tag

Liquid raw tag block translate to an [escape block](language.md#13-escape-block)

> **liquid**
```liquid
This is plain {% raw %}This is raw {% with some tag %} and raw{% endraw %}
```
> **textscript**
```textscript
This is plain {%{This is raw {% with some tag %} and raw}%}
```

[:top:](#liquid-support)

### `assign` tag

There is no `assign` tag in TextScript compared to liquid. Just use a simple [assignment expression](language.md#82-assign-expression)

> **liquid**
```liquid
{% assign variable = 1 %}
{{ variable }}
```
> **textscript**
```textscript
{{ variable = 1 }}
{{ variable }}
```

[:top:](#liquid-support)

### `if` tag

Liquid `if <expression>`/`endif` tags translate to a similar [`if <expression>`/`end`](language.md#92-if-expression-else-else-if-expression). Just swap `endif` for `end`.

> **liquid**
```liquid
{% assign variable = 1 %}
{% if variable == 1 %}
  This is a variable with 1
{% endif %}
```
> **textscript**
```textscript
{{ variable = 1 }}
{{ if variable == 1 }}
  This is a variable with 1
{{ end }}
```

[:top:](#liquid-support)

### `unless` tag

Thereo is no direct equivilant of the liquid `unless <expression>`/`endunless` tags. Instead, use an [`if <expression>`/`end`](language.md#92-if-expression-else-else-if-expression) with a reversed nested `!(expression)`

> **liquid**
```liquid
{% assign variable = 1 %}
{% unless variable == 1 %}
  This is not a variable with 1
{% endunless %}
```

> **textscript**
```textscript
{{ variable = 1 }}
{{ if!( variable == 1 )}}
  This is not a variable with 1
{{ end }}
```

[:top:](#liquid-support)

### `case` and `when` tags

Liquid `case <variable>`/`when <expression>`/`endcase` tags translate to a similar [`case <expression>`/`when <expression>`/`end`](language.md#93-case-and-when). Like `if`, TextScript use a common keyword `end` to denote the end of a scope (`endcase` vs `end`).

> **liquid**
```liquid
{%- assign variable = 5 -%}
{%- case variable -%}
    {%- when 6 -%}
        Yo 6
    {%- when 7 -%}
        Yo 7
    {%- when 5 -%}
        Yo 5
{% endcase -%}
```

> **textscript**
```textscript
{{ variable = 5 -}}
{{ case variable -}}
    {{ when 6 -}}
        Yo 6
    {{- when 7 -}}
        Yo 7
    {{- when 5 -}}
        Yo 5
{{ end }}
```

[:top:](#liquid-support)

### `for` tag

Liquid `for <variable> in <expression>`/`endfor` tags translate to a similar [`for`/`end`](language.md#for-variable-in-expression--end) (`endfor` vs `end`):

> **liquid**
```liquid
{%- for variable in (1..5) -%}
    This is variable {{variable}}
{% endfor -%}
```

> **textscript**
```textscript
{{ for variable in (1..5) -}}
    This is variable {{variable}}
{{ end }}
```

> **NOTE:** TextScript supports all tags arguments: `limit`, `offset`, `reversed`

[:top:](#liquid-support)

### `tablerow` tag

Liquid `tablerow <variable> in <expression>`/`endtablerow` tags is exactly the same [`tablerow`/`end`](language.md#tablerow-variable-in-expression--end) in TextScript.

> **liquid**
```liquid
{%- tablerow variable in (1..5) -%}
    This is variable {{variable}}
{% endtablerow -%}
```

> **textscript**
```textscript
{{ tablerow variable in (1..5) -}}
    This is variable {{variable}}
{{ end }}
```

> NOTE: TextScript supports all tags arguments for `tablerow`: `cols`, `limit`, `offset`, `reversed`

[:top:](#liquid-support)

### `capture` tag

Liquid `capture <variable>`/`endcapture` tags translate to a similar [`capture <expression>`/`end`](language.md#94-capture-variable--end) (`endcapture` vs `end`)

> **liquid**
```liquid
{%- capture variable -%}
    This is a capture
{%- endcapture -%}
{{ variable }}
```

> **textscript**
```textscript
{{ capture variable -}}
    This is a capture
{{- end -}}
{{ variable }}
```

[:top:](#liquid-support)

### Pipe calls

Liquid pipe call works exactly the same way in TextScript [`pipe call`](language.md#89-function-call-expression).

> **liquid**
```liquid
{% assign test = "abcdef" %}
{{ test | truncate: 5 }}
```

> **textscript**
```textscript
{{ test = "abcdef" }}
{{ test | string.truncate 5 }}
```
TextScript works by translating liquid tags to the corresponding TextScript function. However, you can also use `LiquidTemplateContext` for direct tag calls. See [liquid support in runtime](runtime.md#liquid-support).

[:top:](#liquid-support)


Supported filters
-----------------
By default, all liquid filters are translated to TextScript [builtin functions](builtins.md) (through objects like `string` or `array`)

The translation is performed by the [LiquidBuiltinsFunctions mapping](../src/Lizoc.TextScript/Source/Lizoc/TextScript/Functions/LiquidBuiltinsFunctions.cs) in the parser.

This translation can be disabled by setting the `ParserOptions.ConvertLiquidFunctions` property to `false`.

[:top:](#liquid-support)
