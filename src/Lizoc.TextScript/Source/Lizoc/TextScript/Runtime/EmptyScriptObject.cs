using System.Collections.Generic;
using System.Diagnostics;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Syntax;

namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// The empty object (unique singleton, cannot be modified, does not contain any properties)
    /// </summary>
    [DebuggerDisplay("<empty object>")]    
    public sealed class EmptyScriptObject : IScriptObject
    {
        public static readonly EmptyScriptObject Default = new EmptyScriptObject();

        private EmptyScriptObject()
        {
        }

        public int Count => 0;

        public IEnumerable<string> GetMembers()
        {
            yield break;
        }

        public bool Contains(string member)
        {
            return false;
        }

        public bool IsReadOnly
        {
            get => true;
            set { }
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            value = null;
            return false;
        }

        public bool CanWrite(string member)
        {
            return false;
        }

        public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            throw new ScriptRuntimeException(span, RS.CannotSetPropertyOnEmptyObject);
        }

        public bool Remove(string member)
        {
            return false;
        }

        public void SetReadOnly(string member, bool readOnly)
        {
        }

        public IScriptObject Clone(bool deep)
        {
            return this;
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}