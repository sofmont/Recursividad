using System.Data;
using System.Drawing.Drawing2D;

namespace Tarea23_HanoiLaberinto;

/// <summary>Panel con doble búfer para dibujar sin parpadeo.</summary>
public class PanelDibujo : Panel
{
    public PanelDibujo() { DoubleBuffered = true; ResizeRedraw = true; }
}

public class FormPrincipal : Form
{
    // ---------- Hanoi ----------
    private readonly PanelDibujo pnlHanoi = new();
    private readonly NumericUpDown nudDiscos = new();
    private readonly NumericUpDown nudVelocidad = new();
    private readonly Button btnHanoi = new();
    private readonly Button btnHanoiReset = new();
    private readonly Label lblHanoi = new();
    private readonly System.Windows.Forms.Timer timerHanoi = new();
    private List<int>[] torres = { new(), new(), new() };
    private List<(int origen, int destino)> movs = new();
    private int movActual;

    // ---------- Laberinto ----------
    private readonly PanelDibujo pnlLab = new();
    private readonly Button btnLab = new();
    private readonly Button btnLabLimpiar = new();
    private readonly Button btnLabReset = new();
    private readonly Label lblLab = new();
    private LaberintoSolver lab = new(LaberintoSolver.MapaInicial);
    private const int Celda = 44;

    // ---------- Mediciones ----------
    private readonly DataGridView grid = new();
    private readonly Button btnMedHanoi = new();
    private readonly Button btnMedProf = new();
    private readonly Label lblMed = new();

    public FormPrincipal()
    {
        Text = "Tarea 2.3 - Implementación de casos recursivos (Hanoi y Laberinto)";
        ClientSize = new Size(900, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var tabH = new TabPage("Torres de Hanoi");
        var tabL = new TabPage("Laberinto (backtracking)");
        var tabM = new TabPage("Mediciones");
        tabs.TabPages.AddRange(new[] { tabH, tabL, tabM });
        Controls.Add(tabs);

        ConstruirHanoi(tabH);
        ConstruirLaberinto(tabL);
        ConstruirMediciones(tabM);
        ReiniciarHanoi();
    }

    // =====================================================================
    // HANOI
    // =====================================================================
    private void ConstruirHanoi(TabPage tab)
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 90 };
        top.Controls.Add(new Label { Text = "Discos (1-10):", Location = new Point(10, 14), AutoSize = true });
        nudDiscos.Location = new Point(115, 10); nudDiscos.Width = 55;
        nudDiscos.Minimum = 1; nudDiscos.Maximum = 10; nudDiscos.Value = 4;
        nudDiscos.ValueChanged += (_, _) => ReiniciarHanoi();
        top.Controls.Add(new Label { Text = "Velocidad (ms/mov):", Location = new Point(190, 14), AutoSize = true });
        nudVelocidad.Location = new Point(345, 10); nudVelocidad.Width = 70;
        nudVelocidad.Minimum = 10; nudVelocidad.Maximum = 2000; nudVelocidad.Increment = 50; nudVelocidad.Value = 300;

        btnHanoi.Text = "Resolver"; btnHanoi.Location = new Point(440, 8); btnHanoi.Size = new Size(110, 30);
        btnHanoi.Click += (_, _) => IniciarHanoi();
        btnHanoiReset.Text = "Reiniciar"; btnHanoiReset.Location = new Point(560, 8); btnHanoiReset.Size = new Size(110, 30);
        btnHanoiReset.Click += (_, _) => ReiniciarHanoi();

        lblHanoi.Location = new Point(10, 50); lblHanoi.AutoSize = true;
        top.Controls.AddRange(new Control[] { nudDiscos, nudVelocidad, btnHanoi, btnHanoiReset, lblHanoi });

        pnlHanoi.Dock = DockStyle.Fill;
        pnlHanoi.BackColor = Color.White;
        pnlHanoi.Paint += PintarHanoi;
        tab.Controls.Add(pnlHanoi);
        tab.Controls.Add(top);

