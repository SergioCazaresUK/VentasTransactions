using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos
{
    public static class Conexion
    {
        public static string ConnectionString { get; private set; } = "Server=localhost;Database=VentasTransactionDB;Trusted_Connection=True;";
    }
}
