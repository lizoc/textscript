Language
========
In this tutorial you will learn the syntax of the TextScript templating language.

> **NOTE:**
> We will not cover the [syntax](https://shopify.github.io/liquid/) of the `liquid` language here. `liquid` is closely related 
> to TextScript, but you don't need to know `liquid` to follow this tutorial.

## Table of Contents

- [1. Blocks](#1-blocks)
  - [1.1 Code block](#11-code-block)
  - [1.2 Escape block](#12-escape-block)
  - [1.3 Text block](#13-text-block)
  - [1.4 Whitespace control](#14-whitespace-control)
- [2. Comments](#2-comments)
- [3. Literals](#3-literals)
  - [3.1 Strings](#31-strings)
  - [3.2 Numbers](#32-numbers)
  - [3.3 Boolean](#33-boolean)
  - [3.4 null](#34-null)
- [4. Variables](#4-variables)
  - [4.1 The special variable `this`](#41-the-special-variable-this)
  - [4.2 The special variable `empty`](#42-the-special-variable-empty)
- [5. Objects](#5-objects)
  - [5.1 The special property `empty?`](#51-the-special-property-empty)
- [6. Arrays](#6-arrays)
  - [6.1 Array with properties](#61-array-with-properties)
  - [6.2 The special `size` property](#62-the-special-size-property)
- [7. Functions](#7-functions)
- [8. Expressions](#8-expressions)
  - [8.1 Variable path expressions](#81-variable-path-expressions)
  - [8.2 Assign expression](#82-assign-expression)
  - [8.3 Nested expression](#83-nested-expression)
  - [8.4 Arithmetic expressions](#84-arithmetic-expressions)
    - [On numbers](#on-numbers)
    - [On strings](#on-strings)
  - [8.5 Conditional expressions](#85-conditional-expressions)
  - [8.6 Unary expressions](#86-unary-expressions)
  - [8.7 Range expressions](#87-range-expressions)
  - [8.8 The null-coalescing operator `??`](#88-the-null-coalescing-operator-)
  - [8.9 Function call expression](#89-function-call-expression)
    - [Named arguments](#named-arguments)
- [9. Statements](#9-statements)
  - [9.1 Single expression](#91-single-expression)
  - [9.2 `if` ... `else` ... `else if` ... `end`](#92-if--else--else-if--end)
    - [Truthy and Falsy](#truthy-and-falsy)
  - [9.3 `case` ... `when`](#93-case--when)
  - [9.4 Loops](#94-loops)
    - [`for` ... `in` ... `end`](#for--in--end)
      - [The `offset` parameter](#the-offset-parameter)
      - [The `limit` parameter](#the-limit-parameter)
      - [The `reversed` parameter](#the-reversed-parameter)
    - [`while` ... `end`](#while--end)
    - [`tablerow` ... `in` ... `end`](#tablerow--in--end)
      - [The `cols` parameter](#the-cols-parameter)
    - [Special loop variables](#special-loop-variables)
    - [`break` and `continue`](#break-and-continue)
  - [9.5 `capture` ... `end`](#95-capture--end)
  - [9.6 `readonly &lt;variable&gt;`](#96-readonly-variable)
  - [9.7 `import &lt;variable&gt;`](#97-import-variable)
  - [9.8 `with` ... `end`](#98-with--end)
  - [9.9 `wrap &lt;function&gt; &lt;arg1...argn&gt;` ... `end`](#99-wrap-function-arg1argn--end)
  - [9.10 `include &lt;name&gt; arg1?...argn?`](#910-include-name-arg1argn)
  - [9.11 `ret &lt;expression&gt;?`](#911-ret-expression)
- [10. Built-in functions](builtins.md)

[:top:](#language)


1 Blocks
---------
There are 3 types of block in a template:

| Type       | Description
|------------|--------------------------------
| **Code**   | Contains template code
| **Text**   | A plain block of text that is outputed *as is*
| **Escape** | A **text block** that contains **code**

Let's talk more about each of these types.

[:top:](#language)

### 1.1 Code block

A block enclosed by `{{` and `}}` is a **code block**. The templating engine will evaluate its content as a scripting language.

A code block may contain:

- a **single line expression statement**:
   `{{ name }}`
- or **multiline statements**:
    ```
    {{
      if !name
        name = "default"
      end
      name
    }}
    ```
- you can compact multiline statements into a single line using semi-colon `;` separators:
    ```
    {{if !name; name = "default"; end; name }}
    ```

Inside a code block, except for the end-of-line after each statement, the parser generally ignores white spaces characters. The only case where whitespace matters is when we need to disambiguate between an array indexer and an array initializer:

```
{{
  foo[10]   # item at index position 10 in array called foo
  foo [10]  # create an array called foo with parameters
}}
```

If a statement is an expression that is not assigned to anything, the result of the expression will be output to the rendering output of the template:

> **input**
```template-text
{{
  x = "5"   # This assignment will not output anything
  x         # This expression will print 5
  x + 1     # This expression will print 6
}}
```
> **output**
```html
56
```

Note that in the previous example, there is no end-of-line between `5` and `6` because we are inside a code block. To insert a line break between `5` and 
`6`, you can output a newline character `^n` between them. Alternatively, you can mix code and text blocks:

> **input**
```template-text
{{ x = "5"; x }}
{{ x + 1 }}
```
> **output**
```html
5
6
```

[:top:](#language)


### 1.2 Escape block

Anything inside an escape block `{%{` and `}%}` is outputed as-is. You can have **code blocks** inside an escape block and the parser will not attempt 
to interpret them as script.

For example:

> **input**
```template-text
{%{
  Hello this is {{ name }}
}%}
```
> **output**
```html
Hello this is {{ name }}
``` 

If you want to escape an escape block, increase the number of `%` in the starting and ending block:

> **input**
```template-text
{%%{
  This is an {%{ escaped block }%} here
}%%}
```
> **output**
```html
This is an {%{ escaped block }%} here
```

This way, you can effectively nest escape blocks and still be able to escape them. 

Escape blocks need to match. Hence a starting escape block `{%%%%{` will required an ending `}%%%%}`.

[:top:](#language)


### 1.3 Text block

Anything that is outside an code block or escape block is a **text block**. The parser will simply output them as-is:

```
Hello this is {{ name }}, welcome!
______________          _____________________
^ text block            ^ text block

```

[:top:](#language)



### 1.4 Whitespace control

By default, any whitespace (including new lines) before or after a code or escape block are copied as-is to the output. 

> **input**
```template-text
{{ 'foo' }}
bar
```
> **output**
```html
foo
bar
```

There are **two ways** for controlling whitespace:

- The **greedy consumer** using the character `-` (e.g `{{-` or `-}}`), **removes any whitespace, including newlines** 
  Examples with the variable `name = "foo"`:
  
  * Strip whitespace to the left:  
    > **input**
    ```template-text
    This is a <       
    {{- name}}> text
    ``` 
    > **output**
    ```html
    This is a <foo> a text
    ```
    
  * Strip to the right:  
    > **input**
    ```template-text
    This is a <{{ name -}} 
    > text:       
    ``` 
    > **output**
    ```html
    This is a <foo> text
    ```
  
  * Strip both left and right:  
    > **input**
    ```template-text
    This is a <
    {{- name -}} 
    > text:       
    ```
    > **output**
    ```html
    This is a <foo> text
    ```

- The **non greedy consumer** uses the character `~`:
  - Using a `{{~` will remove any **whitespace before** but will **stop on the first newline without including it**
  - Using a `~}}` will remove any **whitespace after including the first newline** but will stop after

  This mode is very convenient when you want to use only a statement on a line, but want that line to be completely 
  removed from the output, but to keep spaces before and after this line intact.

  In the following example, we want to remove entirely the lines `{{~ for fruit in basket ~}}` and `{{~ end ~}}`, but we want
  to keep the indentation of the opening `<li>`.

  Using the greedy consumer `{{-` ... `-}}` would have removed all whitespace and lines and would have put the results on a single line.

  > **input**
  ```
  <ul>
      {{~ for fruit in basket ~}}
      <li>{{ fruit }}</li>
      {{~ end ~}}
  </ul>
  ```
  > **output**
  ```
  <ul>
      <li>Orange</li>
      <li>Banana</li>
      <li>Apple</li>
  </ul>
  ```

Both `~` and '-' can also be used with **escape blocks** `{%%{~` or `~}%%}` or `{%%{-` or `-}%%}`

[:top:](#language)


2 Comments
----------
Within a code block, anything to the right of a single line comment `#` is ignored:

`{{ name   # this is a single line comment }}`

You can have multi-line comments by putting them between 2 `##`:

> **input**
```template-text
{{ ## This 
is a multi
line
comment ## }}
```
> **output**
```html

```

Both single line and multi-line comments can be closed by the presence of a code block exit tag `}}`

[:top:](#language)


3 Literals
-----------
### 3.1 Strings

There are two ways to define strings:

- **regular strings** are enclosed by double quotes `"..."` or single quotes `'...'`. To create a multiline string, simply use line breaks inside quotes, like this:
  > **input**
  ```template-text
  {{ "this text spans
  two lines!" }}
  ``` 
  > **output**
  ```html 
  this text spans
  two lines!
  ``` 

  If you want the string to contain the following characters, they must be preceded by the caret escape character `^`:
  - `^n` new line
  - `^r` carriage return
  - `^t` tab
  - `^b` backspace
  - `^f` form feed
  - `^uxxxx` where xxxx is a unicode hexa code number `0000` to `ffff` 
  - `^x00-^xFF` a hexadecimal ranging from `0x00` to `0xFF`
  - `^^` caret

  The only difference between single and double quote is that the quote character needs to escape itself when inside the string. For example, you can write `foo'bar` 
  as `"foo'bar"`, but must escape it with single quotes `'foo^'bar`. Vice verse, `foo"bar` can be written as `'foo"bar'` or `"foo^"bar`. In either case, preceding the 
  quote character with an escape character `^` escapes it:
  - `"foo^'bar"` becomes `foo'bar`
  - `'foo^"bar'` becomes `foo"bar`

- **verbatim strings** are enclosed by backstick quotes `` `...` ``. Verbatim strings does not support any form of escaping. They are, for example, useful for regex patterns:
  > **input**
  ```template-text
  {{ "this is a text" | regex.split `\s+` }}
  ``` 
  > **output**
  ```html 
  [this, is, a, test]
  ``` 

[:top:](#language)


### 3.2 Numbers

A number like `{{ 100 }}` is similar to JavaScript: 
- Integers: `100`, `1e3`
- Floats: `100.0`, `1.0e3`, `1.0e-3` 

[:top:](#language)


### 3.3 Boolean

The boolean value `{{ true }}` or `{{ false }}`

> **input**
```template-text
{{ true }}
{{ false }}
```
> **output**
```template-text
true
false
```

[:top:](#language)


### 3.4 null

The null value is expressed as `{{ null }}`.

When resolving to a string output, the null value will output an empty string:

> **input**
```template-text
{{ null }}
```
> **output**
```html

```

[:top:](#language)


4 Variables
-----------
Variables can be either **global** or **local**.

A **global/property variable** must start with a letter or underscore `_`, and may be followed by letters `A-Z a-z`, digits `0-9`, and underscore `_`.

The following are valid variable names:

- `var` 
- `var9`
- `_var`

> **NOTE:**
> In liquid, the character `-` is allowed in a variable name, but when translating it to TextScript, you will have to enclose it into a quoted string.

A **local variable** always starts with `$`. A local variable is only accessible within the same include page or function body.

There is a **special local variable** `$`. It is an array containing the arguments passed to the current function or include page.

Along with `$`, there are special local variables `$0`, `$1` ... `$n`, which are just shorthand for `$[0]`, `$[1]` ... `$[n]`. For example, `$0` returns 
the item at index position 0 in `$`.


### 4.1 The special variable `this`

The `this` variable gives you access to the current object bound where you have access to all local variables for the current scope.

To illustrate:

> **input**
```template-text
{{
a = 5
a    # output 5
this.a = 6
a    # output 6
this["a"] = 7
a    # output 7
}}
```
> **output**
```html
567
```
In the case of the `with` statement, the `this` variable refers to the object passed to `with`:

> **input**
```template-text
{{
a = {x: 1, y: 2}
with a
    b = this
end
b.x
}}
```
> **output**
```html
1
```


### 4.2 The special variable `empty`

The `empty` variable represents simply an empty object. You can compare an object with the `empty` object to check if it is empty or not:

> **input**
```template-text
{{
a = {}
b = [1, 2]~}}
{{a == empty}}
{{b == empty}}
```
> **output**
```html
true
false
```

[:top:](#language)


5 Objects
---------
Objects are expressed like JavaScript: `{...}`.

You can initialize an empty object like this:

`{{ myobject = {} }}` 

Or you may want to initialize an object with some members:

`{{ myobject = { member1: "yes", member2: "no" } }}`

Or use JSON:

`{{ myobject = { "member1": "yes", "member2": "no" } }}`

Of course, breaking your code over multiple lines improves readability:

```
{{
  myobject = { 
      member1: "yes", 
      member2: "no" 
  } 
}}
```

Members of an object can be accessed in 2 ways:

- `{{ myobject.member1 }}`, or 
- `{{ myobject["member1"] }}`

You can also add members to it dynamically with simple assignment:

> **input**
```template-text
{{
  myobject = {} 
  myobject.member3 = "may be" 
  myobject.member3
}}
``` 
> **output**
```html
may be
``` 

> **NOTE**
>
> This only works if the object is a "pure" TextScript object: created in the template with `{...}` or instantiated by the runtime as a `ScriptObject`. You can expose 
> .NET objects to a template (discussed later), but it won't support dynamic member assignment.
>
> Also to note, TextScript automatically rename imported .NET object members to snake case, following `liquid` convention. For instance, a property like `IsInstanceRunning` 
> will be renamed as `is_instance_running`. You can use your own rename strategy with the [`MemberRenamer`](runtime.md#member-renamer) delegate.


### 5.1 The special property `empty?`

As an alterative to comparing an object with the `empty` variable, all objects have the special property `.empty?`, which is a boolean that indicates whether it is empty or not:

> **input**
```template-text
{{
a = {}
b = [1, 2]~}}
{{a.empty?}}
{{b.empty?}}
```
> **output**
```html
true
false
```

[:top:](#language)


6 Arrays
--------
An array can be initialized empty:

`{{ myarray = [] }}` 

Or with some items:

`{{ myarray = [1, 2, 3, 4] }}`

Like objects, you can break the initialization statement over multiple lines:

```
{{
  myarray = [ 
    1,
    2,
    3,
    4,
  ] 
}}
```

Items of an array are zero-based indexed:

`{{ myarray[0] }}`

Like objects, you can add items to an array with a simple assignment. The array will expand automatically:

```
{{
  myarray = [] 
  myarray[0] = 1 
  myarray[1] = 2 
  myarray[2] = 3 
  myarray[3] = 4 
}}
``` 

> **NOTE**
> Again, like objects, this only works with "pure" TextScript array: created with a `[...]` or instantiated by the runtime as a `ScriptArray`.
> Imported .NET enumerables won't be able to support dynamic assignments.

You can also manipulate arrays with the [`array` builtin object](#array-builtin).

> **BIG GOTCHA**
> 
> While whitespace characters are mostly not relevant during parsing, an exception to the case is where whitespace **disambiguate between an array indexer and an array initializer**.
>  
> For instance, if a whitespace is found before a `[` and the previous expression was a variable path expressions (see later), the following expression `[...]` will be considered as 
> an array initializer instead of an array indexer:
> 
> ```
> {{
> myfunc [1]  # There is a whitespace after myfunc. 
>             # It will result in a call to function 'myfunc', passing an array as the first argument
> 
> myvar[1]    # Without a whitespace, this is accessing 
>             # an item in the array called 'myvar'
> }}    
> ```


### 6.1 Array with properties

You can treat an array as an object by attaching properties:

> **input**
```template-text
{{
a = [5, 6, 7]
a.x = "yes"
a.x + a[0]
}}
```
> **output**
```html
yes5
```


### 6.2 The special `size` property

Arrays have a `size` property that can be used to query the number of items in the array:

> **input**
```template-text
{{
a = [1, 2, 3]
a.size
}}
```
> **output**
```html
3
```

[:top:](#language)


7 Functions
-----------
You can define your own functions right inside your template:

The following declares a function `sub` that uses its first argument and subtract from it the second argument:

``` 
{{func sub
   ret $0 - $1
end}}
``` 

All argument are passed to the special variable `$` that will contain the list of direct arguments
and named arguments:

- `$0` or `$[0]` will access the first argument
- `$1` or `$[1]` will access the second argument
- `$[-1]` will access the last argument
- `$.named` will access the named argument `named` 

This function can then be used:

> **input**
```
{{sub 5 1}}
{{5 | sub 1}}
```
> **output**
```
4
4
```

As you can notice from the example above, when using the pipe, the result of the pipe is pushed as the **first argument** of the pipe receiver.

Note that you can mix text block and code block inside a function:

``` 
{{func inc}}
   This is a text with the following argument {{ $0 + 1 }}
{{end}}
``` 

Because functions are object, they can be stored into a property of an object, using the alias `@` operator:

```
{{
myobject.myinc = @inc  # Use the @ alias operator to allow to 
                       # use a function without evaluating it
x = 1 | myobject.myinc # x = x + 1
}}

```

The function aliase operator `@` allows you to pass a function as a parameter to another function, enabling powerful functional programming compositions.

[:top:](#language)


8 Expressions
-------------
TextScript supports both unary and binary expressions.

[:top:](#language)


### 8.1 Variable path expressions

A variable path expression contains the path to a variable:

* A simple variable access: `{{ name }}` resolve to the top level variable `name`
* An array access: `{{ myarray[1] }}` resolve to the top level variable `name`
* A member access: `{{ myobject.member1.myarray[2] }}` resolve to the top level variable `myobject`, then the property `member1` this object, the property `myarray` and an indexer to the array returned by `myarray`

Note that a variable path can either point to a simple variable or can result into calling a parameter-less function. 

[:top:](#language)


### 8.2 Assign expression

A value can be assigned to a top level variable or to the member of an object or array:

* `{{ name = "foo" }}` assigns the string `foo` the variable `name` 
* `{{ myobject.member1.myarray[0] = "foo" }}`

[:top:](#language)


### 8.3 Nested expression

An expression enclosed by `(` and `)` 

`{{ name = ('foo' + 'bar') }}`

[:top:](#language)


### 8.4 Arithmetic expressions

#### On numbers

The following binary operators are supported on **numbers**: 

| Operator            | Description
|---------------------|------------
| `<left> + <right>`  | add left to right number 
| `<left> - <right>`  | substract right number from left
| `<left> * <right>`  | multiply left by right number
| `<left> / <right>`  | divide left by right number
| `<left> // <right>` | divide left by right number and round to an integer
| `<left> % <right>`  | calculates the modulus of left by right 

If left or right is a floating point number and the other is an integer, the result of the operation will be a floating point number.

[:top:](#language)


#### On strings

The following binary operators are supported on **strings**: 

| Operator           | Description
|--------------------|------------
| `'left' + <right>` | concatenates left to right string: `"ab" + "c" -> "abc"`
| `'left' * <right>` | concatenates the left string `right` times: `'a' * 5  -> aaaaa`. Left and right can be swapped as long as there is one string and one number.

For addition, as long as there is a string in a binary operation, the other part will be automatically converted to a string.

The following literals are converted to plain strings:

* `null -> ""`. e.g: `"aaaa" + null -> "aaaa"`
* `0 -> "0"`. e.g.: `"aaaa" + 0 -> "aaaa0"`
* `1.0 -> "1.0"`. e.g.: `"aaaa" + 1.0 -> "aaaa1.0"`
* `true -> "true"`. e.g.: `"aaaa" + true -> "aaaatrue"`
* `false -> "false"`. e.g.: `"aaaa" + false -> "aaaafalse"`

[:top:](#language)


### 8.5 Conditional expressions

A conditional expression produces a boolean by comparing a left and right value.

| Operator            | Description
|---------------------|------------
| `<left> == <right>` | Is left equal to right? 
| `<left> != <right>` | Is left not equal to right?
| `<left> > <right>`  | Is left greater than right? 
| `<left> >= <right>` | Is left greater or equal to right?
| `<left> < <right>`  | Is left less than right?
| `<left> <= <right>` | Is left less or equal to right?

They work with `numbers`, `strings` and `datetimes`.

You can combine conditionnal expressions with `&&` (and operator) and `||` (or operator)

| Operator            | Description
|---------------------|------------
| `<left> && <right>` | Is left true and right true? 
| `<left> || <right>` | Is left true or right true?

[:top:](#language)


### 8.6 Unary expressions

| Operator            | Description
|---------------------|------------
| `! <expression>`    | Boolean negate an expression. e.g `if !page` 
| `+ <expression>`    | Arithmetic positive an expression. e.g `+1.5`
| `- <expression>`    | Arithmetic negate an expression  
| `^ <expression>`    | Expand an array passed to arguments of a function call (see function call)
| `@ <expression>`    | Alias the result of an expression that would be evaluated if it was a function call

[:top:](#language)


### 8.7 Range expressions

They are special binary expressions that provides an iterator (used usually with the `for` statement)

The evaluated `left` and `right` expressions must resolve to an integer at runtime.

|Operator         | Description
|-----------------|------------
| `left..right`   | Returns an iterator between `left` and `right` with a step of 1, including `right`. e.g: `1..5` iterates from 1 to 5
| `left..<right`  | Returns an iterator between `left` and `right` with a step of 1, excluding `right`. e.g: `1..<5` iterates from 1 to 4


### 8.8 The null-coalescing operator `??` 

The operator `left ?? right` can be used to return the `right` value if `left` is null.

[:top:](#language)


### 8.9 Function call expression

A function can be called by passing parameters separated by a whitespace:

`{{ myfunction arg1 "arg2" (1+5) }}`

The pipe operator `|` can also be used to pipe the result of an expression to a function:

`{{ date.parse '2016/01/05' | date.to_string '%g' }}` will output `06 Jan 2016`

> **NOTE**
> When a function receives the result of a pipe call (e.g `date.to_string` in the example above), it is passed as 
> the **first argument of the call**. This is valid for both .NET custom functions as well as for built-in functions.

#### Named arguments

When passing multiple arguments to an existing .NET function, you may want to use named arguments.

Suppose you have declared a .NET function like this:

```c#
public static string MyProcessor(string left, string right, int count, string options = null)
{
    // ...
}
```

You can call this function within a template with the following syntax:

```template-text
{{ my_processor "Hello" "World" count: 15 options: "optimized" }}
```

with a pipe we could rewrite this to:

```template-text
{{ "Hello" | my_processor "World" count: 15 options: "optimized" }}
```
> Note that once arguments are named, the following arguments must be all named.

In a custom function declared with `func`, named arguments are accessible through the variable `$` as properties (and not as part of the default array arguments):

> **input**
```template-text
{{
    func my_processor
        "Argument count:" + $.count
        "Argument options:" + $["options"]
        for $x in $
            "arg[" + $x + "]: " + $x
        end
    end

    my_processor "Hello" "World" count: 15 options: "optimized"
}}
```
> **output**
```html
Argument count: 15
Argument options: optimized
arg[0]: Hello
arg[1]: World
```

[:top:](#language)


9 Statements
------------
Each statement must be terminated by a code block `}}` or an end-of-line within a code block.

[:top:](#language)


### 9.1 Single expression

An expression statement is a scripting code:

> **input**
```template-text
{{ 6 + 1 }}
```
> **output**
```html
7
```

[:top:](#language)


### 9.2 `if` ... `else` ... `else if` ... `end`

The general syntax is:

```
{{
if <expression>
  ...
else if <expression>
  ...
else 
  ...
end
}}
```

An `if` statement must be closed by an `end` or followed by `else` or `else if`. An `else` or `else if` statement must be followed by `else`, `else if` or closed by an `end` statement.

An expression evaluated for a `if` or `else if` will be converted to a boolean according to the truthy-and-falsy rules below.

#### Truthy and Falsy

Only `null` and `false` are considered as `false`.

The following values are used for converting literals to boolean:

- `0 -> true`
- `1 -> true` or any non zero value
- **`null` -> `false`**
- **`false` -> `false`**
- `non_null_object` -> `true`
- `""` -> `true` (an empty string returns **true**)
- `"foo"` -> `true` 

Example testing an object:
 
```
{{ 
  if !page 
}}Page not found!{{
  else
    page
  end
}}
``` 

[:top:](#language)


### 9.3 `case` ... `when`

Use this to choose a single section of code to execute from a list of candidates, based on value matching.

- `case <expression>` opens a switch with an expression
- `when <match>` allows to match with the specified expression and the case expression
  - `when` can also be used with multiple values separated by `,` or `||`
- A final `else` can be used to as a default handler in case nothing matched.

> **input**
```template-text
{{
    x = 5
    case x
      when 1, 2, 3
          "Value is 1 or 2 or 3
      when 5
          "Value is 5"
      else
          "Value is " + x
    end
}}
```
> **output**
```html
Value is 5
```

[:top:](#language)


### 9.4 Loops

#### `for` ... `in` ... `end`

```
{{for <variable> in <expression>}} 
  ... 
{{end}}
```

The expression can be an array or a range iterator:

- Loop on an array: `{{ for page in pages }}This is the page {{ page.title }}{{ end }}`  
- Loop on a range: `{{ for x in 1..n }}This is the loop step [{{x}}]{{ end }}`  

The `for` loop (as well as the `tablerow` statement) suports these common parameters: `offset`, `limit` and `reversed`.

##### The `offset` parameter

Specifies the first `n` items that should be skipped. Remember that array index is zero-based:

> **input**
```template-text
{{~ for $i in 4..9 offset:2 ~}}
 {{ $i }}
{{~ endfor ~}}
```
> **output**
```html
6
7
8
9
```

##### The `limit` parameter

Specifies that a maximum number of `n` loops should be performed. If the array has more than `n` items, only `n` loops are performed. If the array has less 
than `n` items, the number of loops is equal to the number of items in the array.

> **input**
```template-text
{{~ for $i in 4..9 limit:2 ~}}
 {{ $i }}
{{~ endfor ~}}
```
> **output**
```html
4
5
```

##### The `reversed` parameter

Iterate the items in the array in reverse order:

> **input**
```template-text
{{~ for $i in 1..3 reversed ~}}
 {{ $i }}
{{~ endfor ~}}
```
> **output**
```html
3
2
1
```

[:top:](#language)


#### `while` ... `end`

The block inside a `while` ... `end` statement will get executed in a loop, until `expression` evaluates to `false`.

```
{{while <expression>}}
  ...
{{end}}
```

Like the `if` statement, the `expression` must be evaluated to a boolean.


#### `tablerow` ... `in` ... `end`

This function generates HTML rows compatible with an HTML table. You should output the &lt;table&gt; tag before this, &lt;/table&gt; after.

This statement works the same way as the `tablerow` tag in `liquid`.

It has similar syntax to the `for` statement.

```
{{tablerow <variable> in <expression>}} 
  ... 
{{end}}
```

> **input**
```template-text
{{
  cart = ['apple', 'banana', 'phone', 'headset']
}}
<table>
  {{~ tablerow $p in cart | array.sort -}}
    {{ $p -}}
  {{ end ~}}
</table>
```
> **output**
```html
<table>
<tr class="row1"><td class="col1">apple</td></tr>
<tr class="row2"><td class="col1">banana</td></tr>
<tr class="row3"><td class="col1">headset</td></tr>
<tr class="row4"><td class="col1">phone</td></tr>
</table>
```

##### The `cols` parameter

Defines the number of columns of the table:

> **input**
```template-text
<table>
  {{~ tablerow $p in (cart | array.sort) limit: 4 cols: 2 -}}
    {{ $p -}}
  {{ end ~}}
</table>
```
> **output**
```html
<table>
<tr class="row1"><td class="col1">apple</td><td class="col2">banana</td></tr>
<tr class="row2"><td class="col1">headset</td><td class="col2">phone</td></tr>
</table>
```

[:top:](#language)


#### Special loop variables

The following variables are accessible within a `for` loop:

| Name                | Description
| ------------------- | -----------
| `{{for.index}}`     | The current `index` of the loop
| `{{for.rindex}}`    | The current `index` of the loop, starting from the end of the list
| `{{for.first}}`     | A boolean indicating whether this is the first cycle in the loop
| `{{for.last}}`      | A boolean indicating whether this is the last cycle in the loop
| `{{for.even}}`      | A boolean indicating whether `{{for.index}}` is **NOT** an odd number.
| `{{for.odd}}`       | A boolean indicating whether `{{for.index}}` is an odd number.
| `{{for.changed}}`   | A boolean indicating whether the current value of this cycle is equal to the value in the previous cycle

Within a `while` statement, the following variables can be used:

| Name                | Description
| ------------------- | -----------
| `{{while.index}}`     | The current `index` of the while loop
| `{{while.first}}`     | A boolean indicating whether this is the first cycle in the loop
| `{{while.even}}`      | A boolean indicating whether `{{while.index}}` is **NOT** an odd number.
| `{{while.odd}}`       | A boolean indicating whether `{{while.index}}` is an odd number.

[:top:](#language)


#### `break` and `continue`

The `break` statement allows you to exit from a loop:

```
{{
  for i in 1..5
    if i > 2
      break
    end
  end
}}
```

The `continue` statement allows you to skip the rest of the code in the loop and continue on to the next loop cycle:

```
{{
  for i in 1..5
    if i == 2
      continue
    end
    ('cycle ' + i)
  end 
}}
```

Will output:

```
cycle 1
cycle 3
cycle 4
cycle 5
```

[:top:](#language)


### 9.5 `capture` ... `end`

You can redirect the template output to a variable using the `capture <variable>` ... `end` statement:

For example:

```
{{ capture myvariable }}
This is the result of a capture!
{{ end }}
```

will set `myvariable` to `This is the result of a capture!^n`.

[:top:](#language)


### 9.6 `readonly <variable>`

Use `readonly` to turn a variable into an immutable. You won't be able to make subsequent modification to its value:

```
{{ x = 1 }}
{{ readonly x }}
{{ x = 2 }} <- this will result in a runtime error 
```

[:top:](#language)


### 9.7 `import <variable>`

The `import <variable>` statement allows you to use the members of an object as variables in the current contextual bound:

```
{{ 
  myobject = { member1: "yes" }
  import myobject
  member1  # this will print "yes" to the output
}}
``` 

[:top:](#language)


### 9.8 `with <variable>` ... `end`

The `with <variable>` ... `end` statement will open a new object context with the passed variable. All assignment will result in setting the members of the passed object. 

```
myobject = {}
with myobject
  member1 = "yes"
end
myobject.member1
```

will output:
```
yes
```

[:top:](#language)


### 9.9 `wrap <function> <arg1...argn>` ... `end`

Pass a block of statements to a function. The function can access this block of statements passed in using the special variable `$$`.

```
{{
func mywrap
	for $i in 1..<$0
		$$   # This special variable evaluates the statements inside the wrap block
	end
end

wrap mywrap 5
	$i + " -> This is inside the wrap!^n"
end
}}
```

will output:

```
1 -> This is inside the wrap!
2 -> This is inside the wrap!
3 -> This is inside the wrap!
4 -> This is inside the wrap!
```

Note that the variables declared in `mywrap` is accessible in the `wrap` block.

[:top:](#language)


### 9.10 `include <name> arg1?...argn?` 

`include` is a special function that can be used to parse and render another template. In order to use this function, you must define a delegate to a template loader [`TemplateOptions.TemplateLoader`](runtime.md#include-and-itemplateloader) property that is passed to the `Template.Parse` method.
 
```
include 'myinclude.html'
x = include 'myinclude.html'
x + " modified"
```

assuming that `myinclude.html` is:
```
{{ y = y + 1 ~}}
This is a string with the value {{ y }}
```

will output:

```
This is a string with the value 1
This is a string with the value 2 modified
```  

[:top:](#language)


### 9.11 `ret <expression>?`

The return statement is used to exit from a top-level/include page or a function, and optionally return a value to the caller.

```
This is a text
{{~  ret ~}}
This text will not appear
```

will output:

```
This is a text
```

[:top:](#language)
