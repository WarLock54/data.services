using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class LogoutDeviceRequestDTO
    {
        public string UserId { get; set; } = null;
        public string DeviceUUID { get; set; }
    }
}
