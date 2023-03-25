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

                        Venta venta = new Venta();
                        venta.ClienteId = 1;
                        venta.Folio = folioActual + 1;
                        venta.Fecha = DateTime.Now;
                        query = "INSERT INTO Ventas (Folio,Fecha,ClienteId,Total) VALUES (@Folio,@Fecha,@ClienteId,@Total); select scope_identity()";
                        using(SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@Folio",venta.Folio);
                            cmd.Parameters.AddWithValue("@Fecha", venta.Fecha);
                            cmd.Parameters.AddWithValue("@ClienteId", venta.ClienteId);
                            cmd.Parameters.AddWithValue("@Total", venta.Total);
                            
                            if (!int.TryParse(cmd.ExecuteScalar().ToString(), out int idVenta))
                            {
                                throw new Exception("Ocurrio un error al obtener el Id de la venta");
                            }
                            venta.Id = idVenta;
                        }

                        foreach(VentaDetalle concepto in venta.Conceptos)
                        {
                            using(SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Transaction = transaction;
                                query = "INSERT INTO VentasDetalles(VentaId,ProductoId,Cantidad,Descripcion,PrecioUnitario,Importe)VALUES(@VentaId,@ProductoId,@Cantidad, @Descripcion, @PrecioUnitario, @Importe)";
                                //@VentaId,@ProductoId,@Cantidad, @Descripcion, @PrecioUnitario, @Importe
                                cmd.Parameters.AddWithValue("@VentaId", venta.Id);
                                cmd.Parameters.AddWithValue("@ProductoId", concepto.ProductoId);
                                cmd.Parameters.AddWithValue("@Cantidad", concepto.Cantidad);
                                cmd.Parameters.AddWithValue("@Descripcion", concepto.Descripcion);
                                cmd.Parameters.AddWithValue("@PrecioUnitario", concepto.PrecioUnitario);
                                cmd.Parameters.AddWithValue("@Importe", concepto.Importe);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Transaction = transaction;
                                query = "UPDATE Existencias set Existencia = Existencia-@Cantidad where ProductoId = @ProductoId";
                                //@VentaId,@ProductoId,@Cantidad, @Descripcion, @PrecioUnitario, @Importe
                                cmd.Parameters.AddWithValue("@ProductoId", concepto.ProductoId);
                                cmd.Parameters.AddWithValue("@Cantidad", concepto.Cantidad);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;
                            query = "UPDATE Folios set Folio = Folio + 1";
                            cmd.ExecuteNonQuery();
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
