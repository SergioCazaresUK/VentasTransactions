using AccesoDatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VentasTransactions
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Conexion.ConnectionString);
            Venta venta = new Venta();
            venta.ClienteId = 1;
            venta.Fecha = DateTime.Now;

            VentaDetalle producto1 = new VentaDetalle();
            producto1.ProductoId = 1;
            producto1.Cantidad = 1;
            producto1.Descripcion = "Azucar 1kg";
            producto1.PrecioUnitario = 27.00m;
            producto1.Importe = producto1.Cantidad * producto1.PrecioUnitario;

            VentaDetalle producto2 = new VentaDetalle();
            producto2.ProductoId = 2;
            producto2.Cantidad = 1;
            producto2.Descripcion = "Jugo Mango";
            producto2.PrecioUnitario = 10.00m;
            producto2.Importe = producto1.Cantidad * producto1.PrecioUnitario;

            venta.Conceptos.Add(producto1);
            venta.Conceptos.Add(producto2);
        }

        //Debemos reubicar este metodo.
        private void GuardarVenta()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Conexion.ConnectionString))
                {
                    SqlTransaction transaction;
                    con.Open();
                    transaction = con.BeginTransaction();

                    try
                    {
                        string query = "select top(1) Folio from Folios";
                        int folioActual = 0;
                        using(SqlCommand cmd = new SqlCommand(query,con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;
                            if(!int.TryParse(cmd.ExecuteScalar().ToString(),out folioActual))
                            {
                                throw new Exception("Ocurrio un error al obtener el Folio");
                            }
                        }








                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception(ex.Message);
                    }
                    
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Ocurrio un error al guardar la venta {ex.Message}");
            }
        }
    }
}
