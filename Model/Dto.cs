using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Dto
    {
        public Dto() { }
        public Dto(bool insertYn, bool updateYn, bool deleteYn)
        {
            AllowInsert = insertYn;
            AllowUpdate = updateYn;
            AllowDelete = deleteYn;
        }
        public void SetFrom(Dto source)
        {
            AllowInsert = source.AllowInsert;
            AllowUpdate = source.AllowUpdate;
            AllowDelete = source.AllowDelete;
            InsertMessage = source.InsertMessage;
            UpdateMessage = source.UpdateMessage;
            DeleteMessage = source.DeleteMessage;
        }

        public void SetInsert(bool allowInsert, string message)
        {
            AllowInsert = AllowInsert && allowInsert;
            InsertMessage = AllowInsert ?
                                "" :
                                (!message.IsNullOrEmpty() ?
                                    message :
                                    (InsertMessage.IsNullOrEmpty() ? "Kayıt ekleme yapılamaz" : InsertMessage));
        }
        public void SetUpdate(bool allowUpdate, string message)
        {
            AllowUpdate = AllowUpdate && allowUpdate;
            UpdateMessage = AllowUpdate ?
                                "" :
                                (!message.IsNullOrEmpty() ?
                                    message :
                                    (UpdateMessage.IsNullOrEmpty() ? "Kayıt güncelleme yapılamaz" : UpdateMessage));
        }

        public void SetDelete(bool allowDelete, string message)
        {
            AllowDelete = AllowDelete && allowDelete;
            DeleteMessage = AllowDelete ?
                                "" :
                                (!message.IsNullOrEmpty() ?
                                    message :
                                    (DeleteMessage.IsNullOrEmpty() ? "Kayıt Silme yapılamaz" : DeleteMessage));
        }
        public void DisableAll(string message)
        {
            AllowInsert = AllowUpdate = AllowDelete = false;
            InsertMessage = UpdateMessage = DeleteMessage = message;
        }

        public bool AllowInsert { set; get; } = true;
        public bool AllowUpdate { set; get; } = true;
        public bool AllowDelete { set; get; } = true;
        public string InsertMessage { set; get; } = null;
        public string UpdateMessage { set; get; } = null;
        public string DeleteMessage { set; get; } = null;
    }
}
