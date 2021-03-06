﻿// -----------------------------------------------------------------------
// <copyright file="ConvertFromTemplateCommand.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Management.Automation;
using SC = System.Collections;
using TS = Lizoc.TextScript;
using TSR = Lizoc.TextScript.Runtime;
using TSP = Lizoc.TextScript.Parsing;
using Lizoc.PowerShell.TextScript;
using Lizoc.TextScript.Runtime;
using System.Threading.Tasks;

namespace Lizoc.PowerShell.Commands
{
    /// <summary>
    /// Renders a TextScript template.
    /// </summary>
    [Cmdlet(
        VerbsData.ConvertFrom, "Template", 
        DefaultParameterSetName = "__AllParameterSets",
        HelpUri = "http://docs.lizoc.com/ps/convertfromtemplate",
        RemotingCapability = RemotingCapability.None
    ), OutputType(typeof(string))]
    public class ConvertFromTemplateCommand : Cmdlet
    {
        private TSR.ScriptObject _templateVariable;
        private int _maxDepth = 0;
        private bool _infiniteDepth = true;
        private int _stackSize = 1024;
        private bool _strictMode = false;
        private bool _fileAccess = false;
        private int _loopLimit = int.MaxValue;
        private TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private bool _infiniteNesting = true;
        private int _nestingLimit = 0;
        private bool _overrideFileSystemProvider = false;
        private ScriptBlock _getPathScript;
        private ScriptBlock _loadFileScript;

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
        /// Restricts the maximum depth of objects passed into the template engine. Do not specify this parameter 
        /// to ensure that the entire <see cref="InputObject"/> is available to the template script.
        /// </summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int Depth 
        { 
            get
            {
                if (_infiniteDepth)
                    return -1;

                return _maxDepth;
            }
            set 
            {
                _maxDepth = value;
                _infiniteDepth = false;
            } 
        }

        /// <summary>
        /// Restricts the number of loops allowed. Defaults to <see cref="int.MaxValue"/>.
        /// </summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int LoopLimit
        {
            get { return _loopLimit; }
            set { _loopLimit = value; }
        }

        /// <summary>
        /// Restricts the stack size. The stack size affects the number of recursive function calls 
        /// allowed. Defaults to 1024.
        /// </summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int StackLimit
        {
            get { return _stackSize; }
            set { _stackSize = value; }
        }

        /// <summary>
        /// Restricts the number of nested expressions allowed, such as if/else and conditions. Do not 
        /// specify this parameter to disable this restriction.
        /// </summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int NestingLimit
        {
            get
            {
                if (_infiniteNesting)
                    return -1;

                return _nestingLimit;
            }
            set
            {
                _nestingLimit = value;
                _infiniteNesting = false;
            }
        }

