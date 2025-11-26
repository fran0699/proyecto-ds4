using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto1
{
    public partial class Form1 : Form
    {
        // ESTADO INTERNO DE LA CALCULADORA 
        private decimal primerNumero = 0m;     // Primer operando
        private string operador = "";          // "+", "-", "*", "/"
        private bool esperandoSegundoNumero = false; // true cuando ya hay operador y falta el 2do número
        private bool huboError = false;        // Para bloquear entradas cuando hay error

        // DB esperada:
        // PROYECTO1 | Tabla: Registros
        // Columnas: fecha date, ultima_operacion varchar(100), resultado float
        private string connectionString =
            @"Server=.\sqlexpress;Database=PROYECTO1;Trusted_Connection=True;";

        public Form1()
        {
            InitializeComponent();
        }

        // UTILIDADES BÁSICAS 
        private void LimpiarTodo()
        {
            primerNumero = 0m;
            operador = "";
            esperandoSegundoNumero = false;
            huboError = false;
            txtDisplay.Clear();
            txtDisplayFront.Clear();
        }

        private void MostrarError(string texto)
        {
            huboError = true;
            txtDisplay.Text = "Error";
            txtDisplayFront.Text = texto;
            operador = "";
            esperandoSegundoNumero = false;
        }

        private void AppendDigito(string d)
        {
            if (huboError) LimpiarTodo();

            // Si acabábamos de poner "=", empezar nueva entrada al escribir un dígito
            if (!esperandoSegundoNumero && operador == "" && txtDisplayFront.Text.EndsWith("="))
            {
                txtDisplayFront.Clear();
                txtDisplay.Clear();
            }

            if (txtDisplay.Text == "0")
            {
                // Reemplaza el 0 inicial
                txtDisplay.Text = d;
            }
            else
            {
                txtDisplay.Text += d;
            }
        }

        private void AppendPunto()
        {
            if (huboError) LimpiarTodo();

            if (string.IsNullOrEmpty(txtDisplay.Text))
            {
                txtDisplay.Text = "0.";
                return;
            }

            // Permitir "-0."
            if (txtDisplay.Text == "-")
            {
                txtDisplay.Text = "-0.";
                return;
            }

            if (!txtDisplay.Text.Contains("."))
                txtDisplay.Text += ".";
        }

        private bool TryLeerDisplay(out decimal valor)
        {
            // Uso cultura invariable para que el "." sea siempre el separador decimal
            return decimal.TryParse(
                txtDisplay.Text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out valor
            );
        }

        private void SetOperador(string op)
        {
            if (huboError)
            {
                LimpiarTodo();
            }

            // Permitir usar "-" como signo negativo:
            if ((op == "-") &&
                ((string.IsNullOrEmpty(operador) && string.IsNullOrEmpty(txtDisplay.Text)) ||
                 (esperandoSegundoNumero && string.IsNullOrEmpty(txtDisplay.Text))))
            {
                txtDisplay.Text = "-";
                return;
            }

            // Si aún no hay operador guardamos el primer número y mostramos en la "línea frontal"
            if (string.IsNullOrEmpty(operador))
            {
                if (!TryLeerDisplay(out primerNumero))
                {
                    // Si el usuario no ha escrito nada, tomamos 0
                    primerNumero = 0m;
                }

                operador = op;
                esperandoSegundoNumero = true;
                txtDisplayFront.Text = primerNumero.ToString(CultureInfo.InvariantCulture) + " " + operador;
                txtDisplay.Clear();
            }
            else
            {
                // Ya había un operador: si el usuario escribió el 2do número, calculamos antes de cambiar operador
                if (!string.IsNullOrEmpty(txtDisplay.Text) && txtDisplay.Text != "-")
                {
                    btnIgual_Click(null, null); // resuelve operación pendiente y guarda en BD
                    operador = op;
                    esperandoSegundoNumero = true;
                    txtDisplayFront.Text = txtDisplay.Text + " " + operador;
                    txtDisplay.Clear();
                }
                else
                {
                    // No hay segundo número: solo reemplazamos el operador en pantalla
                    operador = op;
                    txtDisplayFront.Text = primerNumero.ToString(CultureInfo.InvariantCulture) + " " + operador;
                }
            }
        }

        private decimal Calcular(decimal a, decimal b, string op, out bool ok)
        {
            ok = true;
            switch (op)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "*": return a * b;
                case "/":
                    if (b == 0m)
                    {
                        ok = false;
                        return 0m;
                    }
                    return a / b;
                default:
                    ok = false;
                    return 0m;
            }
        }

        /// <summary>
        /// Guarda en la base de datos:
        /// - la fecha actual (solo fecha, sin hora)
        /// - el texto de la última operación (lo que se muestra en txtDisplayFront)
        /// - el resultado numérico
        /// </summary>
        private void GuardarResultadoEnBD(decimal valor, string descripcionOperacion)
        {
            // Redondeo a 2 decimales por comodidad (puedes quitarlo si no lo quieres)
            decimal valorRedondeado = Math.Round(valor, 2);

            // Solo la parte de fecha (sin hora)
            DateTime fechaActual = DateTime.Now.Date;

            try
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Registros (fecha, ultima_operacion, resultado) " +
                    "VALUES (@fecha, @ultima_operacion, @resultado)", cn))
                {
                    cmd.Parameters.AddWithValue("@fecha", fechaActual);
                    cmd.Parameters.AddWithValue("@ultima_operacion", descripcionOperacion);
                    cmd.Parameters.AddWithValue("@resultado", valorRedondeado);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // No frenamos la app si falló el guardado: solo lo mostramos arriba.
                txtDisplayFront.Text = "No se pudo guardar en BD: " + ex.Message;
            }
        }

        // EVENTOS DE BOTONES: DÍGITOS
        private void btn0_Click(object sender, EventArgs e) { AppendDigito("0"); }
        private void btn1_Click(object sender, EventArgs e) { AppendDigito("1"); }
        private void btn2_Click(object sender, EventArgs e) { AppendDigito("2"); }
        private void btn3_Click(object sender, EventArgs e) { AppendDigito("3"); }
        private void btn4_Click(object sender, EventArgs e) { AppendDigito("4"); }
        private void btn5_Click(object sender, EventArgs e) { AppendDigito("5"); }
        private void btn6_Click(object sender, EventArgs e) { AppendDigito("6"); }
        private void btn7_Click(object sender, EventArgs e) { AppendDigito("7"); }
        private void btn8_Click(object sender, EventArgs e) { AppendDigito("8"); }
        private void btn9_Click(object sender, EventArgs e) { AppendDigito("9"); }

        // EVENTOS DE OPERADORES
        private void btnSumar_Click(object sender, EventArgs e) { SetOperador("+"); }
        private void btnRestar_Click(object sender, EventArgs e) { SetOperador("-"); }
        private void btnMultiplicar_Click(object sender, EventArgs e) { SetOperador("*"); }
        private void btnDividir_Click(object sender, EventArgs e) { SetOperador("/"); }

        // PUNTO DECIMAL 
        private void btnPunto_Click(object sender, EventArgs e) { AppendPunto(); }

        //  CE / C / BORRAR
        private void btnCE_Click(object sender, EventArgs e)
        {
            // Limpia solo lo que está escribiendo ahora
            txtDisplay.Clear();
        }

        private void btnC_Click(object sender, EventArgs e)
        {
            // Limpia todo el estado
            LimpiarTodo();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            if (huboError) { LimpiarTodo(); return; }
            if (!string.IsNullOrEmpty(txtDisplay.Text))
            {
                txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
            }
        }

        // OPERACIONES DE POTENCIADO AL CUADRADO Y RAIZ CUADRADA
        private void btnRaiz_Click(object sender, EventArgs e)
        {
            if (huboError) { LimpiarTodo(); }
            if (!TryLeerDisplay(out decimal x)) x = 0m;

            if (x < 0m)
            {
                MostrarError("Raíz cuadrada: valor negativo");
                return;
            }

            decimal resultado = (decimal)Math.Sqrt((double)x);
            txtDisplayFront.Text = "√(" + x.ToString(CultureInfo.InvariantCulture) + ") =";
            txtDisplay.Text = resultado.ToString(CultureInfo.InvariantCulture);

            // Guardo operación tal cual se ve en la pantalla frontal
            GuardarResultadoEnBD(resultado, txtDisplayFront.Text);
            operador = "";
            esperandoSegundoNumero = false;
        }

        private void btnElevar_Click(object sender, EventArgs e)
        {
            if (huboError) { LimpiarTodo(); }
            if (!TryLeerDisplay(out decimal x)) x = 0m;

            decimal resultado = x * x;
            txtDisplayFront.Text = "(" + x.ToString(CultureInfo.InvariantCulture) + ")^2 =";
            txtDisplay.Text = resultado.ToString(CultureInfo.InvariantCulture);

            // Guardo operación tal cual se ve en la pantalla frontal
            GuardarResultadoEnBD(resultado, txtDisplayFront.Text);
            operador = "";
            esperandoSegundoNumero = false;
        }

        // IGUAL
        private void btnIgual_Click(object sender, EventArgs e)
        {
            if (huboError) { LimpiarTodo(); return; }
            if (string.IsNullOrEmpty(operador)) return;

            // Validar que haya un segundo número y que no sea solo "-"
            if (string.IsNullOrEmpty(txtDisplay.Text) || txtDisplay.Text == "-") return;

            if (!TryLeerDisplay(out decimal segundoNumero))
            {
                MostrarError("Entrada inválida");
                return;
            }

            bool ok;
            decimal resultado = Calcular(primerNumero, segundoNumero, operador, out ok);

            if (!ok)
            {
                MostrarError("Operación inválida (¿división por cero?)");
                return;
            }

            // Muestra "a op b ="
            txtDisplayFront.Text =
                primerNumero.ToString(CultureInfo.InvariantCulture) + " " +
                operador + " " +
                segundoNumero.ToString(CultureInfo.InvariantCulture) + " =";

            txtDisplay.Text = resultado.ToString(CultureInfo.InvariantCulture);

            // Guardo operación tal cual se ve en la pantalla frontal
            GuardarResultadoEnBD(resultado, txtDisplayFront.Text);

            // Dejar el resultado como nuevo primer número por si el usuario continúa
            primerNumero = resultado;
            operador = "";
            esperandoSegundoNumero = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtDisplay.ReadOnly = true;
            txtDisplayFront.ReadOnly = true;
        }
    }
}
