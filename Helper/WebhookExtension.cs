using Google.Cloud.Dialogflow.V2;
using System.Linq;

namespace WebhookDF.Helper
{
    public static class WebhookExtension
    {
        public static string ObterAction(this WebhookRequest webhook)
        {
            return webhook.QueryResult.Action;
        }

        public static bool ActionMatch(this WebhookRequest webhook, string action)
        {
            return ObterAction(webhook) == action;
        }

        public static string ObterParametro(this WebhookRequest webhook, string field)
        {
            return webhook.QueryResult.Parameters.Fields.Where(x => x.Key == field).FirstOrDefault().Value.ListValue.Values.FirstOrDefault().StringValue;
        }

        public static string ObterParametroContexto(this WebhookRequest webhook, string field)
        {
            return webhook.ObterContextos().First().Parameters.Fields[field].ListValue.Values[0].StringValue;
        }

        public static Google.Protobuf.Collections.RepeatedField<Context> ObterContextos(this WebhookRequest webhook)
        {
            return webhook.QueryResult.OutputContexts;
        }

        public static bool PossuiContexto(this WebhookRequest webhook, string contexto)
        {
            return webhook.ObterContextos().Any(x => x.ContextName.ContextId.Contains(contexto))
        }
    }
}
