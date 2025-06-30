# Ejemplos de Código para PixelWallE

Este archivo contiene todos los ejemplos de código para probar las funciones del lenguaje PixelWallE. Todos los ejemplos están diseñados para un canvas de 50x50 píxeles.

## 1. Espiral Colorida
Demuestra: `Forward`, `Turn`, `Color`, `Size`, `Loop`

```pixelwalle
Spawn(25, 25)
Color("red")
Size(2)
Loop i = 1 To 20
    Forward(i * 2)
    Turn(90)
    Color("blue")
    Forward(i * 2)
    Turn(90)
    Color("green")
    Forward(i * 2)
    Turn(90)
    Color("purple")
    Forward(i * 2)
    Turn(90)
EndLoop
```

## 2. Mandala
Demuestra: `DrawCircle`, `Color`, `Turn`, `Loop`

```pixelwalle
Spawn(25, 25)
Color("purple")
Size(1)
Loop i = 1 To 8
    DrawCircle(0, 1, 15)
    Turn(45)
    Color("pink")
    DrawCircle(1, 0, 10)
    Turn(45)
    Color("cyan")
    DrawCircle(0, -1, 5)
    Turn(45)
EndLoop
```

## 3. Casa Simple
Demuestra: `DrawRectangle`, `DrawLine`, `Fill`, `Spawn`

```pixelwalle
Spawn(10, 35)
Color("brown")
Size(2)
DrawRectangle(0, 0, 20, 15)

Spawn(20, 35)
Color("black")
Size(1)
DrawRectangle(0, 0, 3, 3)

Spawn(10, 35)
Color("red")
Size(1)
DrawRectangle(0, -5, 20, 10)

Spawn(15, 30)
Color("yellow")
Size(1)
Fill()
```

## 4. Flor
Demuestra: `DrawCircle`, `Color`, `Turn`, `Forward`

```pixelwalle
Spawn(25, 25)
Color("yellow")
Size(1)
DrawCircle(0, 0, 3)

Color("red")
Loop i = 1 To 8
    DrawCircle(0, 1, 8)
    Turn(45)
EndLoop

Color("green")
Size(2)
Spawn(25, 25)
Move(25, 40)
Forward(10)
```

## 5. Patrón Geométrico
Demuestra: `DrawLine`, `Turn`, `Color`, `Loop`

```pixelwalle
Spawn(25, 25)
Color("blue")
Size(1)
Loop i = 1 To 6
    DrawLine(1, 0, 20)
    Turn(60)
    Color("red")
    DrawLine(0, 1, 20)
    Turn(60)
    Color("green")
    DrawLine(-1, 0, 20)
    Turn(60)
EndLoop
```

## 6. Estrella
Demuestra: `Forward`, `Turn`, `Color`, `Loop`

```pixelwalle
Spawn(25, 25)
Color("yellow")
Size(2)
Turn(18)
Loop i = 1 To 5
    Forward(20)
    Turn(144)
EndLoop

Color("orange")
Size(1)
Spawn(25, 25)
Turn(18)
Loop i = 1 To 5
    Forward(15)
    Turn(144)
EndLoop
```

## 7. Corazón
Demuestra: `DrawCircle`, `DrawLine`, `Color`, `Forward`

```pixelwalle
Spawn(25, 35)
Color("red")
Size(1)
DrawCircle(-1, 0, 8)
DrawCircle(1, 0, 8)

Spawn(25, 35)
Color("red")
Size(2)
Turn(45)
Forward(8)
Turn(90)
Forward(8)
```

## 8. Todas las Funciones
Demuestra todas las funciones del lenguaje

```pixelwalle
Spawn(10, 10)
Color("blue")
Size(2)
Print("Iniciando demostración de todas las funciones")

// Variables y operaciones
Set x = 5
Set y = 10
Set resultado = x + y
Print("Resultado: " + resultado)

// Forward y Backward
Forward(10)
Backward(5)

// Turn y Move
Turn(90)
Move(20, 20)

// DrawLine
Color("red")
DrawLine(1, 0, 15)
DrawLine(0, 1, 15)

// DrawCircle
Color("green")
DrawCircle(0, 1, 8)

// DrawRectangle
Color("purple")
DrawRectangle(0, 0, 10, 8)

// Funciones de consulta
Set posX = GetActualX()
Set posY = GetActualY()
Set canvasSize = GetCanvasSize()
Print("Posición: " + posX + ", " + posY)
Print("Canvas: " + canvasSize)

// Condicionales
If posX > 15
    Color("orange")
    Size(3)
Else
    Color("pink")
    Size(1)
EndIf

// Loops
Loop i = 1 To 3
    Forward(5)
    Turn(120)
EndLoop

// Funciones definidas por el usuario
Func dibujarCuadrado(tamaño)
    Loop i = 1 To 4
        Forward(tamaño)
        Turn(90)
    EndLoop
EndFunc

Color("cyan")
Call dibujarCuadrado(8)

// Rand
Set numero = Rand(1, 10)
Print("Número aleatorio: " + numero)

// Fill
Color("yellow")
Move(30, 30)
Fill()

Print("¡Demostración completada!")
```

