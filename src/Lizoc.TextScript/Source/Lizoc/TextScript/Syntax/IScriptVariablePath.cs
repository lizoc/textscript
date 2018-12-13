namespace Lizoc.TextScript.Syntax
{
    public interface IScriptVariablePath
    {
        object GetValue(TemplateContext context);

        void SetValue(TemplateContext context, object valueToSet);

        string GetFirstPath();
    }
}