using AutoMapper;
using FluentValidation;
using Funq;
using HelperLibrary;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Model;
using Persistence;
using ServiceStack;
using ServiceStack.FluentValidation.Resources;
using ServiceStack.Text;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Business
{
    public class Core : IDisposable
    {
        public IMapper mapper = MapUtil.Mapper;
        internal bool internalUpdate=false;
        private ApplicationDbContext _context;
        public bool DataServiceYn = false;

        public ApplicationDbContext context
        {
            get
            {
                if (_context == null)
                {
                    _context = new ApplicationDbContext();
                }
                return _context;
            }
            set
            {
                _context = value;
            }
        }
        public ApplicationDbContext GetContext()
        {
            return new ApplicationDbContext();
        }
        private SSSessionInfo _session;
        public SSSessionInfo Session
        {
            get
            {
                if (_session == null)
                    _session = new SSSessionInfo() { UserId = -1, IPAddress = "1.1.1.1" };
                return _session;
            }
            set
            {
                _session = value;
                //if (_session != null && _session.UserId == 0)
                //    SessionHelper.GetKullaniciKod(context, _session, false); new List<string> { "EBelediyeClient", "belediyemobileapp" }.Contains( _session.ClientInfo) ? true : false);
            }
        }

        public BsResult BsError(string errorMessage, string errorCode = "")
        {
            return new BsResult(false, "Exception", errorCode, errorMessage);
        }

        public BsResult BsError(Exception ex, string source = "", string errorCode = "")
        {
            BsResult result;
            result = new BsResult(false, "İşlem Sırasında Hata oluştu", errorCode, string.Concat(source, ":", ex.Message), ex: ex);
            return result;
        }

        public BsResult BsOk()
        {
            return new BsResult(true);
        }

        public BsResult BsOk(string resMsg)
        {
            return new BsResult(true, resMsg, null, null);
        }
        protected bool _disposed;
        protected bool _hasPrivateContext = true;
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // No need to call finalizer
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                if (_hasPrivateContext && context != null)
                {
                    context.Dispose();
                }
                if (Session is IDisposable disposableSession)
                    disposableSession.Dispose();
            }
            // No unmanaged resources, but if added in future, clean up here
            _disposed = true;
        }
    }
    public class Core<E, P> : Core, IDisposable where E : class, new()
    {
        protected static readonly ConcurrentDictionary<Type, string> _pkNameCache = new();

        public static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public IStringLocalizer _localizer;
        public ApplicationDbContext context { get; set; }
        public SSSessionInfo Ssesion { get; set; }

        internal bool ConvertDateTimeToDate = true;

        protected static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyInfoCache = new();
        // private static PropertyInfo[] _entityProperties = null;
        private static PropertyInfo[] EntityProperties
        {
            get
            {
                return _propertyInfoCache.GetOrAdd(typeof(E), type =>
                {
                    return typeof(E).GetProperties();
                });

                //if (_properties == null)
                //    _properties = typeof(E).GetProperties();
                //return _properties;
            }
        }

        public Core()
        {
            _localizer = new CustomLocalizer();
        }
        public Core(SSSessionInfo _ssesion)
        {
            Ssesion = _ssesion;
        }
        public Core(SSSessionInfo _ssesion,ApplicationDbContext ApplicationDbContext)
        {
            if(_ssesion==null)
                Ssesion = _ssesion;
            if(ApplicationDbContext==null)
                context= ApplicationDbContext;   
            _localizer= new CustomLocalizer();
        }
        internal string TableName { get { return GetTableName(); } }
        internal string EntityName
        {
            get
            {
                return typeof(E).Name;
            }
        }
        private NameValueCollection _qs;
        public NameValueCollection qs
        {
            set
            {
                if (value == null)
                    _qs = new NameValueCollection();
                else
                    _qs = value;
            }
            get
            {
                if (_qs == null)
                    _qs = new NameValueCollection();
                return _qs;
            }
        }
        public void SetQs(NameValueCollection qs)
        {
            if (qs == null)
                _qs = new NameValueCollection();
            else
                _qs = qs;
        }
        public void ResetQS(string qsStr)
        {
            qs=new NameValueCollection();
            AddQS(qsStr);
        }
        public Core<E,P> SetContext(ApplicationDbContext ApplicationDbContext)
        {
            if (ApplicationDbContext == null)
                this.context = ApplicationDbContext;
            return this;
        }
        public static Core GetInstance(Core _core)
        {
            return new Core { context = _core.context, Session = _core.Session };
        }
        public void AddQS(string qsStr)
        {
            string[] querySeq=qsStr.Split('&');
            foreach(string s in querySeq)
            {
                string[] parts = s.Split('=');
                if(parts.Length > 0)
                {
                    string key = parts[0].Trim(new char[] { '?', ' ' });
                    string value = parts[1].Trim();
                    if (_qs.AllKeys.Contains(key))
                        qs[key] = value;
                    else
                        qs.Add(key, value);
                }
            }
        }
        public string GetTableName()
        {
            try
            {
                // We need dbcontext to access the models
                var models = context.Model;

                // Get all the entity types information
                var entityTypes = models.GetEntityTypes();

                // T is Name of class
                var entityTypeOfT = entityTypes.First(t => t.ClrType == typeof(E));

                var tableNameAnnotation = entityTypeOfT.GetAnnotation("Relational:TableName");
                var TableName = tableNameAnnotation.Value.ToString();
                return TableName;
            }
            catch (Exception ex) { return EntityName; }
        }

        #region VALIDASYON
        private BaseValidator<E> _validator;
        internal BaseValidator<E> DValidator
        {
            get { return GetValidator(); }
        }
        private BaseValidator<E> GetValidator()
        {
            if(_validator == null)
            {
                _validator = new BaseValidator<E>();

                _validator.RuleSet("UpdateRules", () => {
                    _validator.RuleFor(e => e).Custom((item, vContext) =>
                    {
                        _validator.RuleFor(e => e).Custom((item, context) => {
                            SetCustomUdateValidationRules(item, context);
                        });
                    });
                });

                _validator.RuleSet("InsertRules", () => {
                    _validator.RuleFor(e => e).Custom((item, vContext) =>
                    {
                        _validator.RuleFor(e => e).Custom((item, context) => {
                            SetCustomInsertValidationRules(item, context);
                        });
                    });
                });
                _validator.RuleSet("DeleteRules", () => {
                    _validator.RuleFor(e => e).Custom((item, vContext) =>
                    {
                        _validator.RuleFor(e => e).Custom((item, context) => {
                            SetCustomDeleteValidationRules(item, context);
                        });
                    });
                });
                _validator.RuleSet("db", () => SetValidationRules());
                _validator.RuleSet("custom",()=>SetCustomValidationRules());
            }
            return _validator;
        }
        #region validator
        internal virtual void SetCustomUdateValidationRules(E item,FluentValidation.ValidationContext<E> vContext)
        {

        }
        internal virtual void SetCustomDeleteValidationRules(E item, FluentValidation.ValidationContext<E> vContext)
        {

        }
        internal virtual void SetCustomInsertValidationRules(E item, FluentValidation.ValidationContext<E> vContext)
        {

        }
        internal virtual void SetValidationRules()
        {

        }
        internal virtual void SetCustomValidationRules()
        {

        }
        #endregion
        #region dvalidator
        internal virtual void RunCustomValidationRules(E item, DValidationResult vContext)
        {
        }

        internal virtual void RunCustomInsertValidationRules(E item, DValidationResult vContext)
        {
        }


        internal virtual void RunCustomUpdateValidationRules(E item, DValidationResult vContext)
        {
        }

        internal virtual void RunCustomDeleteValidationRules(E item, DValidationResult vContext)
        {
        }
        #endregion
        internal virtual Expression<Func<E,bool>> BsPredicate(ApplicationDbContext context)
        {
            return CreatePredicate(null);
        }
        internal virtual Expression<Func<E,bool>> CreatePredicate(Expression<Func<E,bool>> predicate)
        {
            return predicate;
        }

        public async Task<BsResult<E>> Validate(E item, DBActions Action)
        {
            try
            {
                {
                    var res = PreUpdateAction(Action, item);
                    res.Value = item;
                    return res;
                }
            }
            catch (Exception ex)
            {
                return BsError(ex);
            }
        }

        #region crud helper
        internal virtual BsResult<E> UpdateEntity(E entity)
        {
            return BsOk("Use_BsCore", entity);
        }
        internal virtual BsResult<E> AddEntity(E entity)
        {
            return BsOk("Use_BsCore", entity);
        }
        internal virtual BsResult DeleteEntity(E entity)
        {
            return BsOk("Use_BsCore", entity);
        }
        internal virtual BsResult<E> GetEntity(P key)
        {
            return BsOk("Use_BsCore", null);
        }
        #endregion

        #region relation detach
        private void DetachNavigations(E entity)
        {
            try
            {
                var entry = context.Entry(entity);

                foreach (var navigation in entry.Navigations)
                {
                    if (navigation.Metadata.IsCollection)
                    {
                        if (navigation.CurrentValue is IEnumerable<object> children)
                        {
                            foreach (var child in children)
                            {
                                var childEntry = context.Entry(child);
                                if (childEntry.State != EntityState.Detached)
                                    childEntry.State = EntityState.Unchanged;
                            }
                        }
                    }
                    else
                    {
                        if (navigation.CurrentValue is not null)
                        {
                            var navEntry = context.Entry(navigation.CurrentValue);
                            if (navEntry.State != EntityState.Detached)
                                navEntry.State = EntityState.Unchanged;
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }
        #endregion
        #region async crud 

        public async Task<BsResult<E>> AddAsync(E item, bool saveChangesYn = true, bool IsExceptionDetachEntryYn = false)
        {
            try
            {
                {
                    var res = PreUpdateAction(DBActions.Insert, item);
                    if (!res.Result) return res;

                    var customAddRes = AddEntity(item);
                    if (customAddRes.Message == "Use_BsCore")
                    {
                        DetachNavigations(item);
                        await context.AddAsync<E>(item).ConfigureAwait(true);
                        if (saveChangesYn)
                            await context.SaveChangesAsync().ConfigureAwait(true);

                        return BsOk(res.Value);
                    }
                    else
                        return customAddRes;

                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                if (IsExceptionDetachEntryYn && item != null)
                {
                    // Perform operations with the entity as needed
                    Console.WriteLine("Entity found");

                    // To detach the entity from the context
                    context.Entry(item).State = EntityState.Detached;
                    Console.WriteLine("Entity detached");
                }
                if (e.InnerException != null)
                    return BsError(e.InnerException.Message, "DbUpdateException");
                return BsError(e.Message, "DbUpdateException");

            }
            catch (Exception ex)
            {
                return BsError(ex);
            }
        }
        public async Task<BsResult<E>> UpdateSync(E item, bool saveChangesYn = true, bool IsExceptionDetachEntryYn = false)
        {
            try
            {
                {
                    var res = PreUpdateAction(DBActions.Update, item);
                    if (!res.Result) return res;

                    var customUpdateRes = UpdateEntity(item);
                    if (customUpdateRes.Message == "Use_BsCore")
                    {
                        context.Update(item);
                        if (saveChangesYn)
                            await context.SaveChangesAsync().ConfigureAwait(true);
                        return BsOk(res.Value);
                    }
                    else
                        return customUpdateRes;
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                if (IsExceptionDetachEntryYn && item != null)
                {
                    context.Entry(item).State = EntityState.Detached;
                }

                if (e.InnerException != null)
                    return BsError(e.InnerException.Message, "DbUpdateException");
                return BsError(e.Message, "DbUpdateException");

            }
            catch (Exception e)
            {
                return BsError(e);
            }
        }
        public async Task<BsResult> RemoveAsync(P Kod)
        {
            try
            {
                var item = GetByPK(Kod);
                if (item != null)
                {
                    var res = PreUpdateAction(DBActions.Remove, item);
                    if (!res.Result) return res;

                    var customDeleteRes = DeleteEntity(item);
                    if (customDeleteRes.Message == "Use_BsCore")
                    {
                        context.Entry(item).State = EntityState.Deleted;
                        var cnt = await context.SaveChangesAsync();
                        if (cnt > 0)
                        {
                            return BsOk(null);
                        }
                        if (cnt<0)
                            return BsError("Kayıt Bulunamadı", "NotFound");
                    }
                    else
                        return customDeleteRes;
                }
                return BsError("Kayıt Bulunamadı", "NotFound");
            }
            catch (Exception ex)
            {
                return BsError(ex);
            }
        }

        #endregion
        #region helper methods
        public P GetNextPrimaryKey()
        {
            if (typeof(P).IsNumericType())
            {
                string pkName = GetPKName();
                var parameter = Expression.Parameter(typeof(E), "e");
                var property = Expression.Property(parameter, pkName);
                var lambda = Expression.Lambda<Func<E, P>>(property, parameter);

                var dbSet = context.Set<E>();

                P maxValue;

                if (dbSet.Any())
                {
                    maxValue = dbSet.Select(lambda).Max();
                }
                else
                {
                    maxValue = default(P);
                }

                return ConvertValue<P, long>(Convert.ToInt64(maxValue) + 1);
            }
            //else
            //    return -1;
            return default(P);
        }
        private P ConvertValue<P, U>(U value) where U : IConvertible
        {
            return (P)Convert.ChangeType(value, typeof(P));
        }

        public long GetPKNumberValue(P value)
        {
            try
            {
                return (long)Convert.ChangeType(value, typeof(long));
            }
            catch { return -1; }
        }
        private BsResult<E> SetReqValues(ApplicationDbContext context, E item, DBActions Action, bool onlyValidation = false)
        {
            context = GetContext();
            var entity = context.Entry(item);
            try
            {
                if (ConvertDateTimeToDate)
                {
                    //var dateFields = context.Entry(item).Properties.Where(f => f.GetType() == typeof(DateTime) || f.GetType() == typeof(DateTime?)).ToList();
                    //foreach (var prop in dateFields)
                    //    prop.CurrentValue = ((DateTime)prop.CurrentValue).Date;

                    var properties = EntityProperties.Where(f => (f.PropertyType == typeof(DateTime) || f.PropertyType == typeof(DateTime?)) ).ToList();
                    foreach (var property in properties)
                    {
                        //if (property.PropertyType == typeof(DateTime))
                        //    property.SetValue(item, ((DateTime)property.GetValue(item, null)).Date, null);

                        if ((Action == DBActions.Insert || entity.Property(property.Name).IsModified) && entity.Property(property.Name).CurrentValue != null)
                        {
                            entity.Property(property.Name).CurrentValue = ((DateTime)entity.Property(property.Name).CurrentValue).Date;
                        }
                    }
                }
            }
            catch { }
            try
            {
                if (Action == DBActions.Insert)
                {
                }
                else if (Action == DBActions.Update)
                {
                }
                if (Action == DBActions.Insert || Action == DBActions.Update)
                {
                    //if (!onlyValidation)
                    return OnSetReqValues(context, item, Action);
                }
                return BsOk();
            }
            catch (Exception ex)
            {
                return BsError(ex, "SetReqValues");
            }
        }
        internal virtual BsResult<E> OnSetReqValues(ApplicationDbContext context, E item, DBActions Action)
        {
            return BsOk();
        }

        private BsResult<E> PreUpdateAction(DBActions Action, E item, bool onlyValidation = false)
        {
            var context = GetContext();
            var res = SetReqValues(context, item, Action);
            if (!res.Result) return res;
            BaseValidator<E> FValidator;
            var dvResult = new DValidationResult();
            try
            {
                switch (Action)
                {
                    case DBActions.Insert:
                        //string errorInsert = string.Empty;

                        FValidator = GetValidator();
                        if (FValidator != null)
                        {
                            var vResult0 = FValidator.Validate(item);
                            var vResult = FValidator.Validate(item, options =>
                            {
                                options.IncludeRuleSets("db"); //.IncludeRulesNotInRuleSet();
                            });

                            if (!vResult.IsValid)
                                return new BsResult<E>(vResult);
                        }


                        RunCustomInsertValidationRules(item, dvResult);
                        RunCustomValidationRules(item, dvResult);
                        if (!dvResult.IsValid)
                            return new BsResult<E>(dvResult);
                        break;

                    case DBActions.Update:
                        //if (GetPKValue(item) == null)
                        //    return BsError("Güncellenecek Anahtar Kod Eksik....", "MISSING_PK");

                        FValidator = GetValidator();
                        if (FValidator != null)
                        {
                            var vResult = FValidator.Validate(item, options =>
                            {
                                options.IncludeRuleSets("db").IncludeRulesNotInRuleSet();
                            });
                            if (!vResult.IsValid)
                                return new BsResult<E>(vResult);
                            // errorStr += string.Join(";", vResult.Errors.Select(failure => failure.PropertyName + " > " + failure.ErrorMessage));
                        }
                        //if (errorStr.Length > 0)
                        //    return BsError(errorStr, "400");

                        RunCustomUpdateValidationRules(item, dvResult);
                        RunCustomValidationRules(item, dvResult);
                        if (!dvResult.IsValid)
                            return new BsResult<E>(dvResult);

                        break;
                    case DBActions.Remove:

                        //if (FValidator != null)
                        //{
                        //    var vResult = FValidator.Validate(item, options =>
                        //    {
                        //        options.IncludeRuleSets("DeleteRules");
                        //    });
                        //    if (!vResult.IsValid)
                        //        return new BsResult<E>(vResult);
                        //}

                        RunCustomDeleteValidationRules(item, dvResult);
                        if (!dvResult.IsValid)
                            return new BsResult<E>(dvResult);

                        break;
                    default:
                        break;
                }



                if (!onlyValidation)
                {
                    if (Action == DBActions.Insert || Action == DBActions.Update)
                    {
                        var result = OnPreUpdateAction(Action, item);
                        if (!result.Result) return result;

                    }
                    if (Action == DBActions.Insert)
                    {
                        SetPKCode(item);
                    }
                }
                return BsOk(item);
            }
            catch (Exception ex)
            {
                return BsError(ex);
            }
        }
        internal virtual BsResult<E> OnPreUpdateAction(DBActions Action, E item)
        {
            return BsOk(item);
        }
        internal void SetPKCode(E item)
        {
            context = GetContext();
            try
            {
                if (typeof(P).IsNumericType())
                {
                    if (typeof(P) == typeof(short))
                    {
                        short? key = Convert.ToInt16(context.Entry(item).Property(GetPKName()).CurrentValue);
                        if ((key == null ? 0 : key) > 0)
                            return;
                    }
                    else if (typeof(P) == typeof(int))
                    {
                        int? key = Convert.ToInt32(context.Entry(item).Property(GetPKName()).CurrentValue);
                        if ((key == null ? 0 : key) > 0)
                            return;
                    }
                    else if (typeof(P) == typeof(long))
                    {
                        long? key = Convert.ToInt64(context.Entry(item).Property(GetPKName()).CurrentValue);
                        if ((key == null ? 0 : key) > 0)
                            return;
                    }
                }
                else if (typeof(P) == typeof(string))
                {
                    string key = context.Entry(item).Property(GetPKName()).CurrentValue.ToString();
                    if (!key.IsNullOrEmpty())
                        return;
                }
            }
            catch
            {; }

            if (typeof(P).IsNumericType())
            {
                try
                {
                    var pkcode = GetNextPrimaryKey();
                    context.Entry(item).Property(GetPKName()).CurrentValue = Convert.ChangeType(pkcode, context.Entry(item).Property(GetPKName()).CurrentValue?.GetType());
                }
                catch (Exception ex)
                {
                }
            }
        }

        public E GetByPK(P key)
        {
            var customAddRes = GetEntity(key);
            if (customAddRes.Message == "Use_BsCore")
            {
                return GetSingleByPk(key, false);
            }
            else
                return customAddRes.Value;
        }
        public E GetSingleByPk(P Key, bool AsNoTracking = false)
        {
            try
            {
                context = GetContext();
                E data;
                IQueryable<E> dbquery=context.Set<E>();
                string prop = context.Model.FindEntityType(typeof(E)).FindPrimaryKey().Properties.Select(x => x.Name).Single();

                var parameter=Expression.Parameter(typeof(E),"x");
                var property=Expression.Property(parameter, prop);
                var constant=Expression.Constant(Convert.ChangeType(Key,property.Type),property.Type);

                var predicate = Expression.Lambda<Func<E, bool>>(
                    Expression.Equal(property, constant),parameter);

                dbquery = bsHelper<E>.ApplyFinalPredicate(dbquery, predicate,BsPredicate(context));

                data = dbquery.SingleOrDefault();

                return data;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public BsResult<E> GetNew(NameValueCollection name,bool forInsert = false)
        {
            E item = null;
            context = GetContext();
            var allowedActions = GetAllowedActions(default(P));
            if(allowedActions.AllowInsert)
                return BsError(allowedActions.InsertMessage.IsNullOrEmpty()?"Yetki yok":allowedActions.InsertMessage);
            item = new E();
            if(forInsert)
                return BsOk(item);
            try
            {
                var properties=item.GetType().GetProperties();
                var entitydef = context.Entry(item);
                foreach (var property in properties) {
                    entitydef.Property(property.Name).CurrentValue = DateTime.Today;
                }

            }
            catch
            {

            }
            try
            {
                long anaKod = 0;
                if (qs.HasKeys())
                {
                    anaKod = Convert.ToInt64(qs["AnaKod"]);
                    if (anaKod == 0)
                        anaKod = Convert.ToInt64(qs["AKod"]);
                }
                return OnGetNewItem(item,anaKod);
            }
            catch (Exception ex){
                return BsError(ex);
            }
        }
        internal virtual BsResult<E> OnGetNewItem(E item, long anaKod)
        {
            return BsOk(item);
        }

        public string GetPKName()
        {
            return _pkNameCache.GetOrAdd(typeof(E), type =>
            {
                var entityType = context.Model.FindEntityType(type);
                var pk = entityType.FindPrimaryKey();
                return pk.Properties.Select(p => p.Name).Single();
            });
        }
        public Dto GetAllowedActions(P key = default(P))
        {
            long anaKod = Convert.ToInt64(qs["AKod"]);

            return GetAllowedActions(key, anaKod);
        }
        public Dto GetAllowedActions(P Key,long anakod)
        {
            var allowedActions = new Dto(true, true, true);
            try { }
            catch { }
            return allowedActions;
        }
        public Dto GetAllowedActions(E item,long anakod)
        {
            var allowedActions = new Dto(true, true, true);
            try { }
            catch { }
            return allowedActions;
        }

        public List<E> GetList(Expression<Func<E,bool>> where,params Expression<Func<E, object>>[] navigationProperties) {
            return GetQueryableList(null, where, navigationProperties).ToList();
        }
        public IQueryable<E> GetQueryableList(NameValueCollection name,Expression<Func<E,bool>> where1,params Expression<Func<E, object>>[] navigationProperties)
        {
            context = GetContext();
            try
            {
                IQueryable<E> dbquery=context.Set<E>();
                if (navigationProperties != null)
                {
                    foreach (Expression<Func<E, object>> property in navigationProperties)
                    {
                        dbquery = dbquery.Include<E, object>(property);
                    }
                }
                    dbquery = bsHelper<E>.ApplyFinalPredicate(dbquery, where1, BsPredicate(context));
                    dbquery = dbquery.AsNoTracking();
                    return dbquery;
                
            }
            catch (Exception ex) {
                return null;
            }
        }
        public bool IsAny(Expression<Func<E,bool>> where)
        {
            return GetQueryableList(null,where,null).Any();
        }
        public long GetNextValue(Expression<Func<E,bool>> where,Expression<Func<E,long>> where1)
        {
            try
            {
                var res = GetQueryableList(null, where, null).Max(where1) + 1;
                return res;
            }
            catch(Exception ex)
            {
                return 1;
            }
        }
        public E GetSingle(Expression<Func<E,bool>> where,params Expression<Func<E, object>>[] navigationProperties)
        {
            context = GetContext();
            E item;
            try {
            IQueryable<E> query=context.Set<E>();
                foreach (var property in navigationProperties) {
                    query=query.Include(property);
                }
                query = bsHelper<E>.ApplyFinalPredicate(query, where, BsPredicate(context));
                item = query.FirstOrDefault();
                return item;
            }
            catch
            {
                return null;
            }
        }
        #endregion
        #region returns
        internal BsResult<E> BsError(string errMessage,string errCode = "")
        {
            return new BsResult<E>(false, "Exception", errCode, errMessage);
        }
        internal BsResult<E> BsError(BsResult result)
        {
            var res = new BsResult<E>(result, null);
            res.Result = false;
            if (res.Error == null)
                res.Error = new Error("Fault", "System Faul", null);
            return res;
        }
        internal BsResult<E> BsError(Exception ex,string source="",string errorCode = "")
        {
            return new BsResult<E>(false, "İşlem sırasında hata oluştur", errorCode, string.Concat(source, ":", ex.Message), ex: ex);
        }
        internal BsResult<E> BsOk()
        {
            return new BsResult<E>(true);
        }
        internal BsResult<E> BsOk(E value=null)
        {
            return new BsResult<E>(true, value);
        }
        public BsResult<E> BsOk(string resMsg, E value = null)
        {
            return new BsResult<E>(true, value, resMsg, null, null);
        }
        #endregion
        #endregion
    }
    public enum DBActions : byte
    {
        GetSingle = 0,
        Insert = 1,
        Update = 2,
        Remove = 3
    }
    public static class MyCustomValidators
    {
        public static FluentValidation.IRuleBuilderOptions<T, IList<TElement>> ListMustContainFewerThan<T, TElement>(this FluentValidation.IRuleBuilder<T, IList<TElement>> ruleBuilder, int num)
        {
            return ruleBuilder.Must(list => list.Count < num).WithMessage("The list contains too many items");
        }
    }
}
