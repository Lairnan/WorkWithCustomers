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
    
    public partial class OrderStages
    {
        public long id { get; set; }
        public long idOrder { get; set; }
        public byte idType { get; set; }
        public string name { get; set; }
        public Nullable<long> idTaskStage { get; set; }
        public Nullable<long> idFile { get; set; }
        public Nullable<System.DateTime> factDateStart { get; set; }
        public Nullable<System.DateTime> factDateComplete { get; set; }
        public string description { get; set; }
    
        public virtual HistoryUploaded HistoryUploaded { get; set; }
        public virtual Orders Orders { get; set; }
        public virtual TaskStages TaskStages { get; set; }
        public virtual TypesStage TypesStage { get; set; }
    }
}
