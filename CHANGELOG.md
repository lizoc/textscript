Changelog
=========

1.5.0 (13 Dec 2018)
-------------------
- More functions: 
  - `object.from_string`, `object.format`
  - `html.xmlattrib` and `html.xmltext`
  - `fs.dir` and `fs.test`
  - `array.has`
  - `string.pad_left` and `string.pad_right`
- `math.format` now support the `culture` parameter, which allows you to override current culture.
- More formatting options for `date.to_string`.
- Caret is now the escape character
- Now support decimal type numbers (`System.Decimal`).
- Range now supports the `long` type (`System.Int64`).
- Number parsing is now done using invariant culture
- Option to configure array and object member accesors in relaxed mode. Non-existing members or items won't generate errors in relaxed mode.
- You can now directly configure member renamer and filter delegates directly in `Template.Evaluate`.
- Features for developers:
  - API documentation is now automated.
  - Staging tools upgraded
  - Resource string support for localization.

1.4.0 (25 Sept 2018)
--------------------
- Added PowerShell module
- Added replace_empty function
- Enhanced array.join with a formatter

1.2.1 (1 June 2018)
-------------------
- Add `string.to_int` `string.to_long` `string.to_float` `string.to_double`. (#55)

0.1.0 (31 May 2016)
-------------------
- Initial version
