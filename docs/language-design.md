Language Design
===============
As with all text templating languages, TextScript outputs the same text you input, that is unless you have some **code block** in your text. TextScript recognizes these types of **code block**s:

- Something enclosed by `{{` and `}}` -- the scripting code block
- Something enclosed by `{%{` and `}%}` -- the escaped code block

In the first case, the templating engine will parse what's in the code block and replace the code block with the output.

In the second case, the code block will not be interpreted and outputed as is.

More on these below.


Scripting Code Block
--------------------
A scripting code block can be 
- single lined: ``{{ foo }}`` 
- or **multi-lined**:
```
    {{
      if !name
        name = "default"
      end
      name
    }}
```
- or **separated by a semi-colon** to allow compact forms in some use cases:
```
    {{if !name; name = "default"; end; name }}
```

TextScript generally ignores whitespaces when parsing, but there are a few exceptions:
- at the end of line (thus requiring semi-colon for compact form statements)
- to disambiguate between an array indexer and an array initializer

Also, if a statement is an expression (but not an assignment expression), the result of the expression will be output to the rendering output of the template:

> **input**
```
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
Note that in the previous example, there is no line break between `5` and `6` because we are inside a code block. To produce line breaks, add the line break string `"\n"` or just multiple code blocks:

> **input**
```
{{ x = "5" }}
{{ x }}
{{ x + 1 }}
```
> **output**
```html
5
6
```

Escaped code block
------------------
Anything can be commented out from the parser by enclosing it with `{%{` and `}%}`.

For example:
> **input**: `{%{Hello this is {{ name }}}%}`   
> **output**: `Hello this is {{ name }}` 

Any escape block can be also escaped by increasing the number of `%` in the starting and ending block:
> **input**: `{%%{This is an escaped block: }%} here}%%}`
> **output**: `This is an escaped block: }%} here`

This allow effectively to nest escape blocks and still be able to escape them.

Hence a starting escape block `{%%%%{` will required an ending `}%%%%}`


Stripping
---------
Let's talk about what happends between a code block and normal text. By default, any whitespace (including new lines) before or after a code block are copied as-is to the output:

> **input**
```
xxx{{ foo }}yyy
```

> **output**
```
xxxyyy
```

> **input**
```
xxx
{{ foo }}
yyy
```

> **output**
```
xxx

yyy
```

The second situation above may not be intended sometimes. One solution is to be mindful of the whitespace between text and code blocks when authoring your template. However, the end result may not always be pleasing to look at. The alternative is to *strip* whitespaces between the code block and text:

1. Use the **greedy consumer** to remove all whitespaces (including newlines):

  * Strip whitespace on the left:  
> **input**
```
    This is a <       
    {{- name}}> text
``` 
> **output**
```html
    This is a <foo> a text
```    
  * Strip on the right:  
> **input**
```
    This is a <{{ name -}} 
    > text:       
``` 
> **output**
```html
    This is a <foo> text
```
  
  * Strip on both left and right:  
> **input**
```
    This is a <
    {{- name -}} 
    > text:       
```
> **output**
```html
    This is a <foo> text
```

2. Use the **non greedy consumer**
  - Using a `{{~` will remove any **whitespace before** but will **stop on the first newline without including it**
  - Using a `~}}` will remove any **whitespace after including the first newline** but will stop after

  This mode is very convenient when you want to use only a code block on a line, but want that line to be completely 
  removed from the output, but to keep spaces before and after this line intact.

  In the following example, we want to remove entirely the lines `{{~ for product in products ~}}` and `{{~ end ~}}`, but we want
  for example to keep the indentation of the opening `<li>`.

  Using the greedy mode `{{-` or `-}}` would have removed all whitespace and lines and would have put the results on a single line.

  > **input**
  ```
  <ul>
      {{~ for product in products ~}}
      <li>{{ product.name }}</li>
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

Both mode `~` and '-' can also be used with **escape blocks** `{%%{~` or `~}%%}` or `{%%{-` or `-}%%}`


Comments
--------
Within a code block, you can use single line `#` and multi-line comments `##`:

`{{ name   # this is a single line comment }}`

> **input**
```
{{ ## This 
is a multi
line
comment ## }}
```
> **output**
```html

```

Comments do not affect the closing tag `}}`.


Strings
-------
You can represent **regular strings** by enclosing it in double quotes `"..."` or single quotes `'...'`. In either case, the following escape sequences are always honoured:
  - `^'` single quote
  - `^"` double quote
  - `^^` caret
  - `^n` new line
  - `^r` carriage return
  - `^t` tab
  - `^b` backspace
  - `^f` form feed
  - `^uxxxx` where xxxx is a unicode hexa code number `0000` to `ffff` 
  - `^x00-\xFF` a hexadecimal ranging from `0x00` to `0xFF`

If you do not want escape sequences, use a **literal string** enclosed by backstick quotes `` `...` ``. They are very useful when you need to specify regex patterns:
> **input**
```textscript
  {{ "this is a text" | regex.split `\s+` }}
``` 
  
> **output**
```html 
  [this, is, a, test]
``` 


Numbers
-------
A number in a code block `{{ 100 }}` is similar to a JavaScript number: 

