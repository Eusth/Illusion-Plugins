using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Threading;
using System.Runtime.Serialization;

namespace QuietLauncher
{
    static class Program
    {
        private static string[] TABOO_NAMES = {
            //"Start",
            //"Update",
            //"Awake",
            //"OnDestroy"
        };
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var execPath = Application.ExecutablePath;
            var fileName = Path.GetFileNameWithoutExtension(execPath);
            if (fileName.IndexOf("VR") == -1 && fileName.IndexOf("_") == -1) return;

            bool vrMode = fileName.IndexOf("VR") > 0;
            bool directMode = Application.ExecutablePath.EndsWith("_DirectToRift.exe");
            string baseName = execPath.Substring(0, vrMode
                                                    ? execPath.LastIndexOf("VR")
                                                    : execPath.LastIndexOf("_"));

            string executable = baseName + ".exe";
            var file = new FileInfo(executable);
            if (file.Exists)
            {
                var args = Environment.GetCommandLineArgs().ToList();
                bool created = false;

                var dataFolder = Path.GetFileNameWithoutExtension(file.Name) + "_Data";
                var assemblyPath = Path.Combine(Path.Combine(dataFolder, "Managed"), "Assembly-CSharp.dll");
                var directToRiftPath = baseName + "_DirectToRift.exe";

                try
                {
                    if (directMode)
                    {
                        //args[Array.IndexOf(args, "--direct")] = "-force-d3d11";


                        if (!File.Exists(directToRiftPath))
                        {
                            File.WriteAllBytes(directToRiftPath, Resources.DirectToRift);
                            created = true;
                        }

                        file = new FileInfo(directToRiftPath);
                    }


                    if (vrMode) args.Add("--vr");
                    var arguments = string.Join(" ", args.ToArray());

                    try
                    {
                        Patch(assemblyPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Process.Start(file.FullName, arguments);

                }
                finally
                {
                    if (created && directMode)
                    {
                        var thread = new Thread(new ThreadStart(delegate
                        {
                            int attempts = 0;
                            while (File.Exists(directToRiftPath) && attempts++ < 20)
                            {
                                Thread.Sleep(1000);
                                try
                                {
                                    File.Delete(directToRiftPath);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }));
                        thread.Start();
                        thread.Join();
                        // Clean up
                    }
                }
            }
            else
            {
                MessageBox.Show("Could not find: " + file.FullName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        static void Patch(string file)
        {
        //    args = new string[] { @"D:\Novels\illusion\HaremMate\HaremStudio_Data\Managed\Assembly-CSharp.dll" };
          
            var input = new FileInfo(file);
            string backup = input.FullName + ".Original";

            if (!input.Exists) Fail("File does not exist.");

            if (File.Exists(backup))
            {
                int i = 1;
                string backupBase = backup;
                while (File.Exists(backup))
                {
                    backup = backupBase + i++;
                }
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
            bool isPatched = IsPatched(module);
            bool isVirtualized = IsVirtualized(module);

            if (!isPatched || !isVirtualized || !isSingletonized )
            {
                // Make backup
                input.CopyTo(backup);

                if (!isPatched)
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
                }

                if (!isSingletonized)
                {
                    EliminateStaticClasses(module, ModuleDefinition.ReadModule(injectorPath));
                }


                if (!isVirtualized)
                {
                    Virtualize(module);
                }

              
                module.Write(input.FullName);

                Console.WriteLine("Successfully patched the file!");
            }
            else
            {
                Console.WriteLine("File is already patched");
            }
            
        }


        /// <summary>
        /// The forbidden deed of the gods -- make ALL methods virtual and public
        /// </summary>
        /// <param name="module"></param>
        private static void Virtualize(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                VirtualizeType(type);
            }
        }

        private static void VirtualizeType(TypeDefinition type)
        {
            if (type.IsSealed) return;
            if (type.IsInterface) return;
            if (type.IsAbstract) return;

            // These two don't seem to work.
            if (type.Name == "SceneControl" || type.Name == "ConfigUI") return;

            //if (type.FullName.Contains("RootMotion")) return;
            //if (type.Methods.Any(m => m.Body != null && m.Body.Variables.Any(v => v.VariableType.FullName.Contains("<")))) return;
            //if (!type.FullName.Contains("H_VoiceControl")) return;
            //if (!type.FullName.Contains("Human")) return;
            //if (type.Namespace.Length > 1) return;
            

            // Take care of sub types
            foreach (var subType in type.NestedTypes)
            {
                VirtualizeType(subType);
            }

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

            foreach (var field in type.Fields)
            {
                if (field.IsPrivate) field.IsFamily = true;
                //field.IsPublic = true;
            }

            //foreach (var property in type.Properties)
            //{
            //    property.GetMethod.IsVirtual = true;
            //    property.GetMethod.IsPublic = true;
            //    property.SetMethod.IsVirtual = true;
            //    property.SetMethod.IsPublic = true;
            //}

        }

        private static void EliminateStaticClasses(ModuleDefinition module, ModuleDefinition injectorModule)
        {
            var singletonClass = module.Import(injectorModule.GetType("IllusionInjector.Singleton`1"));

            foreach (var staticClass in module.Types.Where(type => type.HasMethods && !type.Name.Contains("Singleton") &&
                type.Methods.All(m => m.IsStatic || m.IsConstructor )))
            {
                if (staticClass.Namespace == "ASA")
                    EliminateStaticClass(staticClass, singletonClass);
            
            }
            //Program.Fail("");
        }

        private static void EliminateStaticClass(TypeDefinition type, TypeReference singletonClass)
        {
            var resolvedSingleto = singletonClass.Resolve(); 
            //var singletonClass = type.Module.GetType("ASA.SingletonClass`1");
            var parentClass = MakeGenericType(singletonClass, type);
            //MessageBox.Show("Singletonize " + type.Name + " with " + singletonClass.FullName);
            var references = type.Module.Types
                                .SelectMany(t => t.Methods)
                                .Where(m => m.Body != null)
                                .SelectMany(m => m.Body.Instructions)
                                .Where(i => (i.Operand is MethodReference) && ((MethodReference)i.Operand).DeclaringType.Resolve() == type)
                                .Select(i => i.Operand as MethodReference)
                                .ToArray();

            type.BaseType = parentClass;
           
            var staticMethods = type.Methods.ToArray();
            var instanceMethods = new List<MethodDefinition>();

            var getInstance =  type.Module.Import( singletonClass.Resolve().Methods.First(m => m.Name == "get_Instance") );
            getInstance.DeclaringType = parentClass;

            var constructor = type.Methods.First(m => m.IsSpecialName);
            
            constructor.Body.Instructions[1] =
                Instruction.Create(OpCodes.Call, 
                    MakeGeneric(type.Module.Import(parentClass.Resolve().Methods.First(m => m.IsConstructor)), type) );

            int counter = 0;


            Dictionary<MethodDefinition, MethodDefinition> methodMap = new Dictionary<MethodDefinition, MethodDefinition>();
            foreach (var staticMethod in staticMethods.Where(
                m => m.IsStatic && !m.IsGetter && !m.IsSetter && !m.IsSpecialName
            ))
            {

                var instanceMethod = new MethodDefinition("_" + staticMethod.Name, staticMethod.Attributes, staticMethod.ReturnType);
                instanceMethod.IsStatic = false;
                instanceMethod.HasThis = true;
                type.Methods.Add(instanceMethod);
                instanceMethod.DeclaringType = type;

                foreach (var param in staticMethod.Parameters)
                {
                    //instanceMethod.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                    instanceMethod.Parameters.Add(param);
                }

                foreach (var genericParam in staticMethod.GenericParameters)
                {
                    instanceMethod.GenericParameters.Add(CloneGenericParam(genericParam, instanceMethod) ) ;
                    
                }

                foreach (var var in staticMethod.Body.Variables)
                {
                    //instanceMethod.Body.Variables.Add(new VariableDefinition(var.Name, var.VariableType));
                    
                    instanceMethod.Body.Variables.Add(var);
                }

              
                //staticMethod.Body.Variables.Clear();

                foreach (var handler in staticMethod.Body.ExceptionHandlers)
                {
                    //instanceMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType));
                    instanceMethod.Body.ExceptionHandlers.Add(handler);
                }

                if (staticMethod.Name == "GetComponent")
                {

                }

                foreach (var originalInstruction in staticMethod.Body.Instructions)
                {
                    var instruction = (originalInstruction);
                    if (instruction.OpCode == OpCodes.Ldarg_0) instruction.OpCode = OpCodes.Ldarg_1;
                    else if (instruction.OpCode == OpCodes.Ldarg_1) instruction.OpCode = OpCodes.Ldarg_2;
                    else if (instruction.OpCode == OpCodes.Ldarg_2) instruction.OpCode = OpCodes.Ldarg_3;
                    else if (instruction.OpCode == OpCodes.Ldarg_3) { instruction.OpCode = OpCodes.Ldarg_S; instruction.Operand = staticMethod.Parameters[3]; }

                    if(staticMethod.CallingConvention == MethodCallingConvention.Generic && 
                        instruction.Operand is GenericInstanceMethod) {
                        var oldReference = instruction.Operand as GenericInstanceMethod;

                        if (oldReference.HasGenericArguments && oldReference.GenericArguments[0] == staticMethod.GenericParameters[0] )
                        {
                            instruction.Operand = MakeGenericMethod( type.Module.Import(oldReference.Resolve()), instanceMethod.GenericParameters[0]); 
                            //oldReference.GenericParameters.Add(instanceMethod.GenericParameters[0]);
                            //oldReference.GenericArguments[0] = instanceMethod.GenericParameters[0];
                            //oldReference.ReturnType = instanceMethod.GenericParameters[0];
                        }
                    }
                    instanceMethod.Body.Instructions.Add(instruction);
                }
                instanceMethod.Body.InitLocals = staticMethod.Body.InitLocals;

                var oldBody = staticMethod.Body;
                staticMethod.Body = new MethodBody(staticMethod);
                staticMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getInstance));
                MethodReference instanceReference = staticMethod.HasGenericParameters ? MakeGenericMethod(instanceMethod, staticMethod.GenericParameters.ToArray()) : instanceMethod;

                for (int i = 0; i < staticMethod.Parameters.Count; i++)
                {
                    Instruction instruction = null;
                    if (i == 0) instruction = Instruction.Create(OpCodes.Ldarg_0);
                    if (i == 1) instruction = Instruction.Create(OpCodes.Ldarg_1);
                    if (i == 2) instruction = Instruction.Create(OpCodes.Ldarg_2);
                    if (i == 3) instruction = Instruction.Create(OpCodes.Ldarg_3);
                    if (i > 3) instruction = Instruction.Create(OpCodes.Ldarg_S, staticMethod.Parameters[i]);

                    staticMethod.Body.Instructions.Add(instruction);
                }

                staticMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, instanceReference));
                //staticMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));

                staticMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            }

            foreach (var reference in references)
            {
                //var oldMethod = reference.Operand as MethodReference;
                //var newMethod = type.Methods.FirstOrDefault(method => method.Name == oldMethod.Name
                //                          && method.Parameters.Select(p => p.ParameterType.Name).SequenceEqual(oldMethod.Parameters.Select(p => p.ParameterType.Name))
                //    //&& method.GenericParameters.Count == oldMethod.GenericParameters.Count
                //);
                //if (newMethod == null) throw new Exception("Did not find new method: " + oldMethod.FullName);
                try
                {
                    reference.DeclaringType = type;
                }
                catch (Exception e)
                {
                    counter++;
                }
            }

            //MessageBox.Show(counter + " / " + references.Length + " failed");
        }

        private static GenericParameter CloneGenericParam(GenericParameter genericParam, MethodDefinition owner)
        {
            var newParam = new GenericParameter(genericParam.Name, owner);

            foreach (var constraint in genericParam.Constraints)
                newParam.Constraints.Add(constraint);

            return newParam;
        }

        private static Instruction CopyInstruction(Instruction instruction)
        {
            var copy = FormatterServices.GetUninitializedObject(typeof(Instruction)) as Instruction;
            copy.OpCode = instruction.OpCode;
            copy.Operand = instruction.Operand;

            return copy;
        }
        private static bool IsPatched(ModuleDefinition module)
        {
            foreach (var @ref in module.AssemblyReferences)
            {
                if (@ref.Name == "IllusionInjector") return true;
            }
            return false;
        }

        private static bool IsVirtualized(ModuleDefinition module)
        {
            var targetType = module.GetType("StudioMain") ?? module.GetType("BaseScene") ?? module.GetType("Scene");
            return targetType.Methods.First(m => m.Name == "Awake").IsVirtual;
        }

        private static void Fail(string reason) {
            throw new Exception(reason);
        }


        public static bool isSingletonized
        {
            get
            {
                return false;
            }
        }

        public static GenericInstanceType MakeGenericType(TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static MethodReference MakeGenericMethod(MethodReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                DeclaringType = MakeGenericType(self.DeclaringType, arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

    }

}
