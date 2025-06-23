namespace Interfaz.Language
{

    public enum TokenType
    {
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

        PLUS,         
        MINUS,        
        MULTIPLY,     
        DIVIDE,       
        ASSIGN,       
        EQUALS,       
        NOT_EQUALS,   
        LESS,         
        GREATER,      
        LESS_EQUALS,  
        GREATER_EQUALS,
        MODULO, 
        AND,          
        OR,           
        NOT,          
        POWER,        

        LEFT_PAREN,     
        RIGHT_PAREN,    
        LEFT_BRACKET,   
        RIGHT_BRACKET,  
        COMMA,          
        ASSIGN_LEFT, 

        INTEGER_LITERAL, 
        STRING_LITERAL,  

        IDENTIFIER,     
        NEWLINE,        
        EOF,            
        UNKNOWN,

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