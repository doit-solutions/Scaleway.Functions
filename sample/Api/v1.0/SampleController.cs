using Microsoft.AspNetCore.Mvc;

namespace Scaleway.Functions.Api.v1_0
{
    public record GetSampleViewModel(bool RunsAsFunction, string NamespaceId, string ApplicationId);

    [ApiController]
    [Route("api/sample")]
    public class SampleController : Controller
    {
        private readonly ScalewayContext _ctx;

        public SampleController(ScalewayContext ctx)
        {
            // Information about the Scaleway Functions context in which the ASP.NET application is running
            // can be injected by the ASP.NET service provider by requesting an instance of the
            // `Scaleway.Functions.ScalewayContext` class. The information provided by this class should
            // normally not be needed.
            _ctx = ctx;
        }

        public ActionResult<GetSampleViewModel> GetSample()
        {
            return new GetSampleViewModel(_ctx.IsRunningAsFunction, _ctx.NamespaceId ?? string.Empty, _ctx.ApplicationId ?? string.Empty);
        }
    }
}
