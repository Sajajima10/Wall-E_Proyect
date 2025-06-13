using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Interfaz
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap _pixelBitmap;
        private int _canvasSize = 256;
        private const int MaxCanvasSize = 1024;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCanvas();

            ResizeCanvasButton.Click += ResizeCanvasButton_Click;
            ExecuteButton.Click += ExecuteButton_Click;
            LoadButton.Click += LoadButton_Click;
            SaveButton.Click += SaveButton_Click;
            CodeEditor.TextChanged += CodeEditor_TextChanged;
        }

        private void InitializeCanvas()
        {
            if (int.TryParse(CanvasSizeTextBox.Text, out int newSize) && newSize > 0 && newSize <= MaxCanvasSize)
            {
                _canvasSize = newSize;
            }
            else
            {
                CanvasSizeTextBox.Text = _canvasSize.ToString();
                MessageBox.Show($"Dimensiones de canvas no válidas. Debe ser un entero positivo y menor o igual a {MaxCanvasSize}. Se usará el tamaño por defecto.", "Error de Entrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                _canvasSize = 256;
            }

            _pixelBitmap = new WriteableBitmap(_canvasSize, _canvasSize, 96, 96, PixelFormats.Bgr32, null);
            PixelCanvas.Source = _pixelBitmap;
            ClearCanvas();
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

        private void ResizeCanvasButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CanvasSizeTextBox.Text, out int newSize) && newSize > 0 && newSize <= MaxCanvasSize)
            {
                _canvasSize = newSize;
                InitializeCanvas();
                MessageBox.Show($"Canvas redimensionado a {_canvasSize}x{_canvasSize} y limpiado.", "Canvas Redimensionado");
            }
            else
            {
                MessageBox.Show($"Por favor, ingrese un número entero válido, positivo y menor o igual a {MaxCanvasSize} para las dimensiones del canvas.", "Error de Entrada", MessageBoxButton.OK, MessageBoxImage.Error);
                CanvasSizeTextBox.Text = _canvasSize.ToString();
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            string code = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text.TrimEnd('\r', '\n');
            OutputTextBox.Text = ""; // Limpia la salida anterior

            // Lexer y Parser
            Interfaz.Language.Lexer lexer = new Interfaz.Language.Lexer(code);
            List<Interfaz.Language.Token> tokens = lexer.Tokenize();
            Interfaz.Language.Parser parser = new Interfaz.Language.Parser(tokens);
            Interfaz.Language.AST.ProgramNode programNode;

            try
            {
                programNode = parser.ParseProgram();
            }
            catch (Exception ex)
            {
                OutputTextBox.Text = "Error de parsing:\n" + ex.Message;
                return;
            }

            // Interpreter
            Interfaz.Language.Interpreter interpreter = new Interfaz.Language.Interpreter(_pixelBitmap); // Usa tu Canvas aquí
            interpreter.OnOutputMessage += (msg) =>
            {
                OutputTextBox.Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText(msg + Environment.NewLine);
                });
            };

            try
            {
                interpreter.Interpret(programNode);
            }
            catch (Exception ex)
            {
                OutputTextBox.AppendText("Error de ejecución:\n" + ex.Message);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    CodeEditor.Document.Blocks.Clear();
                    CodeEditor.Document.Blocks.Add(new Paragraph(new Run(fileContent)));
                    MessageBox.Show($"Archivo '{System.IO.Path.GetFileName(filePath)}' cargado exitosamente.", "Cargar Archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*";
            saveFileDialog.FileName = "mi_pixel_art.pw";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                if (File.Exists(filePath))
                {
                    MessageBoxResult result = MessageBox.Show($"El archivo '{System.IO.Path.GetFileName(filePath)}' ya existe. ¿Desea sobrescribirlo?", "Confirmar Sobrescritura", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                string fileContent = new TextRange(CodeEditor.Document.ContentStart, CodeEditor.Document.ContentEnd).Text.TrimEnd('\r', '\n');
                try
                {
                    File.WriteAllText(filePath, fileContent);
                    MessageBox.Show($"Archivo '{System.IO.Path.GetFileName(filePath)}' guardado exitosamente.", "Guardar Archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
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
            string[] lines = fullText.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 1; i <= lines.Length; i++)
            {
                sb.AppendLine(i.ToString());
            }
            LineNumbersTextBlock.Text = sb.ToString();
        }
    }
}