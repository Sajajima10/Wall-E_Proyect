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
                BrushIndicator.Visibility = Visibility.Visible;
                Canvas.SetLeft(BrushIndicator, pos.X * scale - BrushIndicator.Width / 2);
                Canvas.SetTop(BrushIndicator, pos.Y * scale - BrushIndicator.Height / 2);
            });
        }
    }
} 