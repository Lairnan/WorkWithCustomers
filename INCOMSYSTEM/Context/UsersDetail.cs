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
        public long idUser { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public Nullable<long> phone { get; set; }
        public byte idPos { get; set; }
        public System.DateTime dateStart { get; set; }
    
        public virtual Customers Customers { get; set; }
        public virtual Employees Employees { get; set; }
        public virtual Positions Positions { get; set; }
    }
}
