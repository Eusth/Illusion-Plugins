using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using System.IO;
using System.Linq;
using Mono.Cecil.Cil;
using Light.Tests.Helpers;
using System.Reflection;

namespace hmpatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { @"D:\Novels\illusion\PlayClub\PlayClub_Data\Managed\Assembly-CSharp.dll.Original" };
            try
            {
                if (args.Length >= 1)
                {
                    var input = new FileInfo(args[0]);
                    string backup = input.FullName + ".Original";

                    if (!input.Exists) Fail("File does not exist.");
                    if (!File.Exists(backup))
                    {
                    //    input.CopyTo(backup);
                    }

                    var directory = input.DirectoryName;
                    var injectorPath = Path.Combine(directory, "IllusionInjector.dll");

                    if (!File.Exists(injectorPath)) Fail("You're missing IllusionInjector.dll. Please make sure to extract all files correctly.");

                    var resolver = new DefaultAssemblyResolver();
                    resolver.AddSearchDirectory(directory);

                    var parameters = new ReaderParameters
                    {
                        //   SymbolReaderProvider = GetSymbolReaderProvider(),
                        AssemblyResolver = resolver,
                    };

                    var module = ModuleDefinition.ReadModule(input.FullName, parameters);

                    if (!IsPatched(module))
                    {
                        // First, let's add the reference
                        var nameReference = new AssemblyNameReference("IllusionInjector", new Version(1, 0, 0, 0));
                        module.AssemblyReferences.Add(nameReference);
                        
                        var targetType = module.GetType("StudioMain") ?? module.GetType("BaseScene") ?? module.GetType("Scene");

                        if (targetType == null) Fail("Couldn't find entry class. Aborting.");

                        var awakeMethod = targetType.Methods.FirstOrDefault(m => m.Name == "Awake");
                        if (awakeMethod == null) Fail("Couldn't find awake method. Aborting.");

                        var injector = ModuleDefinition.ReadModule(injectorPath);
                        var methodReference = module.Import(injector.GetType("IllusionInjector.Injector").Methods.First(m => m.Name == "Inject"));
                        //var methodReference = module.GetMemberReferences().FirstOrDefault(r => r.FullName == "IllusionInjector.Injector");

                        awakeMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, methodReference));
                        Virtualize(module);

                        //module.Write(input.FullName);


                        Console.WriteLine("Successfully patched the file!");


                    }
                    else
                    {
                        Console.WriteLine("File is already patched");
                    }
                }
                else
                {
                    Console.WriteLine("Please provide the file to patch.");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("An unexepected error happened.");
                Console.Error.WriteLine(e);
            }
        }

        private static bool IsPatched(ModuleDefinition module)
        {
            foreach (var @ref in module.AssemblyReferences)
            {
                if (@ref.Name == "IllusionInjector") return true;
            }
            return false;
        }

        private static void Fail(string reason) {
            Console.Error.WriteLine(reason);

            Environment.Exit(0);
        }

        private static int ILOL = 0;
        /// <summary>
        /// The forbidden deed of the gods -- make ALL methods virtual and public
        /// </summary>
        /// <param name="module"></param>
        private static void Virtualize(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                var tempFile = @"D:\Novels\illusion\PlayClub\PlayClub_Data\Managed\Assembly-CSharp-Lol.dll";
                VirtualizeType(type);

                if (ILOL++ > 400)
                {

                    Console.WriteLine("NEXT...");
                    module.Write(tempFile);
                    PEVerifier.Verify(tempFile);
                    Console.WriteLine("DONE...");
                }

            }
        }

        private static void VirtualizeType(TypeDefinition type)
        {
            if (type.IsSealed) return;
            if (type.IsInterface) return;
            if (type.IsAbstract) return;
            if (type.IsEnum) return;
            if (type.Name == "SceneControl" || type.Name == "ConfigUI") return;
            //if (type.FullName.Contains("RootMotion")) return;
            //if (type.Methods.Any(m => m.Body != null && m.Body.Variables.Any(v => v.VariableType.FullName.Contains("<")))) return;
            //if (!type.FullName.Contains("H_VoiceControl")) return;
            //if (!type.FullName.Contains("Human")) return;
            //if (type.Namespace.Length > 1) return;


            // Take care of sub types
            //foreach (var subType in type.NestedTypes)
            //{
            //    VirtualizeType(subType);
            //}

            foreach (var method in type.Methods)
            {
                Console.WriteLine(method.Name);
                if (method.IsManaged
                    && !TABOO_NAMES.Contains(method.Name)
                    && method.IsIL
                    && !method.IsStatic
                    && !method.IsVirtual
                    && !method.IsAbstract
                    && !method.IsAddOn
                    && !method.IsConstructor
                    && !method.IsSpecialName
                    && !method.IsGenericInstance
                    && !method.HasOverrides)
                {
                    method.IsVirtual = true;
                    method.IsPublic = true;
                    method.IsPrivate = false;
                    method.IsNewSlot = true;
                    method.IsHideBySig = true;
                }
            }

            //foreach (var field in type.Fields)
            //{
            //    field.IsPublic = true;
            //}

            //foreach (var property in type.Properties)
            //{
            //    property.GetMethod.IsVirtual = true;
            //    property.GetMethod.IsPublic = true;
            //    property.SetMethod.IsVirtual = true;
            //    property.SetMethod.IsPublic = true;
            //}

        }
        private static string[] TABOO_NAMES = {
            "Start",
            "Update",
            "Awake"  
        };

    }
}
