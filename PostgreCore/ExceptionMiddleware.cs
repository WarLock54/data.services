using System.Net;

namespace DataServiceMvc
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                //var request = context.Request;
                //if (context.Request.Method.Equals("POST") )
                //{
                //    request.EnableBuffering();

                //    var body = request.Body;

                //    var buffer = new byte[Convert.ToInt32(request.ContentLength)];

                //    await request.Body.ReadAsync(buffer, 0, buffer.Length);

                //    var bodyAsText = Encoding.UTF8.GetString(buffer);
                //    request.Body.Position = 0;

                //    string className = request.Path;
                //    className = className.Replace("/odata/", "");
                //    string pattern = @"\([^)]*\)";
                //    className = Regex.Replace(className, pattern, "");

                //    Type type = Type.GetType("DaBNet.Kamu.DB.EntityClasses." + className + ", DaBNet.Kamu.DB.Model");

                //    if(type == null)
                //    {
                //        type = Type.GetType("DaBNet.Kamu.DB.TypedViewClasses." + className + ", DaBNet.Kamu.DB.Model");
                //    }

                //    try
                //    {
                //        if (type == null)
                //        {
                //            throw new Exception("Type cannot found in solution.");

                //        }
                //        var entity = JsonConvert.DeserializeObject(bodyAsText, type);
                //        if (entity == null)
                //        {
                //            throw new Exception("Deserialization resulted in a null entity.");
                //        }

                //    }
                //    catch (JsonReaderException ex)
                //    {

                //        context.Response.StatusCode = 400;
                //        await context.Response.WriteAsync("Invalid JSON format in request body.");
                //        return;
                //    }

                //}

                await _next.Invoke(context);

            }
            catch (Exception e)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

               

                await context.Response.WriteAsJsonAsync(e);

            }

        }
    }
}
