using Behaviorable.Businesses.EntityFramework;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Behaviorable.Business;

namespace Behaviorable.Behaviors.EntityFramework
{
    public class TimeStampableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ITimeStampable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        public TimeStampableBehavior(Business b)
            : base(b)
        {
            
        }

       public override bool? BeforeSave(Poco toSave, BusinessParameters parameters)
        {
            if(toSave.ID == null)
            {
                toSave.ModifiedAt = null;
                toSave.CreatedAt = DateTime.Now;
            } else
            {
                if(toSave.CreatedAt == null)
                {
                    toSave.CreatedAt = (from m in business.Table
                                       where m.ID == toSave.ID
                                       select m.CreatedAt).First();
                }
                toSave.ModifiedAt = DateTime.Now;
            }

            return true;
        }
    }
}