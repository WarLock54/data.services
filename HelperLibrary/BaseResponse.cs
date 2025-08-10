using FluentValidation.Results;
using ServiceStack;

namespace HelperLibrary
{
    public class BaseResponse
    {
        public bool Result { get; set; } = true;
        public string ErrorCode { set; get; }
        public string ErrorMessage { set; get; }
        public string[] Errors { set; get; }
        public string[] Warnings { set; get; }

        public string[] ErrorDetails { get; set; }

        public int TotalPages { set; get; }

        private int _totalCount;
        public int TotalCount
        {
            get { return _totalCount; }
            set { _totalCount = value; if (PageSize > 0) TotalPages = (int)Math.Ceiling((double)value / PageSize); }
        }
        public int PageSize { set; get; }
        public ValidationResult VError { get; set; }

        //public BaseResponse()
        //{
        //    TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);

        //}
        public BaseResponse()
        {
            Result = true;
        }
        public BaseResponse(bool result)
        {
            Result = result;
        }
        public BaseResponse(bool result, string errorCode, string errorMessage) //, string[] errors = null, string[] warnings = null, string[] errorDetails = null)
        {
            Result = result;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            //Errors = errors;
            //Warnings = warnings;
            //ErrorDetails = errorDetails;
        }
        public BaseResponse(Exception ex, bool includeInnerExc = false)
        {
            Result = false;
            if (ex != null)
            {
                ErrorCode = "Ex-" + ex.HResult.ToString();
                var exStr = ex.Message;
                if (includeInnerExc)
                    ;// exStr += " - " + ConvertEx(ex, includeInnerExc, includeEntityValidationErrors);
                ErrorMessage = (exStr).Trim();
            }
            else
            {
                ErrorCode = "Ex-Null";
                ErrorMessage = "Exception is Null";
            }
        }
        public BaseResponse SetError(Exception ex)
        {

            Result = false;
            if (ex != null)
            {
                ErrorCode = "Ex-" + ex.HResult.ToString();
                ErrorMessage = ex.Message;
                //if (includeInnerExc)
                //    ErrorMessage += " - " + ConvertEx(ex, includeInnerExc, includeEntityValidationErrors);
            }
            else
            {
                ErrorCode = "Ex-Null";
                ErrorMessage = "Exception is Null";
            }
            return this;
        }
        public BaseResponse SetError(string errorCode, string errorMessage)
        {

            Result = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            return this;
        }

        public BaseResponse SetResult(BsResult bsResult)
        {

            Result = bsResult == null ? false : bsResult.Result;
            if (!bsResult.Result)
            {
                ErrorCode = bsResult?.Error?.ErrorCode;
                ErrorMessage = bsResult?.Error?.ErrorDescription;
            }
            return this;
        }

        public void SetOK()
        {
            Result = true;
        }
        public string ErrorToString()
        {
            if (!Result)
                return "HataKod:" + ErrorCode + "; Hata:" + ErrorMessage;
            return "";
        }
        public void Inherit(BaseResponse bRes)
        {
            Result = bRes.Result;
            ErrorCode = bRes.ErrorCode;
            ErrorMessage = bRes.ErrorMessage;
        }
        public BaseResponse CopyBase(BaseResponse source)
        {
            this.Inherit(source);
            return this;
        }
    }
}
