using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto2.Models
{
    public class RegistroOperacion
    {
        public DateTime? Fecha { get; set; }
        public string UltimaOperacion { get; set; }
        public double Resultado { get; set; }
    }
}