namespace Interfaz.Language.AST
{
    public abstract class Expression : AstNode
    {
        protected Expression(int line, int column) : base(line, column) { }
    }

    public class IntegerLiteral : Expression
    {
        public int Value { get; }

        public IntegerLiteral(int value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }

    public class StringLiteral : Expression
    {
        public string Value { get; }

        public StringLiteral(string value, int line, int column) : base(line, column)
        {
            Value = value;
        }
    }

    public class IdentifierExpression : Expression
    {
        public string Name { get; }

        public IdentifierExpression(string name, int line, int column) : base(line, column)
        {
            Name = name;
        }
    }

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
    public class UnaryExpression : Expression
    {
        public TokenType Operator { get; }
        public Expression Operand { get; }

        public UnaryExpression(TokenType op, Expression operand, int line, int column)
        : base(line, column)
        {
            Operator = op;
            Operand = operand;
        }
    }
}