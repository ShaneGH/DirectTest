using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Compile;
using DirectTests.Mocks;
using Microsoft.CSharp;

namespace DirectTests.Mocks
{
    internal class Mock
    {
        public readonly Type MockType;
        public readonly ReadOnlyDictionary<string, object> Builder;

        private static readonly object UnAssigned = new object();
        private Object _Object = UnAssigned;
        public Object Object
        {
            get
            {
                return _Object != UnAssigned ? _Object : (_Object = BuildObject());
            }
            set
            {
                _Object = value;
            }
        }

        public Mock(object value)
        {
            _Object = value;
        }

        public Mock(Type mockType, MockBuilder builder)
        {
            if (!mockType.IsInterface)
                throw new NotImplementedException();

            MockType = mockType;
            Builder = builder.Values;
        }

        private static readonly Dictionary<Type, Func<ObjectBase, object>> Constructors = new Dictionary<Type, Func<ObjectBase, object>>();
        void Compile()
        {
            lock (Constructors)
            {
                if (!Constructors.ContainsKey(MockType))
                {
                    var compiled = Compiler.Compile(MockType);
                    var param = Expression.Parameter(typeof(ObjectBase));
                    Constructors.Add(MockType, 
                        Expression.Lambda<Func<ObjectBase, object>>(
                            Expression.New(compiled.GetConstructor(new[] { typeof(ObjectBase) }), param), param).Compile());
                }
            }
        }

        object BuildObject()
        {
            Compile();

            return Constructors[MockType](new ObjectBase(Builder));
        }

        //            string code = @"
        //    using System;
        //
        //    namespace First
        //    {
        //        public class Program
        //        {
        //            public static void Main()
        //            {
        //            " +
        //                "Console.WriteLine(\"Hello, world!\");"
        //                + @"
        //            }
        //        }
        //    }
        //";

        //            var parameters = new CompilerParameters();
        //            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
        //            parameters.GenerateInMemory = true;
        //            parameters.GenerateExecutable = true;

        //            CSharpCodeProvider provider = new CSharpCodeProvider();
        //            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

        //            if (results.Errors.HasErrors)
        //            {
        //                StringBuilder sb = new StringBuilder();

        //                foreach (CompilerError error in results.Errors)
        //                {
        //                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
        //                }

        //                throw new InvalidOperationException(sb.ToString());
        //            }

        //            Assembly assembly = results.CompiledAssembly;
        //            Type program = assembly.GetType("First.Program");
        //            MethodInfo main = program.GetMethod("Main");

        //            main.Invoke(null, null);
    }
}