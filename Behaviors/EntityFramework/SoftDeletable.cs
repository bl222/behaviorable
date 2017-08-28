using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using System;
using System.Linq;

namespace Behaviorable.Behaviors.EntityFramework
{

        public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }

    public class SoftDeletableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ISoftDeletable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        public SoftDeletableBehavior(Business b)
            : base(b)
        {

        }

        public bool ForceDelete(Poco toDelete, BusinessParameters parameters = null)
        {
       
            parameters = BusinessParameters.SetDeepValue(parameters, "soft-deletable.disable", true);

            return business.Delete(toDelete, parameters);

        }

        public bool ForceDelete(int id, BusinessParameters parameters = null)
        {

            parameters = BusinessParameters.SetDeepValue(parameters, "soft-deletable.mode", "all");
            
            return ForceDelete(business.Find(id, parameters),  parameters);
        }

        public override bool? BeforeDelete(Poco toDelete, BusinessParameters parameters)
        {
            if (parameters != null && (parameters.GetDeepValue("soft-deletable.disable") as bool?) == true)
            {
                return true;
            }
            toDelete.DeletedAt = DateTime.Now;
            business.Save(toDelete);

            return null;
        }

        public override IQueryable<Poco> AfterFind(string type, BusinessParameters parameters, IQueryable<Poco> results = null)
        {


            string mode = parameters != null ? parameters.GetDeepValue("soft-deletable.mode") as string : null;

            if (mode == null)
            {
                mode = "not-deleted";
            }

            if (mode == "not-deleted")
            {
                return results.Where(m => m.DeletedAt == null);
            }
            else if (mode == "only-deleted")
            {
                return results.Where(m => m.DeletedAt != null);
            }


            return results;

        }

    }
}