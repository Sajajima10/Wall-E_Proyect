   M S B A M L   `   `   `  ����  Interfaz    Interfaz.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Pixel Wall-E��5   	   $	��700��6   $
��1200��6+   .��5      � $��10q�6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��2* �6   �� 5	      $��5* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $	��100 �6   .+�5   
   �� $��0��6   $��3��6%   $��0��69    �� Orientation$ 
Horizontal=�6F   $��0,0,0,10q�6_   .+�5      �� $��Tamaño:��6   $��Center=�6'   �� 5      -   5         ��Name$ CanvasSizeTextBox��5      $��50��61   $	��5,0q�6<   �� 5      -   5         ��Name$ ResizeCanvasButton��5      $��Redimensionar��61   $	��5,0q�6I   �� 5      -   5        $ 
LoadButton��5      $��Cargar��6)   $	��5,0q�6:   �� 5      -   5        $ 
SaveButton��5      $��Guardar��6)   $	��5,0q�6;   �� 5      -   5        $ ExecuteButton��5      $��Ejecutar��6,   $	��5,0q�6?   5      �� 5   
   -   5         ��Name$ LineNumbersTextBlock��5      $��0��62   $��1��6B   $��Consolas)�5      $��14(�6*   $	��Top=�68    �� TextAlignment$ Right=�5      $��0,0,5,0q�6*   �� 5!   
   -   5         ��Name$ 
CodeEditor��5!      $��1��6*   $��1��6:   $��Consolas)�5"      $��14(�6,   "	 �� VerticalScrollBarVisibility$
	 Auto=�6:   �� 5%   
   $��2��6   $��1��6!   
�������$��1q�6A   
 �� Padding$
 5q�6U   ._�5&      �� $
��Auto=�6   $
��Auto=�6@   .��5'      � .+�5)      �� -   5         ��Name$ PixelCanvas��5)      $
��Left=�5*      $	��Top=�5+      $
��None=�5,       �� BitmapScalingMode$ NearestNeighbor=�5-       � LayoutTransform 5.      �� 5/       �� ScaleX$ 8Q�6-    �� ScaleY$ 8Q�68   51      �� 54      -	   5         ��Name$ GridOverlay��54       J� IsHitTestVisible$ False��55      $
��Left=�56      $	��Top=�57      
������ 58      A� 5:      -
   5         A�Name$ BrushIndicator��5:      $��6��5;      $��6��6#   
���  ��
���   �$��1��5>       �� Left$ 0��5?      
 �� Top$ 0��5@      $ False��5A       J� RenderTransformOrigin$ 0.5,0.5&�5B      $y�	Collapsed=�5C      5D      5E      5F      �� 5I   
   -   5        $ OutputTextBox��5I      $��0��5J      $��3��5K      $��2��5L      $��0,10,0,0q�5M      $��Consolas)�5N      $��14(�5O      $
	 Auto=�5P      $ �� HorizontalScrollBarVisibility$
 Auto=�5Q       �� 
IsReadOnly$
 True��5R       �� AcceptsReturn$
 True��5S       �� TextWrapping$
 Wrap=�5T      5U      5V      