- Integers: `100`, `1e3`
- Floats: `100.0`, `1.0e3`, `1.0e-3`

TextScript implementation in .NET is limited in the range of numbers it can support. The maximum length for integer is defined by `Int64.MaxValue` and for floating point numbers, `Decimal.MaxValue`.


Boolean
-------
TextScript recognizes the keywords `{{ true }}` and `{{ false }}` to indicate boolean values.

> **input**
```
{{ true }}
{{ false }}
```
> **output**
```html
true
false
```


null
----
TextScript recognizes the keyword `{{ null }}` to indicate null value.

When resolving to a string output, the null value will output an empty string:

> **input**
```
{{ null }}
```
> **output**
```html

```


Variables
---------
TextScript distinguishes between **global** and **local** variables.

A **global/property variable** like `{{ name }}` is a liquid like handle, starting by a letter or underscore `_`, and following by letters `A-Z a-z`, digits `0-9`, and underscores `_`. Here are some examples:

- `var` 
- `var9`
- `_var`

> **NOTE:** In liquid, the character `-` is allowed in a variable name, but when translating it to TextScript, you will have to enclose it into a quoted string.

A **local variable** like `{{ $name }}` is an identifier starting with `$`. A local variable is only accessible within the same include page or function body.

The **special local variable** `$` alone is an array containing the arguments passed to the current function or include page.

The special local variables `$0` `$1` ... `$n` is a shorthand of `$[0]`, `$[1]` ... `$[n]`. e.g Using `$0` returns the first argument of the current function or including page.

### The special variable `this`

The `this` variable gives you access to the current object bound where you have access to all local variables for the current scope.

Thus the following variable access are equivalent:

> **input**
```
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
In the case of the `with` statement, the `this` operator refers to the object passed to `with`:

> **input**
```
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

### The special variable `empty`

The `empty` variable always represents an empty object. It mainly exists to be compatible with liquid, by providing a way to compare an object with the `empty` object to check if it is empty or not:

> **input**
```html
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


Objects
-------
Objects are defined like in JavaScript: `{...}`

An object can be initialized empty:

```
{{ 
	myobject = {} 
}}
``` 

Or you can initialize with some members:

```
{{ 
	myobject = { 
		member1: "yes", 
		member2: "no" 
	} 
}}
```

Or use the JSON syntax:

`{{ myobject = { "member1": "yes", "member2": "no" } }}`

Like JSON, you can define an object in a single line.

### Member access
The statement `{{ myobject.member1 }}` is equal to `{{ myobject["member1"] }}`.

You can add members to objects by simple assignments:

> **input**
```
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
> By default, properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](runtime.md#member-renamer) delegate
>
> You cannot add members to objects exposed via the .NET runtime.

### The special property `empty?`

You can quickly check if an object is empty using the special property `.empty?`:

> **input**
```
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


Arrays
------
An array can be initialized empty :

```
{{ 
	myarray = [] 
}}
``` 

Or with some items:

```
{{ 
	myarray = [1, 2, 3, 4] 
}}
```

You can access items in an array by its index, which is 0 based:

`{{ myarray[0] }}`

You can increase the length of the array by simply assigning values by index:

```
{{
  myarray = [] 
  myarray[0] = 1 
  myarray[1] = 2 
  myarray[2] = 3 
  myarray[3] = 4 
}}
``` 

You can also manipulate arrays with functions in the [`array` builtin object](#array-builtin).

> **Important tip**
> 
> While whitespace characters are mostly not relevant while parsing TextScript, there is a case where a **whitespace helps to disambiguate between an array indexer and an array initializer**.
>  
> For instance, if a whitespace is found before a `[` and the previous expression was a variable path expressions (see later), the following expression `[...]` will be considered as an array initializer instead of an array indexer:
> 
> ```
> {{
> myfunction [1]  # There is a whitespace after myfunction. 
>                 # It will result in a call to myfunction passing an array as an argument
> 
> myvariable[1]   # Without a whitespace, this is accessing 
>                 # an element in the array provided by myvariable
> }}    
> ```

### Array with properties
You can attach properties to arrays, somewhat like objects:

> **input**
```
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

### The special `size` property
Arrays have a `size` property that can be used to query the number of elements in the array:

> **input**
```
{{
a = [1, 2, 3]
a.size
}}
```
> **output**
```html
3
```

Functions
---------
TextScript allows you to define custom functions within the template itself.

The following declares a function `mysubtract` that uses its first argument and subtract from it the second argument:

``` 
{{func mysubtract
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
{{mysubtract 5 1}}
{{5 | mysubtract 1}}
```
> **output**
```
4
4
```

As you can notice from the example above, when using the pipe, the result of the pipe is pushed as the **first argument** of the pipe receiver.

Note that a function can have mixed text statements as well:

``` 
{{func inc}}
   This is a text with the following argument {{ $0 + 1 }}
{{end}}
``` 

Because functions are object, they can be stored into a property of an object by using the alias `@` operator:

```
{{
myobject.myinc = @inc     # Use the @ alias operator to treat functions like variables 
                          # the function will not be evaluated this way
x = 1 | myobject.myinc    # x = x + 1
}}

```

The function aliasing operator `@` allows you to pass a function as a parameter to another function, enabling powerful function compositions.

