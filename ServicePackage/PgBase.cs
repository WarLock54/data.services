using HelperLibrary;
using Npgsql;
using System.Data;
using System.Text.Json;

namespace ServicePackage
{
    public class PgBase : IDisposable
    {
        private static readonly string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=your_db";
        protected NpgsqlConnection Connection;
        public string SchemaName { get; set; } = "public";
        private bool _hasParent = false;
        private bool disposed = false;

        private SSSessionInfo _session;
        public SSSessionInfo Session
        {
            get => _session ??= new SSSessionInfo { UserId = 1 };
            set => _session = value;
        }

        public PgBase()
        {
            Connection = new NpgsqlConnection(ConnectionString);
            Connection.Open();
        }

        public PgBase(SSSessionInfo session)
        {
            Session = session;
            Connection = new NpgsqlConnection(ConnectionString);
            Connection.Open();
        }

        public PgBase(PgBase parent)
        {
            Session = parent.Session;
            Connection = parent.Connection;
            _hasParent = true;
        }

        public void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }

        private void Cleanup()
        {
            if (disposed) return;

            if (!_hasParent && Connection != null)
            {
                try
                {
                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                    Connection.Dispose();
                }
                catch { }
            }
            disposed = true;
        }

        protected NpgsqlParameterCollection GetNewParameterCol(NpgsqlCommand cmd)
        {
            return cmd.Parameters;
        }
        protected void ExecuteProcedure(string procedureName, NpgsqlParameterCollection parameters)
        {
            using var cmd = new NpgsqlCommand
            {
                Connection = this.Connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = procedureName
            };

            foreach (NpgsqlParameter p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
        }
        private TY_GEN_CEVAP GetPOCevap(NpgsqlParameterCollection parameters)
        {
            if (parameters.Contains("po_cevap") && parameters["po_cevap"].Value != DBNull.Value)
            {
                var json = parameters["po_cevap"].Value.ToString();
                return JsonSerializer.Deserialize<TY_GEN_CEVAP>(json);
            }
            return null;
        }
        private bool GetPoIslemYapildiEh(NpgsqlParameterCollection parameters)
        {
            return parameters.Contains("po_islem_yapildi_eh") && parameters["po_islem_yapildi_eh"].Value?.ToString() == "E";
        }
        internal BsResult GetBsResult(NpgsqlParameterCollection parameters)
        {
            var poCevap = GetPOCevap(parameters);
            var poIslemYapildiEh = GetPoIslemYapildiEh(parameters);
            return GetBsResult(poCevap, poIslemYapildiEh);
        }

        internal BsResult GetBsResult(TY_GEN_CEVAP poCevap, bool islemYapildiEh = true)
        {
            var res = new BsResult { Result = true };
            try
            {
                if (poCevap != null)
                {
                    if (poCevap.KOD == 0)
                    {
                        if (!islemYapildiEh)
                        {
                            res.Result = false;
                            res.Message = poCevap.ACIKLAMA;
                            res.Error = new Error { ErrorCode = "PO" + poCevap.KOD, ErrorYn = true, ErrorDescription = "İşlem Yapılmadı... " + poCevap.ACIKLAMA };
                        }
                        else
                        {
                            res.Message = poCevap.ACIKLAMA;
                        }
                    }
                    else
                    {
                        res.Result = false;
                        res.Error = new Error { ErrorCode = "PO" + poCevap.KOD, ErrorYn = true, ErrorDescription = poCevap.ACIKLAMA + " " + poCevap.DBS_HATA };
                    }
                }
                else
                {
                    if (!islemYapildiEh)
                    {
                        res.Result = false;
                        res.Message = "";
                        res.Error = new Error { ErrorCode = "", ErrorYn = true, ErrorDescription = "İşlem Yapılmadı..." };
                    }
                    else
                    {
                        res.Message = "0|İşlem Başarılı";
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                res.Result = false;
                res.Message = "CONNECTION TIME OUT";
                res.Error = new Error(ex);
                return res;
            }
        }

        
    }
}
