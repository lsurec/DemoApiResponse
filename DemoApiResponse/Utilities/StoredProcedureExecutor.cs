using DemoApiResponse.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DemoApiResponse.Utilities
{
    public class StoredProcedureExecutor(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("ConnectionString") ?? "";
        private readonly IConfiguration _configuration = configuration;

        //Consumo generico de procedimientos con tabla de respuesta
        protected async Task<ApiResponseModel<List<T>>> ExecuteStoredProcedureAsync<T>(
            string procedureName, //Procedsimiento que se sua
            Func<SqlDataReader, T> mapFunction, //Respuesta que debe retornar
            params SqlParameter[] parameters //parametros para el procedimeito
            )
        {
            //convertir Sql Parameters en un formato legible: Nombre parametro, valor
            var formattedParameters = parameters.ToDictionary(p => p.ParameterName, p => p.Value);

            try
            {
                //Instancia para la conexion
                using SqlConnection sql = new(_connectionString);

                //Instancia para el comando sql 
                using SqlCommand cmd = new(procedureName, sql);

                //Tipo de commando 
                cmd.CommandType = CommandType.StoredProcedure;

                //Agregar parametros
                cmd.Parameters.AddRange(parameters);

                //abrir conexion
                await sql.OpenAsync();

                //ejecutar procedimiento
                using var reader = await cmd.ExecuteReaderAsync();

                //lista para almacenar la tabla
                List<T> response = new();

                //Recorrer cada registro obtenido
                while (await reader.ReadAsync())
                {
                    //agregar objeto mapeado
                    response.Add(mapFunction(reader));
                }

                //respuesta
                return new ApiResponseModel<List<T>>(response, _configuration)
                {
                    Parameters = formattedParameters, //parametros
                    Status = true, //estado 
                    StoredProcedure = procedureName, //nompre pa si aplica
                    Message = "Operacion exitosa" //mensaje si aplica
                };

            }
            //control de errores de sql
            catch (SqlException ex)
            {
                return new ApiResponseModel<List<T>>(new List<T>(), _configuration)
                {
                    Parameters = formattedParameters,
                    Error = ex.Message,
                    ErrorCode = $"1-{ex.Number}", // Prefijo 1 indica error SQL
                    Status = false,
                    StoredProcedure = procedureName,
                    Message = "Error en la base de datos."
                };
            }
            //control de errores por ejecucion del codigo de la API, como errores de mapeo o de logica
            catch (Exception e)
            {
                //respuesta
                return new ApiResponseModel<List<T>>(new List<T>(), _configuration)
                {
                    Parameters = formattedParameters, //parametros si aplica
                    Error = e.Message, //descripcion del error
                    Status = false, //estado 
                    StoredProcedure = procedureName, //nombre del pa si aplica
                    Message = "Error en la lógica de la API.", //mensaje si aplica
                    ErrorCode = "2" // Prefijo 2 indica error en la API
                };


            }


        }
    }
}
