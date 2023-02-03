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
using DocuSignBL.Opetations;
using System.Linq;
using System.Xml.Linq;
using Model.DTO.Docusign;

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
                var auth = new PeticionDocusign().validationAuthentication();

                if (!auth.isAuthenticated)
                {
                    return Ok(new Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>>(auth, new List<envelopeTemplatesDTO>()));
                }


                templatesDTO TemplatesArray = await new PeticionDocusign().peticion<templatesDTO>("templates?order_by=name", HttpMethod.Get);

                var TemplatesFilter = new List<envelopeTemplatesDTO>();

                if (TemplatesArray.envelopeTemplates != null)
                    foreach (var item in TemplatesArray.envelopeTemplates)
                    {
                        if (item.name != "")
                        {
                            TemplatesFilter.Add(item);
                        }
                    }


                Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>> responseAuth = new Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>>(auth, TemplatesFilter);

                return Ok(responseAuth);

            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }


        [HttpGet("TemplatesSigners")]
        public async Task<IActionResult> GetTemplatesSigners()
        {
            try
            {
                var auth = new PeticionDocusign().validationAuthentication();

                if (!auth.isAuthenticated)
                {
                    return Ok(new Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>>(auth, new List<envelopeTemplatesDTO>()));
                }

                templatesDTO TemplatesArray = await new PeticionDocusign().peticion<templatesDTO>("templates?order_by=name&include=recipients,documents", HttpMethod.Get);
                var signers = TemplatesArray.envelopeTemplates;


                Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>> responseAuth = new Tuple<AuthenticationDTO, IList<envelopeTemplatesDTO>>(auth, signers);

                return Ok(responseAuth);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        [HttpGet("envelopes/recipents")]
        public async Task<IActionResult> GetRecipentsEnvelope(string envelope)
        {
            return Ok(await new DocuSignBL.Opetations.DocuSignBL().GetRecipentsEnvelope(envelope));
        }

        /// <summary>
        /// Metodo encargado de consultar los firmantes segun un template especifico
        /// </summary>
        /// <param name="idTemplate"></param>
        /// <returns></returns>
        [HttpGet("SignersByTemplete")]
        public async Task<IActionResult> GetSignersByTemplete(string idTemplate)
        {
            try
            {
                envelopeTemplatesDTO SignersDocuemnts = await new PeticionDocusign().peticion<envelopeTemplatesDTO>($"templates/{idTemplate}/signers?order_by=name", HttpMethod.Get);
                //var auth = new PeticionDocusign().validationAuthentication();
                //Tuple<AuthenticationDTO, envelopeTemplatesDTO> responseAuth = new Tuple<AuthenticationDTO,envelopeTemplatesDTO>(auth, SignersDocuemnts);

                return Ok(SignersDocuemnts);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }

        [HttpPost("envelopes/send")]
        public async Task<IActionResult> SendEnvelope(EnvelopeSendDTO envelope)
        {
            //EnvelopeResponse envelopeResponse2 = new EnvelopeResponse();

            //envelopeResponse2.envelopeId = "467489b2-fddc-4264-977c-fe4944806c71";
            //envelopeResponse2.uri = "/envelopes/467489b2-fddc-4264-977c-fe4944806c71";
            //envelopeResponse2.statusDateTime = "2023-05-05T16:33:16.2970000Z";
            //envelopeResponse2.status = "sent";



            //var auth2 = new PeticionDocusign().validationAuthentication();
            //Tuple<AuthenticationDTO, EnvelopeResponse> responseAuth2 = new Tuple<AuthenticationDTO, EnvelopeResponse>(auth2, envelopeResponse2);
            //return Ok(responseAuth2);

            try
            {
                templateDTO template = await new PeticionDocusign().peticion<templateDTO>("templates/" + envelope.IdTemplate, HttpMethod.Get);


                EnvelopeResponse envelopeResponse = new EnvelopeResponse();
                envelopeTemplatesDTO envelopeToSend = new envelopeTemplatesDTO();




                envelopeToSend.emailSubject = template.emailSubject;

                /*Se obtienen los documentos*/

                documentsDTO docu = new documentsDTO();
                List<documentsDTO> documents = new List<documentsDTO>();

                docu.documentBase64 = envelope.documentoBase64;
                docu.documentId = envelope.documentId;
                docu.fileExtension = envelope.fileExtension;
                docu.name = envelope.name;

                foreach (var doc in template.documents)
                {
                    if (doc.documentId != envelope.documentId)
                    {
                        documentsDTO fileBase64 = await new PeticionDocusign().peticionFile<documentsDTO>("templates/" + envelope.IdTemplate + "/documents/" + doc.documentId, HttpMethod.Get);
                        doc.documentBase64 = fileBase64.documentBase64;
                        doc.fileExtension = "pdf";
                        documents.Add(doc);
                    }                   
                }

                documents.Add(docu);

                envelopeToSend.documents = documents;

                EnvelopeResponse envelopeResponse2 = new EnvelopeResponse();

                envelopeResponse2.envelopeId = "467489b2-fddc-4264-977c-fe4944806c71";
                envelopeResponse2.uri = "/envelopes/467489b2-fddc-4264-977c-fe4944806c71";
                envelopeResponse2.statusDateTime = "2023-05-05T16:33:16.2970000Z";
                envelopeResponse2.status = "sent";



                var auth2 = new PeticionDocusign().validationAuthentication();
                Tuple<AuthenticationDTO, EnvelopeResponse> responseAuth2 = new Tuple<AuthenticationDTO, EnvelopeResponse>(auth2, envelopeResponse2);
                return Ok(responseAuth2);

                /*Se obtienen los firmantes*/

                /*Se obtienen firmantes docising*/

                List<signersDTO> signers = new List<signersDTO>();

                signers.AddRange((from item in template.recipients.signers
                                  where item.email != "" && item.name != "" && item.roleName != "" && !item.roleName.ToLower().Contains("contratista")
                                  select new signersDTO
                                  {
                                      email = item.email,
                                      name = item.name,
                                      recipientId = item.recipientId,
                                      routingOrder = item.routingOrder,
                                      tabs = new tabsDTO
                                      {
                                          signHereTabs = new List<signHereDTO>() {new signHereDTO(){
                                                                                       anchorString = string.Concat("/" + item.roleName.Replace(' ', '_')),
                                                                                       anchorYOffset = "-6",
                                                                                       name = item.name,
                                                                                       optional = "false",
                                                                                       recipientId = item.recipientId,
                                                                                       scaleValue = "1"
                                                                                    }
                                          },
                                      }

                                  }).ToList());

                /*Se obtienen contratista*/

                signersDTO contratista = new signersDTO();

                contratista = template.recipients.signers.Where(c => c.roleName.ToLower().Contains("contratista")).FirstOrDefault();

                contratista.email = envelope.emailTer;
                contratista.name = envelope.nameTer;
                contratista.tabs.signHereTabs = new List<signHereDTO>(){new signHereDTO(){
                                                                                       anchorString = string.Concat("/contratista"),
                                                                                       anchorYOffset = "-6",
                                                                                       name = envelope.nameTer,
                                                                                       optional = "false",
                                                                                       recipientId = contratista.recipientId,
                                                                                       scaleValue = "1"
                                                                                    }
                };

                signers.Add(contratista);
                envelopeToSend.recipients.signers = signers;

                /*Se obtienen los usuarios para copias*/

                foreach (var carbonCopies in template.recipients.carbonCopies)
                {
                    if (carbonCopies.email != "" && carbonCopies.name != "")
                    {
                        envelopeToSend.recipients.carbonCopies.Add(carbonCopies);
                    }
                }

                envelopeToSend.status = "sent";


                envelopeResponse = await new PeticionDocusign().peticion<EnvelopeResponse>("envelopes", HttpMethod.Post, envelopeToSend);

                var auth = new PeticionDocusign().validationAuthentication();
                Tuple<AuthenticationDTO, EnvelopeResponse> responseAuth = new Tuple<AuthenticationDTO, EnvelopeResponse>(auth, envelopeResponse);

                return Ok(responseAuth);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }



        /// <summary>
        /// Metodo encargado de consultar los firmantes segun un template especifico
        /// </summary>
        /// <param name="idTemplate"></param>
        /// <returns></returns>
        [HttpGet("envelopes/history")]
        public async Task<IActionResult> GetEnvelopeHistory(string idenvelope)
        {
            try
            {
                var auth = new PeticionDocusign().validationAuthentication();
                if (!auth.isAuthenticated)
                {
                    return Ok(new Tuple<AuthenticationDTO, ResponseDocusignAuditoriaDTO>(auth, new ResponseDocusignAuditoriaDTO()));
                }

                DocusignAuditoriaDTO SignersDocuemnts = await new PeticionDocusign().peticion<DocusignAuditoriaDTO>($"envelopes/{idenvelope}?include=documents,recipients", HttpMethod.Get);



                EnvelopeDocusignAudit EnvelopeAudit = await new PeticionDocusign().peticion<EnvelopeDocusignAudit>($"envelopes/{idenvelope}/audit_events", HttpMethod.Get);


                Tuple<AuthenticationDTO, ResponseDocusignAuditoriaDTO> responseAuth = new Tuple<AuthenticationDTO, ResponseDocusignAuditoriaDTO>(auth, new ResponseDocusignAuditoriaDTO()
                {
                    encabezado = new ResponseEncabezadoAuditDTO()
                    {
                        documentos = SignersDocuemnts.envelopeDocuments != null
                            ? string.Join(", ", SignersDocuemnts.envelopeDocuments.Select(c => c.name))
                            : "",
                        asunto = SignersDocuemnts.emailSubject,
                        destinatarios = SignersDocuemnts.recipients != null && SignersDocuemnts.recipients.signers != null
                            ? string.Join(", ", SignersDocuemnts.recipients.signers.Where(c => c.recipientType == "signer").Select(c => c.name))
                            : "",
                        fechaCreacion = SignersDocuemnts.createdDateTime.ToString("MM/dd/yyyy h:mm tt"),
                        fechaEnvio = SignersDocuemnts.sentDateTime.ToString("MM/dd/yyyy h:mm tt"),
                        estado = StatusEnvelope(SignersDocuemnts.status)
                    },
                    detalles = GenerateDetail(EnvelopeAudit)

                });

                return Ok(responseAuth);
            }
            catch (Exception e)
            {
                return Ok(new Tuple<AuthenticationDTO, ResponseDocusignAuditoriaDTO>(new AuthenticationDTO() { isAuthenticated = true }, new ResponseDocusignAuditoriaDTO()));
            }
        }

        List<ResponseDetalleAuditDTO> GenerateDetail(EnvelopeDocusignAudit data)
        {
            List<ResponseDetalleAuditDTO> objLst = new List<ResponseDetalleAuditDTO>();


            if (data.auditEvents == null) return objLst;


            data.auditEvents.ForEach(c =>
            {
                var accion = c.eventFields.FirstOrDefault(e => e.name.ToLower().Equals("action")) ?? new EventFieldsDocuSignDTO() { };
                var user = c.eventFields.FirstOrDefault(e => e.name.ToLower().Equals("username")) ?? new EventFieldsDocuSignDTO() { };
                var status = c.eventFields.FirstOrDefault(e => e.name.ToLower().Equals("envelopestatus")) ?? new EventFieldsDocuSignDTO() { };
                var date = c.eventFields.FirstOrDefault(e => e.name.ToLower().Equals("logtime")) ?? new EventFieldsDocuSignDTO() { };
                var message = c.eventFields.FirstOrDefault(e => e.name.ToLower().Equals("message")) ?? new EventFieldsDocuSignDTO() { };


                objLst.Add(new ResponseDetalleAuditDTO()
                {
                    accion = accion != null ? accion.value : string.Empty,
                    actividad = message != null ? message.value : string.Empty,
                    estado = status != null ? status.value : string.Empty,
                    hora = status != null ? Convert.ToDateTime(date.value).ToString("MM/dd/yyyy h:mm tt") : string.Empty,
                    usuario = user != null ? user.value : string.Empty,
                });

            });

            return objLst;
        }


        string StatusEnvelope(string name)
        {
            string statrusName = string.Empty;

            if (name == "sent") statrusName = "Enviado";
            else statrusName = "Completado";

            return statrusName;
        }
    }
}