        /// <summary>
        /// The total number of seconds allowed for the template engine to parse and render. A 
        /// <see cref="TimeoutException"/> is triggered if the operation cannot complete within 
        /// the duration specified. Defaults to 30 seconds.
        /// </summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int TimeoutSeconds
        {
            get
            {
                return (int)_timeout.TotalSeconds;
            }
            set
            {
                _timeout = TimeSpan.FromSeconds(value);
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
        [Parameter(Mandatory = true, ParameterSetName = "FileAccessSet")]
        public SwitchParameter FileAccess
        {
            get { return _fileAccess; }
            set { _fileAccess = value; }
        }

        /// <summary>
        /// Customize the behavior with functions relating to file system access.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "OverrideFileSystemSet")]
        public SwitchParameter OverrideFileSystem
        {
            get { return _overrideFileSystemProvider; }
            set { _overrideFileSystemProvider = value; }
        }

        /// <summary>
        /// A custom function that handles file system path related queries.
        /// </summary>
        /// <remarks>
        /// This function must handle several type of path related queries.
        /// 
        /// If no argument is supplied, the full path of the current directory must be returned.
        /// 
        /// If one to two arguments were supplied, the first argument must be interpreted as the template name, 
        /// which may be an absolute or relative path. The second argument, if present, is the type of path being 
        /// queried, which may be any of the following values: "Container", "Leaf" or "Any". If the path exists, 
        /// the return value should be the absolute path of the template. If the path does not exist, nothing should 
        /// be returned.
        /// 
        /// If three arguments were supplied, the first argument is a directory path, which may be an absolute or 
        /// relative. The second argument is a wildcard expression. The third argument is a boolean, which 
        /// indicates whether this is a recursive search. The return value should be an enumerable collection of string 
        /// values, each being an absolute path that matches the wildcard in the directory specified.
        /// </remarks>
        [Parameter(Mandatory = true, ParameterSetName = "OverrideFileSystemSet")]
        public ScriptBlock GetPathScriptBlock
        {
            get { return _getPathScript; }
            set { _getPathScript = value; }
        }

        /// <summary>
        /// A custom function that returns the content of a template.
        /// </summary>
        /// <remarks>
        /// This function must return the content of a template. The first argument is the full path of the template.
        /// </remarks>
        [Parameter(Mandatory = true, ParameterSetName = "OverrideFileSystemSet")]
        public ScriptBlock LoadTemplateScriptBlock
        {
            get { return _loadFileScript; }
            set { _loadFileScript = value; }
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
                TSP.ParserOptions parserOptions = new TSP.ParserOptions();

                if (!_infiniteNesting && this.NestingLimit > 0)
                    parserOptions.ExpressionDepthLimit = this.NestingLimit;

                templateRuntime = TS.Template.Parse(this.Template, null, parserOptions);

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
            ITemplateLoader templateLoader = null;
            if (_fileAccess)
            {
                string getFSLocationScript = "Get-Location -PSProvider FileSystem | Select-Object -ExpandProperty Path";
                string currentDirectory = ScriptBlock.Create(getFSLocationScript).Invoke()[0].ToString();
                templateLoader = new LocalFileTemplateLoader(currentDirectory);
            }
            else if (_overrideFileSystemProvider)
            {
                templateLoader = new PSCustomFileTemplateLoader(_getPathScript, _loadFileScript);
            }

            TS.TemplateContext templateContext = new TS.TemplateContext()
            {
                // loads file from disk using the 'include' keyword
                TemplateLoader = templateLoader
            };

            try
            {
                if (_strictMode)
                {
                    templateContext.StrictVariables = true;
                    templateContext.EnableRelaxedMemberAccess = false;
                }
                else
                {
                    templateContext.StrictVariables = false;
                    templateContext.EnableRelaxedMemberAccess = true;
                }

                // limit loops
                templateContext.LoopLimit = this.LoopLimit;

                // limit recursive calls
                templateContext.RecursiveLimit = this.StackLimit;

                // set timeout
                templateContext.RegexTimeOut = _timeout;

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

                Task<string> task = Task<string>.Run(() => { return templateRuntime.Render(templateContext); });

                bool completed = task.Wait(_timeout);

                if (!completed)
                    throw new TimeoutException();
                else
                    base.WriteObject(task.Result);

                if (task.IsFaulted == true && task.Exception != null)
                    throw task.Exception;
            }
            catch (AggregateException aex)
            {
                foreach (Exception subex in aex.InnerExceptions)
                {
                    ErrorRecord subErrorRecord = new ErrorRecord(subex, "TemplateRenderFailure", ErrorCategory.ParserError, null);
                    base.WriteError(subErrorRecord);
                }
            }
            catch (Exception ex)
            {
                ErrorRecord renderErrorRecord = new ErrorRecord(ex, "TemplateRenderFailure", ErrorCategory.ParserError, null);
                base.ThrowTerminatingError(renderErrorRecord);                
            }
        }

        private TSR.ScriptArray ConvertArrayObject(SC.IEnumerable array, int recurseDepth = 0)
        {
            if ((_infiniteDepth == false) && (recurseDepth >= this.Depth))
            {
                base.WriteWarning(string.Format(RS.MaxDepthForEnumerableReached, this.Depth));
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
                    if ((_infiniteDepth == true) || (recurseDepth < this.Depth))
                        convertedPropertyArrayValue = ConvertArrayObject((SC.IEnumerable)propertyValue, recurseDepth + 1);
                    else
                        base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "IEnumerable", this.Depth));

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
                            ((_infiniteDepth == true) || (recurseDepth < this.Depth)))
                        {
                            convertedPropertyValue = ConvertHashtableToScriptObject((SC.Hashtable)propertyValue, recurseDepth + 1);
                        }
                        else
                        {
                            // only warn if it's because max depth is reached
                            if (_convertHashtableRecurse == true)
                                base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "Hashtable", this.Depth));
                        }
                    }
                    else if (propertyValue is PSObject)
                    {
                        if ((_infiniteDepth == true) || (recurseDepth < this.Depth))
                            convertedPropertyValue = ConvertPSObjectToScriptObject((PSObject)propertyValue, recurseDepth + 1);
                        else
                            base.WriteWarning(string.Format(RS.MaxDepthReached, propertyName, "PSObject", this.Depth));
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
                    if ((_infiniteDepth == true) || (recurseDepth < this.Depth))
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
                            ((_infiniteDepth == true) || (recurseDepth < this.Depth)))
                        {
                            convertedPropertyValue = ConvertHashtableToScriptObject((SC.Hashtable)propertyValue, recurseDepth + 1);
                        }

                        //base.WriteObject(string.Format("{0} is hashtable", propertyName));
                    }
                    else if (propertyValue is PSObject)
                    {
                        if ((_infiniteDepth == true) || (recurseDepth < this.Depth))
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
