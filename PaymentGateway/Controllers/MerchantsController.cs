using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Model;
using PaymentGateway.Services;

namespace PaymentGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public MerchantsController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult<MerchantCreationRequest>> CreateMerchant([FromBody]MerchantCreationRequest request)
        {
            var response = await _authService.CreateMerchant(request);
            return Ok(response);
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticationResponse>> Authenticate([FromBody]AuthenticationRequest request)
        {
            var response = await _authService.Authenticate(request);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }
    }
}