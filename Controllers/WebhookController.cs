using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebhookDF.Helper;

namespace WebhookDF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private static readonly JsonParser _jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        System.Text.Json.JsonSerializerOptions _jsonSetting = new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public WebhookController()
        {
        }


        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { msg = "deu certo" });
        }

        [HttpPost]
        public ActionResult GetWebhookResponse([FromBody] System.Text.Json.JsonElement dados)
        {
            if (!Autorizado(Request.Headers)) return StatusCode(StatusCodes.Status401Unauthorized);

            var request = ObterRequest(dados);
            var response = new WebhookResponse();
            if (request != null)
            {
                var action = request.ObterAction();

                if (request.ActionMatch("ActionTesteWH"))
                {
                    response.FulfillmentText = "Teste funcionou";
                }

                if (request.ActionMatch("CursoOferta"))
                {
                    string parametroCurso = request.ObterParametro("Curso");

                    response.FulfillmentText = parametroCurso != null ? "Encontrou Parametro." : "Não encontrou";
                }

                if (request.ActionMatch("CursoValor"))
                {
                    var parametroContexto = request.ObterParametroContexto("Curso");

                    if (request.PossuiContexto("ctxcurso") && parametroContexto != null)
                    {
                        response.FulfillmentText = $"Possui o contexto e o parametro veio {parametroContexto}";
                    }
                }
            }

            return Ok(response);
        }

        private static WebhookRequest ObterRequest(System.Text.Json.JsonElement dados)
        {
            return _jsonParser.Parse<WebhookRequest>(dados.GetRawText());
        }

        private bool Autorizado(IHeaderDictionary httpHeader)
        {

            string basicAuth = httpHeader["Authorization"];

            if (!string.IsNullOrEmpty(basicAuth))
            {
                basicAuth = basicAuth.Replace("Basic ", "");

                byte[] aux = System.Convert.FromBase64String(basicAuth);
                basicAuth = System.Text.Encoding.UTF8.GetString(aux);

                if (basicAuth == "nome:token")
                    return true;
            }

            return false;
        }



    }
}
