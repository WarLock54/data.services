using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;

namespace ServicePackage
{
    public class CustomJwtAuthProvider : JwtAuthProvider, IAuthWithRequest
    {
      
        public CustomJwtAuthProvider(IAppSettings appSettings) : base(appSettings)
        {
        }

        public CustomJwtAuthProvider() : base()
        {
        }

        public new const string Name = "jwt2";

        public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
        {
            return true;
        }
    }
}
