using System.Text;

namespace CodeerLowCodeBlazorTemplate.Designer.Lib.SeleniumPageObject
{
    internal class SourceGenerator
    {
        internal List<string> UsingNamespaces { get; } = [];
        internal string Namespace { get; set; } = string.Empty;
        internal List<ClassDecl> Classes { get; } = [];

        internal string Generate()
        {
            var gen = new Generator();
            foreach (var ns in UsingNamespaces)
            {
                gen.Push($"using {ns};");
            }

            gen.LineBreak();
            gen.Push($"namespace {Namespace}");
            gen.PushIndent();
            foreach (var cls in Classes)
            {
                cls.Push(gen);
                gen.LineBreak();
            }

            gen.PopIndent();

            return gen.ToString();
        }
    }

    internal class Generator
    {
        private int _indentLevel = 0;
        private StringBuilder _sb = new StringBuilder();

        internal void Clear() => _sb.Clear();

        internal void Push(string line) => _sb.AppendLine(GetIndent() + line);
        internal void LineBreak() => _sb.AppendLine();
        internal void PushIndent() => _sb.AppendLine(new string(' ', _indentLevel++ * 4) + "{");
        internal void PopIndent() => _sb.AppendLine(new string(' ', --_indentLevel * 4) + "}");

        private string GetIndent() => new string(' ', _indentLevel * 4);

        public override string ToString() => _sb.ToString();
    }

    internal class ClassDecl
    {
        internal List<string> Attributes { get; } = [];
        internal string Name { get; set; } = string.Empty;
        internal string BaseClass { get; set; } = string.Empty;
        internal string GenericsConstraints { get; set; } = string.Empty;
        internal string AccessModifier { get; set; } = "public";
        internal List<PropertyDecl> Properties { get; } = [];
        internal List<ConstructorDecl> Constructors { get; } = [];
        internal List<AbstractMethodDecl> Methods { get; } = [];
        internal List<OperatorDecl> Operators { get; } = [];

        internal void Push(Generator gen)
        {
            foreach (var attr in Attributes)
            {
                gen.Push($"[{attr}]");
            }

            gen.Push(string.IsNullOrEmpty(BaseClass)
                ? $"{AccessModifier} class {Name}"
                : $"{AccessModifier} class {Name} : {BaseClass}");

            if (!string.IsNullOrEmpty(GenericsConstraints))
            {
                gen.Push($"    where {GenericsConstraints}");
            }

            gen.PushIndent();
            foreach (var prop in Properties)
            {
                gen.Push(prop.ToString());
            }

            gen.LineBreak();

            foreach (var ctor in Constructors)
            {
                var baseCtor = ctor.BaseParameters.Count > 0
                    ? $" : base({string.Join(", ", ctor.BaseParameters)})"
                    : string.Empty;
                var statementBody = ctor.Statements.Count > 0
                    ? ""
                    : " { }";
                gen.Push(
                    $"{ctor.AccessModifier} {Name}({string.Join(", ", ctor.Parameters)}){baseCtor}{statementBody}");
                if (ctor.Statements.Count > 0)
                {
                    gen.PushIndent();
                    foreach (var stmt in ctor.Statements)
                    {
                        gen.Push(stmt.ToString());
                    }

                    gen.PopIndent();
                }

                gen.LineBreak();
            }

            foreach (var method in Methods)
            {
                foreach (var attr in method.Attributes)
                {
                    gen.Push($"[{attr}]");
                }

                gen.Push(
                    $"{method.AccessModifier} {method.ReturnType} {method.Name}({string.Join(", ", method.Parameters)})");
                method.GenerateBody(gen);
                gen.LineBreak();
            }

            foreach (var ops in Operators)
            {
                gen.Push(ops.ToString());
            }

            gen.PopIndent();
        }
    }

    internal class PropertyDecl
    {
        internal List<string> Attributes { get; } = [];
        internal string Name { get; set; } = string.Empty;
        internal string Type { get; set; } = string.Empty;
        internal string AccessModifier { get; set; } = "public";

        public override string ToString() => $"{AccessModifier} {Type} {Name} {{ get; set; }}";
    }

    internal class ExpressionPropertyDecl : PropertyDecl
    {
        internal string Expression { get; set; } = string.Empty;

        public override string ToString() => $"{AccessModifier} {Type} {Name} => {Expression};";
    }

    internal abstract class AbstractMethodDecl
    {
        internal List<string> Attributes { get; } = [];
        internal string Name { get; set; } = string.Empty;
        internal string ReturnType { get; set; } = "void";
        internal string AccessModifier { get; set; } = "public";
        internal List<ParameterDecl> Parameters { get; } = [];

        internal abstract void GenerateBody(Generator gen);
    }

    internal class MethodDecl : AbstractMethodDecl
    {
        internal List<Statement> Statements { get; } = [];

        internal override void GenerateBody(Generator gen)
        {
            gen.PushIndent();
            foreach (var statement in Statements)
            {
                gen.Push(statement.ToString());
            }

            gen.PopIndent();
        }
    }

    internal class ExpressionMethodDecl : AbstractMethodDecl
    {
        internal string Expression { get; set; } = string.Empty;

        internal override void GenerateBody(Generator gen)
        {
            gen.Push($"    => {Expression}");
        }
    }

    internal class ConstructorDecl
    {
        internal string AccessModifier { get; set; } = "public";
        internal List<ParameterDecl> Parameters { get; } = [];
        internal List<string> BaseParameters { get; } = [];
        internal List<Statement> Statements { get; } = [];
    }

    internal class ParameterDecl
    {
        internal string Type { get; set; } = string.Empty;
        internal string Name { get; set; } = string.Empty;

        public override string ToString() => $"{Type} {Name}";
    }

    internal class OperatorDecl
    {
        public override string ToString() => "";
    }

    internal class ImplicitOperatorDecl : OperatorDecl
    {
        internal string TargetType { get; set; } = string.Empty;
        internal ParameterDecl Parameter { get; set; } = new();
        internal string AccessModifier { get; set; } = "public";
        internal string Expression { get; set; } = string.Empty;

        public override string ToString() =>
            $"{AccessModifier} static implicit operator {TargetType}({Parameter}) => {Expression}";
    }

    internal class Statement
    {
        internal string Expression { get; set; } = string.Empty;

        public override string ToString() => Expression;
    }
}