        timerHanoi.Tick += (_, _) => PasoHanoi();
    }

    private void ReiniciarHanoi()
    {
        timerHanoi.Stop();
        int n = (int)nudDiscos.Value;
        torres = new[] { new List<int>(), new List<int>(), new List<int>() };
        for (int i = n; i >= 1; i--) torres[0].Add(i);
        movs = new(); movActual = 0;
        btnHanoi.Enabled = true; nudDiscos.Enabled = true;
        lblHanoi.Text = $"Listo. Mínimo de movimientos para {n} discos: {(1 << n) - 1}";
        pnlHanoi.Invalidate();
    }

    private void IniciarHanoi()
    {
        ReiniciarHanoi();
        int n = (int)nudDiscos.Value;
        var h = new HanoiSolver(true);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        h.Resolver(n);                                   // resolución recursiva
        sw.Stop();
        movs = h.Movs;
        lblHanoi.Text = $"Movimientos: {h.Movimientos}   Llamadas recursivas: {h.Llamadas}   " +
                        $"Profundidad máx. de pila: {h.ProfMax}   Tiempo de cálculo: {sw.Elapsed.TotalMilliseconds:F3} ms";
        btnHanoi.Enabled = false; nudDiscos.Enabled = false;
        timerHanoi.Interval = (int)nudVelocidad.Value;
        timerHanoi.Start();
    }

    private void PasoHanoi()
    {
        if (movActual >= movs.Count)
        {
            timerHanoi.Stop();
            btnHanoi.Enabled = true; nudDiscos.Enabled = true;
            return;
        }
        var (o, d) = movs[movActual++];
        int disco = torres[o][^1];
        torres[o].RemoveAt(torres[o].Count - 1);
        torres[d].Add(disco);
        pnlHanoi.Invalidate();
    }

    private void PintarHanoi(object? s, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int w = pnlHanoi.Width, h = pnlHanoi.Height;
        int baseY = h - 50, altoDisco = 22;
        int n = (int)nudDiscos.Value;
        int maxAncho = w / 3 - 30;
        string[] nombres = { "A (origen)", "B (auxiliar)", "C (destino)" };

        for (int t = 0; t < 3; t++)
        {
            int cx = w / 6 + t * (w / 3);
            g.FillRectangle(Brushes.SaddleBrown, cx - maxAncho / 2, baseY, maxAncho, 8);
            g.FillRectangle(Brushes.SaddleBrown, cx - 4, baseY - (n + 1) * altoDisco, 8, (n + 1) * altoDisco);
            var sz = g.MeasureString(nombres[t], Font);
            g.DrawString(nombres[t], Font, Brushes.Black, cx - sz.Width / 2, baseY + 14);
            for (int i = 0; i < torres[t].Count; i++)
            {
                int disco = torres[t][i];
                int ancho = 30 + (maxAncho - 30) * disco / n;
                var rect = new Rectangle(cx - ancho / 2, baseY - (i + 1) * altoDisco, ancho, altoDisco - 2);
                using var br = new SolidBrush(ColorFromHsv(disco * 360.0 / (n + 1), 0.65, 0.9));
                g.FillRectangle(br, rect);
                g.DrawRectangle(Pens.Black, rect);
                g.DrawString(disco.ToString(), Font, Brushes.Black, cx - 6, rect.Y + 1);
            }
        }
    }

    private static Color ColorFromHsv(double h, double s, double v)
    {
        int hi = (int)(h / 60) % 6;
        double f = h / 60 - Math.Floor(h / 60);
        int V = (int)(v * 255), p = (int)(v * (1 - s) * 255), q = (int)(v * (1 - f * s) * 255), t = (int)(v * (1 - (1 - f) * s) * 255);
        return hi switch
        {
            0 => Color.FromArgb(V, t, p), 1 => Color.FromArgb(q, V, p), 2 => Color.FromArgb(p, V, t),
            3 => Color.FromArgb(p, q, V), 4 => Color.FromArgb(t, p, V), _ => Color.FromArgb(V, p, q)
        };
    }

    // =====================================================================
    // LABERINTO
    // =====================================================================
    private void ConstruirLaberinto(TabPage tab)
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 90 };
        btnLab.Text = "Resolver (backtracking)"; btnLab.Location = new Point(10, 8); btnLab.Size = new Size(190, 30);
        btnLab.Click += (_, _) => ResolverLaberinto();
        btnLabLimpiar.Text = "Limpiar camino"; btnLabLimpiar.Location = new Point(210, 8); btnLabLimpiar.Size = new Size(140, 30);
        btnLabLimpiar.Click += (_, _) => { lab.LimpiarResultado(); lblLab.Text = "Haz clic en las celdas para poner/quitar paredes."; pnlLab.Invalidate(); };
        btnLabReset.Text = "Laberinto original"; btnLabReset.Location = new Point(360, 8); btnLabReset.Size = new Size(160, 30);
        btnLabReset.Click += (_, _) => { lab = new LaberintoSolver(LaberintoSolver.MapaInicial); lblLab.Text = "Laberinto restablecido."; pnlLab.Invalidate(); };
        lblLab.Location = new Point(10, 50); lblLab.AutoSize = true;
        lblLab.Text = "Haz clic en las celdas para poner/quitar paredes (S = inicio, E = meta).";
        top.Controls.AddRange(new Control[] { btnLab, btnLabLimpiar, btnLabReset, lblLab });

        pnlLab.Dock = DockStyle.Fill;
        pnlLab.BackColor = Color.White;
        pnlLab.Paint += PintarLaberinto;
        pnlLab.MouseDown += (_, e) =>
        {
            int c = e.X / Celda, f = e.Y / Celda;
            if (f < 0 || f >= lab.Filas || c < 0 || c >= lab.Cols) return;
            if ((f, c) == lab.Inicio || (f, c) == lab.Fin) return;
            lab.Mapa[f, c] = lab.Mapa[f, c] == '#' ? '.' : '#';
            lab.LimpiarResultado();
            pnlLab.Invalidate();
        };
        tab.Controls.Add(pnlLab);
        tab.Controls.Add(top);
    }

    private void ResolverLaberinto()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool ok = lab.Resolver();
        sw.Stop();
        lblLab.Text = ok
            ? $"Camino de {lab.Camino.Count} celdas | llamadas: {lab.Llamadas} | profundidad máx.: {lab.ProfMax} | retrocesos: {lab.Retrocesos} | {sw.Elapsed.TotalMilliseconds:F3} ms"
            : $"SIN SOLUCIÓN | llamadas: {lab.Llamadas} | profundidad máx.: {lab.ProfMax} | retrocesos: {lab.Retrocesos}";
        pnlLab.Invalidate();
    }

    private void PintarLaberinto(object? s, PaintEventArgs e)
    {
        var g = e.Graphics;
        var fuente = new Font("Segoe UI", 12f, FontStyle.Bold);
        for (int f = 0; f < lab.Filas; f++)
            for (int c = 0; c < lab.Cols; c++)
            {
                var r = new Rectangle(c * Celda, f * Celda, Celda, Celda);
                Brush fondo = Brushes.White;
                if (lab.Mapa[f, c] == '#') fondo = Brushes.DimGray;
                else if (lab.Camino.Contains((f, c))) fondo = Brushes.LightGreen;
                else if (lab.Explorados.Contains((f, c))) fondo = Brushes.Khaki;   // explorada y descartada
                g.FillRectangle(fondo, r);
                g.DrawRectangle(Pens.LightGray, r);
                if ((f, c) == lab.Inicio) g.DrawString("S", fuente, Brushes.Blue, r.X + 12, r.Y + 10);
                if ((f, c) == lab.Fin) g.DrawString("E", fuente, Brushes.Red, r.X + 12, r.Y + 10);
            }
        g.DrawString("Verde = camino final   Amarillo = explorado y descartado (backtracking)",
                     Font, Brushes.Black, 5, lab.Filas * Celda + 8);
    }

    // =====================================================================
    // MEDICIONES
    // =====================================================================
    private void ConstruirMediciones(TabPage tab)
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 90 };
        btnMedHanoi.Text = "Medir Hanoi (n = 5, 10, 15, 20, 22)"; btnMedHanoi.Location = new Point(10, 8); btnMedHanoi.Size = new Size(290, 30);
        btnMedHanoi.Click += async (_, _) => await MedirHanoiAsync();
        btnMedProf.Text = "Medir profundidad de llamada"; btnMedProf.Location = new Point(310, 8); btnMedProf.Size = new Size(260, 30);
        btnMedProf.Click += async (_, _) => await MedirProfundidadAsync();
        lblMed.Location = new Point(10, 48); lblMed.AutoSize = true;
        lblMed.Text = "Selecciona las celdas y presiona Ctrl+C para copiar la tabla a tu reporte.";
        top.Controls.AddRange(new Control[] { btnMedHanoi, btnMedProf, lblMed });

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
        tab.Controls.Add(grid);
        tab.Controls.Add(top);
    }

    private async Task MedirHanoiAsync()
    {
        btnMedHanoi.Enabled = btnMedProf.Enabled = false;
        lblMed.Text = "Midiendo... (n = 22 puede tardar unos segundos)";
        var tabla = new DataTable();
        foreach (var col in new[] { "Discos (n)", "Movimientos", "Llamadas", "Prof. máx. pila", "Tiempo (s)" }) tabla.Columns.Add(col);
        foreach (int n in new[] { 5, 10, 15, 20, 22 })
        {
            var f = await Task.Run(() => Medidor.MedirHanoi(n));
            tabla.Rows.Add(f.N, f.Movs, f.Llamadas, f.ProfMax, f.Segundos.ToString("F5"));
            grid.DataSource = tabla;
        }
        lblMed.Text = "Movimientos = 2^n - 1; profundidad de pila = n + 1 (crece solo linealmente).";
        btnMedHanoi.Enabled = btnMedProf.Enabled = true;
    }

    private async Task MedirProfundidadAsync()
    {
        btnMedHanoi.Enabled = btnMedProf.Enabled = false;
        lblMed.Text = "Midiendo con un hilo de pila de 1 MB...";
        var tabla = new DataTable();
        foreach (var col in new[] { "Profundidad", "Tiempo (s)", "Resultado" }) tabla.Columns.Add(col);
        foreach (int p in new[] { 100, 1000, 5000, 10000, 20000, 50000, 100000 })
        {
            var f = await Task.Run(() => Medidor.MedirProfundidad(p));
            tabla.Rows.Add(f.Profundidad, f.Segundos.ToString("F5"), f.Resultado);
            grid.DataSource = tabla;
        }
        lblMed.Text = "En .NET el StackOverflow real cierra el programa; se usa EnsureSufficientExecutionStack() para capturarlo.";
        btnMedHanoi.Enabled = btnMedProf.Enabled = true;
    }
}
