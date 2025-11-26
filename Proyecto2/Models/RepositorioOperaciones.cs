using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;

namespace Proyecto2.Models
{
    public class RepositorioOperaciones
    {
        private readonly string cadenaConexion;

        public RepositorioOperaciones()
        {
            cadenaConexion = ConfigurationManager
                .ConnectionStrings["ConexionProyecto1"]
                .ConnectionString;
        }

        private RegistroOperacion MapearRegistro(SqlDataReader lector)
        {
            var registro = new RegistroOperacion();

            // fecha
            if (lector["fecha"] != DBNull.Value)
            {
                registro.Fecha = Convert.ToDateTime(lector["fecha"]);
            }

            // ultima_operacion
            if (lector["ultima_operacion"] != DBNull.Value)
            {
                registro.UltimaOperacion = lector["ultima_operacion"].ToString();
            }

            // resultado
            if (lector["resultado"] != DBNull.Value)
            {
                registro.Resultado = Convert.ToDouble(lector["resultado"]);
            }

            return registro;
        }

        public List<RegistroOperacion> ObtenerTodas()
        {
            var lista = new List<RegistroOperacion>();

            try
            {
                using (var conexion = new SqlConnection(cadenaConexion))
                using (var comando = new SqlCommand(
                    "SELECT fecha, ultima_operacion, resultado FROM Registros",
                    conexion))
                {
                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var registro = MapearRegistro(lector);
                            lista.Add(registro);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los registros: " + ex.Message, ex);
            }

            return lista;
        }

        public List<RegistroOperacion> ObtenerPorOperador(string operador)
        {
            var lista = new List<RegistroOperacion>();

            try
            {
                string patron = $"% {operador} %";

                using (var conexion = new SqlConnection(cadenaConexion))
                using (var comando = new SqlCommand(
                    "SELECT fecha, ultima_operacion, resultado " +
                    "FROM Registros " +
                    "WHERE ultima_operacion LIKE @patron", conexion))
                {
                    comando.Parameters.AddWithValue("@patron", patron);

                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var registro = MapearRegistro(lector);
                            lista.Add(registro);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener registros por operador: " + ex.Message, ex);
            }

            return lista;
        }

        public List<RegistroOperacion> ObtenerPorFecha(DateTime fechaFiltro)
        {
            var lista = new List<RegistroOperacion>();

            try
            {
                using (var conexion = new SqlConnection(cadenaConexion))
                using (var comando = new SqlCommand(
                    "SELECT fecha, ultima_operacion, resultado " +
                    "FROM Registros " +
                    "WHERE fecha = @fecha", conexion))
                {
                    comando.Parameters.AddWithValue("@fecha", fechaFiltro.Date);

                    conexion.Open();
                    using (var lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        {
                            var registro = MapearRegistro(lector);
                            lista.Add(registro);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener registros por fecha: " + ex.Message, ex);
            }

            return lista;
        }
    }
}