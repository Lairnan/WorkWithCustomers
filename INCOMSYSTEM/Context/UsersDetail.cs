//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace INCOMSYSTEM.Context
{
    using System;
    using System.Collections.Generic;
    
    public partial class UsersDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UsersDetail()
        {
            this.HistoryUploaded = new HashSet<HistoryUploaded>();
            this.Messages = new HashSet<Messages>();
            this.UpdatesHistory = new HashSet<UpdatesHistory>();
        }
    
        public long idUser { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string phone { get; set; }
        public byte idPos { get; set; }
        public string passport { get; set; }
        public string address { get; set; }
        public System.DateTime dateStart { get; set; }
        public bool isOnline { get; set; }
    
        public virtual Customers Customers { get; set; }
        public virtual Employees Employees { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HistoryUploaded> HistoryUploaded { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Messages> Messages { get; set; }
        public virtual Positions Positions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UpdatesHistory> UpdatesHistory { get; set; }

        public string SeriePassport => passport.Substring(0, 4);
        public string NumberPassport => passport.Substring(4, 6);
    }
}
