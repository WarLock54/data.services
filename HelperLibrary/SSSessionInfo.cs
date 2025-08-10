using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class SSSessionInfo
    {
        // public string UserId { get; set; }
        public int UserId { get; set; }
        public long KisiKod { get; set; }
        //public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string IPAddress { get; set; }
        public string TokenId { get; set; }
        public string DeviceUUID { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string BirthDate { get; set; }
        public string City { get; set; }
        public string Company { get; set; }
        public string Dns { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string id { get; set; }
        public List<string> Roles { get; set; }
        public string Gender { get; set; }
        public string Username { get; set; }
        public string ProfileUrl { get; set; }
        public string ClientInfo { get; set; }

        public DateTime AuthTime { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<object> userClaims { get; set; }//ServiceStack.Property to object
        public AuthUserSession userSession { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public bool IsValidUser { get { return Username != "No_Username" || Username.IsNullOrEmpty(); } }
    }
}
