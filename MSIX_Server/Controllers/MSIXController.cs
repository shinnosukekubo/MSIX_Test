using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace MSIX_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MSIXController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using FileStream fs = System.IO.File.Open("MSIX_Test_Package.msix", FileMode.Open);
            using MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            var bytes = ms.ToArray();

            return File(bytes, "application/force-download", "");
        }
    }
}
