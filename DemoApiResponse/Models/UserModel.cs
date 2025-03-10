using DemoApiResponse.Utilities;
using Microsoft.Data.SqlClient;

namespace DemoApiResponse.Models
{
    public class UserModel
    {
        //Propiedades
        public int? Tarea_UserName { get; set; }
        public string? EMail { get; set; }
        public string? UserName { get; set; }

        //Funcion Map y uso de SqlDataReaderExtensions
        public static UserModel MapToModel(SqlDataReader reader)
        {
            return new UserModel()
            {
                Tarea_UserName = reader.GetValueOrDefault<int>("Tarea_UserName"), //Acceso por nombre
                EMail = reader.GetValueOrDefault<string>("EMail"), //Acceso por nombre
                UserName = reader.GetValueOrDefaulInt<string>(2), //Acceso por indice
            };
        }

    }
}
