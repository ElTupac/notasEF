using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Notas.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Notas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly NotasContext db;

        public HomeController(ILogger<HomeController> logger,
            NotasContext contexto)
        {
            this.logger = logger;
            this.db = contexto;
        }

        [HttpPost]
        public IActionResult Login(string email, string username)
        {
            Usuario userCheck = db.Usuario.FirstOrDefault(n => (n.Mail == email));
            if(userCheck != null)
            {
                //checkear que sea el mismo usuario
                if(userCheck.Nombre == username)
                {
                    //Agregar el session
                    AgregarUsuarioALaSession(email, username);

                    List<Nota> notas = db.Nota.ToList();

                    var filtro = new List<Nota>();
                    foreach(var nota in notas){
                        if(nota.CreadorMail == email)
                        {
                            filtro.Add(nota);
                        }
                    };
                    return View("Notas", filtro);
                }
                else
                {
                    //No es el mismo usuario que registro ese mail
                    ViewBag.badLogin = true;
                    return View("Index");
                }
            }
            else
            {
                //crear el usuario
                Usuario nuevoUsuario = new Usuario(){
                    Mail = email,
                    Nombre = username
                };
                db.Usuario.Add(nuevoUsuario);
                db.SaveChanges();

                AgregarUsuarioALaSession(email, username);

                var filtro = new List<Nota>();

                return View("Notas", filtro);
            }
        }

        public IActionResult Index()
        {
            Usuario usuario = HttpContext.Session.Get<Usuario>("UsuarioLogueado");
            if(usuario != null)
            {
                List<Nota> notas = db.Nota.ToList();
                var filtro = new List<Nota>();
                foreach(var nota in notas){
                    if(nota.CreadorMail == usuario.Mail)
                    {
                        filtro.Add(nota);
                    }
                };
                return View("Notas", filtro);
            }
            else
            {
                return View();
            }
        }

        public JsonResult ConsultarNotas()
        {
            return Json(db.Nota.ToList());
        }

        [HttpPost]
        public IActionResult CrearNota(string titulo, string texto)
        {
            Usuario usuario = HttpContext.Session.Get<Usuario>("UsuarioLogueado");
            if(usuario != null)
            {       
                Usuario usuarioBase = db.Usuario.FirstOrDefault(u => u.Mail.Equals(usuario.Mail));
                Nota nuevaNota = new Nota{
                    Titulo = titulo,
                    Cuerpo = (texto != null) ? texto : "",
                    CreadorMail = usuarioBase.Mail
                };

                db.Nota.Add(nuevaNota);
                db.SaveChanges();

                List<Nota> notas = db.Nota.ToList();

                var filtro = new List<Nota>();
                foreach(var nota in notas){
                    if(nota.CreadorMail == usuarioBase.Mail)
                    {
                            filtro.Add(nota);
                    }
                };
                return View("Notas", filtro);
            }
            else
            {
                return View("Index");
            }
            
        }

        public JsonResult AgregarUsuarioALaSession(string mail, string nombre)
        {
            Usuario nuevoUsuario = new Usuario{
                Mail = mail, 
                Nombre = nombre
            };

            HttpContext.Session.Set<Usuario>("UsuarioLogueado", nuevoUsuario);
            return Json(nuevoUsuario);
        }

        public JsonResult ConsultarUsuarioEnSesion()
        {
            Usuario usuario = HttpContext.Session.Get<Usuario>("UsuarioLogueado");
            return Json(usuario);
        }

        public IActionResult SacarUsuarioEnSesion()
        {
            HttpContext.Session.Remove("UsuarioLogueado");
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
