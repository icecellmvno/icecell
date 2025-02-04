using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceSMS.API.Controllers;
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SmsController:BaseController
{
    
}