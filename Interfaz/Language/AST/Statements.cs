namespace Interfaz.Language.AST
{
    public abstract class Statement
    {
        public int Line { get; }
        public int Column { get; }
        protected Statement(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }

    public class ProgramNode : Statement
    {
        public List<Statement> Statements { get; }
        public ProgramNode(List<Statement> statements, int line, int column) : base(line, column)
        {
            Statements = statements;
        }
    }

    public class Assignment : Statement
    {
        public string VariableName { get; }
        public Expression Value { get; }
        public Assignment(string variableName, Expression value, int line, int column) : base(line, column)
        {
            VariableName = variableName;
            Value = value;
        }
    }

    public class PrintStatement : Statement
    {
        public Expression Expression { get; }
        public PrintStatement(Expression expression, int line, int column) : base(line, column)
        {
            Expression = expression;
        }
    }

    public class FunctionCall : Expression
    {
        public string FunctionName { get; }
        public List<Expression> Arguments { get; }
        public FunctionCall(string functionName, List<Expression> arguments, int line, int column) : base(line, column)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; }
        public List<Statement> TrueBlock { get; }
        public List<Statement> FalseBlock { get; }
        public IfStatement(Expression condition, List<Statement> trueBlock, List<Statement> falseBlock, int line, int column) : base(line, column)
        {
            Condition = condition;
            TrueBlock = trueBlock;
            FalseBlock = falseBlock;
        }
    }

    public class LoopStatement : Statement
    {
        public string Iterator { get; }
        public Expression From { get; }
        public Expression To { get; }
        public List<Statement> Body { get; }
        public LoopStatement(string iterator, Expression from, Expression to, List<Statement> body, int line, int column) : base(line, column)
        {
            Iterator = iterator;
            From = from;
            To = to;
            Body = body;
        }
    }

    public class LabelStatement : Statement
    {
        public string Label { get; }
        public LabelStatement(string label, int line, int column) : base(line, column)
        {
            Label = label;
        }
    }

    public class GoToStatement : Statement
    {
        public string Label { get; }
        public Expression Condition { get; }
        public GoToStatement(string label, Expression condition, int line, int column) : base(line, column)
        {
            Label = label;
            Condition = condition;
        }
    }

    // --- FUNCIONES DE USUARIO ---
    public class FunctionDefinition : Statement
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public List<Statement> Body { get; }

        public FunctionDefinition(string name, List<string> parameters, List<Statement> body, int line, int column)
            : base(line, column)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression Value { get; }
        public ReturnStatement(Expression value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }
    public class FunctionCallStatement : Statement
{
    public FunctionCall Call { get; }

    public FunctionCallStatement(FunctionCall call, int line, int column)
        : base(line, column)
    {
        Call = call;
    }
}
}