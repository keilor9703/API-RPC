using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Replic
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Algunas bases remotas (11g/12.1) no ofrecen el verificador 12C que el driver exige por defecto (ORA-28040).
            Oracle.ManagedDataAccess.Client.OracleConfiguration.SqlNetAllowedLogonVersionClient =
                Oracle.ManagedDataAccess.Client.OracleAllowedLogonVersionClient.Version11;

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            // En el servidor (docker) definir ASPNETCORE_URLS, p. ej. http://+:80, para que la central pueda alcanzar la API.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
                webBuilder.UseUrls("http://localhost:44334", "https://localhost:443");
        });

    }
}
