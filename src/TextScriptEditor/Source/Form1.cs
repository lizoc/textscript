// -----------------------------------------------------------------------
// <copyright file="Form1.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text;
using System.Windows.Forms;
using Lizoc.TextScript;
using Lizoc.TextScript.Parsing;

namespace TextScriptEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Template template = Template.Parse(inputBox.Text);
                if (template.HasErrors)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("We got a problem :(");
                    sb.AppendLine("===================");
                    foreach (LogMessage msg in template.Messages)
                    {
                        sb.AppendLine(string.Format("{0} [{1}]", msg.Type.ToString(), msg.Span.ToStringSimple()));
                        sb.AppendLine(msg.Message);
                        sb.AppendLine("-------------------------------");
                    }

                    outputView.Text = sb.ToString();
                }
                else
                {
                    outputView.Text = template.Render();
                }
            }
            catch (Exception ex)
            {
                outputView.Text = ex.Message;
            }
        }
    }
}
