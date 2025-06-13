namespace Interfaz.Language.AST
{
    public abstract class Expression : AstNode
    {
        protected Expression(int line, int column) : base(line, column) { }
    }

    // Literales enteros (ej: 10, -5, 0)
    public class IntegerLiteral : Expression
    {
        public int Value { get; }

        public IntegerLiteral(int value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }

    // Literales de cadena (ej: "red", "hello")
    public class StringLiteral : Expression
    {
        public string Value { get; }

        public StringLiteral(string value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }

    // Identificadores (nombres de variables, ej: x, y)
    public class IdentifierExpression : Expression
    {
        public string Name { get; }

        public IdentifierExpression(string name, int line, int column) : base(line, column)
        {
            Name = name;
        }
    }

    // Expresiones binarias (operaciones como +, -, *, /, ==, !=, <, >, <=, >=)
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public TokenType Operator { get; }
        public Expression Right { get; }

        public BinaryExpression(Expression left, TokenType op, Expression right, int line, int column)
            : base(line, column)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
    public class RandExpression : Expression
    {
        public Expression Min { get; }
        public Expression Max { get; }

        public RandExpression(Expression min, Expression max, int line, int column)
            : base(line, column)
        {
            Min = min;
            Max = max;
        }
    }
}