using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaz.Language
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;

        public Lexer(string input)
        {
            _input = input.Replace("\r\n", "\n"); // Normaliza saltos de línea
            _position = 0;
            _line = 1;
            _column = 1;
        }

        private char CurrentChar => _position < _input.Length ? _input[_position] : '\0';

        private void Advance(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (CurrentChar == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
                _position++;
            }
        }

        private char Peek(int offset = 1)
        {
            int pos = _position + offset;
            return pos < _input.Length ? _input[pos] : '\0';
        }

        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(CurrentChar) && CurrentChar != '\n' && CurrentChar != '\0')
                Advance();
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                SkipWhitespace();

                int startLine = _line;
                int startColumn = _column;

                if (CurrentChar == '\0')
                    break;

                // Saltos de línea
                if (CurrentChar == '\n')
                {
                    tokens.Add(new Token(TokenType.NEWLINE, "\\n", startLine, startColumn));
                    Advance();
                    continue;
                }

                // Delimitadores y símbolos
                if (CurrentChar == '(')
                {
                    tokens.Add(new Token(TokenType.LEFT_PAREN, "(", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == ')')
                {
                    tokens.Add(new Token(TokenType.RIGHT_PAREN, ")", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '[')
                {
                    tokens.Add(new Token(TokenType.LEFT_BRACKET, "[", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == ']')
                {
                    tokens.Add(new Token(TokenType.RIGHT_BRACKET, "]", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == ',')
                {
                    tokens.Add(new Token(TokenType.COMMA, ",", startLine, startColumn));
                    Advance();
                    continue;
                }

                // Operador de asignación izquierda <-
                if (CurrentChar == '<' && Peek() == '-')
                {
                    tokens.Add(new Token(TokenType.ASSIGN_LEFT, "<-", startLine, startColumn));
                    Advance(2);
                    continue;
                }

                // Operadores de dos caracteres
                if (CurrentChar == '=' && Peek() == '=')
                {
                    tokens.Add(new Token(TokenType.EQUALS, "==", startLine, startColumn));
                    Advance(2);
                    continue;
                }
                if (CurrentChar == '!' && Peek() == '=')
                {
                    tokens.Add(new Token(TokenType.NOT_EQUALS, "!=", startLine, startColumn));
                    Advance(2);
                    continue;
                }
                if (CurrentChar == '<' && Peek() == '=')
                {
                    tokens.Add(new Token(TokenType.LESS_EQUALS, "<=", startLine, startColumn));
                    Advance(2);
                    continue;
                }
                if (CurrentChar == '>' && Peek() == '=')
                {
                    tokens.Add(new Token(TokenType.GREATER_EQUALS, ">=", startLine, startColumn));
                    Advance(2);
                    continue;
                }

                // Operadores de un caracter
                if (CurrentChar == '+')
                {
                    tokens.Add(new Token(TokenType.PLUS, "+", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '-')
                {
                    tokens.Add(new Token(TokenType.MINUS, "-", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '*')
                {
                    tokens.Add(new Token(TokenType.MULTIPLY, "*", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '/')
                {
                    tokens.Add(new Token(TokenType.DIVIDE, "/", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '=')
                {
                    tokens.Add(new Token(TokenType.ASSIGN, "=", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '<')
                {
                    tokens.Add(new Token(TokenType.LESS, "<", startLine, startColumn));
                    Advance();
                    continue;
                }
                if (CurrentChar == '>')
                {
                    tokens.Add(new Token(TokenType.GREATER, ">", startLine, startColumn));
                    Advance();
                    continue;
                }

                // Literales numéricos
                if (char.IsDigit(CurrentChar) || (CurrentChar == '-' && char.IsDigit(Peek())))
                {
                    int numberStart = _position;
                    int numberLine = _line;
                    int numberColumn = _column;
                    if (CurrentChar == '-')
                        Advance();
                    while (char.IsDigit(CurrentChar))
                        Advance();
                    string numberStr = _input.Substring(numberStart, _position - numberStart);
                    tokens.Add(new Token(TokenType.INTEGER_LITERAL, numberStr, numberLine, numberColumn));
                    continue;
                }

                // Literales de cadena
                if (CurrentChar == '"')
                {
                    int strLine = _line;
                    int strColumn = _column;
                    Advance(); // Salta la comilla inicial
                    var sb = new StringBuilder();
                    while (CurrentChar != '"' && CurrentChar != '\0' && CurrentChar != '\n')
                    {
                        sb.Append(CurrentChar);
                        Advance();
                    }
                    if (CurrentChar == '"')
                        Advance(); // Salta la comilla final
                    else
                        throw new Exception($"Cadena sin cerrar en línea {strLine}, columna {strColumn}");
                    tokens.Add(new Token(TokenType.STRING_LITERAL, sb.ToString(), strLine, strColumn));
                    continue;
                }

                // Palabras clave e identificadores
                if (char.IsLetter(CurrentChar))
                {
                    int idStart = _position;
                    int idLine = _line;
                    int idColumn = _column;
                    while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '-')
                        Advance();
                    string word = _input.Substring(idStart, _position - idStart);

                    TokenType type = TokenType.IDENTIFIER;
                    switch (word.ToUpper())
                    {
                        case "SPAWN": type = TokenType.SPAWN; break;
                        case "COLOR": type = TokenType.COLOR; break;
                        case "DRAWLINE": type = TokenType.DRAWLINE; break;
                        case "MOVE": type = TokenType.MOVE; break;
                        case "TURN": type = TokenType.TURN; break;
                        case "FORWARD": type = TokenType.FORWARD; break;
                        case "BACKWARD": type = TokenType.BACKWARD; break;
                        case "LOOP": type = TokenType.LOOP; break;
                        case "ENDLOOP": type = TokenType.ENDLOOP; break;
                        case "IF": type = TokenType.IF; break;
                        case "ELSE": type = TokenType.ELSE; break;
                        case "ENDIF": type = TokenType.ENDIF; break;
                        case "FUNC": type = TokenType.FUNC; break;
                        case "ENDFUNC": type = TokenType.ENDFUNC; break;
                        case "CALL": type = TokenType.CALL; break;
                        case "RETURN": type = TokenType.RETURN; break;
                        case "PRINT": type = TokenType.PRINT; break;
                        case "SET": type = TokenType.SET; break;
                        case "RAND": type = TokenType.RAND; break;
                        case "GOTO": type = TokenType.GOTO; break;
                        case "LABEL": type = TokenType.LABEL; break;
                        default: type = TokenType.IDENTIFIER; break;
                    }
                    tokens.Add(new Token(type, word, idLine, idColumn));
                    continue;
                }

                // Caracter desconocido
                tokens.Add(new Token(TokenType.UNKNOWN, CurrentChar.ToString(), startLine, startColumn));
                Advance();
            }

            tokens.Add(new Token(TokenType.EOF, "EOF", _line, _column));
            return tokens;
        }
    }
}