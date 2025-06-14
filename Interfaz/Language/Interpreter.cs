using System;
using System.Collections.Generic;
using Interfaz.Language.AST;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Interfaz.Language
{
    public class Interpreter
    {
        private readonly WriteableBitmap _bitmap;
        private Environment _environment;
        private Random _random;

        private Point _currentPosition;
        private Color _currentColor;
        private double _currentRotation;

        public event Action<string> OnOutputMessage;

        public Interpreter(WriteableBitmap bitmap)
        {
            _bitmap = bitmap;
            _environment = new Environment();
            _random = new Random();
            _currentPosition = new Point(0, 0);
            _currentColor = Colors.Black;
            _currentRotation = 0;
            ClearBitmap();
        }

        private void ClearBitmap()
        {
            _bitmap.Lock();
            unsafe
            {
                int* pBackBuffer = (int*)_bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride / 4;
                for (int y = 0; y < _bitmap.PixelHeight; y++)
                {
                    for (int x = 0; x < _bitmap.PixelWidth; x++)
                    {
                        pBackBuffer[y * stride + x] = unchecked((int)0xFFD3D3D3); // Gris claro
                    }
                }
            }
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        public void Interpret(ProgramNode program)
        {
            _environment = new Environment();
            _currentPosition = new Point(0, 0);
            _currentColor = Colors.Black;
            _currentRotation = 0;
            ClearBitmap();

            // Mapea etiquetas a índices
            var labelMap = new Dictionary<string, int>();
            for (int i = 0; i < program.Statements.Count; i++)
            {
                if (program.Statements[i] is LabelStatement labelStmt)
                    labelMap[labelStmt.Label] = i;
            }

            int pc = 0;
            bool spawnCalled = false;
            while (pc < program.Statements.Count)
            {
                var statement = program.Statements[pc];

                if (!spawnCalled && !(statement is FunctionCall funcCall && funcCall.FunctionName == "Spawn"))
                    throw new Exception($"El código debe comenzar con 'Spawn'.");

                if (statement is FunctionCall sc && sc.FunctionName == "Spawn")
                {
                    if (spawnCalled)
                        throw new Exception($"'Spawn' solo puede llamarse una vez al inicio.");
                    spawnCalled = true;
                }

                if (statement is GoToStatement gotoStmt)
                {
                    var cond = VisitExpression(gotoStmt.Condition);
                    bool jump = false;
                    if (cond is bool b)
                        jump = b;
                    else if (cond is int i)
                        jump = i != 0;
                    else
                        throw new Exception($"La condición de GoTo debe ser booleana o entera (0/1).");

                    if (jump)
                    {
                        if (!labelMap.TryGetValue(gotoStmt.Label, out int target))
                            throw new Exception($"Etiqueta '{gotoStmt.Label}' no encontrada.");
                        pc = target;
                        continue;
                    }
                }
                else if (statement is LabelStatement)
                {
                    // No hacer nada, solo es un marcador
                }
                else
                {
                    VisitStatement(statement);
                }

                pc++;
            }

            if (!spawnCalled)
                throw new Exception("El código no contiene la instrucción 'Spawn(x, y)'.");
        }

        private void VisitStatement(Statement statement)
        {
            switch (statement)
            {
                case FunctionCall funcCall:
                    VisitFunctionCall(funcCall);
                    break;
                case Assignment assignment:
                    VisitAssignment(assignment);
                    break;
                case PrintStatement printStmt:
                    VisitPrintStatement(printStmt);
                    break;
                case IfStatement ifStmt:
                    VisitIfStatement(ifStmt);
                    break;
                default:
                    throw new NotImplementedException($"La sentencia de tipo '{statement.GetType().Name}' aún no está implementada en el intérprete. Línea: {statement.Line}, Columna: {statement.Column}");
            }
        }

        private void VisitIfStatement(IfStatement ifStmt)
        {
            var cond = VisitExpression(ifStmt.Condition);
            bool result = false;
            if (cond is bool b)
                result = b;
            else if (cond is int i)
                result = i != 0;
            else
                throw new Exception("La condición del IF debe ser booleana o entera.");

            var block = result ? ifStmt.TrueBlock : ifStmt.FalseBlock;
            foreach (var stmt in block)
                VisitStatement(stmt);
        }

        private object VisitExpression(Interfaz.Language.AST.Expression expression)
        {
            switch (expression)
            {
                case IntegerLiteral intLiteral:
                    return intLiteral.Value;
                case StringLiteral stringLiteral:
                    return stringLiteral.Value;
                case IdentifierExpression identifier:
                    return _environment.Get(identifier.Name);
                case BinaryExpression binaryExpr:
                    return EvaluateBinaryExpression(binaryExpr);
                case RandExpression randExpr:
                    return EvaluateRandExpression(randExpr);
                default:
                    throw new NotImplementedException($"La expresión de tipo '{expression.GetType().Name}' aún no está implementada en el intérprete. Línea: {expression.Line}, Columna: {expression.Column}");
            }
        }

        private void VisitFunctionCall(FunctionCall funcCall)
        {
            List<object> evaluatedArgs = new List<object>();
            foreach (Interfaz.Language.AST.Expression arg in funcCall.Arguments)
                evaluatedArgs.Add(VisitExpression(arg));

            switch (funcCall.FunctionName)
            {
                case "Spawn":
                    if (evaluatedArgs.Count != 2 || !(evaluatedArgs[0] is int x) || !(evaluatedArgs[1] is int y))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Spawn' espera dos argumentos enteros (x, y).");
                    _currentPosition = new Point(x, y);
                    break;
                case "Color":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is string colorName))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Color' espera un argumento de cadena con el nombre del color.");
                    _currentColor = ParseColor(colorName, funcCall.Line, funcCall.Column);
                    break;
                case "DrawLine":
                    if (evaluatedArgs.Count != 3 || !(evaluatedArgs[0] is int x2) || !(evaluatedArgs[1] is int y2) || !(evaluatedArgs[2] is int thickness))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'DrawLine' espera tres argumentos enteros (x2, y2, thickness).");
                    DrawLineOnBitmap(_currentPosition, new Point(x2, y2), thickness);
                    _currentPosition = new Point(x2, y2);
                    break;
                case "Move":
                    if (evaluatedArgs.Count != 2 || !(evaluatedArgs[0] is int dx) || !(evaluatedArgs[1] is int dy))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Move' espera dos argumentos enteros (dx, dy).");
                    _currentPosition = new Point(_currentPosition.X + dx, _currentPosition.Y + dy);
                    break;
                case "Turn":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int degrees))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Turn' espera un argumento entero (grados).");
                    _currentRotation = (_currentRotation + degrees) % 360;
                    break;
                case "Forward":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int distanceF))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Forward' espera un argumento entero (distancia).");
                    MoveForward(distanceF);
                    break;
                case "Backward":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int distanceB))
                        throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Backward' espera un argumento entero (distancia).");
                    MoveForward(-distanceB);
                    break;
                case "Call":
                    throw new NotImplementedException($"La función 'Call' aún no está implementada. Línea: {funcCall.Line}, Columna: {funcCall.Column}");
                case "Rand":
                    throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: 'Rand' es una expresión y no puede ser una instrucción independiente. Úsala con 'Set'.");
                default:
                    throw new Exception($"Error semántico en Línea {funcCall.Line}, Columna {funcCall.Column}: Función '{funcCall.FunctionName}' no reconocida.");
            }
        }

        private void VisitAssignment(Assignment assignment)
        {
            object value = VisitExpression(assignment.Value);
            _environment.Set(assignment.VariableName, value);
        }

        private void VisitPrintStatement(PrintStatement printStmt)
        {
            object valueToPrint = VisitExpression(printStmt.Expression);
            OnOutputMessage?.Invoke(valueToPrint?.ToString() ?? string.Empty);
        }

        private object EvaluateBinaryExpression(BinaryExpression expr)
        {
            object left = VisitExpression(expr.Left);
            object right = VisitExpression(expr.Right);

            if (left is int lInt && right is int rInt)
            {
                switch (expr.Operator)
                {
                    case TokenType.PLUS: return lInt + rInt;
                    case TokenType.MINUS: return lInt - rInt;
                    case TokenType.MULTIPLY: return lInt * rInt;
                    case TokenType.DIVIDE:
                        if (rInt == 0)
                            throw new Exception($"Error en tiempo de ejecución en Línea {expr.Line}, Columna {expr.Column}: División por cero.");
                        return lInt / rInt;
                    case TokenType.EQUALS: return lInt == rInt;
                    case TokenType.NOT_EQUALS: return lInt != rInt;
                    case TokenType.LESS: return lInt < rInt;
                    case TokenType.LESS_EQUALS: return lInt <= rInt;
                    case TokenType.GREATER: return lInt > rInt;
                    case TokenType.GREATER_EQUALS: return lInt >= rInt;
                    default:
                        throw new Exception($"Error de operación binaria en Línea {expr.Line}, Columna {expr.Column}: Operador '{expr.Operator}' no soportado para enteros.");
                }
            }
            else if (left is string lStr && right is string rStr)
            {
                switch (expr.Operator)
                {
                    case TokenType.EQUALS: return lStr == rStr;
                    case TokenType.NOT_EQUALS: return lStr != rStr;
                    default:
                        throw new Exception($"Error de operación binaria en Línea {expr.Line}, Columna {expr.Column}: Operador '{expr.Operator}' no soportado para cadenas.");
                }
            }
            else
            {
                throw new Exception($"Error de tipo en Línea {expr.Line}, Columna {expr.Column}: Tipos incompatibles para la operación binaria '{expr.Operator}' (izquierda: {left?.GetType().Name}, derecha: {right?.GetType().Name}). Se esperaban enteros o cadenas compatibles.");
            }
        }

        private object EvaluateRandExpression(RandExpression expr)
        {
            object minVal = VisitExpression(expr.Min);
            object maxVal = VisitExpression(expr.Max);

            if (minVal is int minInt && maxVal is int maxInt)
            {
                if (minInt > maxInt)
                    throw new Exception($"Error semántico en Línea {expr.Line}, Columna {expr.Column}: El valor mínimo en 'Rand' ({minInt}) no puede ser mayor que el valor máximo ({maxInt}).");
                return _random.Next(minInt, maxInt + 1);
            }
            else
            {
                throw new Exception($"Error de tipo en Línea {expr.Line}, Columna {expr.Column}: 'Rand' espera dos argumentos enteros.");
            }
        }

        private Color ParseColor(string colorName, int line, int column)
        {
            switch (colorName.ToLower())
            {
                case "red": return Colors.Red;
                case "green": return Colors.Green;
                case "blue": return Colors.Blue;
                case "black": return Colors.Black;
                case "white": return Colors.White;
                case "yellow": return Colors.Yellow;
                case "orange": return Colors.Orange;
                case "purple": return Colors.Purple;
                default:
                    throw new Exception($"Error semántico en Línea {line}, Columna {column}: Color '{colorName}' no reconocido.");
            }
        }

        // Dibuja una línea en el WriteableBitmap usando Bresenham
        private void DrawLineOnBitmap(Point start, Point end, int thickness)
        {
            int x0 = (int)Math.Round(start.X);
            int y0 = (int)Math.Round(start.Y);
            int x1 = (int)Math.Round(end.X);
            int y1 = (int)Math.Round(end.Y);

            _bitmap.Lock();
            try
            {
                for (int t = -thickness / 2; t <= thickness / 2; t++)
                {
                    BresenhamLine(x0, y0 + t, x1, y1 + t, _currentColor);
                }
                _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            }
            finally
            {
                _bitmap.Unlock();
            }
        }

        // Algoritmo de Bresenham para líneas
        private void BresenhamLine(int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            unsafe
            {
                int* pBackBuffer = (int*)_bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride / 4;
                while (true)
                {
                    if (x0 >= 0 && x0 < _bitmap.PixelWidth && y0 >= 0 && y0 < _bitmap.PixelHeight)
                    {
                        int colorInt = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
                        pBackBuffer[y0 * stride + x0] = colorInt;
                    }
                    if (x0 == x1 && y0 == y1) break;
                    e2 = 2 * err;
                    if (e2 >= dy)
                    {
                        err += dy;
                        x0 += sx;
                    }
                    if (e2 <= dx)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }
            }
        }

        private void MoveForward(int distance)
        {
            double angleRad = _currentRotation * Math.PI / 180.0;
            Point start = _currentPosition;
            double endX = _currentPosition.X + distance * Math.Cos(angleRad);
            double endY = _currentPosition.Y + distance * Math.Sin(angleRad);
            Point end = new Point(endX, endY);
            DrawLineOnBitmap(start, end, 1);
            _currentPosition = end;
        }
    }
}