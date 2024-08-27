using Microsoft.AspNetCore.Identity.Data;
using Mossad_API.Moddels.API_Moddels;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Mossad_API.Middleware
{

    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string SimulationServerToken;
        private readonly string MvcToken;
        public TokenValidationMiddleware(RequestDelegate next)
        {
            this._next = next;
        }
        public async Task Invoke(HttpContext context)
        {

        }
    }

}
