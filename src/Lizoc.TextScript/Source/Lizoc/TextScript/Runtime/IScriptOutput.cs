namespace Lizoc.TextScript.Runtime
{
    /// <summary>
    /// Interface used to text output when evaluating a template used by <see cref="TemplateContext.Output"/> and <see cref="TemplateContext.PushOutput()"/>
    /// </summary>
    public interface IScriptOutput
    {
        IScriptOutput Write(char c);

        IScriptOutput Write(string text);

        IScriptOutput Write(int number);

        IScriptOutput Write(string text, int offset, int count);
        
        IScriptOutput WriteLine(string text);

        IScriptOutput WriteLine();
    }
}