## 9. Árbol Fractal
Demuestra: recursión, funciones, condicionales

```pixelwalle
Spawn(25, 45)
Color("brown")
Size(2)

Func rama(longitud, angulo)
    If longitud > 2
        Forward(longitud)
        Turn(angulo)
        Color("green")
        Size(1)
        Call rama(longitud - 3, angulo)
        Turn(-angulo * 2)
        Call rama(longitud - 3, angulo)
        Turn(angulo)
        Backward(longitud)
    EndIf
EndFunc

Call rama(15, 30)
```

## 10. Mariposa
Demuestra: simetría, colores, `DrawCircle`

```pixelwalle
Spawn(25, 25)
Color("purple")
Size(1)

// Ala izquierda
Turn(-45)
Loop i = 1 To 5
    DrawCircle(0, 1, 3)
    Turn(18)
    Color("pink")
    DrawCircle(0, 1, 2)
    Turn(18)
    Color("purple")
EndLoop

// Ala derecha
Spawn(25, 25)
Turn(45)
Loop i = 1 To 5
    DrawCircle(0, 1, 3)
    Turn(-18)
    Color("pink")
    DrawCircle(0, 1, 2)
    Turn(-18)
    Color("purple")
EndLoop

// Cuerpo
Spawn(25, 25)
Color("black")
Size(2)
Forward(8)
```

## 11. Sol y Nubes
Demuestra: `DrawCircle`, `Fill`, `Forward`

```pixelwalle
// Sol
Spawn(40, 10)
Color("yellow")
Size(1)
DrawCircle(0, 0, 5)

// Nube 1
Spawn(15, 15)
Color("white")
DrawCircle(0, 0, 3)
DrawCircle(1, 0, 2)
DrawCircle(-1, 0, 2)
DrawCircle(0, 1, 2)

// Nube 2
Spawn(35, 20)
Color("white")
DrawCircle(0, 0, 2)
DrawCircle(1, 0, 3)
DrawCircle(-1, 0, 2)
DrawCircle(0, -1, 2)

// Rayos del sol
Spawn(40, 10)
Color("orange")
Size(1)
Loop i = 1 To 8
    Forward(8)
    Backward(8)
    Turn(45)
EndLoop
```

## 12. Patrón de Mosaico
Demuestra: `DrawRectangle`, loops anidados, funciones

```pixelwalle
Spawn(5, 5)
Color("blue")
Size(1)

Func cuadradoMosaico(x, y, color)
    Move(x, y)
    Color(color)
    DrawRectangle(0, 0, 8, 8)
EndFunc

Loop fila = 0 To 5
    Loop col = 0 To 6
        If (fila + col) % 2 == 0
            Call cuadradoMosaico(col * 8, fila * 8, "red")
        Else
            Call cuadradoMosaico(col * 8, fila * 8, "green")
        EndIf
    EndLoop
EndLoop

// Borde
Spawn(0, 0)
Color("black")
Size(2)
DrawRectangle(0, 0, 48, 48)
```

## 13. Funciones de Consulta
Demuestra: `GetColorCount`, `IsBrushColor`, `IsBrushSize`, `IsCanvasColor`

```pixelwalle
Spawn(10, 10)
Color("red")
Size(2)
Print("Demostración de funciones de consulta")

// Dibujar área de prueba
DrawRectangle(0, 0, 20, 15)
Fill()

// Consultar colores
Set rojos = GetColorCount("red", 0, 0, 20, 15)
Print("Píxeles rojos: " + rojos)

// Verificar color del pincel
If IsBrushColor("red")
    Print("El pincel es rojo")
Else
    Print("El pincel NO es rojo")
EndIf

// Cambiar color y verificar
Color("blue")
If IsBrushColor("blue")
    Print("El pincel es azul")
EndIf

// Verificar tamaño del pincel
If IsBrushSize(2)
    Print("El pincel tiene tamaño 2")
EndIf

// Cambiar tamaño y verificar
Size(3)
If IsBrushSize(3)
    Print("El pincel tiene tamaño 3")
EndIf

// Verificar color del canvas
Move(5, 5)
If IsCanvasColor("red")
    Print("El pixel en (5,5) es rojo")
EndIf

// Dibujar algo azul y verificar
Color("blue")
Move(25, 25)
DrawRectangle(0, 0, 5, 5)
If IsCanvasColor("blue")
    Print("El pixel en (25,25) es azul")
EndIf
```

## 14. Juego de Adivinanza
Demuestra: `Rand`, condicionales múltiples

