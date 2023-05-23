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
    using INCOMSYSTEM.BehaviorsFiles;
    using System;
    using System.Collections.Generic;
    
    public partial class Tasks
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tasks()
        {
            this.Orders = new HashSet<Orders>();
            this.TaskStages = new HashSet<TaskStages>();
        }
    
        public long id { get; set; }
        public string name { get; set; }
        public int idSpecialization { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public Nullable<byte> discount { get; set; }
        public int approxCompleteTime { get; set; }
        public int supportPeriod { get; set; }
        public byte[] attachment { get; set; }
        public string fileExtension { get; set; }
        public Nullable<long> idFile { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Orders> Orders { get; set; }
        public virtual Specializations Specializations { get; set; }
        public virtual HistoryUploaded HistoryUploaded { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TaskStages> TaskStages { get; set; }


        public bool discoutStyle => discount != null && discount > 10;
        public decimal newPrice => discount != null ? (decimal)(price - (price * (discount / 100m))) : price;
        public bool discountVisible => discount != null && discount > 0;
        public string shortDescription => description.Length > 60 ? description.Substring(0, 60) + "..." : description;

        public string approxString => approxCompleteTime.ConvertDay();
    }
}
