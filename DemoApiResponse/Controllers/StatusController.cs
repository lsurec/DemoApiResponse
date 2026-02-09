using DemoApiResponse.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoApiResponse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StatusController(IConfiguration configuration)
        {
            _configuration = configuration;

        }
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
    }
}
