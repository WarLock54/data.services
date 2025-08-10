
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using ServiceStack.FluentValidation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;
namespace HelperLibrary
{
    public class BsResult
    {
        public bool Result { get; set; }  
        public string Message { get; set; }  
        public Error Error { get; set; }

        public DValidationResult? DVError { get; set; }

        public IList Value { get; set; }

        public ValidationResult? VError { get; set; }
        public BsResult()
        {
            Result = true;
        }
        public BsResult(Exception exception)
        {
            Result = false;
            Error = new Error(exception);
        }
        public BsResult(bool result ,string resMsg="",string errCode="",string errMsg="",Exception ex = null)
        {
            Result= result;
            Message = resMsg;
            Error = new Error(errCode, errMsg, ex);
        }
        public BsResult(ValidationResult _VError)
        {
            VError = _VError;
            //var error = string.Join("; ", _VError.Errors.Select(failure => failure.PropertyName + " - " + failure.ErrorMessage));
            Error = new Error() { ErrorCode = "ValidationError", ErrorDescription = "Kayıt Öncesi Kontrol (FValidation) Hataları mevcut " }; //+ error };
            Result = false;
        }
    }
    public class BsResult<E> : BsResult
    {
        public E Value { get; set; }
        public List<E> ValueList { get; set; }
        public int TotalCount { get; set; }
        public ValidationResult VError { get; set; }
        public BsResult(ValidationResult _VError)
        {
            VError = _VError;
            Error = new Error { ErrorCode = "ValidationErrur", ErrorDescription = "Kayıt öncesi Validation Hataları" };
            Result = false;
        }
        public BsResult(DValidationResult _dvError)
        {
            DVError = _dvError;
            //var error = string.Join("; ", _VError.Errors.Select(failure => failure.PropertyName + " - " + failure.ErrorMessage));
            Error = new Error() { ErrorCode = "DValidationError", ErrorDescription = "Kayıt Öncesi Kontrol (DValidation) Hataları mevcut " }; //+ error };
            Result = false;
        }
        public BsResult(bool result, string resMsg = "", string errCode = "", string errMsg = "", Exception ex = null)
        {
            Result = result;
            Message = resMsg;
            if (!result || ex is not null)
                Error = new Error(errCode, errMsg, ex);
        }

        public BsResult(bool result,E value,string errMsg="",string errCode="",string errMesage="",Exception ex=null)
        {
                Result =(result);
                Message = errMsg;
                Value = value;
                if (!result || ex is not null)
                    Error = new Error(errCode, errMsg, ex);
        }
        public BsResult(bool result,List<E> valueList,int totalCount ,string resMsg = "", string errCode = "", string errMsg = "", Exception ex = null)
        {
            Result = (result);
            Message = resMsg;
            ValueList = valueList;
            TotalCount = totalCount;
            if (!result || ex is not null)
                Error = new Error(errCode, errMsg, ex);
        }


        public BsResult(BsResult res, E value)
        {
            Result = res.Result;
            Message = res.Message;
            Value = value;
            Error = res.Error;
        }
        public BsResult(BsResult res)
        {
            Result = res.Result;
            Message = res.Message;
            Error = res.Error;
        }

        public BsResult(Exception ex)
        {
            Result = false;
            //Message = ex.Message;
            if (ex is not null)
                Error = new Error(ex);
        }
    }
    public class Error
    {
        public bool ErrorYn { get; set; }
        public bool ExYn { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public Exception ex { get; set; }
        public Error()
        {
        }
        public Error(string ErrorCode,string errDescription)
        {
            ErrorYn = true;
            ErrorCode = ErrorCode;
            ErrorDescription=errDescription;
        }
        public Error(Exception ex,bool includeInnerExc=true,bool includeEntityValidator=true)
        {
            ErrorYn = true;
            ExYn = (ex!=null);
            if (ex != null)
            {
                ErrorCode = ex.HResult.ToString();
                var exStr = ex.Message;
                if (includeInnerExc)
                    exStr += " - " + ConvertEx(ex, includeInnerExc, includeEntityValidator);
                ErrorDescription = (exStr).Trim();
            }
            else
            {
                ErrorCode = "NullEx";
                ErrorDescription = "Exception is Null";
            }
        }
        public Error(string errCode,string errDescription,Exception ex,bool includeInnerExc=true,bool includeEntityValidator = true)
        {
            ErrorYn = true;
            if (string.IsNullOrEmpty(errCode) && ex != null)
                ErrorCode = ex.HResult.ToString();
            else
                ErrorCode= errCode;
            if(ex!=null)
            {
                var exStr = ConvertEx(ex, includeInnerExc, includeEntityValidator);
                ErrorDescription=("Hata:"+errDescription+" "+exStr).Trim();
            }
            else
                ErrorDescription = ("Hata:" + errDescription).Trim();
        }
        public string ConvertEx(Exception ex, bool includeInnerExc, bool includeEntityValidator = true)
        {
            if (ex == null) return "ex is null";
            var exstr = ex.Message;
            if (ex.GetType() == typeof(DbUpdateException)) {
                string rs = "";
                exstr += ": "+rs;
            }
            if (includeInnerExc && ex.InnerException != null) { 
                var exInnerStr=ConvertEx(ex,includeInnerExc,includeEntityValidator);
                if (exInnerStr != null) {
                    exstr = exstr + ";Inner:" + exInnerStr;
                }
            }
            return exstr.Trim();
        }
    }
}
