// -----------------------------------------------------------------------
// <copyright file="ConvertFromTemplateCommand.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Linq;
using SC = System.Collections;
using TS = Lizoc.TextScript;
using TSR = Lizoc.TextScript.Runtime;
using TSP = Lizoc.TextScript.Parsing;
using Lizoc.PowerShell.TextScript;

namespace Lizoc.PowerShell.Commands
{
    /// <summary>
    /// Renders a TextScript template.
    /// </summary>
    [Cmdlet(
        VerbsData.ConvertFrom, "Template", 
        HelpUri = "http://docs.lizoc.com/ps/convertfromtemplate",
        RemotingCapability = RemotingCapability.None
    ), OutputType(typeof(string))]
    public class ConvertFromTemplateCommand : Cmdlet
    {
        private TSR.ScriptObject _templateVariable;
        private int _maxRecurseDepth = 64;
        private bool _infiniteRecurseDepth = false;
        private bool _strictMode = false;
        private bool _fileAccess = true;

        private bool _convertHashtableRecurse = true;

        /// <summary>
        /// The template script.
        /// </summary>
        [AllowEmptyString, Parameter(Mandatory = true, Position = 0)]
        public string Template { get; set; }

        /// <summary>
        /// A data object that the template will have access to.
        /// </summary>
        [Parameter(Position = 1, ValueFromPipeline = true)]
        public object InputObject { get; set; }

        /// <summary>
        /// Restrict the maximum recursion depth.
        /// </summary>
        [Parameter()]
        [ValidateRange(0, int.MaxValue)]
        public int Depth 
        { 
            get { return _maxRecurseDepth; }
            set 
            {
                _maxRecurseDepth = value;
                _infiniteRecurseDepth = (_maxRecurseDepth == 0);
            } 
        }

        /// <summary>
        /// Render the template in strict mode.
        /// </summary>
        [Parameter()]
        public SwitchParameter StrictMode
        {
            get { return _strictMode; }
            set { _strictMode = value; }
        }

        /// <summary>
        /// Grants the template access to the underlying file system.
        /// </summary>
        [Parameter()]
        public SwitchParameter FileAccess
        {
            get { return _fileAccess; }
            set { _fileAccess = value; }
        }

        /// <see cref="Cmdlet.BeginProcessing()" />
        protected override void BeginProcessing()
        {
        }

        /// <see cref="Cmdlet.ProcessRecord()" />
        protected override void ProcessRecord()
        {
            //_inputObjectBuffer.Add(this.InputObject);
        }

        /// <see cref="Cmdlet.EndProcessing()" />
        protected override void EndProcessing()
        {
            // ignore if template is empty
            if (string.IsNullOrEmpty(this.Template))
            {
                // returns an empty string if input is an empty string
                if (this.Template != null)
                    base.WriteObject(string.Empty);

                return;
            }

            // template runtime
            TS.Template templateRuntime = null;

            // parse first
            try
            {
                templateRuntime = TS.Template.Parse(this.Template);

                // this is mostly to output warnings
                if (templateRuntime != null && 
                    templateRuntime.Messages != null && 
                    templateRuntime.Messages.Count > 0)
                {
                    WriteTemplateParseMessages(templateRuntime.Messages);
                }
            }
            catch (Exception ex)
            {
                // the exception will contain all errors and warnings
                ErrorRecord parseErrorRecord = new ErrorRecord(ex, "TemplateParseFailure", ErrorCategory.ParserError, null);
                base.ThrowTerminatingError(parseErrorRecord);
            }

            // add variable if specified
            // then return rendered result
            TS.TemplateContext templateContext = new TS.TemplateContext()
            {
                // loads file from disk using the 'include' keyword
                TemplateLoader = (_fileAccess ? new TSR.LocalFileTemplateLoader() : null)
            };
            try
            {
                if (_strictMode)
                    templateContext.StrictVariables = true;

                if (this.InputObject != null)
                {
                    if (this.InputObject is SC.Hashtable)
                        _templateVariable = ConvertHashtableToScriptObject((SC.Hashtable)this.InputObject);
                    else if (this.InputObject is PSObject)
                        _templateVariable = ConvertPSObjectToScriptObject((PSObject)this.InputObject);
                    else
                        throw new ArgumentException(string.Format(RS.ArgumentTypeNotSupported, "hashtable, PSObject"), "InputObject");

                    templateContext.PushGlobal(_templateVariable);
                }

                base.WriteObject(templateRuntime.Render(templateContext)); 
            }
            catch (Exception ex)
            {
                ErrorRecord renderErrorRecord = new ErrorRecord(ex, "TemplateRenderFailure", ErrorCategory.ParserError, null);
                base.ThrowTerminatingError(renderErrorRecord);                
            }
        }

        private TSR.ScriptArray ConvertArrayObject(SC.IEnumerable array, int recurseDepth = 0)
        {
            if ((_infiniteRecurseDepth == false) && (recurseDepth >= _maxRecurseDepth))
            {
                base.WriteWarning(string.Format(RS.MaxDepthForEnumerableReached, _maxRecurseDepth));
                return new TSR.ScriptArray(array);
            }

            TSR.ScriptArray scriptArray = new TSR.ScriptArray();

            foreach (object item in array)
            {
                if (item is Array)
                {
                    scriptArray.Add(ConvertArrayObject((SC.IEnumerable)item, recurseDepth + 1));
                }
                else
                {
                    if (item is SC.Hashtable)
                        scriptArray.Add(ConvertHashtableToScriptObject((SC.Hashtable)item, recurseDepth + 1));
                    else if (item is PSObject)
                        scriptArray.Add(ConvertPSObjectToScriptObject((PSObject)item, recurseDepth + 1));
                    else
                        scriptArray.Add(item);
                }
            }

            return scriptArray;
        }

