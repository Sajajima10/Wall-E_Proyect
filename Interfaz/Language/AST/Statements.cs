using System.Collections.Generic;
using Interfaz.Language.AST;

namespace Interfaz.Language.AST
{
    // Clase base para todas las sentencias
    public abstract class Statement : AstNode
    {
        protected Statement(int line, int column) : base(line, column) { }
    }

    // Representa el programa completo (una lista de sentencias)
    public class ProgramNode : AstNode
    {
        public List<Statement> Statements { get; }

        public ProgramNode(List<Statement> statements, int line, int column) : base(line, column)
        {
            Statements = statements;
        }
    }

    // Representa una llamada a función (ej: Spawn(10, 20), Move(x, y))
    public class FunctionCall : Statement
    {
        public string FunctionName { get; }
        public List<Expression> Arguments { get; }

        public FunctionCall(string functionName, List<Expression> arguments, int line, int column)
            : base(line, column)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
    }

    // Representa una asignación (ej: Set(x, 10))
    public class Assignment : Statement
    {
        public string VariableName { get; }
        public Expression Value { get; }

        public Assignment(string variableName, Expression value, int line, int column)
            : base(line, column)
        {
            VariableName = variableName;
            Value = value;
        }
    }

    // Representa la instrucción Print (ej: Print("Hello World"))
    public class PrintStatement : Statement
    {
        public Expression Expression { get; }

        public PrintStatement(Expression expression, int line, int column)
            : base(line, column)
        {
            Expression = expression;
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

    public class IfStatement : Statement
    {
        public Expression Condition { get; }
        public List<Statement> TrueBlock { get; }
        public List<Statement> FalseBlock { get; }

        public IfStatement(Expression condition, List<Statement> trueBlock, List<Statement> falseBlock, int line, int column)
        : base(line, column)
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

        public LoopStatement(string iterator, Expression from, Expression to, List<Statement> body, int line, int column)
        : base(line, column)
        {
            Iterator = iterator;
            From = from;
            To = to;
            Body = body;
        }
    }
}