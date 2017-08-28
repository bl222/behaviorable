using MvcMovie.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Behaviorable.Attributes;
using Behaviorable.Businesses.EntityFramework;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;
using Behaviorable.Business;

namespace Behaviorable.Behaviors
{
    public interface IBehavior<T>
    {
        IQueryable<T> BeforeFind(string type, BusinessParameters parameters, IQueryable<T> results = null);
        //D BeforeFindOne(string type, dynamic parameters);
        IQueryable<T> AfterFind(string type, BusinessParameters parameters, IQueryable<T> results = null);
        //bool AfterFindOne(string type, dynamic parameters);

        bool? BeforeSave(T toSave, BusinessParameters parameters);
        bool? AfterSave(T toSave, BusinessParameters parameters);

        bool? BeforeDelete(T toDelete, BusinessParameters parameters);
        bool? AfterDelete(T toDelete, BusinessParameters parameters);

    }

    public class BaseBehavior<T> : IBehavior<T>
    {
        public BaseBehavior() {


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
        where Db: System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
        
    {
        protected Business business { get; set; }
        
        public EFBehavior(Business b)
        {
            business = b;
        }
    }

}
