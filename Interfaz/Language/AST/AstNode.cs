namespace Interfaz.Language.AST
{
    public abstract class AstNode
    {
        public int Line { get; }
        public int Column { get; }

        protected AstNode(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }
}