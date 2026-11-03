using System.Text;

namespace Tarea22_Catalogo;

public class FormPrincipal : Form
{
    private readonly ComboBox cboEjercicio = new();
    private readonly TextBox txtEntrada = new();
    private readonly TextBox txtObjetivo = new();
    private readonly Label lblEntrada = new();
    private readonly Label lblObjetivo = new();
    private readonly Label lblAyuda = new();
    private readonly Button btnEjecutar = new();
    private readonly CheckBox chkTraza = new();
    private readonly TextBox txtSalida = new();

    private static readonly string[] Ejercicios =
        { "Factorial", "Fibonacci", "Torres de Hanoi", "Suma de arreglo", "Búsqueda lineal" };

    public FormPrincipal()
    {
        Text = "Tarea 2.2 - Catálogo: de iterativo a recursivo";
        ClientSize = new Size(820, 620);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10f);

        var lblEj = new Label { Text = "Ejercicio:", Location = new Point(15, 18), AutoSize = true };
        cboEjercicio.Location = new Point(95, 14);
        cboEjercicio.Width = 200;
        cboEjercicio.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEjercicio.Items.AddRange(Ejercicios);
        cboEjercicio.SelectedIndexChanged += (_, _) => ActualizarEntradas();

        lblEntrada.Location = new Point(15, 58);
        lblEntrada.AutoSize = true;
        txtEntrada.Location = new Point(190, 54);
        txtEntrada.Width = 300;

        lblObjetivo.Text = "Valor a buscar:";
        lblObjetivo.Location = new Point(510, 58);
        lblObjetivo.AutoSize = true;
        txtObjetivo.Location = new Point(630, 54);
        txtObjetivo.Width = 80;

        lblAyuda.Location = new Point(15, 92);
        lblAyuda.AutoSize = true;
        lblAyuda.ForeColor = Color.DimGray;

        chkTraza.Text = "Mostrar traza de la pila de llamadas";
        chkTraza.Checked = true;
        chkTraza.Location = new Point(15, 122);
        chkTraza.AutoSize = true;

        btnEjecutar.Text = "Ejecutar (iterativo vs recursivo)";
        btnEjecutar.Location = new Point(560, 116);
        btnEjecutar.Size = new Size(240, 34);
        btnEjecutar.Click += (_, _) => Ejecutar();

