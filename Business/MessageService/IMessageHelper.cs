using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.MessageService
{
    public interface IMessageHelper
    {
        Task<BaseResultDTO> LoginFCMTokenRequest(LoginDeviceRequestDTO request);
        Task<BaseResultDTO> LogoutFCMTokenRequest(LogoutDeviceRequestDTO request);
    }
}