```pixelwalle
Spawn(25, 25)
Color("purple")
Size(2)
Print("Juego de Adivinanza - Dibuja según el número")

Set numero = Rand(1, 5)
Print("Número aleatorio: " + numero)

If numero == 1
    Color("red")
    DrawCircle(0, 0, 10)
    Print("Dibujé un círculo rojo")
Else If numero == 2
    Color("blue")
    DrawRectangle(0, 0, 15, 10)
    Print("Dibujé un rectángulo azul")
Else If numero == 3
    Color("green")
    Loop i = 1 To 4
        Forward(10)
        Turn(90)
    EndLoop
    Print("Dibujé un cuadrado verde")
Else If numero == 4
    Color("yellow")
    Loop i = 1 To 5
        Forward(8)
        Turn(72)
    EndLoop
    Print("Dibujé una estrella amarilla")
Else
    Color("orange")
    Loop i = 1 To 6
        Forward(6)
        Turn(60)
    EndLoop
    Print("Dibujé un hexágono naranja")
EndIf
```

## 15. Animación Simulada
Demuestra: loops, movimiento, múltiples objetos

```pixelwalle
Spawn(5, 25)
Color("red")
Size(1)
Print("Animación de un objeto moviéndose")

// Objeto que se mueve
Loop frame = 1 To 20
    Set x = frame * 2
    Move(x, 25)
    DrawCircle(0, 0, 2)
    Color("blue")
    DrawCircle(0, 0, 1)
    Color("red")
EndLoop

// Efecto de rebote
Spawn(45, 25)
Color("green")
Loop frame = 1 To 10
    Set y = 25 - frame
    Move(45, y)
    DrawCircle(0, 0, 1)
EndLoop

Loop frame = 1 To 10
    Set y = 15 + frame
    Move(45, y)
    DrawCircle(0, 0, 1)
EndLoop

Print("¡Animación completada!")
```

## 16. Etiquetas y GoTo
Demuestra: control de flujo con etiquetas y `GoTo`

```pixelwalle
Spawn(25, 25)
Color("purple")
Size(2)
Print("Etiquetas y GoTo")

// Definir etiquetas
Label inicio
Label fin

// Usar etiquetas
Forward(10)
If GetActualX() > 15
    GoTo fin
EndIf

// Dibujar algo rojo
Color("red")
DrawCircle(0, 0, 10)

GoTo inicio

Label fin

Print("¡Demostración completada!")
```

## Colores Disponibles

El lenguaje soporta los siguientes colores:
- `red` (rojo)
- `green` (verde)
- `blue` (azul)
- `black` (negro)
- `white` (blanco)
- `yellow` (amarillo)
- `orange` (naranja)
- `purple` (púrpura)
- `brown` (marrón)
- `gray` (gris)
- `pink` (rosa)
- `cyan` (cian)
- `magenta` (magenta)

## Funciones Disponibles

### Funciones de Dibujo
- `Spawn(x, y)` - Posiciona el pincel en las coordenadas especificadas
- `Color(colorName)` - Cambia el color del pincel
- `Size(size)` - Cambia el grosor del pincel
- `Forward(distance)` - Mueve el pincel hacia adelante dibujando
- `Backward(distance)` - Mueve el pincel hacia atrás dibujando
- `Turn(degrees)` - Gira el pincel en grados
- `Move(x, y)` - Mueve el pincel sin dibujar
- `DrawLine(dirX, dirY, distance)` - Dibuja una línea en dirección específica
- `DrawCircle(dirX, dirY, radius)` - Dibuja un círculo
- `DrawRectangle(dx, dy, width, height)` - Dibuja un rectángulo
- `Fill()` - Rellena un área con el color actual

### Funciones de Consulta
- `GetActualX()` - Obtiene la posición X actual del pincel
- `GetActualY()` - Obtiene la posición Y actual del pincel
- `GetCanvasSize()` - Obtiene el tamaño del canvas como lista [ancho, alto]
- `GetColorCount(color, x1, y1, x2, y2)` - Cuenta píxeles de un color en una región
- `IsBrushColor(color)` - Verifica si el pincel tiene un color específico
- `IsBrushSize(size)` - Verifica si el pincel tiene un tamaño específico
- `IsCanvasColor(color)` - Verifica si el pixel actual tiene un color específico

### Funciones de Control
- `Rand(min, max)` - Genera un número aleatorio entre min y max
- `Print(expression)` - Imprime un valor en la consola

### Estructuras de Control
- `If condition ... Else ... EndIf` - Condicionales
- `Loop variable = from To to ... EndLoop` - Bucles
- `Label name` - Define una etiqueta
- `GoTo label` - Salta a una etiqueta
- `Func name(params) ... EndFunc` - Define una función
- `Call name(args)` - Llama una función

### Operadores
- Aritméticos: `+`, `-`, `*`, `/`, `%`, `^`
- Comparación: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Lógicos: `AND`, `OR`, `NOT` 