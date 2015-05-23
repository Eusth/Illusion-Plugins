using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReplaceString
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length >= 1)
                {
                    var translations = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "translations.txt"));
                    var input = new FileInfo(args[0]);
                    string backup = input.FullName + ".BeforeTranslation";

                    if (!input.Exists) Fail("File does not exist.");
                    if (!translations.Exists) Fail("No translations found.");

                    if (!File.Exists(backup))
                    {
                        input.CopyTo(backup);
                    }

                    var directory = input.DirectoryName;
                  
                    var resolver = new DefaultAssemblyResolver();
                    resolver.AddSearchDirectory(directory);

                    var parameters = new ReaderParameters
                    {
                        //   SymbolReaderProvider = GetSymbolReaderProvider(),
                        AssemblyResolver = resolver,
                    };

                    var module = ModuleDefinition.ReadModule(input.FullName, parameters);

                    foreach (var line in File.ReadAllLines(translations.FullName).Select(l => l.Trim()).Where(l => l.Length > 0 && l.IndexOf(":") > 0))
                    {
                        var colon = line.IndexOf(":");
                        var original = line.Substring(0, colon);
                        var translation = line.Substring(colon + 1).Trim();
                        if (translations.Length > 0)
                        {
                            Console.WriteLine("{0} -> {1}", original, translation);
                            ReplaceString(original, translation, module.Assembly);
                        }
                    }
                    module.Write(input.FullName);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private static void Fail(string reason)
        {
            Console.Error.WriteLine(reason);

            Environment.Exit(0);
        }


        public static void ReplaceString(string old, string replacement, AssemblyDefinition asm)
        {
            foreach (ModuleDefinition mod in asm.Modules)
            {
                foreach (TypeDefinition td in mod.Types)
                {
                    IterateType(td, old, replacement);
                }
            }
        }
        public static void IterateType(TypeDefinition td, string old, string replacement)
        {
            foreach (TypeDefinition ntd in td.NestedTypes)
            {
                IterateType(ntd, old, replacement);
            }

            foreach (MethodDefinition md in td.Methods)
            {
                if (md.HasBody)
                {
                    for (int i = 0; i < md.Body.Instructions.Count - 1; i++)
                    {
                        Instruction inst = md.Body.Instructions[i];
                        if (inst.OpCode == OpCodes.Ldstr)
                        {
                            if (inst.Operand.ToString().Equals(old))
                            {
                                inst.Operand = replacement;
                            }
                        }
                    }
                }
            }
        }

    }
}
