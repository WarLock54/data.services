using AutoMapper;
using Business;
using HelperLibrary;
using Microsoft.Extensions.Logging;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    
    public class MessageServiceUtil
    : IMessageServiceUtil
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MessageServiceUtil> _logger;

        public MessageServiceUtil(
                                     IMapper mapper,
                                     ILogger<MessageServiceUtil> logger

            )
        {
            _mapper = mapper;

            _logger = logger;

     }
        public async Task<string> BindMessageParamsAsync(string messageText, Dictionary<string, string> parameters)
        {
            string returnText = messageText;

            if (!String.IsNullOrWhiteSpace(messageText) &&
                messageText.IndexOf('@') != -1 &&
                parameters?.Count > 0)
            {
                foreach (var param in parameters)
                {
                    if (String.IsNullOrWhiteSpace(param.Key)) continue;

                    returnText = returnText.Replace("@" + param.Key + "@", param.Value ?? "");
                }
            }

            return returnText;
        }
        public async Task<MesajSablon> GetMessageTemplateAsync(MessageTemplateTypes templateType, string templateCode)
        {
            using var yBs = MesajSablonBs.GetInstance(null, null);
            var template = await yBs.GetTemplateAsync(templateType, templateCode); ;
            return template;
        }
        public async Task<MesajSablon> GetMessageTemplateWidthBindAsync(MessageTemplateTypes messageTemplateType, string templateCode, Dictionary<string, string> @params)
        {
            MesajSablon template =  await GetMessageTemplateAsync(messageTemplateType, templateCode);
            if (template != null && @params?.Count > 0)
            {
                template.Konu = await BindMessageParamsAsync(template.Konu, @params);
                template.Mesaj = await BindMessageParamsAsync(template.Mesaj, @params);
            }

            return template;
        }
    }
    
}
