# Guía de Implementación de APIs en .NET Core 8 con ADO.NET C#: Estructura de Clases y Buenas Prácticas

En el desarrollo de aplicaciones modernas, la creación de APIs eficientes y escalables es esencial para ofrecer servicios robustos y de alto rendimiento. A continuación, se presentará una guía detallada para la implementación de un API utilizando .NET Core 8, C# y ADO.NET. Esta solución está orientada a la interacción directa con bases de datos mediante consultas SQL integradas en el código, específicamente en el uso de procedimientos almacenados, lo que proporciona un enfoque eficiente y flexible para manejar distintas operaciones.

ADO.NET es una tecnología de acceso a datos que permite la interacción directa con bases de datos relacionales. A través de clases como SqlConnection, SqlCommand y SqlDataReader, los desarrolladores pueden ejecutar consultas SQL, gestionar conexiones y manejar resultados de manera eficiente. Este enfoque, si bien es sencillo, requiere buenas prácticas en cuanto a manejo de conexiones y seguridad, por lo que en esta guía también se abordarán esos aspectos.

El objetivo de esta documentación es proporcionar los pasos necesarios para construir una API RESTful básica que consuma servicios de bases de datos usando ADO.NET.

**CONTENIDO:**
- [Guía de Implementación de APIs en .NET Core 8 con ADO.NET C#: Estructura de Clases y Buenas Prácticas](#guía-de-implementación-de-apis-en-net-core-8-con-adonet-c-estructura-de-clases-y-buenas-prácticas)
  - [Requisitos previos](#requisitos-previos)
  - [1. Creación del proyecto](#1-creación-del-proyecto)
  - [2. Estructura del proyecto](#2-estructura-del-proyecto)
  - [3. Configuración de la cadena de conexión a SQL Server](#3-configuración-de-la-cadena-de-conexión-a-sql-server)
  - [4. Clase SqlDataReaderExtensions.cs](#4-clase-sqldatareaderextensionscs)
  - [5. Clase ApiResponseModel.cs](#5-clase-apiresponsemodelcs)
  - [6. Clase StoredProcedureExecutor.cs](#6-clase-storedprocedureexecutorcs)
  - [7. Capa de configuracion de procedimientos almacenados](#7-capa-de-configuracion-de-procedimientos-almacenados)
  - [8. Capa de configuracion de controladdores y endpoints](#8-capa-de-configuracion-de-controladdores-y-endpoints)
  - [9. Configuración de versión en `appsettings.json`](#9-configuración-de-versión-en-appsettingsjson)
  - [10. # Estado de la aplicación `StatusController`](#10--estado-de-la-aplicación-statuscontroller)
  - [11. Control de errores en la API](#11-control-de-errores-en-la-api)
  - [12. Manejo de errores no controlados (validación de modelos)](#12-manejo-de-errores-no-controlados-validación-de-modelos)
  - [Notas](#notas)


## Requisitos previos
- Visual Studio 2022
- .NET Core 8 SDK
- SQL Server
- C#

## 1. Creación del proyecto

1. Abre Visual Studio 
2. Haz click en `crear un proyecto`
3. Busca la plantilla `ASP.NET Core Web API` y seleccionala.
4. Asignale un nombre a tu proyecto, en este ejemplo usaremos el nombre `DemoApiResponse`.
5. Selecciona la direccion donde quieres guardar tu proyecto o usa la configuracion por defecto y haz click en `Siguiente`.
6. Verifica que este seleccionada la version de .net correcta (.Net 8) y `Crea el proyecto`.
7. Por defecto se crean 2 archivos `WeatherForecastController.cs` y `WeatherForecast.cs`, puedes eliminarlos, no vamos a usarlos.
8. Ejecuta el proyecto
9. Deberas ver la ventana por defecto de swagger, felicidades, haz creado un nuevo proyecto.

## 2. Estructura del proyecto

Crearemos las siguientes carpetas

- /Controllers: Contiene los controladores con los endpoint disponibles, unicamnete encontraremos las peticiones http y su manejo.
- /Models: Modelos de datos para la construcción de objetos.
- /Services: Configuracion de las consultas a sql.
- /Utilites: Clases con funcionalidades reutilizables dentro del proyecto.

## 3. Configuración de la cadena de conexión a SQL Server
Para que el API pueda interactuar con la base de datos, es necesario definir una cadena de conexión, aunque esta cadena puede ubicarse en cualuier parte del proyecto, se recomienda que esté en el archivo de configuración de la aplicación parea que esté disponible de manera global y que sea fácil de mantener. Esta cadena contiene la información necesaria para establecer la conexión con el servidor de bases de datos, incluyendo el nombre del servidor, la base de datos y las credenciales de autenticación.

En el archivo appsettings.json, agrega la sección ConnectionStrings con la cadena de conexión adecuada para tu entorno:

```JSON
"ConnectionStrings": {
  "ConnectionString": "Data Source=MI_SERVIDOR;Initial Catalog=MI_BASE_DE_DATOS;User ID=MI_USUARIO;Password=MI_CLAVE"
},
```

**Notas:**
* Reemplaza MI_SERVIDOR por el nombre del servidor de SQL Server.
* Reemplaza MI_BASE_DE_DATOS por el nombre de tu base de datos.
* Si usas autenticación de SQL Server, proporciona User Id y Password.

## 4. Clase SqlDataReaderExtensions.cs

Cuando se trabaja con ADO.NET y SqlDataReader, los valores de las columnas en la base de datos pueden contener NULL, lo que en .NET se representa como DBNull.Value. Si intentamos acceder directamente a estos valores sin manejarlos correctamente, podemos encontrar excepciones o recibir datos en formatos no deseados.

Para solucionar esto, se crea una clase estatica de extensión SqlDataReaderExtensions, la cual proporciona métodos para obtener valores de manera segura y convertirlos a tipos específicos sin riesgo de errores.

Esta clase incluye:

* ``GetValueOrDefault<T>(SqlDataReader, string columnName):`` Obtiene el valor de una columna por nombre y lo convierte al tipo deseado. Si la columna no existe o su valor es NULL, devuelve default(T).

* ``GetValueOrDefaulInt<T>(SqlDataReader, int indexColumn):`` Similar al método anterior, pero accediendo a la columna por su índice en lugar de su nombre.

Estos métodos garantizan que los valores nulos sean representados correctamente como null en los objetos de respuesta, en lugar de estructuras vacías ``({})``, facilitando así el consumo de los datos en el API.

Crea la clase ``SqlDataReaderExtension`` en la carpeta ``/Utilities`` con el siguiente contenido:

```C#
public static class SqlDataReaderExtensions
{
    //Obtener valor de columna por nombre
    public static T? GetValueOrDefault<T>(this SqlDataReader reader, string columnName)
    {
         // Verifica si la columna existe en el SqlDataReader
        if (reader.HasColumn(columnName))
        {
            object value = reader[columnName];
            return value != DBNull.Value ? (T)value : default;
        }

         // Si la columna no existe, devuelve null o el valor por defecto
        return default;
    }

    //Obtener valor de columna por indice
    public static T? GetValueOrDefaulInt<T>(this SqlDataReader reader, int indexColumn)
    {
        // Verifica si el índice de la columna está dentro del rango de columnas
        if (indexColumn >= 0 && indexColumn < reader.FieldCount)
        {
            object value = reader[indexColumn];
            return value != DBNull.Value ? (T)value : default;
        }

         // Si el índice de la columna es inválido, devuelve null o el valor por defecto
        return default;
    }

    // Método auxiliar para verificar si la columna existe en el SqlDataReader
    private static bool HasColumn(this SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
```

**Notas:**
* Asegurate instalar el paquete ``Microsoft.Data.SqlClient``

## 5. Clase ApiResponseModel.cs

Para mantener un formato consistente en todas las respuestas de la API, se define una estructura genérica `ApiResponseModel<T>`. Esta clase encapsula la respuesta de cada endpoint y proporciona información adicional sobre el estado de la transacción, mensajes de error y otros metadatos.  

**Objetivo**  
- Garantizar uniformidad en las respuestas de la API.  
- Facilitar el manejo de errores y mensajes en el cliente.  
- Incluir información adicional como la versión de la API y la marca de tiempo de la respuesta.  

Crea la clase ``ApiResponseModel`` en la carpeta ``/Models`` con el siguiente contenido:

```C#
 public class ApiResponseModel<T>
 {
     public bool Status { get; set; } = false; //Estado de la transaccion
     public string Message { get; set; } = String.Empty; //Mensaje opcional
     public string Error { get; set; } = string.Empty; //Descripcion del error
     public string StoredProcedure { get; set; } = string.Empty; //Nombre del procedimiento almacenado si aplica
     public Dictionary<string, object>? Parameters { get; set; } //Objeto con los parametros del procedimiento almacenado si aplica
     public T Data { get; set; } //Respuesta del api
     public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow; //Fecha y hora de la respuesta en formato UTC
     public string Version { get; set; } //Version de la aplicacion
     public DateTimeOffset? ReleaseDate { get; set; } = null; //Fecha de lanzamiento de la version, si se proporciona en la configuracion, en formato UTC
     public string ErrorCode { get; set; } = string.Empty; // Ej: "1-547" o "2" 1 sql, 2 api, 3 no controlado

     //Constructor para que data y configuration sean obligatorios
     public ApiResponseModel(T data, IConfiguration configuration)
     {
         Data = data; //Asignar data a la propiedad
         Version = configuration["Version"] ?? "Desconocida"; // Usar un valor por defecto si no está configurado
         ReleaseDate = ApiResponseModel<T>.GetReleaseDate(configuration); // Usar un valor por defecto si no está configurado
     }
     private static DateTimeOffset? GetReleaseDate(IConfiguration configuration)
     {
         try
         {
             var year = configuration["ReleaseDate:Year"];
             var month = configuration["ReleaseDate:Month"];
             var day = configuration["ReleaseDate:Date"];
             var hour = configuration["ReleaseDate:Hour"];
             var minute = configuration["ReleaseDate:Minute"];

             if (year == null || month == null || day == null || hour == null || minute == null)
                 return null;

             return new DateTimeOffset(
                 int.Parse(year),
                 int.Parse(month),
                 int.Parse(day),
                 int.Parse(hour),
                 int.Parse(minute),
                 0,
                 TimeSpan.Zero // UTC
             );
         }
         catch
         {
             return null;
         }
     }
 }
```
---

## Propiedades

### `bool Status`

**Descripción:**
Indica el estado final de la transacción.

**Uso:**

* `true` → La operación fue exitosa
* `false` → Ocurrió un error o la operación no fue válida

---

### `string Message`

**Descripción:**
Mensaje informativo u opcional para el consumidor del API.

**Uso típico:**

* Confirmaciones: `"Operación realizada correctamente"`
* Mensajes de negocio: `"Acceso autorizado"`, `"Dispositivo bloqueado"`

---

### `string Error`

**Descripción:**
Contiene la descripción técnica o funcional del error cuando `Status` es `false`.

**Uso:**

* Mensajes de error controlados
* Excepciones capturadas y traducidas a texto

---

### `string StoredProcedure`

**Descripción:**
Nombre del procedimiento almacenado ejecutado, si aplica.

**Objetivo:**

* Facilitar **trazabilidad**
* Auditoría
* Debug en ambientes productivos

---

### `Dictionary<string, object>? Parameters`

**Descripción:**
Parámetros enviados al procedimiento almacenado, si aplica.

**Objetivo:**

* Registro de entrada
* Diagnóstico de errores
* Análisis de ejecuciones

---

### `T Data`

**Descripción:**
Contiene la **respuesta principal del API**, tipada según el endpoint.

**Ejemplos:**

* Lista de registros
* Un objeto
* Un valor simple
* `null` en caso de error

---

### `DateTimeOffset Timestamp`

**Descripción:**
Fecha y hora (UTC) en que se generó la respuesta.

**Objetivo:**

* Trazabilidad
* Sincronización entre sistemas
* Auditoría

**Nota:**
Se inicializa automáticamente con `DateTimeOffset.UtcNow`.

---

### `string Version`

**Descripción:**
Versión actual de la aplicación que genera la respuesta.

**Origen:**
Se obtiene desde `IConfiguration`.

---

### `DateTimeOffset? ReleaseDate`

**Descripción:**
Fecha y hora del release de la versión desplegada.

**Características:**

* Nullable (`null` si no está configurada)
* Se arma a partir de valores individuales en configuración

---

### `string ErrorCode`

**Descripción:**
Código estandarizado de error para clasificación.

**Convención sugerida:**

* `1-xxx` → Error SQL
* `2` → Error de lógica/API
* `3` → Error no controlado

**Objetivo:**

* Manejo centralizado de errores
* Compatibilidad con frontends y otros sistemas

---

## Constructor

```csharp
public ApiResponseModel(T data, IConfiguration configuration)
```

### Descripción

Constructor principal que **obliga a definir los datos de respuesta y la configuración** de la aplicación.

### Funciones:

* Asigna el contenido de `Data`
* Obtiene la versión desde configuración
* Calcula la fecha de release usando un método interno

### Ventaja:

Garantiza que **todas las respuestas incluyan versión y release**, evitando respuestas incompletas.

---

## Método privado `GetReleaseDate`

```csharp
private static DateTimeOffset? GetReleaseDate(IConfiguration configuration)
```

### Descripción

Obtiene y construye la fecha de release de la aplicación a partir de valores definidos en configuración.

### Claves de configuración utilizadas:

* `ReleaseDate:Year`
* `ReleaseDate:Month`
* `ReleaseDate:Date`
* `ReleaseDate:Hour`
* `ReleaseDate:Minute`

---

### Funcionamiento

1. Lee los valores desde `IConfiguration`
2. Valida que ninguno sea `null`
3. Convierte los valores a enteros
4. Construye un `DateTimeOffset` en **UTC**
5. Si ocurre cualquier error → retorna `null`

---

### Motivo del `try/catch`

* Evita que errores de configuración rompan la respuesta del API
* Permite que el API siga funcionando aunque la fecha de release no esté definida

---

## Beneficios del diseño

* Respuesta consistente en todos los endpoints
* Facilita debugging y soporte
* Mejora trazabilidad en producción
* Compatible con SQL, APIs externas y lógica interna
* Escalable para nuevos metadatos


## 6. Clase StoredProcedureExecutor.cs

En esta sección se implementa una clase para ejecutar procedimientos almacenados en SQL Server de manera genérica, utilizando ADO.NET. Este enfoque permite reutilizar código y estructurar la ejecución de consultas de forma eficiente.  

**Objetivo**  
- Centralizar la ejecución de procedimientos almacenados.  
- Estandarizar la estructura de respuesta de la API.  
- Manejar excepciones y formatear la respuesta adecuadamente.  

**Flujo del método `ExecuteStoredProcedureAsync<T>`**  

1. Se establece la conexión con la base de datos mediante `SqlConnection`.  
2. Se crea y configura un comando SQL (`SqlCommand`) para ejecutar el procedimiento almacenado.  
3. Se agregan los parámetros recibidos.  
4. Se abre la conexión y se ejecuta el procedimiento (`ExecuteReaderAsync`).  
5. Se leen los datos obtenidos y se mapean en una lista de tipo `T`.  
6. Se encapsula la respuesta en un `ApiResponseModel<List<T>>`, incluyendo información del procedimiento y parámetros utilizados.  
7. En caso de error, se devuelve un objeto con `Status = false` y detalles del error.  

Crea la clase ``StoredProcedureExecutor`` en la carpeta ``/Utilities`` con el siguiente contenido:


```C#
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
```

**Notas:**
* Existirán casos en donde este ejecutor de procedimientos almacenados no se adapte a la funcionalidad deseada, para esos casos especificos utiliza el metodo que mejor convenga, pero recuerda utilizar ApiResponseModel para la respuesta del endpoint. 

## 7. Capa de configuracion de procedimientos almacenados

La capa de configuración de procedimientos almacenados organiza los procedimientos en servicios específicos dentro de la carpeta ``/Services``. Cada servicio agrupa los procedimientos almacenados relacionados con un módulo específico de la aplicación (por ejemplo, RestaurantService, PosService, ClientService).

**Beneficios**
* Facilita la escalabilidad y el mantenimiento.
* Agrupa procedimientos almacenados por módulos.
* Mejora la organización y reutilización del código.
* Reduce la duplicación al extender de StoredProcedureExecutor.
* Reutilización del procedimiento almacenado en varios controllers.

### Ejemplo de uso:
Para este ejemplo usaremos este procedimiento almacenado:

```SQL

DECLARE @RC int
DECLARE @pUserName varchar(30)
DECLARE @pTarea smallint

EXECUTE @RC = [dbo].[PA_bsc_Tarea_Invitado] 
   @pUserName 
  ,@pTarea 
GO
```

Que corresponde a la lista de responsables para una tarea (respuesta sql):

| Tarea_UserName | EMail                              | UserName |
| -------------- | ---------------------------------- | -------- |
| 527            | soportesistemas@demosoftonline.com | DESA003  |
---

1. **Creación del modelo de datos**

En la carpeta ``/Models`` creamos el modelo ``UserModel.cs``, esta clase contiene las propiedades segun la respuesta del procedimiento almacenado y la funcion para mapear estas propiedades:

```C#
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
```

2. **Definición de un Servicio de Procedimientos Almacenados**
En la carpeta ``/Services`` creamos la clase ``ExampleService.cs`` donde configuraremos el Procedimiento almacenado de este ejemplo.

Cada servicio hereda de StoredProcedureExecutor e implementa métodos con el mismo nombre que los procedimientos almacenados que ejecutan.

```C#
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

        //Parametros
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
```

**Flujo de Ejecución**
* Definir un nuevo servicio en /Services/ que herede de StoredProcedureExecutor.
* Crear un método por cada procedimiento almacenado, con el mismo nombre del procedimiento.
* Definir los parámetros utilizando SqlParameter[].
* Llamar a ExecuteStoredProcedureAsync y mapear la respuesta al modelo correspondiente.
* Devolver un ApiResponseModel<T> con los datos obtenidos.

## 8. Capa de configuracion de controladdores y endpoints

La capa de controladores gestiona las solicitudes HTTP y las respuestas de la API, separando la lógica de SQL de la lógica del controlador. Su función principal es:

* Gestionar estados HTTP y respuestas.
* Delegar la lógica de negocio a los servicios correspondientes.
* Evitar la interacción directa con la base de datos dentro del controlador.

Cada controlador se enfoca en exponer los endpoints de un módulo específico y delega la ejecución de procedimientos almacenados a la capa de servicios instanciando el servicio correspondiente.

**Flujo de Ejecución**
* Crear un nuevo controlador ``ExampleController.cs`` en ``/Controllers`` utilizando la plantilla "Controlador de API en blanco".
* Inyectar el servicio correspondiente, que contiene los métodos para ejecutar procedimientos almacenados.
* Definir los endpoints con atributos [HttpGet], [HttpPost], etc.
* Llamar al servicio en cada endpoint, delegando la ejecución de SQL.
* Manejar las respuestas HTTP, devolviendo Ok() para respuestas exitosas y BadRequest() u otros estados según sea necesario.

```C#
[Route("api/[controller]")]
[ApiController]
public class ExampleController(IConfiguration configuration) : ControllerBase
{
     //Servicion con el consumo de los procedimientos
    private readonly ExampleService _exampleService = new(configuration);

    //Obtener guarniciones
    [HttpGet("{user}/{task}")]
    public async Task<ActionResult> GetGarnishs(
        string user,
        int task
    )
    {
        //Consumo del procedimiento
        ApiResponseModel<List<UserModel>> response = await _exampleService.PA_bsc_Tarea_Invitado(
            user: user,
            task: task
        );

        //logica de negocios aqui*/

        //Respuesta del api, manejo de estados http
        if (response.Status)
            return Ok(response);

        return BadRequest(response);
    }
}
```

**Respuesta existosa (200 Ok):**

```JSON
{
  "status": true,
  "message": "Operacion exitosa",
  "error": "",
  "storeProcedure": "PA_bsc_Tarea_Invitado",
  "parameters": {
    "@pUserName": "sa",
    "@pTarea": 114
  },
  "data": [
    {
      "tarea_UserName": 527,
      "eMail": "soportesistemas@demosoftonline.com",
      "userName": "DESA003"
    }
  ],
  "timestamp": "2025-03-10T17:58:37.4769396Z",
  "version": "Desconocida"
}
```

**Respuesta con error (400 Bad Request):**

```JSON
{
  "status": false,
  "message": "Operacion fallida",
  "error": "La conexión con el servidor se ha establecido correctamente, pero se ha producido un error durante el proceso de inicio de sesión. (provider: Proveedor de SSL, error: 0 - La cadena de certificación fue emitida por una entidad en la que no se confía.)",
  "storeProcedure": "PA_bsc_Tarea_Invitado",
  "parameters": {
    "@pUserName": "sa",
    "@pTarea": 114
  },
  "data": [],
  "timestamp": "2025-03-10T17:44:55.8725289Z",
  "version": "Desconocida"
}
```
---
## 9. Configuración de versión en `appsettings.json`

### Descripción general

La aplicación debe definir explícitamente su **versión** y **fecha de release** dentro del archivo `appsettings.json`.
Esta información es consumida por el modelo estándar `ApiResponseModel<T>` y se incluye automáticamente en **todas las respuestas del API**.

Esto permite:

* Identificar exactamente **qué versión del sistema respondió**
* Facilitar **soporte, auditoría y debugging**
* Detectar problemas asociados a un despliegue específico

---

## Estructura de configuración

```json
"Version": "1.1.44",
"ReleaseDate": {
  "Year": "2026",   // yyyy
  "Month": "02",   // MM
  "Date": "05",    // dd
  "Hour": "12",    // Formato 24 horas
  "Minute": "25"   // mm
}
```

---

## Descripción de cada campo

### `Version`

**Tipo:** `string`
**Ejemplo:** `"1.1.44"`

**Descripción:**
Identifica la versión de la aplicación desplegada.

**Convención recomendada:**

* `MAJOR.MINOR.BUILD`
* Ejemplo:

  * `1.0.0` → Release inicial
  * `1.1.0` → Mejora funcional
  * `1.1.44` → Fix o build incremental

---

### `ReleaseDate`

Objeto que representa la **fecha y hora exacta del despliegue**.

> Se define por partes para evitar problemas de formato, cultura o zona horaria.

---

#### `ReleaseDate:Year`

* Año del release
* Formato: `yyyy`
* Ejemplo: `"2026"`

---

#### `ReleaseDate:Month`

* Mes del release
* Formato: `MM`
* Rango válido: `01–12`

---

#### `ReleaseDate:Date`

* Día del mes
* Formato: `dd`
* Rango válido: `01–31`

---

#### `ReleaseDate:Hour`

* Hora del release
* Formato: 24 horas
* Rango válido: `00–23`

---

#### `ReleaseDate:Minute`

* Minutos del release
* Formato: `mm`
* Rango válido: `00–59`

---

## ¿Cómo llenar estos valores?

### Regla obligatoria

**Cada vez que la aplicación se compila o se despliega**, estos valores **deben actualizarse**.

---

### Proceso recomendado

1. Incrementar el número de `Version`
2. Colocar la **fecha y hora real del build**
3. Validar que todos los campos existan
4. Desplegar la aplicación

---

### Ejemplo práctico

Si se realiza un despliegue el **5 de febrero de 2026 a las 12:25 UTC**, la configuración debe quedar:

```json
"Version": "1.1.44",
"ReleaseDate": {
  "Year": "2026",
  "Month": "02",
  "Date": "05",
  "Hour": "12",
  "Minute": "25"
}
```

---

## ¿Por qué es una buena práctica?

### 1. Trazabilidad total

Permite identificar exactamente:

* Qué versión generó un error
* En qué despliegue apareció un bug
* Si un cliente está usando una versión obsoleta

---

### 2. Soporte y debugging más rápido

Evita preguntas como:

> “¿En qué versión estás?”

La respuesta viene **directamente en el API**.

---

### 3. Auditoría y control de cambios

* Facilita el análisis histórico
* Permite correlacionar logs, errores y releases

---

### 4. Independencia del entorno

* No depende del servidor
* No depende del sistema operativo
* No depende de la cultura regional

---

### 5. Prevención de errores por caché o despliegues incompletos

Si dos ambientes responden con versiones distintas, el problema es visible inmediatamente.

---

## Comportamiento ante configuración incompleta

Si alguno de los campos de `ReleaseDate`:

* No existe
* Tiene un formato inválido

`ReleaseDate` se devolverá como `null`, **sin afectar el funcionamiento del API**.

Esto evita caídas por errores de configuración.

---

## Recomendación final

**La versión y la fecha de release son parte del contrato del API**, no solo información interna.

Deben:

* Estar siempre presentes
* Actualizarse en cada compilación
* Ser visibles en todas las respuestas
---

## 10. # Estado de la aplicación `StatusController`

### Descripción general

El `StatusController` es un **controlador técnico de monitoreo**, cuyo objetivo es **verificar el estado de la aplicación publicada** sin ejecutar lógica de negocio ni depender de procesos complejos.

Este controlador permite confirmar rápidamente que:

* La aplicación está **levantada**
* La configuración es **válida**
* La versión desplegada es la correcta
* El API responde usando el **formato estándar**

---

## Endpoint: `GET /status`

Este endpoint sirve para:

* Validar que el API está **operativo**
* Confirmar la **versión y fecha de release** desplegada
* Verificar que la **respuesta estándar** funciona correctamente
* Proveer información técnica **útil para el encargado del despliegue**
* Facilitar pruebas rápidas post-deploy

---

## Implementación

```csharp
[HttpGet]
public IActionResult StatusApp()
{
    ApiResponseModel<List<object>> status = new(new List<object>(), _configuration)
    {
        Status = true,
        Message = "Ok"
    };

    return Ok(status);
}
```

---

## Análisis del método `StatusApp`

### Tipo

* **HTTP Method:** `GET`
* **Acción:** Solo lectura
* **Side effects:** Ninguno

---

### Flujo de ejecución

1. Se crea una instancia de `ApiResponseModel<List<object>>`
2. Se inicializa `Data` como una lista vacía
3. Se inyecta `IConfiguration` para:

   * Obtener la versión
   * Obtener la fecha de release
4. Se establece:

   * `Status = true`
   * `Message = "Ok"`
5. Se retorna una respuesta HTTP `200 (OK)`

---

## ¿Por qué `List<object>`?

* No se retorna información de negocio
* Permite extender el endpoint en el futuro
* Evita romper el contrato del API
* Mantiene una estructura consistente

---

## Información que devuelve el endpoint

Gracias al uso de `ApiResponseModel<T>`, el endpoint retorna automáticamente:

* Estado de la aplicación (`Status`)
* Mensaje general (`Message`)
* Versión del API (`Version`)
* Fecha del release (`ReleaseDate`)
* Timestamp de la respuesta (`Timestamp`)

---

## Ejemplo de respuesta JSON

```json
{
  "status": true,
  "message": "Ok",
  "error": "",
  "storedProcedure": "",
  "parameters": null,
  "data": [],
  "timestamp": "2026-02-09T18:30:15.123Z",
  "version": "1.1.44",
  "releaseDate": "2026-02-05T12:25:00+00:00",
  "errorCode": ""
}
```

---

## ¿Para quién es este endpoint?

### DevOps / Encargado de despliegue

* Verifica que el deploy fue exitoso
* Confirma que la versión es la correcta
* Detecta despliegues incompletos

---

### Soporte técnico

* Identifica rápidamente la versión en producción
* Facilita diagnóstico de incidencias

---

### Monitoreo

* Puede ser consumido por:

  * Load balancers
  * Health checks
  * Scripts automáticos
  * Pipelines de CI/CD

---

## Buenas prácticas aplicadas

### 1. No depende de base de datos

Evita falsos negativos por fallas externas.

---

### 2. Respuesta rápida

Ideal para monitoreo constante.

---

### 3. Usa el contrato estándar

Garantiza consistencia con el resto del API.

---

### 4. Seguro

No expone:

* Credenciales
* Datos sensibles
* Lógica de negocio

---

## Recomendaciones adicionales

* No requerir autenticación
* No incluir lógica pesada
* Mantenerlo siempre disponible
* Usarlo como **primer punto de validación post-deploy**

---

## Recomendación final

**Todo API debe exponer un endpoint de estado** que permita validar rápidamente:

> *“La aplicación está viva, esta es su versión y este es su release.”*

Este controlador cumple exactamente ese propósito.

---

## 11. Control de errores en la API

### Descripción general

La API implementa un **manejo de errores estandarizado**, basado en bloques `try / catch`, con el objetivo de:

* Capturar errores de forma controlada
* Clasificar el tipo de error ocurrido
* Retornar siempre una respuesta válida y consistente
* Evitar que excepciones no controladas rompan el contrato del API

Todos los errores se devuelven utilizando el modelo estándar `ApiResponseModel<T>`.

---

## Estructura general

```csharp
try
{
    // Lógica principal del endpoint
}
```

El bloque `try` contiene:

* Ejecución de procedimientos almacenados
* Mapeo de resultados
* Lógica de negocio del API

Cualquier excepción generada dentro de este bloque será interceptada por los `catch` definidos.

---

## Control de errores SQL

```csharp
catch (SqlException ex)
{
    return new ApiResponseModel<List<T>>(new List<T>(), _configuration)
    {
        Parameters = formattedParameters,
        Error = ex.Message,
        ErrorCode = $"1-{ex.Number}",
        Status = false,
        StoredProcedure = procedureName,
        Message = "Error en la base de datos."
    };
}
```

### Tipo de error

* Excepciones generadas por SQL Server
* Problemas en ejecución de procedimientos almacenados
* Violaciones de llaves, tipos, restricciones, timeouts, etc.

---

### Convención de `ErrorCode`

* Prefijo `1` → Error de base de datos (SQL)
* `ex.Number` → Código nativo del error SQL

**Ejemplo:**

```
1-547   → Violación de constraint
1-2627  → Llave duplicada
```

Esto permite:

* Identificar rápidamente el origen del problema
* Filtrar errores por tipo
* Integrar con sistemas de monitoreo

---

### Campos relevantes devueltos

| Campo             | Descripción                         |
| ----------------- | ----------------------------------- |
| `Status`          | `false`, indica fallo               |
| `Message`         | Mensaje genérico para el consumidor |
| `Error`           | Mensaje técnico del error SQL       |
| `ErrorCode`       | Código estandarizado del error      |
| `StoredProcedure` | Procedimiento almacenado ejecutado  |
| `Parameters`      | Parámetros enviados al SP           |

---

### ¿Por qué separar errores SQL?

* SQL es una dependencia externa
* Sus errores tienen códigos propios
* Requieren diagnóstico distinto al de la lógica del API

---

## Control de errores de la API (lógica / ejecución)

```csharp
catch (Exception e)
{
    return new ApiResponseModel<List<T>>(new List<T>(), _configuration)
    {
        Parameters = formattedParameters,
        Error = e.Message,
        Status = false,
        StoredProcedure = procedureName,
        Message = "Error en la lógica de la API.",
        ErrorCode = "2"
    };
}
```

---

### Tipo de error

* Errores de mapeo de datos
* Errores de conversión
* Errores de lógica
* Excepciones no SQL
* Fallos internos del código del API

---

### Convención de `ErrorCode`

* `2` → Error en la lógica o ejecución del API

Este tipo de error indica:

> “La base de datos respondió, pero la API falló al procesar el resultado o la lógica.”

---

### Campos relevantes devueltos

| Campo             | Descripción                     |
| ----------------- | ------------------------------- |
| `Status`          | `false`                         |
| `Message`         | Mensaje funcional               |
| `Error`           | Mensaje técnico de la excepción |
| `ErrorCode`       | Clasificación del error         |
| `StoredProcedure` | SP asociado (si aplica)         |
| `Parameters`      | Parámetros de entrada           |

---

## Buenas prácticas aplicadas

### 1. Nunca se rompe el contrato del API

Siempre se retorna `ApiResponseModel<T>`, incluso ante errores.

---

### 2. Clasificación clara de errores

Permite distinguir rápidamente:

* Error SQL
* Error de API
* Error no controlado ( prefijo `3`)

---

### 3. Información técnica sin exponer lógica sensible

* `Message` es genérico
* `Error` es técnico (útil para soporte)
* No se exponen stack traces

---

### 4. Facilita soporte y debugging

Con:

* Nombre del SP
* Parámetros enviados
* Código de error estandarizado
* Versión y release (incluidos automáticamente)

---

## Ejemplo de respuesta ante error SQL

```json
{
  "status": false,
  "message": "Error en la base de datos.",
  "error": "The INSERT statement conflicted with the FOREIGN KEY constraint...",
  "errorCode": "1-547",
  "storedProcedure": "sp_insert_documento",
  "parameters": {
    "@Id": 10
  },
  "data": [],
  "version": "1.1.44",
  "releaseDate": "2026-02-05T12:25:00+00:00"
}
```

---

## Recomendación final

**Todo error debe ser controlado, clasificado y documentado**, no lanzado directamente al cliente.

Este esquema:

* Hace el API más robusto
* Reduce tiempos de soporte
* Mejora la calidad del despliegue
* Facilita monitoreo y auditoría

---

## 12. Manejo de errores no controlados (validación de modelos)

### Ubicación

`Program.cs`

---

## Descripción general

Este bloque configura el **manejo global de errores de validación del modelo** (`ModelState`) en la API.

Su objetivo es:

* Interceptar errores **antes de que entren al controller**
* Evitar respuestas automáticas inconsistentes de ASP.NET
* Retornar siempre la **respuesta estándar del API**
* Clasificar estos errores como **errores no controlados (código 3)**

---

## Contexto del problema

Por defecto, ASP.NET Core:

* Retorna un `400 Bad Request`
* Con una estructura propia (`ProblemDetails`)
* Sin versión, sin release, sin contrato estándar

Esto rompe la consistencia del API.

---

## Solución aplicada

Se sobreescribe el comportamiento por defecto usando:

```csharp
ApiBehaviorOptions.InvalidModelStateResponseFactory
```

Esto permite **controlar completamente la respuesta** cuando:

* Faltan campos obligatorios
* Fallan validaciones (`[Required]`, `[MaxLength]`, etc.)
* El modelo no puede mapearse correctamente

---

## Implementación

```csharp
var configuration = builder.Configuration;

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value.Errors.Select(err => err.ErrorMessage).ToArray()
            );

        ApiResponseModel<List<object>> response = new(new List<object>(), configuration)
        {
            Error = JsonConvert.SerializeObject(errors),
            Message = "Error no controlado",
            ErrorCode = "3"
        };

        return new BadRequestObjectResult(response);
    };
});
```

---

## Flujo de ejecución

1. El cliente envía una solicitud inválida
2. ASP.NET detecta errores de validación
3. **No se ejecuta el controller**
4. Se activa `InvalidModelStateResponseFactory`
5. Se construye una respuesta estándar
6. Se retorna `400 Bad Request`

---

## Procesamiento de errores

### Extracción de errores

```csharp
context.ModelState
```

Se recorre el `ModelState` para obtener:

* Nombre del campo
* Mensajes de error asociados

El resultado final es un diccionario como:

```json
{
  "Nombre": ["El campo Nombre es obligatorio"],
  "Edad": ["El valor debe ser mayor a 0"]
}
```

---

### Campo `Error`

Los errores se serializan a JSON y se asignan a:

```csharp
Error = JsonConvert.SerializeObject(errors)
```

Esto permite:

* Mantener la estructura completa
* Facilitar lectura desde frontend
* No perder detalle técnico

---

## Convención de `ErrorCode`

### Código `3` → Error no controlado

Este tipo de error indica:

* Fallo de validación
* Error de entrada del cliente
* Problemas antes de ejecutar la lógica del API

No es:

* Error SQL
* Error de lógica interna

---

## Campos devueltos

| Campo         | Descripción                      |
| ------------- | -------------------------------- |
| `Status`      | Implícitamente `false`           |
| `Message`     | `"Error no controlado"`          |
| `Error`       | Detalle de validaciones fallidas |
| `ErrorCode`   | `"3"`                            |
| `Data`        | Lista vacía                      |
| `Version`     | Versión del API                  |
| `ReleaseDate` | Fecha del release                |
| `Timestamp`   | Momento de la respuesta          |

---

## Ejemplo de respuesta

```json
{
  "status": false,
  "message": "Error no controlado",
  "error": "{\"Nombre\":[\"El campo Nombre es obligatorio\"]}",
  "errorCode": "3",
  "data": [],
  "version": "1.1.44",
  "releaseDate": "2026-02-05T12:25:00+00:00"
}
```

---

## ¿Por qué manejarlo en `Program.cs`?

### 1. Centralización

* No se repite código en cada controller
* Aplica a toda la API

---

### 2. Consistencia

* Todas las respuestas siguen el mismo contrato
* No hay respuestas “especiales” de ASP.NET

---

### 3. Separación de responsabilidades

* Controllers → lógica de negocio
* Program.cs → comportamiento global del API

---

### 4. Prevención de errores silenciosos

* El cliente siempre sabe **por qué falló**
* El backend no ejecuta lógica innecesaria

---

## Relación con la clasificación de errores

| Código  | Tipo de error                    |
| ------- | -------------------------------- |
| `1-XXX` | Error SQL                        |
| `2`     | Error de lógica de la API        |
| `3`     | Error no controlado / validación |

---

## Recomendación final

**Todo API profesional debe manejar errores de validación a nivel global**.

Este enfoque:

* Refuerza el contrato del API
* Simplifica soporte
* Mejora la experiencia del consumidor
* Hace el sistema más predecible y robusto

---
## Notas
* En este proyecto no se incluyen la base de datos o credenciales, verifica tu cadena de conexión y que tengas disponible un procedimiento almacenado.
* Modifica el procedmiento almacenado y sus parametros al tuyo en el servicio, esto incluye que crees un nuevo modelo de datos para la respuesta.
* Recuerda siempre usar ``ResponseApiModel`` para las respuestas de cualquier endpoint.
