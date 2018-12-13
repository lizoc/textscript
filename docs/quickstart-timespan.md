Quickstart Timespan
===================

Timespan means a period of time. The `timespan` object can be created in a variety of ways:

- Cast number as days: {{ (timespan.from_days 5) }}
- Cast number as hours: {{ (timespan.from_hours 5) }}
- Cast number as minutes: {{ (timespan.from_minutes 5) }}
- Cast number as seconds: {{ (timespan.from_seconds 5) }}
- Cast number as milliseconds: {{ (timespan.from_milliseconds 5) }}
- Parse it directly: {{ timespan.parse "1.2:00" }}

Once you have a timespan object, you can easily get different 'parts' of it.
{{ x = timespan.from_milliseconds 32923432479324 }}
For example, {{ x }} is equal to:

- {{ x.days }} days
- {{ x.hours }} hours
- {{ x.minutes }} minutes
- {{ x.seconds }} seconds, and
- {{ x.milliseconds }} milliseconds

Maybe you just want to convert the whole timespan to hours or minutes, etc. No problem:

- {{ x.total_days }} days
- {{ x.total_hours }} hours
- {{ x.total_minutes }} minutes
- {{ x.total_seconds }} seconds
- {{ x.total_milliseconds }} milliseconds

That's it!
