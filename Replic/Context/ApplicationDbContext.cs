using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Replic.Models;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Configuration;
using System;

namespace Replic.Context
{
    
    // Clase DbContext para interactuar con la base de datos
    public class ApplicationDbContext : DbContext
    {
        //BLOQUE PARA CONECTARSE A LA BASE DE DATOS LOCAL, DEBE AJUSTAR EL ARCHIVO APPSETITNGS
        //private readonly IConfiguration _configuration;

        //public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        //    : base(options)
        //{
        //    _configuration = configuration;
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //   // Establece la versión del protocolo de autenticación permitida para el cliente Oracle
        //    OracleConfiguration.SqlNetAllowedLogonVersionClient = OracleAllowedLogonVersionClient.Version11;
        //    // Obtiene la cadena de conexión de la configuración de la aplicación
        //    string connectionString = _configuration.GetConnectionString("SECADMETIB");
        //    // Configura EF Core para usar Oracle con la cadena de conexión obtenida
        //    optionsBuilder.UseOracle(connectionString);
        //}


        // Constructor que recibe opciones de configuración para el contexto
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet que representa la tabla Eventos en la base de datos
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
       

        //// DbSet que representa la tabla EventoOracles en la base de datos
        //public DbSet<EventoOracle> EventoOracles { get; set; }

        // Método para configurar el modelo de datos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Pedido>()
               .HasKey(p => new { p.P_SitioGraba, p.P_NumeLlamada });

         



            // Configuración del modelo para la entidad Evento
            // Definir la clave primaria de la entidad Evento
            modelBuilder.Entity<Evento>()
                .HasKey(p => p.p_sitiograba);
        }
      


    }
}
