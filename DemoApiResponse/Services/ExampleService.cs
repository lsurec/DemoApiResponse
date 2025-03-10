using DemoApiResponse.Models;
using DemoApiResponse.Utilities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DemoApiResponse.Services
{
    public class ExampleService(IConfiguration configuration) : StoredProcedureExecutor(configuration)
    {

        //Funcion para el procedimiento almacenado
        public async Task<ApiResponseModel<List<UserModel>>> PA_bsc_Tarea_Invitado(
           string user,
           int task
           )
        {
            //Nombre del procedimeinto alamacenado
            string storeProcedure = "PA_bsc_Tarea_Invitado";


            var parameters = new SqlParameter[]
            {
               new("@pUserName", SqlDbType.VarChar, 30) {Value = user},
               new("@pTarea", SqlDbType.SmallInt) { Value = task },
               //Mas parametros si son necesarios
               //new("@Output", SqlDbType.VarChar, 200) { Direction = ParameterDirection.Output }, //ejmplo: output, intput, etc
            };



            //consumo y respuesta del procedimiento
            return await ExecuteStoredProcedureAsync(storeProcedure, UserModel.MapToModel, parameters);
        }

    }
}
