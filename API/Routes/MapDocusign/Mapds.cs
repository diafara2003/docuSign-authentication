﻿using Docusign.Services;

namespace API.Routes.MapDocusign
{
    public static class Mapds
    {
        public static void RegisterDocusignDS(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/ds/callback",  (IDocusignCallbackService _docusignService, IWebHostEnvironment _webHostEnvironment, string code) =>
            {
                _docusignService.SaveTokenFile(code, _webHostEnvironment.ContentRootPath);

                return Results.Ok("Se autentico en DocuSign correctamente, puede cerrar esta ventana.");
            }).WithTags("Docusign Callback");


            app.MapGet("/api/ds/callback/verificacion", (IDocusignCallbackService _docusignService, IWebHostEnvironment _webHostEnvironment, string code) =>
            {
                return Results.Ok(_docusignService.ReadTokenFile(_webHostEnvironment.ContentRootPath));
            }).WithTags("Docusign Callback");
        }
    }
}
