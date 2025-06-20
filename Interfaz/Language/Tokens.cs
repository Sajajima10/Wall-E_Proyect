// En un archivo nuevo: Tokens.cs (o dentro de una carpeta Language)
namespace Interfaz.Language
{

    public enum TokenType
    {
        // Palabras Clave (Instrucciones y Control de Flujo)
        SPAWN,
        COLOR,
        DRAWLINE,
        DRAWCIRCLE,
        DRAWRECTANGLE,
        MOVE,
        TURN,
        FORWARD,
        BACKWARD,
        LOOP,
        TO,
        ENDLOOP,
        IF,
        ELSE,
        ENDIF,
        FUNC,
        ENDFUNC,
        CALL,
        RETURN,
        PRINT,
        SET,
        RAND,
        GOTO,
        LABEL,
        SIZE,
        FILL,

        // Operadores
        PLUS,         // +
        MINUS,        // -
        MULTIPLY,     // *
        DIVIDE,       // /
        ASSIGN,       // =
        EQUALS,       // ==
        NOT_EQUALS,   // !=
        LESS,         // <
        GREATER,      // >
        LESS_EQUALS,  // <=
        GREATER_EQUALS,// >=
        MODULO, // %
        AND,          // &&
        OR,           // ||
        NOT,          // !
        POWER,        // **

        // Delimitadores / Símbolos
        LEFT_PAREN,     // (
        RIGHT_PAREN,    // )
        LEFT_BRACKET,   // [
        RIGHT_BRACKET,  // ]
        COMMA,          // ,
        ASSIGN_LEFT, // <-

        // Literales
        INTEGER_LITERAL, // Ej: 123, 0, -45
        STRING_LITERAL,  // Ej: "Blue", "Red"

        // Otros
        IDENTIFIER,     // Nombres de variables, funciones, etiquetas
        NEWLINE,        // Salto de línea (importante como separador de instrucciones)
        EOF,            // End Of File (Fin de la entrada)
        UNKNOWN,

        // Sistema
        GETACTUALX,
        GETACTUALY,
        GETCANVASSIZE,
        GETCOLORCOUNT,
        ISBRUSHCOLOR,
        ISBRUSHSIZE,
        ISCANVASCOLOR,
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"[{Type}] '{Value}' (Line: {Line}, Col: {Column})";
        }
    }
}