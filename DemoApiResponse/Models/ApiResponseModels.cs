namespace DemoApiResponse.Models
{
    public class ApiResponseModel<T>
    {
        public bool Status { get; set; } = false; //Estado de la transaccion
        public string Message { get; set; } = String.Empty; //Mensaje opcional
        public string Error { get; set; } = string.Empty; //Descripcion del error
        public string StoreProcedure { get; set; } = string.Empty; //Nombre del procedimiento almacenado si aplica
        public Dictionary<string, object>? Parameters { get; set; } //Objeto con los parametros del procedimiento almacenado si aplica
        public T Data { get; set; } //Respuesta del api
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; //Hora en la que se realizó la petición
        public string Version { get; set; } //Version de la aplicacion

        //Constructor para que data y configuration sean obligatorios
        public ApiResponseModel(T data, IConfiguration configuration)
        {
            Data = data; //Asignar data a la propiedad
            Version = configuration["Version"] ?? "Desconocida"; // Usar un valor por defecto si no está configurado
        }

    }
}
