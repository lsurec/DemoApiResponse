using DemoApiResponse.Models;
using DemoApiResponse.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoApiResponse.Controllers
{
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
}
