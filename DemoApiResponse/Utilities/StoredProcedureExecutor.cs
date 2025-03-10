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
                List<T> response = [];

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
                    StoreProcedure = procedureName, //nompre pa si aplica
                    Message = "Operacion exitosa" //mensaje si aplica
                };

            }
            catch (Exception e)
            {
                //respuesta
                return new ApiResponseModel<List<T>>([], _configuration)
                {
                    Parameters = formattedParameters, //parametros si aplica
                    Error = e.Message, //descripcion del error
                    Status = false, //estado 
                    StoreProcedure = procedureName, //nombre del pa si aplica
                    Message = "Operacion fallida" //mensaje si aplica
                };
            }
        }
    }
}
