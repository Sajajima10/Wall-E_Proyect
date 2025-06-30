using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _currentThickness = 1;
        private double _currentRotation;

        public event Action<string> OnOutputMessage;
        public event Action<Point> OnBrushMoved;

        private Dictionary<string, FunctionDefinition> _functions = new();

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

            // 1. Guarda todas las funciones definidas por el usuario
            foreach (var stmt in program.Statements)
            {
                if (stmt is FunctionDefinition funcDef)
                    _functions[funcDef.Name] = funcDef;
            }

            // 2. Ejecuta el resto normalmente (ignora definiciones de función)
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

                if (statement is FunctionDefinition)
                {
                    pc++;
                    continue; // No ejecutar definiciones de función aquí
                }

                if (statement is FunctionCallStatement funcStmt)
                {
                    if (!spawnCalled)
                    {
                        if (funcStmt.Call.FunctionName != "Spawn")
                            throw new Exception($"El código debe comenzar con 'Spawn'.");
                        spawnCalled = true;
                    }
                    else if (funcStmt.Call.FunctionName == "Spawn")
                    {
                        throw new Exception($"'Spawn' solo puede llamarse una vez al inicio.");
                    }
                }

                try
                {
                    if (statement is LabelStatement)
                    {
                        // No hacer nada, solo es un marcador
                    }
                    else
                    {
                        VisitStatement(statement);
                    }
                }
                catch (GoToException ex)
                {
                    if (!labelMap.TryGetValue(ex.Label, out int target))
                        throw new Exception($"Etiqueta '{ex.Label}' no encontrada.");
                    pc = target;
                    continue;
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
                case FunctionCallStatement funcStmt:
                    VisitFunctionCall(funcStmt.Call);
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
                case LoopStatement loopStmt:
                    VisitLoopStatement(loopStmt);
                    break;
                case ReturnStatement retStmt:
                    throw new ReturnException(VisitExpression(retStmt.Value));
                case DrawRectangleStatement drawRectStmt:
                    VisitDrawRectangleStatement(drawRectStmt);
                    break;
                case GoToStatement gotoStmt:
                    var cond = VisitExpression(gotoStmt.Condition);
                    bool jump = false;
                    if (cond is bool b)
                        jump = b;
                    else if (cond is int i)
                        jump = i != 0;
                    else
                        throw new Exception($"La condición de GoTo debe ser booleana o entera (0/1).");
                    if (jump)
                        throw new GoToException(gotoStmt.Label);
                    break;
                default:
                    throw new NotImplementedException($"La sentencia de tipo '{statement.GetType().Name}' aún no está implementada en el intérprete. Línea: {statement.Line}, Columna: {statement.Column}");
            }
        }

        private void VisitFunctionCall(FunctionCall funcCall)
        {
            List<object> evaluatedArgs = funcCall.Arguments.Select(VisitExpression).ToList();

            switch (funcCall.FunctionName)
            {
                case "Spawn":
                    if (evaluatedArgs.Count != 2 || !(evaluatedArgs[0] is int x) || !(evaluatedArgs[1] is int y))
                        throw new Exception($"'Spawn' espera dos enteros (x, y).");
                    _currentPosition = new Point(x, y);
                    OnBrushMoved?.Invoke(_currentPosition);
                    break;

                case "Color":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is string colorName))
                        throw new Exception($"'Color' espera un string con el nombre del color.");
                    _currentColor = ParseColor(colorName, funcCall.Line, funcCall.Column);
                    break;

                case "DrawLine":
                    if (evaluatedArgs.Count != 3 || !(evaluatedArgs[0] is int dirX) || !(evaluatedArgs[1] is int dirY) || !(evaluatedArgs[2] is int distance))
                        throw new Exception($"'DrawLine' espera tres enteros (dirX, dirY, distance).");
                    DrawLineDirectional(dirX, dirY, distance);
                    break;

                case "DrawCircle":
                    if (evaluatedArgs.Count != 3 || !(evaluatedArgs[0] is int circleDirX) || !(evaluatedArgs[1] is int circleDirY) || !(evaluatedArgs[2] is int radius))
                        throw new Exception($"'DrawCircle' espera tres enteros (dirX, dirY, radius).");
                    DrawCircle(circleDirX, circleDirY, radius);
                    break;

                case "Forward":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int forwardDist))
                        throw new Exception($"'Forward' espera un entero (distancia).");
                    MoveForward(forwardDist);
                    break;

                case "Backward":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int backwardDist))
                        throw new Exception($"'Backward' espera un entero (distancia).");
                    MoveBackward(backwardDist);
                    break;

                case "Size":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int size))
                        throw new Exception($"'Size' espera un entero.");
                    if (size <= 0)
                        throw new Exception($"'Size' debe ser mayor que 0.");
                    _currentThickness = size % 2 == 0 ? size - 1 : size;
                    break;

                case "Fill":
                    if (evaluatedArgs.Count != 0)
                        throw new Exception("'Fill' no recibe argumentos.");
                    FillArea();
                    break;

                case "Call":
                    if (evaluatedArgs.Count < 1 || !(evaluatedArgs[0] is string funcName))
                        throw new Exception("'Call' espera al menos el nombre de la función como string.");
                    var callArgs = evaluatedArgs.Skip(1).ToList();
                    CallUserFunction(funcName, callArgs);
                    break;

                case "Turn":
                    if (evaluatedArgs.Count != 1 || !(evaluatedArgs[0] is int angle))
                        throw new Exception("'Turn' espera un entero (grados).");
                    _currentRotation = (_currentRotation + angle) % 360;
                    break;

                default:
                    throw new Exception($"Función '{funcCall.FunctionName}' no reconocida o no implementada.");
            }
        }

        private object CallUserFunction(string name, List<object> args)
        {
            if (!_functions.TryGetValue(name, out var funcDef))
                throw new Exception($"Función '{name}' no definida.");

            var prevEnv = _environment;
            _environment = new Environment(prevEnv);

            for (int i = 0; i < funcDef.Parameters.Count; i++)
                _environment.Set(funcDef.Parameters[i], args[i]);

            object returnValue = null;
            try
            {
                foreach (var stmt in funcDef.Body)
                {
                    try
                    {
                        VisitStatement(stmt);
                    }
                    catch (ReturnException rex)
                    {
                        returnValue = rex.Value;
                        break;
                    }
                }
            }
            finally
            {
                _environment = prevEnv;
            }
            return returnValue;
        }

        private void VisitAssignment(Assignment assignment)
        {
            object value = VisitExpression(assignment.Value);
            _environment.Set(assignment.VariableName, value);
        }

        private void VisitPrintStatement(PrintStatement printStmt)
        {
            object valueToPrint = VisitExpression(printStmt.Expression);
            string output;

            if (valueToPrint is IEnumerable<int> list)
                output = "[" + string.Join(", ", list) + "]";
            else
                output = valueToPrint?.ToString() ?? string.Empty;

            OnOutputMessage?.Invoke(output);
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

        private void VisitLoopStatement(LoopStatement loopStmt)
        {
            int from = Convert.ToInt32(VisitExpression(loopStmt.From));
            int to = Convert.ToInt32(VisitExpression(loopStmt.To));
            for (int i = from; i <= to; i++)
            {
                _environment.Set(loopStmt.Iterator, i);
                foreach (var stmt in loopStmt.Body)
                    VisitStatement(stmt);
            }
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

        case UnaryExpression unaryExpr:
            return EvaluateUnaryExpression(unaryExpr);

        case RandExpression randExpr:
            return EvaluateRandExpression(randExpr);

        case FunctionCall funcCallExpr:
    if (funcCallExpr.FunctionName == "GetActualX")
    {
        if (funcCallExpr.Arguments.Any())
            throw new Exception($"Error en Línea {funcCallExpr.Line}, Columna {funcCallExpr.Column}: La función GetActualX no recibe argumentos.");
        return (int)_currentPosition.X;
    }
    else if (funcCallExpr.FunctionName == "GetActualY")
    {
        if (funcCallExpr.Arguments.Any())
            throw new Exception($"Error en Línea {funcCallExpr.Line}, Columna {funcCallExpr.Column}: La función GetActualY no recibe argumentos.");
        return (int)_currentPosition.Y;
    }
    else if (funcCallExpr.FunctionName == "GetCanvasSize")
    {
        if (funcCallExpr.Arguments.Any())
            throw new Exception($"Error en Línea {funcCallExpr.Line}, Columna {funcCallExpr.Column}: La función GetCanvasSize no recibe argumentos.");
        return new List<int> { _bitmap.PixelWidth, _bitmap.PixelHeight };
    }
    else if (funcCallExpr.FunctionName == "Call")
    {
        if (funcCallExpr.Arguments.Count < 1)
            throw new Exception("CALL espera al menos el nombre de la función.");
        string funcName = (string)VisitExpression(funcCallExpr.Arguments[0]);
        var callArgs = funcCallExpr.Arguments.Skip(1).Select(VisitExpression).ToList();
        return CallUserFunction(funcName, callArgs);
    }
    else if (_functions.ContainsKey(funcCallExpr.FunctionName))
    {
        var callArgs = funcCallExpr.Arguments.Select(VisitExpression).ToList();
        return CallUserFunction(funcCallExpr.FunctionName, callArgs);
    }
    else if (funcCallExpr.FunctionName == "GetColorCount")
    {
        if (funcCallExpr.Arguments.Count != 5)
            throw new Exception($"'GetColorCount' espera exactamente 5 argumentos. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

        var args = funcCallExpr.Arguments.Select(VisitExpression).ToList();

        if (!(args[0] is string colorName) ||
            !(args[1] is int x1) || !(args[2] is int y1) ||
            !(args[3] is int x2) || !(args[4] is int y2))
        {
            throw new Exception($"Argumentos inválidos para 'GetColorCount'. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");
        }

        Color targetColor = ParseColor(colorName, funcCallExpr.Line, funcCallExpr.Column);
        return GetColorCountInRegion(targetColor, x1, y1, x2, y2);
    }
    else if (funcCallExpr.FunctionName == "IsBrushColor")
{
    if (funcCallExpr.Arguments.Count != 1)
        throw new Exception($"'IsBrushColor' espera un argumento. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    var arg = VisitExpression(funcCallExpr.Arguments[0]);
    if (arg is not string colorName)
        throw new Exception($"El argumento de 'IsBrushColor' debe ser un string. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    Color colorToCheck = ParseColor(colorName, funcCallExpr.Line, funcCallExpr.Column);
    return AreColorsEqual(_currentColor, colorToCheck);
}

else if (funcCallExpr.FunctionName == "IsBrushSize")
{
    if (funcCallExpr.Arguments.Count != 1)
        throw new Exception($"'IsBrushSize' espera un argumento. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    var arg = VisitExpression(funcCallExpr.Arguments[0]);
    if (arg is not int size)
        throw new Exception($"El argumento de 'IsBrushSize' debe ser un entero. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    return _currentThickness == size;
}

else if (funcCallExpr.FunctionName == "IsCanvasColor")
{
    if (funcCallExpr.Arguments.Count != 1)
        throw new Exception($"'IsCanvasColor' espera un argumento. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    var arg = VisitExpression(funcCallExpr.Arguments[0]);
    if (arg is not string colorName)
        throw new Exception($"El argumento de 'IsCanvasColor' debe ser un string. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");

    Color target = ParseColor(colorName, funcCallExpr.Line, funcCallExpr.Column);
    Color actual = GetPixelColor((int)_currentPosition.X, (int)_currentPosition.Y);

    return AreColorsEqual(actual, target);
}
    else
                    {
                        throw new Exception($"Función '{funcCallExpr.FunctionName}' no reconocida como expresión. Línea: {funcCallExpr.Line}, Columna: {funcCallExpr.Column}");
                    }

        default:
            throw new NotImplementedException($"La expresión de tipo '{expression.GetType().Name}' aún no está implementada en el intérprete. Línea: {expression.Line}, Columna: {expression.Column}");
    }
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
                    case TokenType.POWER: 
                    if (rInt < 0)
                        {
                            throw new Exception($"Error semántico: El exponente debe ser un entero no negativo para potencias enteras. Línea {expr.Line}, Columna {expr.Column}");
                        }
                        return (int)Math.Pow(lInt, rInt);
                    case TokenType.DIVIDE:
                        if (rInt == 0)
                            throw new Exception($"Error en tiempo de ejecución en Línea {expr.Line}, Columna {expr.Column}: División por cero.");
                        return lInt / rInt;
                    case TokenType.MODULO:
                        if (rInt == 0)
                            throw new Exception($"Error en tiempo de ejecución en Línea {expr.Line}, Columna {expr.Column}: División por cero.");
                        return lInt % rInt;
                    case TokenType.EQUALS: return lInt == rInt;
                    case TokenType.NOT_EQUALS: return lInt != rInt;
                    case TokenType.LESS: return lInt < rInt;
                    case TokenType.LESS_EQUALS: return lInt <= rInt;
                    case TokenType.GREATER: return lInt > rInt;
                    case TokenType.GREATER_EQUALS: return lInt >= rInt;
                }
            }
            else if (left is bool lBool && right is bool rBool)
            {
                switch (expr.Operator)
                {
                    case TokenType.AND: return lBool && rBool;
                    case TokenType.OR: return lBool || rBool;
                    case TokenType.EQUALS: return lBool == rBool;
                    case TokenType.NOT_EQUALS: return lBool != rBool;
                    default:
                        throw new Exception($"Error de operación binaria en Línea {expr.Line}, Columna {expr.Column}: Operador '{expr.Operator}' no soportado para booleanos.");
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
                switch (expr.Operator)
                {
                    case TokenType.AND: return ToBool(left) && ToBool(right);
                    case TokenType.OR: return ToBool(left) || ToBool(right);
                }
                throw new Exception($"Error de tipo en Línea {expr.Line}, Columna {expr.Column}: Tipos incompatibles para la operación binaria '{expr.Operator}' (izquierda: {left?.GetType().Name}, derecha: {right?.GetType().Name}). Se esperaban enteros, booleanos o cadenas compatibles.");
            }

            throw new Exception($"Error de operación binaria en Línea {expr.Line}, Columna {expr.Column}: Operador '{expr.Operator}' no soportado.");
        }

        private object EvaluateUnaryExpression(UnaryExpression expr)
        {
            var operand = VisitExpression(expr.Operand);
            switch (expr.Operator)
            {
                case TokenType.NOT:
                    return !ToBool(operand);
                case TokenType.MINUS:
                    if (operand is int i)
                        return -i;
                    throw new Exception("Operador unario '-' solo se puede aplicar a enteros.");
                case TokenType.PLUS:
                    if (operand is int j)
                        return +j;
                    throw new Exception("Operador unario '+' solo se puede aplicar a enteros.");
                default:
                    throw new Exception($"Operador unario '{expr.Operator}' no soportado.");
            }
        }

        private bool ToBool(object value)
        {
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            return value != null;
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
                case "brown": return Colors.Brown;
                case "gray": return Colors.Gray;
                case "pink": return Colors.Pink;
                case "cyan": return Colors.Cyan;
                case "magenta": return Colors.Magenta;
                default:
                    throw new Exception($"Color '{colorName}' no reconocido en Línea: {line}, Columna: {column}.");
            }
        }

        private int GetColorCountInRegion(Color targetColor, int x1, int y1, int x2, int y2)
        {
            int count = 0;
            _bitmap.Lock();
            unsafe
            {
                int* pBackBuffer = (int*)_bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride / 4; 

                int startX = Math.Min(x1, x2);
                int endX = Math.Max(x1, x2);
                int startY = Math.Min(y1, y2);
                int endY = Math.Max(y1, y2);

                int targetColorInt = (targetColor.A << 24) | (targetColor.R << 16) | (targetColor.G << 8) | targetColor.B;

                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        if (pBackBuffer[y * stride + x] == targetColorInt)
                        {
                            count++;
                        }
                    }
                }
            }
            _bitmap.Unlock();
            return count;
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
            OnBrushMoved?.Invoke(_currentPosition);
        }

        private void MoveBackward(int distance)
        {
            double angleRad = (_currentRotation + 180) * Math.PI / 180.0;
            Point start = _currentPosition;
            double endX = _currentPosition.X + distance * Math.Cos(angleRad);
            double endY = _currentPosition.Y + distance * Math.Sin(angleRad);
            Point end = new Point(endX, endY);
            DrawLineOnBitmap(start, end, 1);
            _currentPosition = end;
            OnBrushMoved?.Invoke(_currentPosition);
        }

        private void DrawLineDirectional(int dirX, int dirY, int distance)
        {
            if (!(new[] { -1, 0, 1 }.Contains(dirX)) || !(new[] { -1, 0, 1 }.Contains(dirY)) || (dirX == 0 && dirY == 0))
                throw new Exception("Los valores dirX y dirY deben ser -1, 0 o 1, y no ambos 0.");

            Point start = _currentPosition;
            double endX = _currentPosition.X + dirX * distance;
            double endY = _currentPosition.Y + dirY * distance;
            Point end = new Point(endX, endY);

            DrawLineOnBitmap(start, end, _currentThickness); // Usa grosor actual
            _currentPosition = end;
            OnBrushMoved?.Invoke(_currentPosition);
        }

        private void DrawCircle(int dirX, int dirY, int radius)
        {
            if (!(new[] { -1, 0, 1 }.Contains(dirX)) || !(new[] { -1, 0, 1 }.Contains(dirY)) || (dirX == 0 && dirY == 0))
                throw new Exception("Los valores dirX y dirY deben ser -1, 0 o 1, y no ambos 0.");

            double centerX = _currentPosition.X + dirX * radius;
            double centerY = _currentPosition.Y + dirY * radius;
            Point center = new Point(centerX, centerY);

            int steps = 360;
            double angleStep = 2 * Math.PI / steps;
            Point? prev = null;
            for (int i = 0; i <= steps; i++)
            {
                double angle = i * angleStep;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                Point current = new Point(x, y);
                if (prev != null)
                    DrawLineOnBitmap(prev.Value, current, _currentThickness);
                prev = current;
            }

            _currentPosition = center;
            OnBrushMoved?.Invoke(_currentPosition); // La nueva posición es el centro del círculo
        }
        // Excepción interna para manejar return
        private class ReturnException : Exception
        {
            public object Value { get; }
            public ReturnException(object value) { Value = value; }
        }

        public object VisitDrawRectangleStatement(DrawRectangleStatement node)
        {
            int dx = Convert.ToInt32(VisitExpression(node.Dx));
            int dy = Convert.ToInt32(VisitExpression(node.Dy));
            int width = Convert.ToInt32(VisitExpression(node.Width));
            int height = Convert.ToInt32(VisitExpression(node.Height));

            DrawRectangle(dx, dy, width, height);
            return null;
        }
        private void DrawRectangle(int dx, int dy, int width, int height)
        {
            // Calcula la esquina superior izquierda real basada en la posición actual
            double startX = _currentPosition.X + dx;
            double startY = _currentPosition.Y + dy;

            // Asegura que las coordenadas estén dentro de los límites del bitmap (opcional pero recomendado)
            startX = Math.Max(0, Math.Min(_bitmap.PixelWidth - 1, startX));
            startY = Math.Max(0, Math.Min(_bitmap.PixelHeight - 1, startY));

            double endX = startX + width;
            double endY = startY + height;

            endX = Math.Max(0, Math.Min(_bitmap.PixelWidth - 1, endX));
            endY = Math.Max(0, Math.Min(_bitmap.PixelHeight - 1, endY));

            // Dibuja las cuatro líneas del rectángulo
            // Línea superior
            DrawLineOnBitmap(new Point(startX, startY), new Point(endX, startY), _currentThickness);
            // Línea derecha
            DrawLineOnBitmap(new Point(endX, startY), new Point(endX, endY), _currentThickness);
            // Línea inferior
            DrawLineOnBitmap(new Point(endX, endY), new Point(startX, endY), _currentThickness);
            // Línea izquierda
            DrawLineOnBitmap(new Point(startX, endY), new Point(startX, startY), _currentThickness);
        }

        private void FillArea()
        {
            int width = _bitmap.PixelWidth;
            int height = _bitmap.PixelHeight;
            int startX = (int)_currentPosition.X;
            int startY = (int)_currentPosition.Y;

            Color targetColor = GetPixelColor(startX, startY);
            if (AreColorsEqual(targetColor, _currentColor)) return;

            Queue<Point> queue = new Queue<Point>();
            HashSet<(int, int)> visited = new();
            queue.Enqueue(new Point(startX, startY));
            visited.Add((startX, startY));

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int x = (int)p.X;
                int y = (int)p.Y;

                Color current = GetPixelColor(x, y);
                if (AreColorsEqual(current, targetColor))
                {
                    DrawPixelOnBitmap(x, y, _currentColor);

                    foreach ((int dx, int dy) in new[] { (0, 1), (1, 0), (0, -1), (-1, 0) })
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited.Contains((nx, ny)))
                        {
                            queue.Enqueue(new Point(nx, ny));
                            visited.Add((nx, ny));
                        }
                    }
                }
            }
        }

        private Color GetPixelColor(int x, int y)
        {
            _bitmap.Lock();
            unsafe
            {
                IntPtr pBackBuffer = _bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride;
                byte* buffer = (byte*)pBackBuffer;
                int index = y * stride + x * 4;
                byte b = buffer[index];
                byte g = buffer[index + 1];
                byte r = buffer[index + 2];
                byte a = buffer[index + 3];
                _bitmap.Unlock();
                return Color.FromArgb(a, r, g, b);
            }
        }

        private bool AreColorsEqual(Color c1, Color c2)
        {
            return c1.A == c2.A && c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        private void DrawPixelOnBitmap(int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= _bitmap.PixelWidth || y >= _bitmap.PixelHeight)
                return;

            _bitmap.Lock();
            unsafe
            {
                IntPtr pBackBuffer = _bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride;
                byte* buffer = (byte*)pBackBuffer;
                int index = y * stride + x * 4;

                buffer[index] = color.B;
                buffer[index + 1] = color.G;
                buffer[index + 2] = color.R;
                buffer[index + 3] = color.A;
            }
            _bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            _bitmap.Unlock();
        }
        private object EvaluateFunctionExpression(FunctionCall call)
        {
            var args = call.Arguments.Select(VisitExpression).ToList();

            switch (call.FunctionName)
            {
                case "Rand":
                    if (args.Count != 2 || !(args[0] is int min) || !(args[1] is int max))
                        throw new Exception("Rand espera dos argumentos enteros.");
                    return _random.Next(min, max + 1);

                case "GetActualX":
                    if (args.Count != 0)
                        throw new Exception("GetActualX no recibe argumentos.");
                    return (int)_currentPosition.X;

                case "GetActualY":
                    if (args.Count != 0)
                        throw new Exception("GetActualY no recibe argumentos.");
                    return (int)_currentPosition.Y;
                
                case "GetCanvasSize":
                    if (args.Count != 0)
                        throw new Exception("GetCanvasSize no recibe argumentos.");
                    return new List<int> { _bitmap.PixelWidth, _bitmap.PixelHeight };

                case "GetColorCount": 
                   
                    if (args.Count != 5 ||
                        !(args[0] is string colorName) ||
                        !(args[1] is int x1) ||
                        !(args[2] is int y1) ||
                        !(args[3] is int y2) || 
                        !(args[4] is int x2) )
                    {
                        if (args.Count != 5 ||
                            !(args[0] is string) ||
                            !(args[1] is int) ||
                            !(args[2] is int) ||
                            !(args[3] is int) ||
                            !(args[4] is int) )
                        {
                            throw new Exception($"'GetColorCount' espera 5 argumentos: (color: string, x1: int, y1: int, x2: int, y2: int). Recibidos: {args.Count} argumentos con tipos incorrectos en Línea: {call.Line}, Columna: {call.Column}.");
                        }
                        colorName = (string)args[0];
                        x1 = (int)args[1];
                        y1 = (int)args[2];
                        x2 = (int)args[3]; 
                        y2 = (int)args[4]; 
                    }

                    int canvasWidth = _bitmap.PixelWidth;
                    int canvasHeight = _bitmap.PixelHeight;

                    if (x1 < 0 || x1 >= canvasWidth || y1 < 0 || y1 >= canvasHeight)
                    {
                        return 0;
                    }

                    if (x2 < 0 || x2 >= canvasWidth || y2 < 0 || y2 >= canvasHeight)
                    {
                        return 0;
                    }

                    Color targetColor = ParseColor(colorName, call.Line, call.Column);

                    return GetColorCountInRegion(targetColor, x1, y1, x2, y2);    

                default:
                    if (_functions.ContainsKey(call.FunctionName))
                        return CallUserFunction(call.FunctionName, args);
                    throw new Exception($"Función '{call.FunctionName}' no reconocida.");
            }
        }

        private class GoToException : Exception
        {
            public string Label { get; }
            public GoToException(string label) { Label = label; }
        }
    }
}