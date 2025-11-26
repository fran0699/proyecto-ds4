using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Proyecto2.Models;

namespace Proyecto2.Controllers
{
    [RoutePrefix("api/operaciones")]

    public class OperacionesController : ApiController
    {
        private readonly RepositorioOperaciones repositorio;

        public OperacionesController()
        {
            // inicialiamos el repositorio para hablar con la BD
            repositorio = new RepositorioOperaciones();
        }

        // GET api/operaciones/todas
        [HttpGet]
        [Route("todas")]
        public IHttpActionResult GetTodas()
        {
            try
            {
                var lista = repositorio.ObtenerTodas();
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/operaciones/sumas
        [HttpGet]
        [Route("sumas")]
        public IHttpActionResult GetSumas()
        {
            try
            {
                var lista = repositorio.ObtenerPorOperador("+");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/operaciones/restas
        [HttpGet]
        [Route("restas")]
        public IHttpActionResult GetRestas()
        {
            try
            {
                var lista = repositorio.ObtenerPorOperador("-");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/operaciones/multiplicaciones
        [HttpGet]
        [Route("multiplicaciones")]
        public IHttpActionResult GetMultiplicaciones()
        {
            try
            {
                var lista = repositorio.ObtenerPorOperador("*");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/operaciones/divisiones
        [HttpGet]
        [Route("divisiones")]
        public IHttpActionResult GetDivisiones()
        {
            try
            {
                var lista = repositorio.ObtenerPorOperador("/");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // EJEMPLO "LIBRE":
        // GET api/operaciones/por-fecha?fecha=2025-11-26
        [HttpGet]
        [Route("por-fecha")]
        public IHttpActionResult GetPorFecha(string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime fechaFiltro))
                {
                    return BadRequest("Fecha inválida. Usa el formato AAAA-MM-DD.");
                }

                var lista = repositorio.ObtenerPorFecha(fechaFiltro);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
