namespace Interfaz.Language.AST
{
    public abstract class AstNode
    {
        // Propiedades para rastrear la ubicación del nodo en el código fuente
        public int Line { get; }
        public int Column { get; }

        protected AstNode(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }
}