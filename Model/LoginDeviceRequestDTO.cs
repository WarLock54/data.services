using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class LoginDeviceRequestDTO
    {
        public string UserId { get; set; }
        public string FcmToken { get; set; }
        public string SocketId { get; set; }
    }
}
