using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Functions
{
    /// <summary>
    /// Array functions available through the object `array`.
    /// </summary>
    public partial class ArrayFunctions : ScriptObject
    {
        /// <summary>
        /// Adds a value to the end of the input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="value">The value to add at the end of the list.</param>
        /// <returns>A new list with the value added</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 2, 3] | array.add 4 }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4]
        /// ```
        /// </remarks>
        public static IList Add(IList list, object value)
        {
            if (list == null)
                return new ScriptArray { value };

            list = new ScriptArray(list) { value };
            return list;
        }


        /// <summary>
        /// Concatenates two lists.
        /// </summary>
        /// <param name="list1">The 1st input list.</param>
        /// <param name="list2">The 2nd input list.</param>
        /// <returns>The concatenation of the two input lists.</returns>
        /// <remarks>
        /// The `add_range` function is an alias to the `concat` function.
        /// 
        /// ```template-text
        /// {{ [1, 2, 3] | array.concat [4, 5] }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4, 5]
        /// ```
        /// </remarks>
        public static IEnumerable AddRange(IEnumerable list1, IEnumerable list2)
        {
            return Concat(list1, list2);
        }


        /// <summary>
        /// Removes any `null` values from the input list.
        /// </summary>
        /// <param name="list">An input list.</param>
        /// <returns>The list with all `null` values removed.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, null, 3] | array.compact }}
        /// ```
        /// ```html
        /// [1, 3]
        /// ```
        /// </remarks>
        public static ScriptArray Compact(IEnumerable list)
        {
            if (list == null)
                return null;

            ScriptArray result = new ScriptArray();
            foreach (var item in list)
            {
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Concatenates two lists.
        /// </summary>
        /// <param name="list1">The 1st input list.</param>
        /// <param name="list2">The 2nd input list.</param>
        /// <returns>The concatenation of the two input lists.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 2, 3] | array.concat [4, 5] }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4, 5]
        /// ```
        /// </remarks>
        public static IEnumerable Concat(IEnumerable list1, IEnumerable list2)
        {
            if (list2 == null && list1 == null)
                return null;

            if (list2 == null)
                return list1;

            if (list1 == null)
                return list2;

            ScriptArray result = new ScriptArray(list1);
            foreach (var item in list2)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Outputs an item from a list of strings in a cyclic manner. Each time this function is used on a list, the next item or the first item is returned.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="list">An input list.</param>
        /// <param name="group">The group used. Default is `null`.</param>
        /// <returns>The first item if the list was used by this function for the first time, or the previous output was the last item in the list. Otherwise, the output is the item at the index position one larger than the previous output item.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// ```
        /// ```html
        /// one
        /// two
        /// three
        /// one
        /// ```
        /// `cycle` accepts a parameter called cycle group in cases where you need multiple cycle blocks in one template. 
        /// If no name is supplied for the cycle group, then it is assumed that multiple calls with the same parameters are one group.
        /// </remarks>
        public static object Cycle(TemplateContext context, SourceSpan span, IList list, object group = null)
        {
            if (list == null)
                return null;

            string strGroup = group == null
                ? Join(context, span, list, ",")
                : context.ToString(span, group);

            // We create a cycle variable that is dependent on the exact AST context.
            // So we allow to have multiple cycle running in the same loop
            CycleKey cycleKey = new CycleKey(strGroup);

            object cycleValue;
            Dictionary<object, object> currentTags = context.Tags;
            if (!currentTags.TryGetValue(cycleKey, out cycleValue) || !(cycleValue is int))
                cycleValue = 0;

            int cycleIndex = (int)cycleValue;
            cycleIndex = list.Count == 0 ? 0 : cycleIndex % list.Count;
            object result = null;
            if (list.Count > 0)
            {
                result = list[cycleIndex];
                cycleIndex++;
            }
            currentTags[cycleKey] = cycleIndex;

            return result;
        }

        /// <summary>
        /// Returns the first item in an input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <returns>The first item of the input list.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6] | array.first }}
        /// ```
        /// ```html
        /// 4
        /// ```
        /// Use the `last` function to return the last item in a list.
        ///
        /// Use the `limit` function to return the first `n` number of items, or `slice` function to return a subset of a list.
        /// </remarks>
        public static object First(IEnumerable list)
        {
            if (list == null)
                return null;

            var realList = list as IList;
            if (realList != null)
                return realList.Count > 0 ? realList[0] : null;

            foreach (var item in list)
            {
                return item;
            }

            return null;
        }

        /// <summary>
        /// Inserts a value at the specified index in the input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="index">The index position in the list where an item should be inserted. If this number is less than one, the new item will be inserted at the first position.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A new list with the item inserted. All existing items at or beyond the index position will be shifted to the next index position.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ ["a", "b", "c"] | array.insert_at 2 "Yo" }}
        /// ```
        /// ```html
        /// [a, b, Yo, c]
        /// ```
        /// </remarks>
        public static IList InsertAt(IList list, int index, object value)
        {
            if (index < 0)
                index = 0;

            list = list == null ? new ScriptArray() : new ScriptArray(list);
            // Make sure that the list has already inserted elements before the index
            for (int i = list.Count; i < index; i++)
            {
                list.Add(null);
            }

            list.Insert(index, value);

            return list;
        }

        /// <summary>
        /// Joins all items in a list using a delimiter string.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="list">The input list.</param>
        /// <param name="delimiter">The delimiter string to use to separate items in the list.</param>
        /// <param name="format">An optional string to specify how each item should be formatted.</param>
        /// <returns>A new list with the items joined into a string.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 2, 3] | array.join "|" }}
        /// {{ [1, 2, 3] | array.join "," "*{0}*" }}
        /// ```
        /// ```html
        /// 1|2|3
        /// *1*,*2*,*3*
        /// ```
        /// </remarks>
        public static string Join(TemplateContext context, SourceSpan span, IEnumerable list, string delimiter, string format = null)
        {
            if (list == null)
                return string.Empty;

            StringBuilder text = new StringBuilder();
            bool afterFirst = false;
            foreach (var obj in list)
            {
                if (afterFirst)
                    text.Append(delimiter);

                if (format == null)
                    text.Append(context.ToString(span, obj));
                else
                    text.Append(string.Format(format, context.ToString(span, obj)));

                afterFirst = true;
            }
            return text.ToString();
        }

        /// <summary>
        /// Returns the last item in an input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <returns>The last item of the input list.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6] | array.last }}
        /// ```
        /// ```html
        /// 6
        /// ```
        /// Use the `first` function to return the first item in a list.
        ///
        /// Use the `slice` function to return a subset of a list.
        /// </remarks>
        public static object Last(IEnumerable list)
        {
            if (list == null)
                return null;

            var readList = list as IList;
            if (readList != null)
                return readList.Count > 0 ? readList[readList.Count - 1] : null;

            // Slow path, go through the whole list
            return list.Cast<object>().LastOrDefault();
        }

        /// <summary>
        /// Returns no more than the specified number of items from the input list, starting from the first index position.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="count">The number of elements to return from the input list. If this number is less than one, no item is returned.</param>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6] | array.limit 2 }}
        /// ```
        /// ```html
        /// [4, 5]
        /// ```
        /// </remarks>
        public static ScriptArray Limit(IEnumerable list, int count)
        {
            if (list == null)
                return null;

            ScriptArray result = new ScriptArray();
            foreach (var item in list)
            {
                count--;
                if (count < 0)
                    break;

                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Accepts a list of objects, and outputs the value of a specified property in each object.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="list">The input list.</param>
        /// <param name="member">The name of the property. An item must be an object and contains this property in order for this function to output its value.</param>
        /// <remarks>
        /// ```template-text
        /// {{ 
        /// products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
        /// products | array.map "type" | array.uniq | array.sort }}
        /// ```
        /// ```html
        /// [electronics, fruit, furniture]
        /// ```
        /// </remarks>
        public static IEnumerable Map(TemplateContext context, SourceSpan span, object list, string member)
        {
            if (list == null || member == null)
                yield break;

            var enumerable = list as IEnumerable;
            List<object> realList = enumerable?.Cast<object>().ToList() ?? new List<object>(1) { list };
            if (realList.Count == 0)
                yield break;

            foreach (var item in realList)
            {
                IObjectAccessor itemAccessor = context.GetMemberAccessor(item);
                if (itemAccessor.HasMember(context, span, item, member))
                {
                    itemAccessor.TryGetValue(context, span, item, member, out object value);
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Returns the remaining items in a list after the specified offset.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="index">The starting index position of the list where items should be returned. If the index is less than 1, all items are returned.</param>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6, 7, 8] | array.offset 2 }}
        /// ```
        /// ```html
        /// [6, 7, 8]
        /// ```
        /// </remarks>
        public static ScriptArray Offset(IEnumerable list, int index)
        {
            if (list == null)
                return null;

            ScriptArray result = new ScriptArray();
            foreach (var item in list)
            {
                if (index <= 0)
                    result.Add(item);
                else
                    index--;
            }
            return result;
        }

        /// <summary>
        /// Removes an item at the specified index position from the input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="index">The index position of the item to remove.</param>
        /// <returns>A new list with the item removed. If index is negative, this function will remove from the end of the list.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6, 7, 8] | array.remove_at 2 }}
        /// ```
        /// ```html
        /// [4, 5, 7, 8]
        /// ```
        /// If the `index` is negative, removes from the end of the list. Notice that we need to put -1 in parenthesis to avoid confusing the parser with a binary `-` operation.
        /// ```template-text
        /// {{ [4, 5, 6, 7, 8] | array.remove_at (-1) }}
        /// ```
        /// ```html
        /// [4, 5, 6, 7]
        /// ```
        /// </remarks>
        public static IList RemoveAt(IList list, int index)
        {
            if (list == null)
                return new ScriptArray();

            list = new ScriptArray(list);

            // If index is negative, start from the end
            if (index < 0)
                index = list.Count + index;

            if (index >= 0 && index < list.Count)
                list.RemoveAt(index);

            return list;
        }

        /// <summary>
        /// Returns a string based on whether an array is empty.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="replace">The string to replace if the input list is null or empty.</param>
        /// <param name="notEmpty">The string to return if the input list contains items.</param>
        /// <returns>A string based on whether the list is null or empty.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6, 7, 8] | array.replace_empty "i am empty" "i am full" }}
        /// ```
        /// ```html
        /// i am full
        /// ```
        /// </remarks>
        public static string ReplaceEmpty(IEnumerable list, string replace, string notEmpty = null)
        {
            if (list == null || Size(list) == 0)
                return replace;

            return notEmpty;
        }

        /// <summary>
        /// Reverses the input list.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>A new list in reversed order.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6, 7] | array.reverse }}
        /// ```
        /// ```html
        /// [7, 6, 5, 4]
        /// ```
        /// </remarks>
        public static IEnumerable Reverse(IEnumerable list)
        {
            if (list == null)
                return Enumerable.Empty<object>();

            // #TODO provide a special path for IList
            //var list = list as IList;
            //if (list != null)
            //{
            //}
            return list.Cast<object>().Reverse();
        }

        /// <summary>
        /// Returns the number of items in the input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <returns>The number of items in the input list.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [4, 5, 6] | array.size }}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
        public static int Size(IEnumerable list)
        {
            if (list == null)
                return 0;

            var collection = list as ICollection;
            if (collection != null)
                return collection.Count;

            // Slow path, go through the whole list
            return list.Cast<object>().Count();
        }
        
        /// <summary>
        /// Sorts the elements of the input list according to the value of each item or the value of the specified property of each object-type item.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="span">The source span.</param>
        /// <param name="list">The input list.</param>
        /// <param name="member">The property name to sort according to its value. This is `null` by default, meaning that the item's value is used instead.</param>
        /// <returns>A list sorted according to the value of each item or the value of the specified property of each object-type item.</returns>
        /// <remarks>
        /// Sort by value: 
        /// ```template-text
        /// {{ [10, 2, 6] | array.sort }}
        /// ```
        /// ```html
        /// [2, 6, 10]
        /// ```
        /// Sorts by object property's value: 
        /// ```template-text
        /// {{
        /// products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
        /// products | array.sort "title" | array.map "title"
        /// }}
        /// ```
        /// ```html
        /// [computer, orange, sofa]
        /// ```
        /// </remarks>
        public static IEnumerable Sort(TemplateContext context, SourceSpan span, object list, string member = null)
        {
            if (list == null)
                return Enumerable.Empty<object>();

            var enumerable = list as IEnumerable;
            if (enumerable == null)
                return new ScriptArray(1) { list };

            List<object> realList = enumerable.Cast<object>().ToList();
            if (realList.Count == 0)
                return realList;

            if (string.IsNullOrEmpty(member))
            {
                realList.Sort();
            }
            else
            {
                realList.Sort((a, b) =>
                {
                    IObjectAccessor leftAccessor = context.GetMemberAccessor(a);
                    IObjectAccessor rightAccessor = context.GetMemberAccessor(b);

                    object leftValue = null;
                    object rightValue = null;

                    if (!leftAccessor.TryGetValue(context, span, a, member, out leftValue))
                        context.TryGetMember?.Invoke(context, span, a, member, out leftValue);

                    if (!rightAccessor.TryGetValue(context, span, b, member, out rightValue))
                        context.TryGetMember?.Invoke(context, span, b, member, out rightValue);

                    return Comparer<object>.Default.Compare(leftValue, rightValue);
                });
            }

            return realList;
        }

        /// <summary>
        /// Returns the unique items of the input list.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <returns>A list of unique elements of the input list.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 1, 4, 5, 8, 8] | array.uniq }}
        /// ```
        /// ```html
        /// [1, 4, 5, 8]
        /// ```
        /// </remarks>
        public static IEnumerable Uniq(IEnumerable list)
        {
            return list?.Cast<object>().Distinct();
        }

        /// <summary>
        /// Checks whether a list contains the specified item.
        /// </summary>
        /// <param name="list">The input list.</param>
        /// <param name="item">The item that should be in the input list.</param>
        /// <param name="ignoreCase">If `true`, use case-insensitive comparison for strings. Otherwise, `false`. This is `false` by default.</param>
        /// <returns>`true` if the item exists in the list. Otherwise, `false`.</returns>
        /// <remarks>
        /// ```template-text
        /// {{ [1, 4, "foo"] | array.has "Foo" true }}
        /// {{ [1, 4, "foo"] | array.has 1 }}
        /// ```
        /// ```html
        /// true
        /// true
        /// ```
        /// </remarks>
        public static bool Has(IEnumerable list, object item, bool ignoreCase = false)
        {
            foreach (var member in list)
            {
                if ((item is string) && (member is string))
                {
                    if (((string)item).Equals((string)member, (ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
                        return true;
                }
                else if ((item is int) && (member is int))
                {
                    if (((int)item) == ((int)member))
                        return true;
                }
                else if (((item is int) || (item is long)) && ((member is int) || (member is long)))
                {
                    if (((long)item) == ((long)member))
                        return true;
                }
                else
                {
                    if (item.Equals(member))
                        return true;
                }
            }

            return false;
        }

        private class CycleKey : IEquatable<CycleKey>
        {
            public readonly string Group;

            public CycleKey(string @group)
            {
                Group = @group;
            }

            public bool Equals(CycleKey other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return string.Equals(Group, other.Group);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((CycleKey) obj);
            }

            public override int GetHashCode()
            {
                return (Group != null ? Group.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return ("cycle " + Group);
            }
        }
    }
}