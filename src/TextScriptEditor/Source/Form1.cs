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
