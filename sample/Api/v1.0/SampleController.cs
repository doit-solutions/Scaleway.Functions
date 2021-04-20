using Microsoft.AspNetCore.Mvc;

namespace Scaleway.Functions.Api.v1_0
{
    [ApiController]
    [Route("api/samples")]
    public class SampleController : Controller
    {
        public IActionResult GetSamples()
        {
            return Ok(new int[] { 1, 2, 3, 4, 5 });
        }
    }
}
