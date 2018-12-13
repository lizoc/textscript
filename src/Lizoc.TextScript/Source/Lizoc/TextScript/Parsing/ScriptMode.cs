namespace Lizoc.TextScript.Parsing
{
    /// <summary>
    /// Defines how the parser should parse a TextScript text.
    /// </summary>
    public enum ScriptMode
    {
        /// <summary>
        /// The template contains a regular content (text and script mixed).
        /// </summary>
        Default,

        /// <summary>
        /// The template contains a liquid content (text and script mixed).
        /// </summary>
        Liquid,

        /// <summary>
        /// The template contains a frontmatter (script only) and the parser will parse only this part.
        /// </summary>
        FrontMatterOnly,

        /// <summary>
        /// The template contains a frontmatter (script only) and a content (text and script mixed) and will parse both.
        /// </summary>
        FrontMatterAndContent,

        /// <summary>
        /// The template is directly code (script only) so it is not necessary {{ }} for entering a code block
        /// </summary>
        ScriptOnly,
    }
}