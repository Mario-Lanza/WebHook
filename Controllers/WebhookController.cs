using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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

            var request = _jsonParser.Parse<WebhookRequest>(dados.GetRawText());
            var response = new WebhookResponse();
            if (request != null)
            {
                var action = request.QueryResult.Action;

                if (action == "ActionTesteWH")
                {
                    response.FulfillmentText = "Teste agora";
                }

                if (action == "CursoOferta")
                {
                    string parametroCurso = ObterParametro(request, "Curso");

                    var curso = new DAL.CursoDAL().ObterCurso(parametroCurso);


                    response.FulfillmentText = curso != null ? "Sim temos." : "Não temos";
                }

                if (action == "CursoValor")
                {
                    var contexto = request.QueryResult.OutputContexts;
                    var contextCurso = contexto[0].Parameters.Fields["Curso"];
                    string curso = contextCurso.ListValue.Values[0].StringValue;

                    var dado = new DAL.CursoDAL();

                    var cursoDado = dado.ObterCurso(curso);

                    if (contexto.Any(x => x.ContextName.ContextId.Contains("ctxcurso")) && cursoDado != null)
                    {
                        response.FulfillmentText = $"A mensalidade para {cursoDado.Nome} é {cursoDado.Preco}.";
                    }
                }
            }

            return Ok(response);
        }

        private static string ObterParametro(WebhookRequest request, string parametro)
        {
            return request.QueryResult.Parameters.Fields.Where(x => x.Key == parametro).FirstOrDefault().Value.ListValue.Values.First().StringValue;
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
