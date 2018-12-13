Builtins
========
This is the reference documentation for all built-in functions that is available in text templates.

- [`array` functions](#array-functions)
- [`date` functions](#date-functions)
- [`fs` functions](#fs-functions)
- [`html` functions](#html-functions)
- [`math` functions](#math-functions)
- [`object` functions](#object-functions)
- [`regex` functions](#regex-functions)
- [`string` functions](#string-functions)
- [`timespan` functions](#timespan-functions)

[:top:](#builtins)

************************************************************************

array functions
--------------

Array functions available through the object `array`.

- [`array.add`](#arrayadd)
- [`array.add_range`](#arrayadd_range)
- [`array.compact`](#arraycompact)
- [`array.concat`](#arrayconcat)
- [`array.cycle`](#arraycycle)
- [`array.first`](#arrayfirst)
- [`array.insert_at`](#arrayinsert_at)
- [`array.join`](#arrayjoin)
- [`array.last`](#arraylast)
- [`array.limit`](#arraylimit)
- [`array.map`](#arraymap)
- [`array.offset`](#arrayoffset)
- [`array.remove_at`](#arrayremove_at)
- [`array.replace_empty`](#arrayreplace_empty)
- [`array.reverse`](#arrayreverse)
- [`array.size`](#arraysize)
- [`array.sort`](#arraysort)
- [`array.uniq`](#arrayuniq)
- [`array.has`](#arrayhas)

[:top:](#builtins)

************************************************************************

### `array.add`

#### SYNTAX
```
array.add <list> <value>
```

#### DESCRIPTION
Adds a value to the end of the input list.

#### PARAMETERS
- `list`: The input list.
- `value`: The value to add at the end of the list.


#### RETURNS
A new list with the value added

#### EXAMPLES
> **input**
```template-text
{{ [1, 2, 3] | array.add 4 }}
```
> **output**
```html
[1, 2, 3, 4]
```


[:top:](#builtins)

************************************************************************

### `array.add_range`

#### SYNTAX
```
array.add_range <list1> <list2>
```

#### DESCRIPTION
Concatenates two lists.

#### PARAMETERS
- `list1`: The 1st input list.
- `list2`: The 2nd input list.


#### RETURNS
The concatenation of the two input lists.

#### EXAMPLES
The `add_range` function is an alias to the `concat` function.

> **input**
```template-text
{{ [1, 2, 3] | array.concat [4, 5] }}
```
> **output**
```html
[1, 2, 3, 4, 5]
```


[:top:](#builtins)

************************************************************************

### `array.compact`

#### SYNTAX
```
array.compact <list>
```

#### DESCRIPTION
Removes any `null` values from the input list.

#### PARAMETERS
- `list`: An input list.


#### RETURNS
The list with all `null` values removed.

#### EXAMPLES
> **input**
```template-text
{{ [1, null, 3] | array.compact }}
```
> **output**
```html
[1, 3]
```


[:top:](#builtins)

************************************************************************

### `array.concat`

#### SYNTAX
```
array.concat <list1> <list2>
```

#### DESCRIPTION
Concatenates two lists.

#### PARAMETERS
- `list1`: The 1st input list.
- `list2`: The 2nd input list.


#### RETURNS
The concatenation of the two input lists.

#### EXAMPLES
> **input**
```template-text
{{ [1, 2, 3] | array.concat [4, 5] }}
```
> **output**
```html
[1, 2, 3, 4, 5]
```


[:top:](#builtins)

************************************************************************

### `array.cycle`

#### SYNTAX
```
array.cycle <list> <group>?
```

#### DESCRIPTION
Outputs an item from a list of strings in a cyclic manner. Each time this function is used on a list, the next item or the first item is returned.

#### PARAMETERS
- `list`: An input list.
- `group`: The group used. Default is `null`.


#### RETURNS
The first item if the list was used by this function for the first time, or the previous output was the last item in the list. Otherwise, the output is the item at the index position one larger than the previous output item.

#### EXAMPLES
> **input**
```template-text
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
```
> **output**
```html
one
two
three
one
```
`cycle` accepts a parameter called cycle group in cases where you need multiple cycle blocks in one template. 
If no name is supplied for the cycle group, then it is assumed that multiple calls with the same parameters are one group.


[:top:](#builtins)

************************************************************************

### `array.first`

#### SYNTAX
```
array.first <list>
```

#### DESCRIPTION
Returns the first item in an input list.

#### PARAMETERS
- `list`: The input list.


#### RETURNS
The first item of the input list.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6] | array.first }}
```
> **output**
```html
4
```
Use the `last` function to return the last item in a list.

Use the `limit` function to return the first `n` number of items, or `slice` function to return a subset of a list.


[:top:](#builtins)

************************************************************************

### `array.insert_at`

#### SYNTAX
```
array.insert_at <list> <index> <value>
```

#### DESCRIPTION
Inserts a value at the specified index in the input list.

#### PARAMETERS
- `list`: The input list.
- `index`: The index position in the list where an item should be inserted. If this number is less than one, the new item will be inserted at the first position.
- `value`: The value to insert.


#### RETURNS
A new list with the item inserted. All existing items at or beyond the index position will be shifted to the next index position.

#### EXAMPLES
> **input**
```template-text
{{ ["a", "b", "c"] | array.insert_at 2 "Yo" }}
```
> **output**
```html
[a, b, Yo, c]
```


[:top:](#builtins)

************************************************************************

### `array.join`

#### SYNTAX
```
array.join <list> <delimiter> <format>?
```

#### DESCRIPTION
Joins all items in a list using a delimiter string.

#### PARAMETERS
- `list`: The input list.
- `delimiter`: The delimiter string to use to separate items in the list.
- `format`: An optional string to specify how each item should be formatted.


#### RETURNS
A new list with the items joined into a string.

#### EXAMPLES
> **input**
```template-text
{{ [1, 2, 3] | array.join "|" }}
{{ [1, 2, 3] | array.join "," "*{0}*" }}
```
> **output**
```html
1|2|3
*1*,*2*,*3*
```


[:top:](#builtins)

************************************************************************

### `array.last`

#### SYNTAX
```
array.last <list>
```

#### DESCRIPTION
Returns the last item in an input list.

#### PARAMETERS
- `list`: The input list.


#### RETURNS
The last item of the input list.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6] | array.last }}
```
> **output**
```html
6
```
Use the `first` function to return the first item in a list.

Use the `slice` function to return a subset of a list.


[:top:](#builtins)

************************************************************************

### `array.limit`

#### SYNTAX
```
array.limit <list> <count>
```

#### DESCRIPTION
Returns no more than the specified number of items from the input list, starting from the first index position.

#### PARAMETERS
- `list`: The input list.
- `count`: The number of elements to return from the input list. If this number is less than one, no item is returned.


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6] | array.limit 2 }}
```
> **output**
```html
[4, 5]
```


[:top:](#builtins)

************************************************************************

### `array.map`

#### SYNTAX
```
array.map <list> <member>
```

#### DESCRIPTION
Accepts a list of objects, and outputs the value of a specified property in each object.

#### PARAMETERS
- `list`: The input list.
- `member`: The name of the property. An item must be an object and contains this property in order for this function to output its value.


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ 
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.map "type" | array.uniq | array.sort }}
```
> **output**
```html
[electronics, fruit, furniture]
```


[:top:](#builtins)

************************************************************************

### `array.offset`

#### SYNTAX
```
array.offset <list> <index>
```

#### DESCRIPTION
Returns the remaining items in a list after the specified offset.

#### PARAMETERS
- `list`: The input list.
- `index`: The starting index position of the list where items should be returned. If the index is less than 1, all items are returned.


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6, 7, 8] | array.offset 2 }}
```
> **output**
```html
[6, 7, 8]
```


[:top:](#builtins)

************************************************************************

### `array.remove_at`

#### SYNTAX
```
array.remove_at <list> <index>
```

#### DESCRIPTION
Removes an item at the specified index position from the input list.

#### PARAMETERS
- `list`: The input list.
- `index`: The index position of the item to remove.


#### RETURNS
A new list with the item removed. If index is negative, this function will remove from the end of the list.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6, 7, 8] | array.remove_at 2 }}
```
> **output**
```html
[4, 5, 7, 8]
```
If the `index` is negative, removes from the end of the list. Notice that we need to put -1 in parenthesis to avoid confusing the parser with a binary `-` operation.
> **input**
```template-text
{{ [4, 5, 6, 7, 8] | array.remove_at (-1) }}
```
> **output**
```html
[4, 5, 6, 7]
```


[:top:](#builtins)

************************************************************************

### `array.replace_empty`

#### SYNTAX
```
array.replace_empty <list> <replace> <notEmpty>?
```

#### DESCRIPTION
Returns a string based on whether an array is empty.

#### PARAMETERS
- `list`: The input list.
- `replace`: The string to replace if the input list is null or empty.
- `notEmpty`: The string to return if the input list contains items.


#### RETURNS
A string based on whether the list is null or empty.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6, 7, 8] | array.replace_empty "i am empty" "i am full" }}
```
> **output**
```html
i am full
```


[:top:](#builtins)

************************************************************************

### `array.reverse`

#### SYNTAX
```
array.reverse <list>
```

#### DESCRIPTION
Reverses the input list.

#### PARAMETERS
- `list`: The input list


#### RETURNS
A new list in reversed order.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6, 7] | array.reverse }}
```
> **output**
```html
[7, 6, 5, 4]
```


[:top:](#builtins)

************************************************************************

### `array.size`

#### SYNTAX
```
array.size <list>
```

#### DESCRIPTION
Returns the number of items in the input list.

#### PARAMETERS
- `list`: The input list.


#### RETURNS
The number of items in the input list.

#### EXAMPLES
> **input**
```template-text
{{ [4, 5, 6] | array.size }}
```
> **output**
```html
3
```


[:top:](#builtins)

************************************************************************

### `array.sort`

#### SYNTAX
```
array.sort <list> <member>?
```

#### DESCRIPTION
Sorts the elements of the input list according to the value of each item or the value of the specified property of each object-type item.

#### PARAMETERS
- `list`: The input list.
- `member`: The property name to sort according to its value. This is `null` by default, meaning that the item's value is used instead.


#### RETURNS
A list sorted according to the value of each item or the value of the specified property of each object-type item.

#### EXAMPLES
Sort by value: 
> **input**
```template-text
{{ [10, 2, 6] | array.sort }}
```
> **output**
```html
[2, 6, 10]
```
Sorts by object property's value: 
> **input**
```template-text
{{
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.sort "title" | array.map "title"
}}
```
> **output**
```html
[computer, orange, sofa]
```


[:top:](#builtins)

************************************************************************

### `array.uniq`

#### SYNTAX
```
array.uniq <list>
```

#### DESCRIPTION
Returns the unique items of the input list.

#### PARAMETERS
- `list`: The input list.


#### RETURNS
A list of unique elements of the input list.

#### EXAMPLES
> **input**
```template-text
{{ [1, 1, 4, 5, 8, 8] | array.uniq }}
```
> **output**
```html
[1, 4, 5, 8]
```


[:top:](#builtins)

************************************************************************

### `array.has`

#### SYNTAX
```
array.has <list> <item> <ignoreCase: False>?
```

#### DESCRIPTION
Checks whether a list contains the specified item.

#### PARAMETERS
- `list`: The input list.
- `item`: The item that should be in the input list.
- `ignoreCase`: If `true`, use case-insensitive comparison for strings. Otherwise, `false`. This is `false` by default.


#### RETURNS
`true` if the item exists in the list. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ [1, 4, "foo"] | array.has "Foo" true }}
{{ [1, 4, "foo"] | array.has 1 }}
```
> **output**
```html
true
true
```

[:top:](#builtins)

************************************************************************

date functions
--------------

A `datetime` object represents an instant in time, expressed as a date and time of day. 

| Name             | Description
|--------------    |-----------------
| `.year`          | Gets the year of a date object 
| `.month`         | Gets the month of a date object
| `.day`           | Gets the day in the month of a date object
| `.day_of_year`   | Gets the day within the year
| `.hour`          | Gets the hour of the date object
| `.minute`        | Gets the minute of the date object
| `.second`        | Gets the second of the date object
| `.millisecond`   | Gets the millisecond of the date object

[:top:](#builtins)

#### Binary operations
The substract operation `date1 - date2`: Substract `date2` from `date1` and return a `timespan` object (see `timespan` object below).

Other comparison operators(`==`, `!=`, `<=`, `>=`, `<`, `>`) are also working with `datetime` objects.

- [`date.now`](#datenow)
- [`date.add_days`](#dateadd_days)
- [`date.add_months`](#dateadd_months)
- [`date.add_years`](#dateadd_years)
- [`date.add_hours`](#dateadd_hours)
- [`date.add_minutes`](#dateadd_minutes)
- [`date.add_seconds`](#dateadd_seconds)
- [`date.add_milliseconds`](#dateadd_milliseconds)
- [`date.parse`](#dateparse)
- [`date.clone`](#dateclone)
- [`date.to_string`](#dateto_string)

[:top:](#builtins)

************************************************************************

### `date.now`

#### SYNTAX
```
date.now
```

#### DESCRIPTION
Returns a `datetime` object of the current time, including the hour, minutes, seconds and milliseconds.

#### PARAMETERS


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ date.now.year }}
```
> **output**
```html
2018
```


[:top:](#builtins)

************************************************************************

### `date.add_days`

#### SYNTAX
```
date.add_days <date> <days>
```

#### DESCRIPTION
Adds the specified number of days to the input `datetime`. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `days`: The days.


#### RETURNS
A new `datetime` object.

#### EXAMPLES
> **input**
```template-text
{{ date.parse '2016/01/05' | date.add_days 1 }}
```
> **output**
```html
06 Jan 2016
```


[:top:](#builtins)

************************************************************************

### `date.add_months`

#### SYNTAX
```
date.add_months <date> <months>
```

#### DESCRIPTION
Adds the specified number of months to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `months`: The months.


#### RETURNS
A new `datetime` object.

#### EXAMPLES
> **input**
```template-text
{{ date.parse '2016/01/05' | date.add_months 1 }}
```
> **output**
```html
05 Feb 2016
```


[:top:](#builtins)

************************************************************************

### `date.add_years`

#### SYNTAX
```
date.add_years <date> <years>
```

#### DESCRIPTION
Adds the specified number of years to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `years`: The years.


#### RETURNS
A new `datetime` object.

#### EXAMPLES
> **input**
```template-text
{{ date.parse '2016/01/05' | date.add_years 1 }}
```
> **output**
```html
05 Jan 2017
```


[:top:](#builtins)

************************************************************************

### `date.add_hours`

#### SYNTAX
```
date.add_hours <date> <hours>
```

#### DESCRIPTION
Adds the specified number of hours to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `hours`: The hours.


#### RETURNS
A new `datetime` object.

#### EXAMPLES



[:top:](#builtins)

************************************************************************

### `date.add_minutes`

#### SYNTAX
```
date.add_minutes <date> <minutes>
```

#### DESCRIPTION
Adds the specified number of minutes to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `minutes`: The minutes.


#### RETURNS
A new `datetime` object.

#### EXAMPLES



[:top:](#builtins)

************************************************************************

### `date.add_seconds`

#### SYNTAX
```
date.add_seconds <date> <seconds>
```

#### DESCRIPTION
Adds the specified number of seconds to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `seconds`: The seconds.


#### RETURNS
A new `datetime` object.

#### EXAMPLES



[:top:](#builtins)

************************************************************************

### `date.add_milliseconds`

#### SYNTAX
```
date.add_milliseconds <date> <millis>
```

#### DESCRIPTION
Adds the specified number of milliseconds to the input `datetime` object. 

#### PARAMETERS
- `date`: The input `datetime` object.
- `millis`: The milliseconds.


#### RETURNS
A new `datetime` object.

#### EXAMPLES



[:top:](#builtins)

************************************************************************

### `date.parse`

#### SYNTAX
```
date.parse <text>
```

#### DESCRIPTION
Parses the specified input string to a `datetime` object. 

#### PARAMETERS
- `text`: A text representing a date.


#### RETURNS
A `datetime` object.

#### EXAMPLES
> **input**
```template-text
{{ date.parse '2016/01/05' }}
```
> **output**
```html
05 Jan 2016
```


[:top:](#builtins)

************************************************************************

### `date.clone`

#### SYNTAX
```
date.clone <deep>
```

#### DESCRIPTION
Clones a `datetime` object.

#### PARAMETERS
- `deep`: Performs a deep clone.


#### RETURNS
An  object.

#### EXAMPLES



[:top:](#builtins)

************************************************************************

### `date.to_string`

#### SYNTAX
```
date.to_string <datetime> <pattern> <culture>
```

#### DESCRIPTION
Converts a `datetime` object to a textual representation using the specified format string.

By default, if you are using a date, it will use the format specified by `date.format` which defaults to 
`date.default_format` (readonly) which default to `%d %b %Y`.

You can override the format used for formatting all dates by assigning the a new format: 
`date.format = '%a %b %e %T %Y';`.

You can recover the default format by using `date.format = date.default_format;`.

By default, the `to_string` format is using the **current culture**, but you can switch to an invariant 
culture by using the modifier `%g`.

For example, using `%g %d %b %Y` will output the date using an invariant culture.

If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.

Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the 
format modifiers:

| Format | Result         | Description
|--------|----------------|--------------------------------------------
| `%a`   | `Thu`          | Day of week in short form
| `%A`   | `Thursday`     | Day of week in full form
| `%b`   | `Sep`          | Month in short form
| `%B`   | `September`    | Month in full form
| `%c`   | `Sun Dec 09 14:34:20 2018` | Date and time (%a %b %e %T %Y)
| `%C`   | `20`           | Century in 2 digits
| `%d`   | `03`           | Day of the month (01-31)
| `%D`   | `09/12/13`     | Date (month-year-date)
| `%e`   | `3`            | Day of the month (without padding, 1-31)
| `%F`   | `2013-09-12`   | ISO 8601 date (year-month-date)
| `%h`   | `Sep`          | Same as `%b`
| `%H`   | `22`           | Hour in 24 hour format (00-24)
| `%I`   | `10`           | Hour in 12 hour format (00-12)
| `%j`   | `255`          | Day of year (001..366)
| `%k`   | `22`           | Hour in 24 hour format (without padding, 0-23)
| `%l`   | `10`           | Hour in 12 hour format (without padding 0-12)
| `%L`   | `000`          | Millisecond (000-999)
| `%m`   | `09`           | Month (01-12)
| `%M`   | `49`           | Minute (00-59)
| `%n`   |                | Newline character `\n`
| `%N`   | `000000000`    | Nanoseconds (000000000-999999999)
| `%p`   | `PM`           | AM/PM in upper case
| `%P`   | `pm`           | AM/PM in lower case
| `%r`   | `10:49:27 PM`  | Time in 12 hour long format
| `%R`   | `22:49`        | Time in 24 hour short format
| `%s`   |                | Number of seconds since epoch time (1970-01-01 00:00:00 +0000)
| `%S`   | `27`           | Seconds (00-59)
| `%t`   |                | Tab character `\t`
| `%T`   | `22:49:27`     | Time in 24 hour long format
| `%u`   | `4`            | Day of week (Mon to Sun, 1-7)
| `%U`   | `36`           | Week of year (first Sunday of year as first week, 00-53)
| `%v`   | `12-SEP-2013`  | VMS date (culture invariant)
| `%V`   | `37`           | Week of year according to ISO 8601 (01-53)
| `%W`   | `36`           | Week of year (first Monday of year as first week, 00-53)
| `%w`   | `4`            | Day of week of the time (from 0 for Sunday to 6 for Saturday)
| `%x`   |                | Default date (no time)
| `%X`   |                | Default time (no date)
| `%y`   | `13`           | Year in 2 digits
| `%Y`   | `2013`         | Year in 4 digits
| `%Z`   | `IST`          | Time zone
| `%%`   | `%`            | Percent character `%`

Note that the format is using a good part of the [ruby format](http://apidock.com/ruby/DateTime/strftime).

> **input**
```template-text
{{ date.parse '2016/01/05' | date.to_string `%d %b %Y` }}
```
> **output**
```html
05 Jan 2016
```

#### PARAMETERS
- `datetime`: The input datetime to format
- `pattern`: The date format pattern.
- `culture`: The culture used to format the datetime


#### RETURNS
A  that represents this instance.

#### EXAMPLES


[:top:](#builtins)

************************************************************************

fs functions
--------------

File system functions available through the builtin object `fs`.

- [`fs.test`](#fstest)
- [`fs.dir`](#fsdir)

[:top:](#builtins)

************************************************************************

### `fs.test`

#### SYNTAX
```
fs.test <path> <type: "any">?
```

#### DESCRIPTION
Tests for the existance of a path.

#### PARAMETERS
- `path`: The path to test.
- `type`: The type of path to test. May be one of the following: "leaf", "container" or "any". Defaults to "any".


#### RETURNS
If the path exists, `true`. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ '.\foo.txt' | fs.test }}
```
> **output**
```html
true
```


[:top:](#builtins)

************************************************************************

### `fs.dir`

#### SYNTAX
```
fs.dir <path> <type: "any">?
```

#### DESCRIPTION
Returns items in a container path.

#### PARAMETERS
- `path`: The path to query. Wildcard is supported. For recursive search, use the syntax `**\foo.txt`.
- `type`: The type of children items to return. May be one of the following: "leaf", "container" or "any". Defaults to "any".


#### RETURNS
A list of children items under the path specified.

#### EXAMPLES
> **input**
```template-text
{{ '**\fo?.txt' | fs.dir }}
```
> **output**
```html
[C:\foo.txt, C:\temp\foa.txt]
```

[:top:](#builtins)

************************************************************************

html functions
--------------

Html functions available through the builtin object `html`.

- [`html.strip`](#htmlstrip)
- [`html.escape`](#htmlescape)
- [`html.url_encode`](#htmlurl_encode)
- [`html.url_escape`](#htmlurl_escape)
- [`html.xmlattrib`](#htmlxmlattrib)
- [`html.xmltext`](#htmlxmltext)

[:top:](#builtins)

************************************************************************

### `html.strip`

#### SYNTAX
```
html.strip <text>
```

#### DESCRIPTION
Removes any HTML tags from the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string with all HTML tags removed.

#### EXAMPLES
> **input**
```template-text
{{ "<p>This is a paragraph</p>" | html.strip }}
```
> **output**
```html
This is a paragraph
```


[:top:](#builtins)

************************************************************************

### `html.escape`

#### SYNTAX
```
html.escape <text>
```

#### DESCRIPTION
Escapes a HTML input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The escaped string.

#### EXAMPLES
> **input**
```template-text
{{ "<p>This is a paragraph</p>" | html.escape }}
```
> **output**
```html
&lt;p&gt;This is a paragraph&lt;/p&gt;
```


[:top:](#builtins)

************************************************************************

### `html.url_encode`

#### SYNTAX
```
html.url_encode <text>
```

#### DESCRIPTION
Converts any URL-unsafe characters in a string into percent-encoded characters.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The url encoded string.

#### EXAMPLES
> **input**
```template-text
{{ "john@liquid.com" | html.url_encode }}
```
> **output**
```html
john%40liquid.com
```


[:top:](#builtins)

************************************************************************

### `html.url_escape`

#### SYNTAX
```
html.url_escape <text>
```

#### DESCRIPTION
Identifies all characters in a string that are not allowed in URLS, and replaces the characters with their escaped variants.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The url escaped string.

#### EXAMPLES
> **input**
```template-text
{{ "<hello> & <world>" | html.url_escape }}
```
> **output**
```html
%3Chello%3E%20&%20%3Cworld%3E
```


[:top:](#builtins)

************************************************************************

### `html.xmlattrib`

#### SYNTAX
```
html.xmlattrib <text>
```

#### DESCRIPTION
Escapes text for usage as XML attribute value.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The string escaped for usage as an XML attribute.

#### EXAMPLES
> **input**
```template-text
{{ 'hello "my" world' | html.xmlattrib }}
```
> **output**
```html
hello &quot;my&quot; world
```


[:top:](#builtins)

************************************************************************

### `html.xmltext`

#### SYNTAX
```
html.xmltext <text>
```

#### DESCRIPTION
Escapes text for usage as XML text value.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The string escaped for usage as XML text.

#### EXAMPLES
> **input**
```template-text
{{ 'hello <my> world & friends' | html.xmltext }}
```
> **output**
```html
hello &lt;my&gt; world &amp; friends
```

[:top:](#builtins)

************************************************************************

math functions
--------------

Math functions available through the object `math`.

- [`math.abs`](#mathabs)
- [`math.ceil`](#mathceil)
- [`math.divided_by`](#mathdivided_by)
- [`math.floor`](#mathfloor)
- [`math.format`](#mathformat)
- [`math.is_number`](#mathis_number)
- [`math.minus`](#mathminus)
- [`math.modulo`](#mathmodulo)
- [`math.plus`](#mathplus)
- [`math.round`](#mathround)
- [`math.times`](#mathtimes)

[:top:](#builtins)

************************************************************************

### `math.abs`

#### SYNTAX
```
math.abs <value>
```

#### DESCRIPTION
Returns the absolute value of a specified number.

#### PARAMETERS
- `value`: The input value.


#### RETURNS
The absolute value of the input value.

#### EXAMPLES
> **input**
```template-text
{{ -15.5 | math.abs }}
{{ -5 | math.abs }}
```
> **output**
```html
15.5
5
```


[:top:](#builtins)

************************************************************************

### `math.ceil`

#### SYNTAX
```
math.ceil <value>
```

#### DESCRIPTION
Returns the smallest integer greater than or equal to the specified number.

#### PARAMETERS
- `value`: The input value.


#### RETURNS
The smallest integer greater than or equal to the specified number.

#### EXAMPLES
> **input**
```template-text
{{ 4.6 | math.ceil }}
{{ 4.3 | math.ceil }}
```
> **output**
```html
5
5
```


[:top:](#builtins)

************************************************************************

### `math.divided_by`

#### SYNTAX
```
math.divided_by <value> <divisor>
```

#### DESCRIPTION
Divides the specified value by another value. If the divisor is an integer, the result will
be floor to and converted back to an integer.

#### PARAMETERS
- `value`: The input value.
- `divisor`: The divisor value.


#### RETURNS
The division of `value` by `divisor`.

#### EXAMPLES
> **input**
```template-text
{{ 8.4 | math.divided_by 2.0 | math.round 1 }}
{{ 8.4 | math.divided_by 2 }}
```
> **output**
```html
4.2
4
```


[:top:](#builtins)

************************************************************************

### `math.floor`

#### SYNTAX
```
math.floor <value>
```

#### DESCRIPTION
Returns the largest integer less than or equal to the specified number.

#### PARAMETERS
- `value`: The input value.


#### RETURNS
The largest integer less than or equal to the specified number.

#### EXAMPLES
> **input**
```template-text
{{ 4.6 | math.floor }}
{{ 4.3 | math.floor }}
```
> **output**
```html
4
4
```


[:top:](#builtins)

************************************************************************

### `math.format`

#### SYNTAX
```
math.format <value> <format> <culture>?
```

#### DESCRIPTION
Formats a number value with specified [.NET standard numeric format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)

#### PARAMETERS
- `value`: The input value.
- `format`: The format string.
- `culture`: The culture as a string (e.g `en-US`). By default the culture from  is used.


#### RETURNS
The number value formatted to a string.

#### EXAMPLES
> **input**
```template-text
{{ 255 | math.format "X4" }}
```
> **output**
```html
00FF
```


[:top:](#builtins)

************************************************************************

### `math.is_number`

#### SYNTAX
```
math.is_number <value>
```

#### DESCRIPTION
Returns a boolean indicating if the input value is a number.

#### PARAMETERS
- `value`: The input value.


#### RETURNS
`true` if the input value is a number. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ 255 | math.is_number }}
{{ "yo" | math.is_number }}
```
> **output**
```html
true
false
```


[:top:](#builtins)

************************************************************************

### `math.minus`

#### SYNTAX
```
math.minus <value> <with>
```

#### DESCRIPTION
Substracts the specified number from the input value.

#### PARAMETERS
- `value`: The input value.
- `with`: The number to subtract.


#### RETURNS
The results of the substraction: `value` - `with`

#### EXAMPLES
> **input**
```template-text
{{ 255 | math.minus 5}}
```
> **output**
```html
250
```


[:top:](#builtins)

************************************************************************

### `math.modulo`

#### SYNTAX
```
math.modulo <value> <with>
```

#### DESCRIPTION
Performs the modulo of the input value with the specified number.

#### PARAMETERS
- `value`: The input value that is the dividend.
- `with`: The divisor.


#### RETURNS
The results of the modulo: `value` % `with`

#### EXAMPLES
> **input**
```template-text
{{ 11 | math.modulo 10}}
```
> **output**
```html
1
```


[:top:](#builtins)

************************************************************************

### `math.plus`

#### SYNTAX
```
math.plus <value> <with>
```

#### DESCRIPTION
Adds the specified number to the input value.

#### PARAMETERS
- `value`: The input value.
- `with`: The number to add.


#### RETURNS
The results of the addition: `value` + `with`

#### EXAMPLES
> **input**
```template-text
{{ 1 | math.plus 2}}
```
> **output**
```html
3
```


[:top:](#builtins)

************************************************************************

### `math.round`

#### SYNTAX
```
math.round <value> <precision: 0>?
```

#### DESCRIPTION
Rounds a value to the nearest integer or to the specified number of fractional digits.

#### PARAMETERS
- `value`: The input value.
- `precision`: The number of fractional digits in the return value. Default is 0.


#### RETURNS
A value rounded to the nearest integer or to the specified number of fractional digits.

#### EXAMPLES
> **input**
```template-text
{{ 4.6 | math.round }}
{{ 4.3 | math.round }}
{{ 4.5612 | math.round 2 }}
```
> **output**
```html
5
4
4.56
```


[:top:](#builtins)

************************************************************************

### `math.times`

#### SYNTAX
```
math.times <value> <with>
```

#### DESCRIPTION
Multiply the input value with the specified number.

#### PARAMETERS
- `value`: The input value.
- `with`: The multiplier.


#### RETURNS
The results of the multiplication: `value` * `with`

#### EXAMPLES
> **input**
```template-text
{{ 2 | math.times 3}}
```
> **output**
```html
6
```

[:top:](#builtins)

************************************************************************

object functions
--------------

Object functions available through the builtin object `object`.

- [`object.default`](#objectdefault)
- [`object.format`](#objectformat)
- [`object.has_key`](#objecthas_key)
- [`object.has_value`](#objecthas_value)
- [`object.keys`](#objectkeys)
- [`object.size`](#objectsize)
- [`object.from_string`](#objectfrom_string)
- [`object.typeof`](#objecttypeof)
- [`object.values`](#objectvalues)

[:top:](#builtins)

************************************************************************

### `object.default`

#### SYNTAX
```
object.default <value> <defaultValue>
```

#### DESCRIPTION
The `default` value is returned if the input `value` is `null` or an empty string. A string containing whitespace characters 
will not resolve to the default value.

#### PARAMETERS
- `value`: The input value to check if it is null or an empty string.
- `defaultValue`: The default value to return if the input value is `null` or an empty string.


#### RETURNS
The value specified by `default` is returned if the input value is `null` or an empty string. Otherwise, the input value is 
returned.

#### EXAMPLES
> **input**
```template-text
{{ undefined_var | object.default "Yo" }}
```
> **output**
```html
Yo
```


[:top:](#builtins)

************************************************************************

### `object.format`

#### SYNTAX
```
object.format <value> <format> <culture>?
```

#### DESCRIPTION
Formats an object using the specified format string.

#### PARAMETERS
- `value`: The input value. The input value must implement the  interface.
- `format`: The format string that defines how  should be formatted.
- `culture`: The culture as a string (e.g `en-US`). By default the culture from  is used.


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ 255 | object.format "X4" }}
{{ 1523 | object.format "N" "fr-FR" }}
```
> **output**
```html
00FF
1 523,00
```


[:top:](#builtins)

************************************************************************

### `object.has_key`

#### SYNTAX
```
object.has_key <value> <key>
```

#### DESCRIPTION
Checks if the specified object contains the specified property.

#### PARAMETERS
- `value`: The input object.
- `key`: The name of the property to check.


#### RETURNS
`true` if the input object contains the specified property. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ product | object.has_key "title" }}
```
> **output**
```html
true
```
To check that the property value is not `null`, use the `has_value` function.


[:top:](#builtins)

************************************************************************

### `object.has_value`

#### SYNTAX
```
object.has_value <value> <key>
```

#### DESCRIPTION
Checks if the specified object contains the specified property, and that the property value is not `null`.

#### PARAMETERS
- `value`: The input object.
- `key`: The name of the property to check.


#### RETURNS
`true` if the input object contains the specified property and the property value is not `null`. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ product | object.has_value "title" }}
```
> **output**
```html
true
```


[:top:](#builtins)

************************************************************************

### `object.keys`

#### SYNTAX
```
object.keys <value>
```

#### DESCRIPTION
Return all property names of an object.

#### PARAMETERS
- `value`: The input object.


#### RETURNS
A list of property names of the object.

#### EXAMPLES
> **input**
```template-text
{{ product | object.keys | array.sort }}
```
> **output**
```html
[title, type]
```


[:top:](#builtins)

************************************************************************

### `object.size`

#### SYNTAX
```
object.size <value>
```

#### DESCRIPTION
Returns the size of the input object. 
- If the input object is a string, it will return the length.
- If the input is a list, it will return the number of elements.

All unsupported types will return -1.

#### PARAMETERS
- `value`: The input object.


#### RETURNS
The size of the input object, or -1.

#### EXAMPLES
> **input**
```template-text
{{ [1, 2, 3] | object.size }}
```
> **output**
```html
3
```


[:top:](#builtins)

************************************************************************

### `object.from_string`

#### SYNTAX
```
object.from_string <text>
```

#### DESCRIPTION
This is similar to the Powershell command `ConvertFrom-StringData`, but uses the character "`" instead 
of "\"" as the escape character.

For a list of characters that must be escaped, refer to 
the [MSDN documentation](https://msdn.microsoft.com/library/system.text.regularexpressions.regex.unescape)

#### PARAMETERS
- `text`: Text in the string data syntax.


#### RETURNS
An object representing key-value pairs of strings.

#### EXAMPLES
> **input**
```template-text
{{ localized = include 'localization.txt' | object.from_string
localized.foo
}}
```
> **output**
```html
bar!
```


[:top:](#builtins)

************************************************************************

### `object.typeof`

#### SYNTAX
```
object.typeof <value>
```

#### DESCRIPTION
Returns a string representing the type of the input object. Supported types are:
- string
- boolean
- number
- array
- iterator
- object

#### PARAMETERS
- `value`: The input object.


#### RETURNS


#### EXAMPLES
> **input**
```template-text
{{ null | object.typeof }}
{{ true | object.typeof }}
{{ 1 | object.typeof }}
{{ 1.0 | object.typeof }}
{{ "text" | object.typeof }}
{{ 1..5 | object.typeof }}
{{ [1,2,3,4,5] | object.typeof }}
{{ {} | object.typeof }}
{{ object | object.typeof }}
```
> **output**
```html

boolean
number
number
string
iterator
array
object
object
```


[:top:](#builtins)

************************************************************************

### `object.values`

#### SYNTAX
```
object.values <value>
```

#### DESCRIPTION
Returns the value of each property of an object as an array.

#### PARAMETERS
- `value`: The input object.


#### RETURNS
An array consisting of the value of each property of the input object.

#### EXAMPLES
> **input**
```template-text
{{ product | object.values | array.sort }}
```
> **output**
```html
[fruit, Orange]
```

[:top:](#builtins)

************************************************************************

regex functions
--------------

Functions exposed through `regex` builtin object.

- [`regex.escape`](#regexescape)
- [`regex.match`](#regexmatch)
- [`regex.replace`](#regexreplace)
- [`regex.split`](#regexsplit)
- [`regex.unescape`](#regexunescape)

[:top:](#builtins)

************************************************************************

### `regex.escape`

#### SYNTAX
```
regex.escape <pattern>
```

#### DESCRIPTION
Escapes a minimal set of characters (`\`, `*`, `+`, `?`, `|`, `{`, `[`, `(`,`)`, `^`, `$`,`.`, `#`, and white space) 
by replacing them with their escape codes. 

This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.

#### PARAMETERS
- `pattern`: The input string that contains the text to convert.


#### RETURNS
A string of characters with metacharacters converted to their escaped form.

#### EXAMPLES
> **input**
```template-text
{{ "(abc.*)" | regex.escape }}
```
> **output**
```html
\(abc\.\*\)
```


[:top:](#builtins)

************************************************************************

### `regex.match`

#### SYNTAX
```
regex.match <text> <pattern> <options>?
```

#### DESCRIPTION
Searches an input string for a substring that matches a regular expression pattern and returns an array with the match occurences. 

#### PARAMETERS
- `text`: The string to search for a match.
- `pattern`: The regular expression pattern to match.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching. 
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and 
              not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character 
              except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`. 


#### RETURNS
An array that contains all the match groups. The first group contains the entire match. The other elements contain regex 
matched groups `(..)`. An empty array returned means no match.

#### EXAMPLES
> **input**
```template-text
{{ "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` }}
```
> **output**
```html
[is a text123, is, text123]
```
Notice that the first element returned in the array is the entire regex match, followed by the regex group matches.


[:top:](#builtins)

************************************************************************

### `regex.replace`

#### SYNTAX
```
regex.replace <text> <pattern> <replace> <options>?
```

#### DESCRIPTION
In a specified input string, replaces strings that match a regular expression pattern with a specified replacement string. 

#### PARAMETERS
- `text`: The string to search for a match.
- `pattern`: The regular expression pattern to match.
- `replace`: The replacement string.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching. 
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and 
              not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character 
              except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`. 


#### RETURNS
A new string that is identical to the input string, except that the replacement string takes the place of each matched string. 
If pattern is not matched in the current instance, the method returns the current instance unchanged.

#### EXAMPLES
> **input**
```template-text
{{ "abbbbcccd" | regex.replace "b+c+" "-Yo-" }}
```
> **output**
```html
a-Yo-d
```


[:top:](#builtins)

************************************************************************

### `regex.split`

#### SYNTAX
```
regex.split <text> <pattern> <options>?
```

#### DESCRIPTION
Splits an input string into an array of substrings at the positions defined by a regular expression match.

#### PARAMETERS
- `text`: The string to split.
- `pattern`: The regular expression pattern to match.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching. 
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and 
              not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character 
              except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`. 


#### RETURNS
A string array of the splitted text.

#### EXAMPLES
> **input**
```template-text
{{ "a, b   , c,    d" | regex.split `\s*,\s*` }}
```
> **output**
```html
[a, b, c, d]
```


[:top:](#builtins)

************************************************************************

### `regex.unescape`

#### SYNTAX
```
regex.unescape <pattern>
```

#### DESCRIPTION
Converts any escaped characters in the input string.

#### PARAMETERS
- `pattern`: The input string containing the text to convert.


#### RETURNS
A string of characters with any escaped characters converted to their unescaped form.

#### EXAMPLES
> **input**
```template-text
{{ "\\(abc\\.\\*\\)" | regex.unescape }}
```
> **output**
```html
(abc.*)
```

[:top:](#builtins)

************************************************************************

string functions
--------------

String functions available through the builtin object `string`.

- [`string.append`](#stringappend)
- [`string.bool`](#stringbool)
- [`string.capitalize`](#stringcapitalize)
- [`string.capitalizewords`](#stringcapitalizewords)
- [`string.contains`](#stringcontains)
- [`string.downcase`](#stringdowncase)
- [`string.ends_with`](#stringends_with)
- [`string.handleize`](#stringhandleize)
- [`string.lstrip`](#stringlstrip)
- [`string.pluralize`](#stringpluralize)
- [`string.prepend`](#stringprepend)
- [`string.remove`](#stringremove)
- [`string.remove_first`](#stringremove_first)
- [`string.replace`](#stringreplace)
- [`string.replace_empty`](#stringreplace_empty)
- [`string.replace_first`](#stringreplace_first)
- [`string.rstrip`](#stringrstrip)
- [`string.size`](#stringsize)
- [`string.slice`](#stringslice)
- [`string.slice1`](#stringslice1)
- [`string.split`](#stringsplit)
- [`string.starts_with`](#stringstarts_with)
- [`string.strip`](#stringstrip)
- [`string.strip_newlines`](#stringstrip_newlines)
- [`string.to_int`](#stringto_int)
- [`string.to_long`](#stringto_long)
- [`string.to_float`](#stringto_float)
- [`string.to_double`](#stringto_double)
- [`string.truncate`](#stringtruncate)
- [`string.truncatewords`](#stringtruncatewords)
- [`string.upcase`](#stringupcase)
- [`string.pad_left`](#stringpad_left)
- [`string.pad_right`](#stringpad_right)
- [`string.md5`](#stringmd5)
- [`string.sha1`](#stringsha1)
- [`string.sha256`](#stringsha256)
- [`string.hmac_sha1`](#stringhmac_sha1)
- [`string.hmac_sha256`](#stringhmac_sha256)

[:top:](#builtins)

************************************************************************

### `string.append`

#### SYNTAX
```
string.append <text> <with>
```

#### DESCRIPTION
Concatenates two strings.

#### PARAMETERS
- `text`: The input string.
- `with`: The text to append.


#### RETURNS
The two strings concatenated.

#### EXAMPLES
> **input**
```template-text
{{ "Hello" | string.append " World" }}
```
> **output**
```html
Hello World
```


[:top:](#builtins)

************************************************************************

### `string.bool`

#### SYNTAX
```
string.bool <condition> <truthy> <falsy>
```

#### DESCRIPTION
Returns a string based on whether a condition is evaluated to `true` or `false`.

#### PARAMETERS
- `condition`: The condition to evaluate.
- `truthy`: The string to return if the condition evaluates to `true`.
- `falsy`: The string to return if the condition evaluates to `false`.


#### RETURNS
A string based on condition evaluated.

#### EXAMPLES
> **input**
```template-text
{{ true | string.bool "Got it!" "Not here" }}
```
> **output**
```html
Got it!
```


[:top:](#builtins)

************************************************************************

### `string.capitalize`

#### SYNTAX
```
string.capitalize <text>
```

#### DESCRIPTION
Converts the first character of the passed string to a upper case character.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The capitalized input string.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.capitalize }}
```
> **output**
```html
Test
```


[:top:](#builtins)

************************************************************************

### `string.capitalizewords`

#### SYNTAX
```
string.capitalizewords <text>
```

#### DESCRIPTION
Converts the first character of each word in the passed string to a upper case character.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The capitalized input string.

#### EXAMPLES
> **input**
```template-text
{{ "This is easy" | string.capitalizewords }}
```
> **output**
```html
This Is Easy
```


[:top:](#builtins)

************************************************************************

### `string.contains`

#### SYNTAX
```
string.contains <text> <value>
```

#### DESCRIPTION
Returns a boolean indicating whether the input string contains the specified substring.

#### PARAMETERS
- `text`: The input string.
- `value`: The substring to look for.


#### RETURNS
`true` if the substring was found in in input text. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ "This is easy" | string.contains "easy" }}
```
> **output**
```html
true
```


[:top:](#builtins)

************************************************************************

### `string.downcase`

#### SYNTAX
```
string.downcase <text>
```

#### DESCRIPTION
Converts the string to lower case.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string in lower case.

#### EXAMPLES
> **input**
```template-text
{{ "TeSt" | string.downcase }}
```
> **output**
```html
test
```


[:top:](#builtins)

************************************************************************

### `string.ends_with`

#### SYNTAX
```
string.ends_with <text> <value>
```

#### DESCRIPTION
Returns a boolean indicating whether the input string ends with the specified substring.

#### PARAMETERS
- `text`: The input string.
- `value`: The substring that should be the suffix of the input string.


#### RETURNS
`true` if the input string ends with the substring specified. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ "This is easy" | string.ends_with "easy" }}
```
> **output**
```html
true
```


[:top:](#builtins)

************************************************************************

### `string.handleize`

#### SYNTAX
```
string.handleize <text>
```

#### DESCRIPTION
Returns a url handle from the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
A url handle based on the input string.

#### EXAMPLES
> **input**
```template-text
{{ '100% M & Ms!!!' | string.handleize }}
```
> **output**
```html
100-m-ms
```


[:top:](#builtins)

************************************************************************

### `string.lstrip`

#### SYNTAX
```
string.lstrip <text>
```

#### DESCRIPTION
Removes any whitespace characters on the **beginning** of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string without any beginning whitespace characters.

#### EXAMPLES
> **input**
```template-text
{{ '   too many spaces           ' | string.lstrip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
too many spaces           
```


[:top:](#builtins)

************************************************************************

### `string.pluralize`

#### SYNTAX
```
string.pluralize <number> <singular> <plural>
```

#### DESCRIPTION
Outputs the singular or plural version of a string based on the value of a number. 

#### PARAMETERS
- `number`: The number to check.
- `singular`: The singular string to return if number is equal to 1
- `plural`: The plural string to return if number is not equal to 1


#### RETURNS
The singular or plural string based on number specified.

#### EXAMPLES
> **input**
```template-text
{{ products.size }} {{products.size | string.pluralize 'product' 'products' }}
```
> **output**
```html
7 products
```


[:top:](#builtins)

************************************************************************

### `string.prepend`

#### SYNTAX
```
string.prepend <text> <by>
```

#### DESCRIPTION
Concatenates two strings by prepending the input string with a prefix.

#### PARAMETERS
- `text`: The input string.
- `by`: The string to prepend to the input string.


#### RETURNS
The two strings concatenated.

#### EXAMPLES
> **input**
```template-text
{{ "World" | string.prepend "Hello " }}
```
> **output**
```html
Hello World
```


[:top:](#builtins)

************************************************************************

### `string.remove`

#### SYNTAX
```
string.remove <text> <remove>
```

#### DESCRIPTION
Removes all occurrences of a substring from a string.

#### PARAMETERS
- `text`: The input string.
- `remove`: The substring to remove.


#### RETURNS
The input string with the all occurence of a substring removed.

#### EXAMPLES
> **input**
```template-text
{{ "Hello, world. Goodbye, world." | string.remove "world" }}
```
> **output**
```html
Hello, . Goodbye, .
```


[:top:](#builtins)

************************************************************************

### `string.remove_first`

#### SYNTAX
```
string.remove_first <text> <remove>
```

#### DESCRIPTION
Removes the first occurrence of a substring from a string.

#### PARAMETERS
- `text`: The input string.
- `remove`: The first occurence of substring to remove.


#### RETURNS
The input string with the first occurence of a substring removed.

#### EXAMPLES
> **input**
```template-text
{{ "Hello, world. Goodbye, world." | string.remove_first "world" }}
```
> **output**
```html
Hello, . Goodbye, world.
```


[:top:](#builtins)

************************************************************************

### `string.replace`

#### SYNTAX
```
string.replace <text> <match> <replace>
```

#### DESCRIPTION
Replaces all occurrences of a substring with the value specified.

#### PARAMETERS
- `text`: The input string.
- `match`: The substring to find in the input string.
- `replace`: The value that should be used to replace all occurances of the substring.


#### RETURNS
The input string replaced.

#### EXAMPLES
> **input**
```template-text
{{ "Hello, world. Goodbye, world." | string.replace "world" "buddy" }}
```
> **output**
```html
Hello, buddy. Goodbye, buddy.
```


[:top:](#builtins)

************************************************************************

### `string.replace_empty`

#### SYNTAX
```
string.replace_empty <text> <replace> <notEmpty>?
```

#### DESCRIPTION
Substitutes a `null` or empty string with the specified string.

#### PARAMETERS
- `text`: The input string.
- `replace`: The string to return if the input value is `null` or an empty string.
- `notEmpty`: The string to return if the input value is a not an empty string. Defaults to the inputbstring itself.


#### RETURNS
A string based on whether the input string is `null` or an empty string.

#### EXAMPLES
> **input**
```template-text
{{ "" | string.replace_empty "its empty" "its full" }}
```
> **output**
```html
its empty
```


[:top:](#builtins)

************************************************************************

### `string.replace_first`

#### SYNTAX
```
string.replace_first <text> <match> <replace>
```

#### DESCRIPTION
Replaces the first occurrence of a substring with the value specified.

#### PARAMETERS
- `text`: The input string.
- `match`: The substring to find.
- `replace`: The value used to replace the substring.


#### RETURNS
The input string replaced.

#### EXAMPLES
> **input**
```template-text
{{ "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" }}
```
> **output**
```html
Hello, buddy. Goodbye, world.
```


[:top:](#builtins)

************************************************************************

### `string.rstrip`

#### SYNTAX
```
string.rstrip <text>
```

#### DESCRIPTION
Removes any whitespace characters at the **end** of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string without any whitespace characters at the end.

#### EXAMPLES
> **input**
```template-text
{{ '   too many spaces           ' | string.rstrip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
   too many spaces
```


[:top:](#builtins)

************************************************************************

### `string.size`

#### SYNTAX
```
string.size <text>
```

#### DESCRIPTION
Returns the number of characters from the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The length of the input string.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.size }}
```
> **output**
```html
4
```


[:top:](#builtins)

************************************************************************

### `string.slice`

#### SYNTAX
```
string.slice <text> <start> <length: 0>?
```

#### DESCRIPTION
The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. 
If no second parameter is given, a substring with the remaining characters will be returned.

#### PARAMETERS
- `text`: The input string.
- `start`: The starting index character where the slice should start from the input string.
- `length`: The number of character. Default is 0, meaning that the remaining of the input string will be returned.


#### RETURNS
The input string sliced

#### EXAMPLES
> **input**
```template-text
{{ "hello" | string.slice 0 }}
{{ "hello" | string.slice 1 }}
{{ "hello" | string.slice 1 3 }}
{{ "hello" | string.slice 1 length:3 }}
```
> **output**
```html
hello
ello
ell
ell
```


[:top:](#builtins)

************************************************************************

### `string.slice1`

#### SYNTAX
```
string.slice1 <text> <start> <length: 1>?
```

#### DESCRIPTION
The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring. 
If no second parameter is given, the character at the specified index will be returned.

#### PARAMETERS
- `text`: The input string.
- `start`: The starting index where the slice should start from the input string.
- `length`: The number of character to slice. Default is 1, meaning that only the first character at the starting position will be returned.


#### RETURNS
The input string sliced.

#### EXAMPLES
> **input**
```template-text
{{ "hello" | string.slice1 0 }}
{{ "hello" | string.slice1 1 }}
{{ "hello" | string.slice1 1 3 }}
{{ "hello" | string.slice1 1 length: 3 }}
```
> **output**
```html
h
e
ell
ell
```


[:top:](#builtins)

************************************************************************

### `string.split`

#### SYNTAX
```
string.split <text> <match>
```

#### DESCRIPTION
Split a string into an array, using a substring as the delimiter.

You can output different parts of the output array using various `array` functions.

#### PARAMETERS
- `text`: The input string.
- `match`: The substring used to split the input string.


#### RETURNS
An array consisting of the substrings of the input string between the delimiter.

#### EXAMPLES
> **input**
```template-text
{{ for word in "Hi, how are you today?" | string.split ' ' ~}}
{{ word }}
{{ end ~}}
```
> **output**
```html
Hi,
how
are
you
today?
```


[:top:](#builtins)

************************************************************************

### `string.starts_with`

#### SYNTAX
```
string.starts_with <text> <value>
```

#### DESCRIPTION
Returns a boolean indicating whether the input string starts with the specified substring.

#### PARAMETERS
- `text`: The input string.
- `value`: The prefix to look for.


#### RETURNS
`true` if the input string starts with the prefix specified. Otherwise, `false`.

#### EXAMPLES
> **input**
```template-text
{{ "This is easy" | string.starts_with "This" }}
```
> **output**
```html
true
```


[:top:](#builtins)

************************************************************************

### `string.strip`

#### SYNTAX
```
string.strip <text>
```

#### DESCRIPTION
Removes any whitespace characters from the **start** and **end** of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string without any starting or ending whitespace characters.

#### EXAMPLES
> **input**
```template-text
{{ '   too many spaces           ' | string.strip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
too many spaces
```


[:top:](#builtins)

************************************************************************

### `string.strip_newlines`

#### SYNTAX
```
string.strip_newlines <text>
```

#### DESCRIPTION
Removes any line breaks/newlines from a string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string without any breaks/newlines characters.

#### EXAMPLES
> **input**
```template-text
{{ "This is a string.^r^n With ^nanother ^rstring" | string.strip_newlines  }}
```
> **output**
```html
This is a string. With another string
```


[:top:](#builtins)

************************************************************************

### `string.to_int`

#### SYNTAX
```
string.to_int <text>
```

#### DESCRIPTION
Converts a string to a 32-bit integer.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
A 32-bit integer, or `null` if the conversion failed.

#### EXAMPLES
> **input**
```template-text
{{ "123" | string.to_int + 1 }}
```
> **output**
```html
124
```


[:top:](#builtins)

************************************************************************

### `string.to_long`

#### SYNTAX
```
string.to_long <text>
```

#### DESCRIPTION
Converts a string to a 64-bit integer.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
A 64-bit integer, or `null` if the conversion failed.

#### EXAMPLES
> **input**
```template-text
{{ "123678912345678" | string.to_long + 1 }}
```
> **output**
```html
123678912345679
```


[:top:](#builtins)

************************************************************************

### `string.to_float`

#### SYNTAX
```
string.to_float <text>
```

#### DESCRIPTION
Converts a string to a single precision floating point number.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
A 32-bit floating point number, or `null` if the conversion failed.

#### EXAMPLES
> **input**
```template-text
{{ "123.4" | string.to_float + 1 }}
```
> **output**
```html
124.4
```


[:top:](#builtins)

************************************************************************

### `string.to_double`

#### SYNTAX
```
string.to_double <text>
```

#### DESCRIPTION
Converts a string to a double precision floating point number.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
A 64-bit floating point number, or `null` if the conversion failed.

#### EXAMPLES
> **input**
```template-text
{{ "123.4" | string.to_double + 1 }}
```
> **output**
```html
124.4
```


[:top:](#builtins)

************************************************************************

### `string.truncate`

#### SYNTAX
```
string.truncate <text> <length> <ellipsis>?
```

#### DESCRIPTION
Truncates a string down to the number of characters passed as the first parameter. An ellipsis is appended to the truncated string 
and is included in the character count.

#### PARAMETERS
- `text`: The input string.
- `length`: The maximum length of the output string, including the length of the ellipsis.
- `ellipsis`: The ellipsis to append to the end of the truncated string. Defaults to 3 dots (...).


#### RETURNS
The truncated input string.

#### EXAMPLES
> **input**
```template-text
{{ "The cat came back the very next day" | string.truncate 13 }}
```
> **output**
```html
The cat ca...
```


[:top:](#builtins)

************************************************************************

### `string.truncatewords`

#### SYNTAX
```
string.truncatewords <text> <count> <ellipsis>?
```

#### DESCRIPTION
Truncates a string down to the number of words passed as the first parameter. An ellipsis is appended to the truncated string.

#### PARAMETERS
- `text`: The input string.
- `count`: The number of words to keep from the input string before appending the ellipsis.
- `ellipsis`: The ellipsis to append to the end of the truncated string. Defaults to 3 dots (...).


#### RETURNS
The truncated input string.

#### EXAMPLES
> **input**
```template-text
{{ "The cat came back the very next day" | string.truncatewords 4 }}
```
> **output**
```html
The cat came back...
```


[:top:](#builtins)

************************************************************************

### `string.upcase`

#### SYNTAX
```
string.upcase <text>
```

#### DESCRIPTION
Converts the string to upper case.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The input string in upper case.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.upcase }}
```
> **output**
```html
TEST
```


[:top:](#builtins)

************************************************************************

### `string.pad_left`

#### SYNTAX
```
string.pad_left <text> <width> <paddingChar:  >?
```

#### DESCRIPTION
Pads a string with leading spaces, such that the total length of the result string is of the specified length.

#### PARAMETERS
- `text`: The input string.
- `width`: The number of characters that the result string should have.
- `paddingChar`: The character to use for padding. Defaults to the space character.


#### RETURNS
The input string padded.

#### EXAMPLES
> **input**
```template-text
hello{{ "world" | string.pad_left 10 }}
```
> **output**
```html
hello     world
```


[:top:](#builtins)

************************************************************************

### `string.pad_right`

#### SYNTAX
```
string.pad_right <text> <width> <paddingChar:  >?
```

#### DESCRIPTION
Pads a string with trailing spaces, such that the total length of the result string is of the specified length.

#### PARAMETERS
- `text`: The input string
- `width`: The number of characters in the resulting string
- `paddingChar`: The character to use for padding. Defaults to the space character.


#### RETURNS
The input string padded.

#### EXAMPLES
> **input**
```template-text
{{ "hello" | string.pad_right 10 }}world
```
> **output**
```html
hello     world
```


[:top:](#builtins)

************************************************************************

### `string.md5`

#### SYNTAX
```
string.md5 <text>
```

#### DESCRIPTION
Computes the `md5` hash of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The `md5` hash of the input string.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.md5 }}
```
> **output**
```html
098f6bcd4621d373cade4e832627b4f6
```


[:top:](#builtins)

************************************************************************

### `string.sha1`

#### SYNTAX
```
string.sha1 <text>
```

#### DESCRIPTION
Computes the `sha1` hash of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The `sha1` hash of the input string.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.sha1 }}
```
> **output**
```html
a94a8fe5ccb19ba61c4c0873d391e987982fbbd3
```


[:top:](#builtins)

************************************************************************

### `string.sha256`

#### SYNTAX
```
string.sha256 <text>
```

#### DESCRIPTION
Computes the `sha256` hash of the input string.

#### PARAMETERS
- `text`: The input string.


#### RETURNS
The `sha256` hash of the input string.

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.sha256 }}
```
> **output**
```html
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
```


[:top:](#builtins)

************************************************************************

### `string.hmac_sha1`

#### SYNTAX
```
string.hmac_sha1 <text> <secretKey>
```

#### DESCRIPTION
Converts a string into a SHA-1 hash using a hash message authentication code (HMAC). A secret key parameter is required.

#### PARAMETERS
- `text`: The input string.
- `secretKey`: The secret key.


#### RETURNS
The `SHA-1` hash of the input string using a hash message authentication code (HMAC).

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.hmac_sha1 "secret" }}
```
> **output**
```html
1aa349585ed7ecbd3b9c486a30067e395ca4b356
```


[:top:](#builtins)

************************************************************************

### `string.hmac_sha256`

#### SYNTAX
```
string.hmac_sha256 <text> <secretKey>
```

#### DESCRIPTION
Converts a string into a SHA-256 hash using a hash message authentication code (HMAC). A secret key parameter is required.

#### PARAMETERS
- `text`: The input string.
- `secretKey`: The secret key.


#### RETURNS
The `SHA-256` hash of the input string using a hash message authentication code (HMAC).

#### EXAMPLES
> **input**
```template-text
{{ "test" | string.hmac_sha256 "secret" }}
```
> **output**
```html
0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
```

[:top:](#builtins)

************************************************************************

timespan functions
--------------

A timespan object represents a time interval.

| Name             | Description
|--------------    |-----------------
| `.days`          | Gets the number of days of this interval 
| `.hours`         | Gets the number of hours of this interval
| `.minutes`       | Gets the number of minutes of this interval
| `.seconds`       | Gets the number of seconds of this interval
| `.milliseconds`  | Gets the number of milliseconds of this interval 
| `.total_days`    | Gets the total number of days in fractional part
| `.total_hours`   | Gets the total number of hours in fractional part
| `.total_minutes` | Gets the total number of minutes in fractional part
| `.total_seconds` | Gets the total number of seconds  in fractional part
| `.total_milliseconds` | Gets the total number of milliseconds  in fractional part

- [`timespan.from_days`](#timespanfrom_days)
- [`timespan.from_hours`](#timespanfrom_hours)
- [`timespan.from_minutes`](#timespanfrom_minutes)
- [`timespan.from_seconds`](#timespanfrom_seconds)
- [`timespan.from_milliseconds`](#timespanfrom_milliseconds)
- [`timespan.parse`](#timespanparse)

[:top:](#builtins)

************************************************************************

### `timespan.from_days`

#### SYNTAX
```
timespan.from_days <days>
```

#### DESCRIPTION
Returns a `timespan` object by specifying an interval in `days`.

#### PARAMETERS
- `days`: A number that represents days.


#### RETURNS
A `timespan` object.

#### EXAMPLES
> **input**
```template-text
{{ (timespan.from_days 5).days }}
```
> **output**
```html
5
```


[:top:](#builtins)

************************************************************************

### `timespan.from_hours`

#### SYNTAX
```
timespan.from_hours <hours>
```

#### DESCRIPTION
Returns a `timespan` object by specifying an interval in `hours`.

#### PARAMETERS
- `hours`: A number that represents hours.


#### RETURNS
A `timespan` object.

#### EXAMPLES
> **input**
```template-text
{{ (timespan.from_hours 5).hours }}
```
> **output**
```html
5
```


[:top:](#builtins)

************************************************************************

### `timespan.from_minutes`

#### SYNTAX
```
timespan.from_minutes <minutes>
```

#### DESCRIPTION
Returns a `timespan` object by specifying an interval in `minutes`.

#### PARAMETERS
- `minutes`: A number that represents minutes.


#### RETURNS
A `timespan` object.

#### EXAMPLES
> **input**
```template-text
{{ (timespan.from_minutes 5).minutes }}
```
> **output**
```html
5
```


[:top:](#builtins)

************************************************************************

### `timespan.from_seconds`

#### SYNTAX
```
timespan.from_seconds <seconds>
```

#### DESCRIPTION
Returns a `timespan` object by specifying an interval in `seconds`.

#### PARAMETERS
- `seconds`: A number that represents seconds.


#### RETURNS
A `timespan` object.

#### EXAMPLES
> **input**
```template-text
{{ (timespan.from_seconds 5).seconds }}
```
> **output**
```html
5
```


[:top:](#builtins)

************************************************************************

### `timespan.from_milliseconds`

#### SYNTAX
```
timespan.from_milliseconds <millis>
```

#### DESCRIPTION
Returns a `timespan` object by specifying an interval in `milliseconds`.

#### PARAMETERS
- `millis`: A number that represents milliseconds.


#### RETURNS
A `timespan` object.

#### EXAMPLES
> **input**
```template-text
{{ (timespan.from_milliseconds 5).milliseconds }}
```
> **output**
```html
5
```


[:top:](#builtins)

************************************************************************

### `timespan.parse`

#### SYNTAX
```
timespan.parse <text>
```

#### DESCRIPTION
Parses the specified input string into a `timespan` object. 

#### PARAMETERS
- `text`: A string representation of a timespan.


#### RETURNS
A timespan object parsed from the input string.

#### EXAMPLES
> **input**
```template-text
{{ "6:12:14" | timespan.parse }}
```
> **output**
```html
6:12:14
```



************************************************************************

> Last updated on 13/12/2018 8:08:16 PM
> This page is generated by a tool. Any changes will be lost on the next update cycle.

