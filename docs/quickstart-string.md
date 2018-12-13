Quickstart for Strings
======================
String manipulation is done via the `string` object.


Get info
--------
- Get string length: {{ "abcdef" | string.size }}


Concatation
-----------
Adding strings together:

- Concat with `append`: {{ string.append "foo" "bar" }}, {{ "hi" | string.append "baz" }}
- Add string before: {{ "foo" | string.prepend "hi" }}
- Add string after: {{ "foo" | string.append "bar" }}


Capitalization
--------------
- Make the first letter capital: {{ "test" | string.capitalize }}
- Make the first letter of each word capital: {{ "a grand title" | string.capitalizewords }}
- Convert to lower case: {{ "WoW" | string.downcase }}
- Convert to upper case: {{ "WoW" | string.upcase }}


Substring
---------
This covers replacing, splitting and contains:

- Contains, case sensitive: {{ "foobarbaz" | string.contains "bar" }}
- Ends with: {{ "foobarbaz" | string.ends_with "baz" }}
- Starts with: {{ "foobarbaz" | string.starts_with "fo" }}
- Remove occurance of substring: {{ "hi world bye world" | string.remove "world" }}
- Remove first occurance of substring: {{ "hi world bye world" | string.remove_first "world" }}
- Replace occurance of substring with something else: {{ "hi world bye world" | string.replace "world" "universe" }}
- Replace first occurance of substring with something else: {{ "hi world bye world" | string.replace_first "world" "universe" }}
- Replace a null or empty string with something (no need to use if/else): {{ "" | string.replace_empty "its empty!" "there is text" }}
- Get a substring. Starting from index position 1: {{ "aloha" | string.slice 1 }}, from index 1 for a length of 2: {{ "aloha" | string.slice 1 2 }}. If string is shorter than specified length, the rest of the string is returned without error: {{ "aloha" | string.slice 1 200 }}
- slice1 is the same as slice x 1: {{ "aloha" | string.slice 3 1 }} = {{ "aloha" | string.slice1 3 }}
- Split a string into an array: {{ "how are you" | string.split " " }}


Web
---
- Make string into a url handle: {{ "my 100% awesome day!" | string.handleize }}


Stripping
---------
Removing characters and whitespace:

- remove whitespace to left: {{ "  fdsa" | string.lstrip }}
- remove whitespace to right: {{ "fdsa    " | string.rstrip }}
- remove leading and trailing whitespaces: {{ "   fdsa    " | string.strip }}
- remove newlines: {{ "dfsads\nfasd" | string.strip_newlines }}
- truncate to n chars or less. If you added an eclipse, it counts towards the total number of characters: {{ "this is a drag" | string.truncate 7 "~~~" }}
- truncate to n words or less: {{ "this is a drag" | string.truncatewords 3 "..." }}


Linguistics
-----------
- Return singular or plural: 5 {{ 5 | string.pluralize "orange" "oranges" }}


Numeric conversion
------------------
- Convert to integer: {{ ("32" | string.to_int) + 1 }}
- To long: {{ ("32" | string.to_long) + 1 }}
- To float: {{ ("32.32" | string.to_float) + 1.2 }}
- To double: {{ ("32.32" | string.to_double) + 1.2 }}


Hashing
-------
- To hmac_sha1: {{ "secret" | string.hmac_sha1 "mysecret" }}
- To sha256: {{ "secret" | string.sha256 }}
- To sha1: {{ "secret" | string.sha1 }}
- To md5: {{ "secret" | string.md5 }}


That's a lot, but autocomplete will help you along the way :)
