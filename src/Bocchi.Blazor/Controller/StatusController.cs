using Bocchi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Bocchi.Blazor.Controller;

public class StatusController : BocchiController
{
    [HttpGet("api/bocchi/status")]
    public ActionResult NoPasswordLogin()
        => Ok("success");
}