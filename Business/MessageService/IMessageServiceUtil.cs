using HelperLibrary;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Business
{
    public partial interface IMessageServiceUtil
    {
        Task<MesajSablon> GetMessageTemplateAsync(MessageTemplateTypes templateType, string templateCode);
        Task<string> BindMessageParamsAsync(string messageText, Dictionary<string, string> parameters);
        Task<MesajSablon> GetMessageTemplateWidthBindAsync(MessageTemplateTypes messageTemplateType, string templateCode, Dictionary<string, string> @params);
    }
}
