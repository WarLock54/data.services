using Business;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.Helpers;
using FluentValidation.Results;
using HelperLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Model;
using Newtonsoft.Json;
using ServiceStack;
using ServiceStack.Logging;
using SimplePatch;
using System.Collections.ObjectModel;
using System.Web;

namespace PostgreCore
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class DaBDataController<TEntity, P, TBS> : Controller
       where TEntity : class, new()
       where TBS : Core<TEntity, P>, new()
    {
        private NLog.Logger _logger { get { return LogUtil._odataLogger; } }

        private TBS _bs = null; // new TBS();

        internal void SetTBS(TBS value)
        {
            if (_bs != value)
            {
                _bs = value;
            }
        }

        private TBS CreateBs()
        {
            if (_bs != null)
            {
                _bs.Session = Session;
                _bs.DataServiceYn = true;
                return _bs;
            }


            var __bs = new TBS();
            __bs.Session = Session;
            return __bs;
        }

        //redis
        private readonly IRedisService<TEntity> _redisService;

        public DaBDataController(IRedisService<TEntity> redisService = null)
        {
            _redisService = redisService;
        }
        #region Redis Opsiyonel Metodlar

        [HttpGet("Redis/{key}")]
        public async Task<IActionResult> GetFromRedis(string key)
        {
            if (_redisService == null)
                return NotFound("Redis servis aktif değil.");

            var entity = await _redisService.FindAsync(key);
            if (entity == null)
                return NotFound();
            return GetJsonResult(entity);
        }

        [HttpPost("Redis")]
        public async Task<IActionResult> AddToRedis([FromBody] TEntity data)
        {
            if (_redisService == null)
                return NotFound("Redis servis aktif değil.");
            if (data == null)
                return BadRequest("Key ve entity zorunludur.");

            await _redisService.AddAsync(data);
            return Ok();
        }

        [HttpPut("Redis/{key}")]
        public async Task<IActionResult> UpdateRedis(string key, [FromBody] TEntity entity)
        {
            if (_redisService == null)
                return NotFound("Redis servis aktif değil.");
            if (entity == null)
                return BadRequest("Entity boş olamaz.");

            await _redisService.UpdateAsync(key, entity);
            return Ok();
        }

        [HttpPatch("Redis/{key}")]
        public async Task<IActionResult> PatchRedis(string key, [FromBody] Delta<TEntity> entityPatch)
        {
            if (_redisService == null)
                return NotFound("Redis servis aktif değil.");
            if (entityPatch == null)
                return BadRequest("Patch veri boş olamaz.");

            var existingEntity = await _redisService.FindAsync(key);
            if (existingEntity == null)
                return NotFound();

            entityPatch.Patch(existingEntity);
            await _redisService.UpdateAsync(key, existingEntity);

            return Ok();
        }

        [HttpDelete("Redis/{key}")]
        public async Task<IActionResult> DeleteFromRedis(string key)
        {
            if (_redisService == null)
                return NotFound("Redis servis aktif değil.");

            await _redisService.DeleteAsync(key);
            return Ok();
        }

        #endregion
        private SSSessionInfo _session; // new TBS();
        internal SSSessionInfo Session
        {
            get
            {
                if (_session == null)
                {
                    _session = SessionHelper.GetWebApiSessionInfo(this.HttpContext);
                }
                return _session;
            }
        }
        //bu istek  içerisinde tablonun özelliklerini sağlayan verileri filtreler ve listeler 
        [HttpGet]
        public async Task<IActionResult> Get(DataSourceLoadOptions loadOptions)
        {
            try
            {
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs = qs;
                var status = !qs["status"].ToString().IsNullOrEmpty() ? qs["status"].ToString().ToUpper() : "";
                if(status == "A")
                {
                    return GetJsonResult(tBs.GetAllowedActions());
                }
                loadOptions.StringToLower = true;
                var dbSet = tBs.GetQueryableList(null, null);
                var result= await DataSourceLoader.LoadAsync(dbSet, loadOptions);
                return GetJsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Get-" + typeof(TEntity).Name);
                return GetError_Exception(ex);
            }
        }
        // id sini gönder obje olarak geri dönsün.
        [HttpGet]
        public async Task<IActionResult> Get(P key)
        {
            try
            {
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs = qs;

                var status = !qs["status"].ToString().IsNullOrEmpty() ? qs["status"].ToString().ToUpper() : " ";
                bool allowedAction = false;
                Dto dto = null;
                if (status == "A" || status == "E") {
                    dto = tBs.GetAllowedActions(key);
                    if (status == "A")
                        return GetJsonResult(dto);
                    allowedAction= true;
                }
                var entityQuery = tBs.GetSingleByPk(key);
                if (entityQuery == null) {
                    return CreateUnprocessableEntity(new DataError
                    {
                        Target = "Entity",
                        Code = "NotFound",
                        Message = "Kayıt bulunamadı.",
                        Details = null
                    });
                }
                if(allowedAction == true)
                {
                    return GetJsonResult(new DataEntityDto { AllowedActions=dto ?? tBs.GetAllowedActions(key), Entity =  entityQuery });
                }
                return GetJsonResult(entityQuery);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Get-" + typeof(TEntity).Name + " Key: " + key.ToString());
                return GetError_Exception(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]TEntity entity)
        {
            try
            {
                if (entity == null)
                    return GetError_NullEntity("Post");
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs = qs;
                var status = !qs["status"].ToString().IsNullOrEmpty() ? qs["status"].ToString().ToUpper() : "";
                bool allowedAction = false;
                Dto dto = null;
                if(status == "V")
                {
                    var vRes=await tBs.Validate(entity, DBActions.Insert);
                    if (vRes.Result)
                        return Created("Valid", vRes.Value);
                    else
                        return GetError_BsResult(vRes);
                }
                else if (status == "A")
                {
                    return GetJsonResult(tBs.GetAllowedActions());
                }

                var res= await tBs.AddAsync(entity);
                if (res.Result)
                    return GetJsonResult(res.Value);
                else
                {
                    return GetError_BsResult(res);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "post-" + typeof(TEntity).Name);
                return GetError_Exception(ex);
            }
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Put(Delta<TEntity> entity,P key)
        {
            try
            {
                if (entity == null)
                    return GetError_NullEntity("Put");
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs= qs;
                var originalEntity = tBs.GetSingleByPk(key);
                if (originalEntity == null)
                {
                    return CreateUnprocessableEntity(new DataError
                    {
                        Target = "Entity",
                        Code = "NotFound",
                        Message = "Kayıt bulunamadı.",
                        Details = null
                    });
                }
                entity.Patch(originalEntity);
                var status = !qs["status"].ToString().IsNullOrEmpty() ? qs["status"].ToString().ToUpper() : "";  // Get Allowed Actions
                if (status == "V")
                {
                    var vRes = await tBs.Validate(originalEntity, DBActions.Update);
                    if (vRes.Result)
                        return GetJsonResult(originalEntity);
                    else
                        return GetError_BsResult(vRes);
                }
                else if(status == "A")
                {
                    return GetJsonResult(tBs.GetAllowedActions(key));
                }
                var res = await tBs.UpdateSync(originalEntity).ConfigureAwait(true);
                if (res.Result)
                    return GetJsonResult(originalEntity);
                else
                    return GetError_BsResult(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Put" + typeof(TEntity).Name);
                return GetError_Exception(ex);
            }
        }

        [HttpPatch("{key}")]
        public async Task<IActionResult> Patch(Delta<TEntity> entity,P key)
        {
            try
            {
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs = qs;
                var originalEntity=tBs.GetSingleByPk(key);
                if (originalEntity == null)
                    return CreateUnprocessableEntity(new DataError
                    {
                        Target = "Entity",
                        Code = "NotFound",
                        Message = "Kayıt bulunamadı.",
                        Details = null
                    });
                entity.Patch(originalEntity);
                var res = await tBs.UpdateSync(originalEntity);
                if (res.Result)
                    return GetJsonResult(originalEntity);
                else 
                    return GetError_BsResult(res);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Patch" + typeof(TEntity).Name);
                return GetError_Exception(ex);
            }
        }
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(P key)
        {
            try {
                using var tBs = CreateBs();
                var qs = HttpUtility.ParseQueryString(Request.QueryString.Value ?? "");
                tBs.qs = qs;
                var res = await tBs.RemoveAsync(key).ConfigureAwait(true);
                if (res.Result)
                    return Ok();
                else if (res.Error.ErrorCode == "NotFound")
                {
                    return CreateUnprocessableEntity(new DataError
                    {
                        Target = "Entity",
                        Code = "NotFound",
                        Message = "Kayıt bulunamadı.",
                        Details = null
                    });
                }
                else if (res.Error.ErrorCode == "Unauthorized")
                {
                    return Unauthorized();
                }
                else
                {
                    return GetError_BsResult(res);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Delete" + typeof(TEntity).Name + " Key: " + key.ToString());
                return GetError_Exception(ex);
            }
        }
        // helper metots
            #region helper metots
        private JsonResult GetJsonResult(object? data)
        {
            return Json(data, new JsonSerializerSettings()
            {
                //NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                ,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
                ,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
                ,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss"
            });
        }
        protected UnprocessableEntityObjectResult GetError_NullEntity(string method)
        {
            _logger.Error(method + "-" + typeof(TEntity).Name);
            return CreateUnprocessableEntity(new DataError() { Code = "NullEntity", Message = "Entity Boş. JSON çevirim hatası olabilir." });
        }

        protected UnprocessableEntityObjectResult GetError(string code, string message)
        {
            return CreateUnprocessableEntity(new DataError() { Code = code, Message = message });
        }
        protected UnprocessableEntityObjectResult GetError_BsResult(BsResult res)
        {
            Collection<DataErrorDetail> detail = new Collection<DataErrorDetail>();
            if (res.VError != null && res.VError.Errors.Count > 0)
                foreach (ValidationFailure failure in res.VError.Errors)
                    detail.Add(new DataErrorDetail() { Target = failure.PropertyName, Code = failure.ErrorCode, Message = failure.ErrorMessage });

            if (res.DVError != null && res.DVError.Errors.Count > 0)
                foreach (DValidationFailure failure in res.DVError.Errors)
                    detail.Add(new DataErrorDetail() { Target = failure.PropertyName, Code = failure.ErrorCode, Message = failure.ErrorMessage });
            // Hata mesajını işle
            string errorDescription = res.Error.ErrorDescription;
            return CreateUnprocessableEntity(new DataError()
            {
                Target = "entity",
                Code = res.Error.ErrorCode,
                Message = errorDescription,
                Details = detail.Count > 0 ? detail : null
            });
        }
        protected UnprocessableEntityObjectResult GetError_BsResult(BsResult<TEntity> res)
        {
            Collection<DataErrorDetail> detail = new Collection<DataErrorDetail>();
            if (res.VError != null && res.VError.Errors.Count > 0)
                foreach (ValidationFailure failure in res.VError.Errors)
                    detail.Add(new DataErrorDetail() { Target = failure.PropertyName, Code = failure.ErrorCode, Message = failure.ErrorMessage });

            if (res.DVError != null && res.DVError.Errors.Count > 0)
                foreach (DValidationFailure failure in res.DVError.Errors)
                    detail.Add(new DataErrorDetail() { Target = failure.PropertyName, Code = failure.ErrorCode, Message = failure.ErrorMessage });
            // Hata mesajını işle
            string errorDescription = res.Error.ErrorDescription;
            return CreateUnprocessableEntity(new DataError()
            {
                Target = "entity",
                Code = res.Error.ErrorCode,
                Message = errorDescription,
                Details = detail.Count > 0 ? detail : null
            });
        }
        protected UnprocessableEntityObjectResult GetError_Exception(Exception e)
        {
            //_logger.Error(e, method + "-" + typeof(TEntity).Name
            //        + (key.IsNullOrEmpty() ? "" : "(" + key.ToString() + ")")
            //        + (entity is null ? "" : entity.ToString())
            //        );
            return CreateUnprocessableEntity(new DataError() { Code = "Ex", Message = e.Message, Target = "Exception" });
            //return DataErrorResult(new DataError() { ErrorCode = e.ToErrorCode(), Message = e.Message, Target = "Exception" });
        }

        protected UnprocessableEntityObjectResult GetError_ValidationResult(Collection<DataErrorDetail> detail)
        {
            return CreateUnprocessableEntity(new DataError() { Target = "entity", Code = "AlternateKey", Message = "Bazı Değerler Değiştirilemez.", Details = detail.Count > 0 ? detail : null });
        }

        private UnprocessableEntityObjectResult CreateUnprocessableEntity(DataError err)
        {
            return UnprocessableEntity(new UnprocessableEntityObject(err));
        }
        #endregion
    }
    [ModelBinder(BinderType = typeof(DataSourceLoadOptionsBinder))]
    public class DataSourceLoadOptions : DataSourceLoadOptionsBase
    {
    }
    public class DataSourceLoadOptionsBinder : Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder
    {

        public Task BindModelAsync(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext bindingContext)
        {
            var loadOptions = new DataSourceLoadOptions();
            DataSourceLoadOptionsParser.Parse(loadOptions, key => bindingContext.ValueProvider.GetValue(key).FirstOrDefault());
            bindingContext.Result = Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.Success(loadOptions);
            return Task.CompletedTask;
        }

    }
    public class DataEntityDto
    {
        public object Entity { set; get; } = null;
        public Dto AllowedActions { set; get; } = null;
    }
}
