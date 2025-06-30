using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Interfaz
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap _pixelBitmap;
        private int _canvasSize = 256;
        private const int MaxCanvasSize = 1024;

        private readonly List<(string Titulo, string Codigo)> ejemplosPixelWallE = new()
        {
            ("Líneas Básicas", 
@"Spawn(40,40)
Color(""red"")
DrawLine(1,0,20)
Color(""green"")
DrawLine(0,1,20)
Color(""blue"")
DrawLine(-1,0,20)
Color(""black"")
DrawLine(0,-1,20)
"),
            ("Círculos en Cuatro Direcciones", 
@"Spawn(40,40)
Color(""blue"")
DrawCircle(1,0,10)
DrawCircle(0,1,10)
DrawCircle(-1,0,10)
DrawCircle(0,-1,10)
"),
            ("Cambio de Color y Grosor", 
@"Spawn(10,10)
Color(""red"")
Size(5)
DrawLine(1,1,20)
Color(""green"")
Size(3)
DrawLine(1,-1,20)
Color(""blue"")
Size(1)
DrawLine(-1,1,20)
"),
            ("Relleno de Área", 
@"Spawn(20,20)
Color(""black"")
DrawLine(1,0,20)
DrawLine(0,1,20)
DrawLine(-1,0,20)
DrawLine(0,-1,20)
Color(""yellow"")
Fill()
"),
            ("Bucle Simple", 
@"Spawn(5,75)
Color(""purple"")
Loop i = 0 To 10
    DrawLine(1,-1,7)
    DrawLine(1,1,7)
EndLoop
"),
            ("Condicional IF", 
@"Spawn(10,10)
Set x = 1
If x == 1
    Color(""orange"")
    DrawLine(1,1,30)
EndIf
"),
            ("Funciones de Usuario", 
@"Spawn(40,40)
Func Cruz()
    Color(""red"")
    DrawLine(1,0,10)
    DrawLine(-1,0,10)
    Color(""blue"")
    DrawLine(0,1,10)
    DrawLine(0,-1,10)
EndFunc
Call(""Cruz"")
"),
            ("GoTo y Etiquetas", 
@"Spawn(10,10)
Set n = 0
Loop i = 0 To 3
    Color(""green"")
    DrawLine(1,1,10)
    n = n + 1
    GoTo[REPETIR](n < 3)
EndLoop
REPETIR:
Color(""red"")
DrawLine(-1,1,10)
"),
            ("Funciones de Consulta", 
@"Spawn(5,5)
Color(""blue"")
DrawLine(1,1,10)
Print(GetActualX())
Print(GetActualY())
Print(GetCanvasSize())
Print(IsBrushColor(""blue""))
Print(IsBrushSize(1))
"),
            ("Forward, Backward y Turn", 
@"Spawn(40,40)
Color(""magenta"")
Loop i = 0 To 4
    Forward(20)
    Turn(72)
EndLoop
Loop i = 0 To 4
    Backward(20)
    Turn(-72)
EndLoop
"),
            ("Espiral de Colores", 
@"Spawn(25,25)
Set(ang,0)
Set(dist,1)
Loop i = 0 To 18
    If i % 3 == 0
        Color(""red"")
    EndIf
    If i % 3 == 1
        Color(""green"")
    EndIf
    If i % 3 == 2
        Color(""blue"")
    EndIf
    Forward(dist)
    Turn(20)
    Set(dist,dist+1)
EndLoop
"),
            ("Mandala Simple", 
@"Spawn(25,25)
Loop i = 0 To 7
    Color(""purple"")
    DrawLine(1,0,15)
    Backward(15)
    Turn(45)
EndLoop
Loop i = 0 To 3
    Color(""cyan"")
    DrawCircle(1,0,7)
    Turn(90)
EndLoop
"),
            ("Flor Pixelada", 
@"Spawn(25,25)
Loop i = 0 To 5
    Color(""yellow"")
    DrawLine(1,1,10)
    Backward(10)
    Turn(60)
EndLoop
Color(""green"")
DrawLine(0,1,12)
"),
            ("Estrella de 8 Puntas", 
@"Spawn(25,25)
Loop i = 0 To 7
    Color(""orange"")
    DrawLine(1,0,15)
    Backward(15)
    Turn(45)
EndLoop
"),
            ("Corazón Pixelado", 
@"Spawn(25,15)
Color(""red"")
DrawLine(1,1,7)
DrawLine(-1,1,7)
DrawLine(1,0,5)
DrawLine(-1,0,5)
DrawLine(0,1,5)
Color(""pink"")
Fill()
"),
            ("Árbol Fractal", 
@"Spawn(25,48)
Func Rama(n)
    If n == 0
        Return(0)
    EndIf
    Color(""brown"")
    Forward(n)
    Turn(-30)
    Call(""Rama"", n-4)
    Turn(60)
    Call(""Rama"", n-4)
    Turn(-30)
    Backward(n)
EndFunc
Call(""Rama"", 12)
"),
            ("Mariposa Simétrica", 
@"Spawn(25,25)
Loop i = 0 To 3
    Color(""blue"")
    DrawCircle(1,1,7)
    Turn(90)
EndLoop
Color(""black"")
DrawLine(0,1,10)
DrawLine(0,-1,10)
"),
            ("Sol y Nubes", 
@"Spawn(10,10)
Color(""yellow"")
DrawCircle(1,0,5)
Color(""white"")
Spawn(40,8)
DrawCircle(1,0,4)
Spawn(43,15)
DrawCircle(1,0,3)
Spawn(37,15)
DrawCircle(1,0,3)
"),
            ("Mosaico de Colores", 
@"Spawn(3,3)
Loop i = 0 To 4
    Loop j = 0 To 4
        If (i+j)%3 == 0
            Color(""red"")
        EndIf
        If (i+j)%3 == 1
            Color(""green"")
        EndIf
        If (i+j)%3 == 2
            Color(""blue"")
        EndIf
        DrawRectangle(0,0,6,6)
        Spawn(3+j*10,3+i*10)
    EndLoop
EndLoop
"),
            ("Rueda de Colores", 
@"Spawn(25,25)
Loop i = 0 To 11
    If i%4 == 0
        Color(""red"")
    EndIf
    If i%4 == 1
        Color(""green"")
    EndIf
    If i%4 == 2
        Color(""blue"")
    EndIf
    If i%4 == 3
        Color(""yellow"")
    EndIf
    DrawLine(1,0,15)
    Backward(15)
    Turn(30)
EndLoop
")
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas();

            ResizeCanvasButton.Click += ResizeCanvasButton_Click;
            ExecuteButton.Click += ExecuteButton_Click;
            LoadButton.Click += LoadButton_Click;
            SaveButton.Click += SaveButton_Click;
            CodeEditor.TextChanged += CodeEditor_TextChanged;
            ExamplesButton.Click += ExamplesButton_Click;
        }

        private void InitializeCanvas()
        {
            if (!int.TryParse(CanvasSizeTextBox.Text, out _canvasSize) || _canvasSize <= 0 || _canvasSize > MaxCanvasSize)
            {
                _canvasSize = 256; 
                CanvasSizeTextBox.Text = _canvasSize.ToString();
                MessageBox.Show($"Dimensiones de canvas no válidas. Se usará el tamaño por defecto de {_canvasSize}x{_canvasSize}.", "Error de Entrada", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            _pixelBitmap = new WriteableBitmap(_canvasSize, _canvasSize, 96, 96, PixelFormats.Bgr32, null);
            PixelCanvas.Source = _pixelBitmap;

            ClearCanvas(); 
            DrawGrid();    
        }

        private void ClearCanvas()
        {
            _pixelBitmap.Lock();
            unsafe
            {
                int* pBackBuffer = (int*)_pixelBitmap.BackBuffer;
                int stride = _pixelBitmap.BackBufferStride / 4;
                for (int y = 0; y < _canvasSize; y++)
                {
                    for (int x = 0; x < _canvasSize; x++)
                    {
                        pBackBuffer[y * stride + x] = unchecked((int)0xFFD3D3D3);
                    }
                }
            }
            _pixelBitmap.AddDirtyRect(new Int32Rect(0, 0, _canvasSize, _canvasSize));
            _pixelBitmap.Unlock();
        }

        private void DrawGrid()
        {
            GridOverlay.Children.Clear();
            double scale = ((ScaleTransform)PixelCanvas.LayoutTransform).ScaleX;

            if (scale <= 1) return; 

            GridOverlay.Width = _canvasSize * scale;
            GridOverlay.Height = _canvasSize * scale;

            for (int i = 0; i <= _canvasSize; i++)
            {
                GridOverlay.Children.Add(new Line
                {
                    X1 = i * scale,
                    Y1 = 0,
                    X2 = i * scale,
                    Y2 = _canvasSize * scale,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                });
            }

            for (int i = 0; i <= _canvasSize; i++)
            {
                GridOverlay.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = i * scale,
                    X2 = _canvasSize * scale,
                    Y2 = i * scale,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5
                });
            }
        }

        private void ResizeCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeCanvas();
            MessageBox.Show($"Canvas redimensionado a {_canvasSize}x{_canvasSize}.", "Canvas Redimensionado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string code = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text.Trim();
            OutputTextBox.Text = ""; 

            var lexer = new Language.Lexer(code);
            var tokens = lexer.Tokenize();
            var parser = new Language.Parser(tokens);

            try
            {
                var programNode = parser.ParseProgram();
                var interpreter = new Language.Interpreter(_pixelBitmap);
                interpreter.OnBrushMoved += UpdateBrushIndicator;

                interpreter.OnOutputMessage += (msg) =>
                {
                    OutputTextBox.Dispatcher.Invoke(() => OutputTextBox.AppendText(msg + Environment.NewLine));
                };

                interpreter.Interpret(programNode);
            }
            catch (Exception ex)
            {
                OutputTextBox.AppendText($"Error: {ex.Message}");
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    CodeEditor.Document.Blocks.Clear();
                    CodeEditor.Document.Blocks.Add(new Paragraph(new Run(fileContent)));
                    MessageBox.Show($"Archivo '{System.IO.Path.GetFileName(openFileDialog.FileName)}' cargado exitosamente.", "Cargar Archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*",
                FileName = "mi_pixel_art.pw"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string fileContent = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text.TrimEnd('\r', '\n');
                try
                {
                    File.WriteAllText(filePath, fileContent);
                    MessageBox.Show($"Archivo '{System.IO.Path.GetFileName(filePath)}' guardado exitosamente.", "Guardar Archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo: {ex.Message}", "Error al Guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateLineNumbers();
        }

        private void UpdateLineNumbers()
        {
            string fullText = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text;
            string[] lines = fullText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= lines.Length; i++)
            {
                sb.AppendLine(i.ToString());
            }
            LineNumbersTextBlock.Text = sb.ToString();
        }
        
        private void UpdateBrushIndicator(Point pos)
        {
            Dispatcher.Invoke(() =>
            {
                double scale = 8;
                BrushIndicator.Visibility = Visibility.Collapsed;
                // Limpia cualquier cruz previa
                var toRemove = new List<UIElement>();
                foreach (var child in GridOverlay.Children)
                {
                    if (child is Line l && l.Tag as string == "BrushCross")
                        toRemove.Add(l);
                }
                foreach (var el in toRemove)
                    GridOverlay.Children.Remove(el);
                // Dibuja la cruz azul del tamaño de un pixel
                double cx = Math.Round(pos.X) * scale;
                double cy = Math.Round(pos.Y) * scale;
                double size = scale;
                var hLine = new Line
                {
                    X1 = cx,
                    Y1 = cy + size / 2,
                    X2 = cx + size,
                    Y2 = cy + size / 2,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    Tag = "BrushCross"
                };
                var vLine = new Line
                {
                    X1 = cx + size / 2,
                    Y1 = cy,
                    X2 = cx + size / 2,
                    Y2 = cy + size,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    Tag = "BrushCross"
                };
                GridOverlay.Children.Add(hLine);
                GridOverlay.Children.Add(vLine);
            });
        }

        private void ExamplesButton_Click(object sender, RoutedEventArgs e)
        {
            var selector = new Window { Title = "Selecciona un ejemplo", Width = 400, Height = 500, WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
            var listBox = new ListBox { Margin = new Thickness(10) };
            foreach (var ej in ejemplosPixelWallE)
                listBox.Items.Add(ej.Titulo);
            var btn = new Button { Content = "Cargar", Margin = new Thickness(10), IsDefault = true };
            btn.Click += (s, ev) => selector.DialogResult = true;
            var stack = new StackPanel();
            stack.Children.Add(listBox);
            stack.Children.Add(btn);
            selector.Content = stack;
            if (selector.ShowDialog() == true && listBox.SelectedIndex >= 0)
            {
                var codigo = ejemplosPixelWallE[listBox.SelectedIndex].Codigo;
                CodeEditor.Document.Blocks.Clear();
                CodeEditor.Document.Blocks.Add(new Paragraph(new Run(codigo)));
            }
        }
    }
} 