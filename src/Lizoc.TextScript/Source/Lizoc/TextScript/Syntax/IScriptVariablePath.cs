// -----------------------------------------------------------------------
// <copyright file="IScriptVariablePath.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

namespace Lizoc.TextScript.Syntax
{
    public interface IScriptVariablePath
    {
        object GetValue(TemplateContext context);

        void SetValue(TemplateContext context, object valueToSet);

        string GetFirstPath();
    }
}