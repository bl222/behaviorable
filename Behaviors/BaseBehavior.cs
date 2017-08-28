using Behaviorable.Behaviors.EntityFramework;
using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using System.Linq;

namespace Behaviorable.Behaviors
{
    public class BaseBehavior<T> : IBehavior<T>
    {
        public BaseBehavior()
        {


        }

        public virtual IQueryable<T> BeforeFind(string type, BusinessParameters parameters, IQueryable<T> results = null)
        {
            return results;
        }

        public virtual IQueryable<T> AfterFind(string type, BusinessParameters parameters, IQueryable<T> results = null)
        {
            return results;
        }

        public virtual bool? BeforeSave(T toSave, BusinessParameters parameters)
        {
            return true;
        }
        public virtual bool? AfterSave(T toSave, BusinessParameters parameters)
        {
            return true;
        }

        public virtual bool? BeforeDelete(T toDelete, BusinessParameters parameters)
        {
            return true;
        }
        public virtual bool? AfterDelete(T toDelete, BusinessParameters parameters)
        {

            return true;
        }


    }

    public class EFBehavior<Poco, Db, Business> : BaseBehavior<Poco>
        where Poco : class, IBasePoco
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>

    {
        protected Business business { get; set; }

        public EFBehavior(Business b)
        {
            business = b;
        }
    }
}