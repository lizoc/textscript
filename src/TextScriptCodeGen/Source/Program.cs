// -----------------------------------------------------------------------
// <copyright file="Program.cs" repo="TextScript">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextScriptCodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
        	if (args.Length != 2)
        	{
        		Console.WriteLine("ERROR!");
        		Console.WriteLine("TextScriptCodeGen <path_to_lizoc_textscript.dll> <output_filename.cs>");
        		return;
        	}

        	string assemblyPath = args[0];
        	string outputCodePath = args[1];

			Console.WriteLine("TextScriptCodeGen:");
			Console.WriteLine("[i] " + assemblyPath);
			Console.WriteLine("[o] " + outputCodePath);
			Console.WriteLine("");

			Console.WriteLine("Generating...");
            var codegen = new CodeGenerator(assemblyPath, outputCodePath);
			codegen.GenerateCode(assemblyPath, outputCodePath);
			Console.WriteLine("Done.");
        }
    }
}