        txtSalida.Location = new Point(15, 165);
        txtSalida.Size = new Size(790, 440);
        txtSalida.Multiline = true;
        txtSalida.ReadOnly = true;
        txtSalida.ScrollBars = ScrollBars.Both;
        txtSalida.WordWrap = false;
        txtSalida.Font = new Font("Consolas", 10f);
        txtSalida.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        Controls.AddRange(new Control[] { lblEj, cboEjercicio, lblEntrada, txtEntrada, lblObjetivo, txtObjetivo,
                                          lblAyuda, chkTraza, btnEjecutar, txtSalida });
        cboEjercicio.SelectedIndex = 0;
    }

    private void ActualizarEntradas()
    {
        bool busqueda = cboEjercicio.SelectedIndex == 4;
        lblObjetivo.Visible = txtObjetivo.Visible = busqueda;
        switch (cboEjercicio.SelectedIndex)
        {
            case 0: lblEntrada.Text = "n (0 a 20):"; txtEntrada.Text = "5";
                lblAyuda.Text = "Caso base: n <= 1 devuelve 1.   Caso recursivo: n * factorial(n-1)."; break;
            case 1: lblEntrada.Text = "n (0 a 30):"; txtEntrada.Text = "6";
                lblAyuda.Text = "Caso base: n < 2 devuelve n.   Caso recursivo: fib(n-1) + fib(n-2)."; break;
            case 2: lblEntrada.Text = "Número de discos (1 a 12):"; txtEntrada.Text = "3";
                lblAyuda.Text = "Caso base: n == 0.   Caso recursivo: mover n-1, mover disco mayor, mover n-1."; break;
            case 3: lblEntrada.Text = "Arreglo (separado por comas):"; txtEntrada.Text = "4, 8, 15, 16, 23, 42";
                lblAyuda.Text = "Caso base: i == longitud devuelve 0.   Caso recursivo: arr[i] + suma(i+1)."; break;
            case 4: lblEntrada.Text = "Arreglo (separado por comas):"; txtEntrada.Text = "4, 8, 15, 16, 23, 42";
                txtObjetivo.Text = "15";
                lblAyuda.Text = "Casos base: i == longitud (-1) o arr[i] == objetivo (i).   Recursivo: busqueda(i+1)."; break;
        }
        txtSalida.Clear();
    }

    private static int[]? LeerArreglo(string texto)
    {
        try
        {
            return texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => int.Parse(s)).ToArray();
        }
        catch { return null; }
    }

    private void Ejecutar()
    {
        var sb = new StringBuilder();
        var t = chkTraza.Checked ? new Tracer() : null;
        try
        {
            switch (cboEjercicio.SelectedIndex)
            {
                case 0:
                {
                    if (!int.TryParse(txtEntrada.Text, out int n) || n < 0 || n > 20) { Error("Ingresa un entero de 0 a 20."); return; }
                    long it = Catalogo.FactorialIter(n), rec = Catalogo.FactorialRec(n, t);
                    sb.AppendLine($"Iterativo : {n}! = {it}");
                    sb.AppendLine($"Recursivo : {n}! = {rec}");
                    sb.AppendLine(it == rec ? "Resultado: AMBAS VERSIONES COINCIDEN" : "Resultado: NO COINCIDEN");
                    break;
                }
                case 1:
                {
                    if (!int.TryParse(txtEntrada.Text, out int n) || n < 0 || n > 30) { Error("Ingresa un entero de 0 a 30."); return; }
                    long it = Catalogo.FibonacciIter(n), rec = Catalogo.FibonacciRec(n, t);
                    sb.AppendLine($"Iterativo : fib({n}) = {it}");
                    sb.AppendLine($"Recursivo : fib({n}) = {rec}");
                    sb.AppendLine(it == rec ? "Resultado: AMBAS VERSIONES COINCIDEN" : "Resultado: NO COINCIDEN");
                    if (t != null) sb.AppendLine($"(la versión recursiva hizo {t.Llamadas} llamadas; la iterativa solo {n} iteraciones)");
                    break;
                }
                case 2:
                {
                    if (!int.TryParse(txtEntrada.Text, out int n) || n < 1 || n > 12) { Error("Ingresa un entero de 1 a 12."); return; }
                    var it = Catalogo.HanoiIter(n);
                    var rec = new List<(char, char)>();
                    Catalogo.HanoiRec(n, 'A', 'B', 'C', rec, t);
                    sb.AppendLine($"Iterativo : {it.Count} movimientos");
                    sb.AppendLine($"Recursivo : {rec.Count} movimientos (2^{n} - 1 = {(1 << n) - 1})");
                    sb.AppendLine(it.SequenceEqual(rec) ? "Resultado: MISMA SECUENCIA DE MOVIMIENTOS" : "Resultado: DIFIEREN");
                    if (n <= 5)
                    {
                        sb.AppendLine();
                        for (int i = 0; i < rec.Count; i++) sb.AppendLine($"  Mov {i + 1,3}: {rec[i].Item1} -> {rec[i].Item2}");
                    }
                    break;
                }
                case 3:
                {
                    var arr = LeerArreglo(txtEntrada.Text);
                    if (arr == null || arr.Length == 0) { Error("Arreglo inválido. Ejemplo: 4, 8, 15"); return; }
                    int it = Catalogo.SumaIter(arr), rec = Catalogo.SumaRec(arr, 0, t);
                    sb.AppendLine($"Iterativo : suma = {it}");
                    sb.AppendLine($"Recursivo : suma = {rec}");
                    sb.AppendLine(it == rec ? "Resultado: AMBAS VERSIONES COINCIDEN" : "Resultado: NO COINCIDEN");
                    break;
                }
                case 4:
                {
                    var arr = LeerArreglo(txtEntrada.Text);
                    if (arr == null || arr.Length == 0 || !int.TryParse(txtObjetivo.Text, out int obj)) { Error("Datos inválidos."); return; }
                    int it = Catalogo.BusquedaIter(arr, obj), rec = Catalogo.BusquedaRec(arr, obj, 0, t);
                    sb.AppendLine($"Iterativo : índice = {it}");
                    sb.AppendLine($"Recursivo : índice = {rec}");
                    sb.AppendLine(it == rec ? "Resultado: AMBAS VERSIONES COINCIDEN" : "Resultado: NO COINCIDEN");
                    break;
                }
            }
            if (t != null)
            {
                sb.AppendLine();
                sb.AppendLine($"=== TRAZA DE LA PILA (llamadas: {t.Llamadas}, profundidad máxima: {t.ProfundidadMax}) ===");
                sb.AppendLine("-> entra a la llamada (se apila)   <- sale (se desapila)");
                sb.Append(t.Sb);
            }
            txtSalida.Text = sb.ToString();
        }
        catch (Exception ex) { Error(ex.Message); }
    }

    private static void Error(string msg) =>
        MessageBox.Show(msg, "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
