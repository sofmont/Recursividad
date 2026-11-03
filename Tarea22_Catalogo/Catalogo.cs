using System.Text;

namespace Tarea22_Catalogo;

/// <summary>Registra las llamadas recursivas para mostrar el comportamiento de la pila (call stack).</summary>
public class Tracer
{
    public StringBuilder Sb { get; } = new();
    public int Llamadas { get; private set; }
    public int ProfundidadMax { get; private set; }
    private int _lineas;
    private const int MaxLineas = 3000;

    public void Entrar(string texto, int prof)
    {
        Llamadas++;
        ProfundidadMax = Math.Max(ProfundidadMax, prof);
        Agregar(new string(' ', (prof - 1) * 2) + "-> " + texto);
    }

    public void Salir(string texto, int prof) =>
        Agregar(new string(' ', (prof - 1) * 2) + "<- " + texto);

    private void Agregar(string linea)
    {
        if (_lineas < MaxLineas) Sb.AppendLine(linea);
        else if (_lineas == MaxLineas) Sb.AppendLine("... (traza truncada)");
        _lineas++;
    }
}

/// <summary>Tarea 2.2 - Catálogo: versión iterativa y recursiva de cada ejercicio.</summary>
public static class Catalogo
{
    // 1. FACTORIAL -----------------------------------------------------------
    public static long FactorialIter(int n)
    {
        long r = 1;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
    }

    public static long FactorialRec(int n, Tracer? t = null, int prof = 1)
    {
        t?.Entrar($"factorial({n})", prof);
        long r;
        if (n <= 1)                                   // CASO BASE: 0! = 1! = 1
        {
            r = 1;
            t?.Salir("CASO BASE -> retorna 1", prof);
        }
        else                                          // CASO RECURSIVO: n! = n * (n-1)!
        {
            r = n * FactorialRec(n - 1, t, prof + 1);
            t?.Salir($"retorna {n} * factorial({n - 1}) = {r}", prof);
        }
        return r;
    }

    // 2. FIBONACCI -----------------------------------------------------------
    public static long FibonacciIter(int n)
    {
        long a = 0, b = 1;
        for (int i = 0; i < n; i++) (a, b) = (b, a + b);
        return a;
    }

    public static long FibonacciRec(int n, Tracer? t = null, int prof = 1)
    {
        t?.Entrar($"fib({n})", prof);
        long r;
        if (n < 2)                                    // CASO BASE: F(0)=0, F(1)=1
        {
            r = n;
            t?.Salir($"CASO BASE -> retorna {r}", prof);
        }
        else                                          // CASO RECURSIVO
        {
            r = FibonacciRec(n - 1, t, prof + 1) + FibonacciRec(n - 2, t, prof + 1);
            t?.Salir($"retorna fib({n - 1}) + fib({n - 2}) = {r}", prof);
        }
        return r;
    }

    // 3. TORRES DE HANOI -----------------------------------------------------
    /// <summary>Versión iterativa con pila explícita (simula el call stack).</summary>
    public static List<(char, char)> HanoiIter(int n, char origen = 'A', char aux = 'B', char destino = 'C')
    {
        var movs = new List<(char, char)>();
        var pila = new Stack<(int k, char o, char a, char d, bool expandido)>();
        pila.Push((n, origen, aux, destino, false));
        while (pila.Count > 0)
        {
            var (k, o, a, d, exp) = pila.Pop();
            if (k == 0) continue;
            if (exp) movs.Add((o, d));
            else
            {
                pila.Push((k - 1, a, o, d, false));
                pila.Push((k, o, a, d, true));
                pila.Push((k - 1, o, d, a, false));
            }
        }
        return movs;
    }

    public static void HanoiRec(int n, char o, char a, char d, List<(char, char)> movs, Tracer? t = null, int prof = 1)
    {
        t?.Entrar($"hanoi({n}, {o}, {a}, {d})", prof);
        if (n == 0)                                   // CASO BASE: sin discos
        {
            t?.Salir("CASO BASE -> no hace nada", prof);
            return;
        }
        HanoiRec(n - 1, o, d, a, movs, t, prof + 1);  // CASO RECURSIVO: n-1 a auxiliar
        movs.Add((o, d));                             // mover el disco mayor
        t?.Salir($"mueve disco {n}: {o} -> {d}", prof);
        HanoiRec(n - 1, a, o, d, movs, t, prof + 1);  // n-1 a destino
    }

    // 4. SUMA DE ARREGLO -----------------------------------------------------
    public static int SumaIter(int[] arr)
    {
        int total = 0;
        foreach (int x in arr) total += x;
        return total;
    }

    public static int SumaRec(int[] arr, int i = 0, Tracer? t = null, int prof = 1)
    {
        t?.Entrar($"suma(i={i})", prof);
        int r;
        if (i == arr.Length)                          // CASO BASE: no quedan elementos
        {
            r = 0;
            t?.Salir("CASO BASE -> retorna 0", prof);
        }
        else                                          // CASO RECURSIVO
        {
            r = arr[i] + SumaRec(arr, i + 1, t, prof + 1);
            t?.Salir($"retorna arr[{i}]={arr[i]} + suma(i={i + 1}) = {r}", prof);
        }
        return r;
    }

    // 5. BÚSQUEDA LINEAL -----------------------------------------------------
    public static int BusquedaIter(int[] arr, int objetivo)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == objetivo) return i;
        return -1;
    }

    public static int BusquedaRec(int[] arr, int objetivo, int i = 0, Tracer? t = null, int prof = 1)
    {
        t?.Entrar($"busqueda(i={i})", prof);
        int r;
        if (i == arr.Length)                          // CASO BASE 1: no encontrado
        {
            r = -1;
            t?.Salir("CASO BASE (no encontrado) -> retorna -1", prof);
        }
        else if (arr[i] == objetivo)                  // CASO BASE 2: encontrado
        {
            r = i;
            t?.Salir($"CASO BASE (arr[{i}]={arr[i]} == {objetivo}) -> retorna {i}", prof);
        }
        else                                          // CASO RECURSIVO
        {
            r = BusquedaRec(arr, objetivo, i + 1, t, prof + 1);
            t?.Salir($"arr[{i}]={arr[i]} != {objetivo}; retorna {r}", prof);
        }
        return r;
    }
}