        private TSR.ScriptObject ConvertPSObjectToScriptObject(PSObject psObject, int recurseDepth = 0)
        {
            if (psObject == null)
                return null;

            TSR.ScriptObject templateScriptObj = new TSR.ScriptObject();

            foreach (PSPropertyInfo psProperty in psObject.Properties)
            {
                string propertyName = psProperty.Name;
                object propertyValue = psProperty.Value;
                TSR.ScriptObject convertedPropertyValue = null;
                TSR.ScriptArray convertedPropertyArrayValue = null;

                // handle object arrays
                if (propertyValue is Array)
                {
                    if ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth))
                        convertedPropertyArrayValue = ConvertArrayObject((SC.IEnumerable)propertyValue, recurseDepth + 1);
                    else
                        base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "IEnumerable", _maxRecurseDepth));

                    if (convertedPropertyArrayValue != null)
                        templateScriptObj.Add(propertyName, convertedPropertyArrayValue);
                    else
                        templateScriptObj.Add(propertyName, propertyValue);
                }
                else
                {
                    if (propertyValue is SC.Hashtable)
                    {
                        if (_convertHashtableRecurse && 
                            ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth)))
                        {
                            convertedPropertyValue = ConvertHashtableToScriptObject((SC.Hashtable)propertyValue, recurseDepth + 1);
                        }
                        else
                        {
                            // only warn if it's because max depth is reached
                            if (_convertHashtableRecurse == true)
                                base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "Hashtable", _maxRecurseDepth));
                        }
                    }
                    else if (propertyValue is PSObject)
                    {
                        if ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth))
                            convertedPropertyValue = ConvertPSObjectToScriptObject((PSObject)propertyValue, recurseDepth + 1);
                        else
                            base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "PSObject", _maxRecurseDepth));
                    }
                    else 
                    {
                        // do nothing
                        //base.WriteObject(string.Format("{0} is {1}", propertyName, propertyValue.GetType().ToString()));
                    }

                    if (convertedPropertyValue != null)
                        templateScriptObj.Add(propertyName, convertedPropertyValue);
                    else
                        templateScriptObj.Add(propertyName, propertyValue);
                }
            }

            return templateScriptObj;
        }

        private TSR.ScriptObject ConvertHashtableToScriptObject(SC.Hashtable hashtable, int recurseDepth = 0)
        {
            if (hashtable == null)
                return null;

            TSR.ScriptObject templateScriptObj = new TSR.ScriptObject();

            SC.ICollection keys = hashtable.Keys;
            foreach (object key in keys)
            {
                string propertyName = key.ToString();
                object propertyValue = hashtable[key];
                TSR.ScriptObject convertedPropertyValue = null;
                TSR.ScriptArray convertedPropertyArrayValue = null;

                if (propertyValue is Array)
                {
                    if ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth))
                        convertedPropertyArrayValue = ConvertArrayObject((SC.IEnumerable)propertyValue, recurseDepth + 1);

                    if (convertedPropertyArrayValue != null)
                        templateScriptObj.Add(propertyName, convertedPropertyArrayValue);
                    else
                        templateScriptObj.Add(propertyName, propertyValue);
                }
                else
                {
                    // recursively convert property value to script object 
                    // supports hashtable, psobject and pscustomobject
                    if (propertyValue is SC.Hashtable)
                    {
                        if (_convertHashtableRecurse && 
                            ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth)))
                        {
                            convertedPropertyValue = ConvertHashtableToScriptObject((SC.Hashtable)propertyValue, recurseDepth + 1);
                        }

                        //base.WriteObject(string.Format("{0} is hashtable", propertyName));
                    }
                    else if (propertyValue is PSObject)
                    {
                        if ((_infiniteRecurseDepth == true) || (recurseDepth < _maxRecurseDepth))
                            convertedPropertyValue = ConvertPSObjectToScriptObject((PSObject)propertyValue, recurseDepth + 1);

                        //base.WriteObject(string.Format("{0} is psobject", propertyName));
                    }
                    else
                    {
                        // do nothing
                    }

                    if (convertedPropertyValue != null)
                        templateScriptObj.Add(propertyName, convertedPropertyValue);
                    else
                        templateScriptObj.Add(propertyName, propertyValue);
                }
            }

            return templateScriptObj;
        }

        private void WriteTemplateParseMessages(List<TSP.LogMessage> messages)
        {
            foreach (TSP.LogMessage msg in messages)
            {
                if (msg.Type == TSP.ParserMessageType.Error)
                {
                    Exception parseException = new Exception(msg.ToString());
                    ErrorRecord parseErrorRecord = new ErrorRecord(parseException, "TemplateParseError", ErrorCategory.ParserError, null);
                    base.WriteError(parseErrorRecord);
                }
                else if (msg.Type == TS.Parsing.ParserMessageType.Warning)
                {
                    base.WriteWarning(msg.ToString());
                }
            }
        }
    }
}
