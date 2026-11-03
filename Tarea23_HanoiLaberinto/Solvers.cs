using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tarea23_HanoiLaberinto;

/// <summary>Torres de Hanoi recursivo con métricas (movimientos, llamadas, profundidad).</summary>
public class HanoiSolver
{
    public List<(int origen, int destino)> Movs { get; } = new();
    public long Movimientos { get; private set; }
    public long Llamadas { get; private set; }
    public int ProfMax { get; private set; }
    private readonly bool _guardar;

    public HanoiSolver(bool guardarMovimientos) => _guardar = guardarMovimientos;

    public void Resolver(int n) => Resolver(n, 0, 1, 2, 1);

    private void Resolver(int k, int o, int a, int d, int prof)
    {
        Llamadas++;
        if (prof > ProfMax) ProfMax = prof;
        if (k == 0) return;                       // CASO BASE: sin discos
        Resolver(k - 1, o, d, a, prof + 1);       // CASO RECURSIVO: k-1 a auxiliar
        Movimientos++;
        if (_guardar) Movs.Add((o, d));           // mover disco mayor
        Resolver(k - 1, a, o, d, prof + 1);       // k-1 a destino
    }
}

/// <summary>Laberinto resuelto con recursividad y backtracking.</summary>
public class LaberintoSolver
{
    public static readonly string[] MapaInicial =
    {
        "S.#.......",
        ".##.#####.",
        "....#...#.",
        ".####.#.#.",
        "......#...",
        "#####.###.",
        "......#...",
        ".####.#.##",
        "......#..E",
    };

    public char[,] Mapa { get; private set; }
    public int Filas { get; }
    public int Cols { get; }
    public (int f, int c) Inicio { get; private set; }
    public (int f, int c) Fin { get; private set; }
    public List<(int f, int c)> Camino { get; private set; } = new();
    public HashSet<(int f, int c)> Explorados { get; private set; } = new();
    public long Llamadas { get; private set; }
    public int ProfMax { get; private set; }
    public int Retrocesos { get; private set; }

    public LaberintoSolver(string[] mapa)
    {
        Filas = mapa.Length;
        Cols = mapa[0].Length;
        Mapa = new char[Filas, Cols];
        for (int i = 0; i < Filas; i++)
            for (int j = 0; j < Cols; j++)
            {
                Mapa[i, j] = mapa[i][j];
                if (mapa[i][j] == 'S') Inicio = (i, j);
                if (mapa[i][j] == 'E') Fin = (i, j);
            }
    }

    public void LimpiarResultado()
    {
        Camino = new();
        Explorados = new();
        Llamadas = 0; ProfMax = 0; Retrocesos = 0;
    }

    public bool Resolver()
    {
        LimpiarResultado();
        return Resolver(Inicio, 1);
    }

    private bool Resolver((int f, int c) pos, int prof)
    {
        Llamadas++;
        if (prof > ProfMax) ProfMax = prof;
        var (f, c) = pos;
        if (f < 0 || f >= Filas || c < 0 || c >= Cols) return false;   // fuera de límites
        if (Mapa[f, c] == '#' || Explorados.Contains(pos)) return false; // pared o ya visitada

        Explorados.Add(pos);
        Camino.Add(pos);
        if (pos == Fin) return true;                                     // CASO BASE: meta

        (int df, int dc)[] dirs = { (0, 1), (1, 0), (0, -1), (-1, 0) };
        foreach (var (df, dc) in dirs)                                   // CASO RECURSIVO
            if (Resolver((f + df, c + dc), prof + 1)) return true;

        Camino.RemoveAt(Camino.Count - 1);                               // BACKTRACKING
        Retrocesos++;
        return false;
    }
}

/// <summary>Mediciones de tiempo y de profundidad de llamada.</summary>
public static class Medidor
{
    public record FilaHanoi(int N, long Movs, long Llamadas, int ProfMax, double Segundos);
    public record FilaProf(int Profundidad, double Segundos, string Resultado);

    public static FilaHanoi MedirHanoi(int n)
    {
        var h = new HanoiSolver(false);
        var sw = Stopwatch.StartNew();
        h.Resolver(n);
        sw.Stop();
        return new FilaHanoi(n, h.Movimientos, h.Llamadas, h.ProfMax, sw.Elapsed.TotalSeconds);
    }

    // En .NET un StackOverflow NO se puede capturar y termina el proceso.
    // EnsureSufficientExecutionStack() lanza una excepción capturable ANTES de que se agote la pila.
    private static int Cuenta(int n)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        return n == 0 ? 0 : 1 + Cuenta(n - 1);
    }

    public static FilaProf MedirProfundidad(int profundidad, int pilaBytes = 1024 * 1024)
    {
        string resultado = "OK";
        double seg = 0;
        // Hilo con tamaño de pila conocido (1 MB, igual que el hilo principal por defecto)
        var hilo = new Thread(() =>
        {
            var sw = Stopwatch.StartNew();
            try { Cuenta(profundidad); }
            catch (InsufficientExecutionStackException) { resultado = "Desbordamiento de pila (capturado)"; }
            sw.Stop();
            seg = sw.Elapsed.TotalSeconds;
        }, pilaBytes);
        hilo.Start();
        hilo.Join();
        return new FilaProf(profundidad, seg, resultado);
    }
}
