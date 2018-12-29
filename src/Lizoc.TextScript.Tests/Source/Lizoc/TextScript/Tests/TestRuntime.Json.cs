﻿// -----------------------------------------------------------------------
// <copyright file="TestRuntime.Json.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
//     All or part thereof may be subject to other licenses documented below this header and 
//     the THIRD-PARTY-LICENSE file in the repository root directory.
// </copyright>
// -----------------------------------------------------------------------

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

#if INCLUDE_NEWTONSOFT_JSON

using System;
using System.Globalization;
using System.Reflection;
using Xunit;
using Lizoc.TextScript.Parsing;
using Lizoc.TextScript.Runtime;
using Lizoc.TextScript.Syntax;
using Newtonsoft.Json;

namespace Lizoc.TextScript.Tests
{
    partial class TestRuntime
    {
        [Test]
        public void TestJson()
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");

            System.Data.DataRow dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Hello";
            dataRow["Column2"] = "World";
            dataTable.Rows.Add(dataRow);

            dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Bonjour";
            dataRow["Column2"] = "le monde";
            dataTable.Rows.Add(dataRow);

            string json = JsonConvert.SerializeObject(dataTable);
            _output.WriteLine("Json: " + json);

            var parsed = JsonConvert.DeserializeObject(json);
            _output.WriteLine("Parsed: " + parsed);

            string myTemplate = @"
[
  { {{ for tbr in tb }}
    ""N"": {{tbr.Column1}},
    ""M"": {{tbr.Column2}}
    {{ end }}
  },
]
{{tb}}
";

            // Parse the template
            var template = Template.Parse(myTemplate);

            // Render
            var context = new TemplateContext { MemberRenamer = member => member.Name };
            var scriptObject = new ScriptObject();
            scriptObject.Import(new { tb = parsed });
            context.PushGlobal(scriptObject);
            var result = template.Render(context);
            context.PopGlobal();

            var expected =
                @"
[
  { 
    ""N"": Hello,
    ""M"": World
    
    ""N"": Bonjour,
    ""M"": le monde
    
  },
]
[[[Hello], [World]], [[Bonjour], [le monde]]]
";

            TextAssert.Equal(expected, result);
        }
    }
}

#endif
