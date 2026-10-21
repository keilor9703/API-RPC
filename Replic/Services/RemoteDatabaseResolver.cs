using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Replic.Services
{
    /// <summary>
    /// Resuelve el string de conexion de la base remota a partir del dblink/codigo
    /// que devuelve la central. Los strings se leen de la seccion "RemoteDatabases".
    /// </summary>
    public interface IRemoteDatabaseResolver
    {
        bool TryGetConnectionString(string dblink, out string connectionString);
    }

    public class RemoteDatabaseResolver : IRemoteDatabaseResolver
    {
        private readonly Dictionary<string, string> _connections;

        public RemoteDatabaseResolver(IConfiguration configuration)
        {
            _connections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var child in configuration.GetSection("RemoteDatabases").GetChildren())
            {
                if (!string.IsNullOrWhiteSpace(child.Value))
                    _connections[child.Key.Trim()] = child.Value;
            }
        }

        public bool TryGetConnectionString(string dblink, out string connectionString)
        {
            connectionString = null;
            return !string.IsNullOrWhiteSpace(dblink)
                   && _connections.TryGetValue(dblink.Trim(), out connectionString);
        }
    }
}
