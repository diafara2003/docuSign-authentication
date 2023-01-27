﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System;
using DocuSignBL.Peticion;
using Model.DTO;
using Model.DTO.Users;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Docusign.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocusignController : ControllerBase
    {
        [HttpGet("docusign")]
        public IActionResult GetDocuSign()
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = "/Docusign/userinfo" });
        }

        [HttpGet("userInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            //if (!User.Identity.IsAuthenticated) return Challenge(new AuthenticationProperties() { RedirectUri = "/Docusign/userInfo" });

            //var auth = new PeticionDocusign().validationAuthentication();
            //if (!auth.isAuthenticated)
            //{
            //    Tuple<AuthenticationDTO, string> responseAuth = new Tuple<AuthenticationDTO, string>(auth, string.Empty);
            //    return Ok(responseAuth);
            //}
            try
            {
                var x = await new PeticionDocusign().peticion<userDTO>("users", HttpMethod.Get);

                return Ok(x);

            }
            catch (Exception e)
            {

                return Ok(e.Message);
            }

        }
        
        [HttpGet("Templates")]
        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                templatesDTO TemplatesArray = await new PeticionDocusign().peticion<templatesDTO>("templates?order_by=name", HttpMethod.Get);

                var TemplatesFilter = new List<envelopeTemplatesDTO>();

                foreach (var item in TemplatesArray.envelopeTemplates)
                {
                    if (item.name != "")
                    {
                        TemplatesFilter.Add(item);
                    }
                }

                var auth = new PeticionDocusign().validationAuthentication();
                Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>> responseAuth = new Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>>(auth, TemplatesFilter);

                return Ok(responseAuth);

            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

            }


        }
    }
}
