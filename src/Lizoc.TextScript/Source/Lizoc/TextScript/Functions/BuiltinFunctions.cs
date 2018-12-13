using Lizoc.TextScript.Runtime;

namespace Lizoc.TextScript.Functions
{
    public class BuiltinFunctions : ScriptObject
    {
        /// <summary>
        /// This object is readonly, should not be modified by any other objects internally.
        /// </summary>
        internal static readonly ScriptObject Default = new DefaultBuiltins();

        public BuiltinFunctions() : base(10)
        {
            ((ScriptObject)Default.Clone(true)).CopyTo(this);
        }

        /// <summary>
        /// Use an internal object to create all default builtins just once to avoid allocations of delegates/IScriptCustomFunction
        /// </summary>
        private class DefaultBuiltins : ScriptObject
        {
            public DefaultBuiltins() : base(10, false)
            {
                SetValue("array", new ArrayFunctions(), true);
                SetValue("empty", EmptyScriptObject.Default, true);
                SetValue("include", new IncludeFunction(), true);
                SetValue(DateTimeFunctions.DateVariable.Name, new DateTimeFunctions(), true);
                SetValue("html", new HtmlFunctions(), true);
                SetValue("math", new MathFunctions(), true);
                SetValue("object", new ObjectFunctions(), true);
                SetValue("regex", new RegexFunctions(), true);
                SetValue("string", new StringFunctions(), true);
                SetValue("timespan", new TimeSpanFunctions(), true);
                SetValue("fs", new FileSystemFunctions(), true);
            }
        }
    }
}