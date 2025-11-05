using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.API
{
    [Controller]
    [Route("api/TrabalhoSF/[controller]")]
    [Authorize]
    public class ArquivosController
    {
    }
}
