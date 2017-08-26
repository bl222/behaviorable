using Behaviorable.Attributes;
using Behaviorable.Behaviors;
using Behaviorable.Business;
using MvcMovie.Models;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Behaviorable.Businesses.EntityFramework
{
    public class EFBusinessQuery<T>
    {
        public System.Linq.Expressions.Expression<Func<T, bool>> Where { get; set; }
        public System.Linq.Expressions.Expression<Func<T, dynamic>> OrderBy { get; set; }
      
    }

    public class EFBusiness<T, D> : GenericBusiness<T> 
        where T : class, IBasePoco
        where D : DbContext
    {

        public DbSet<T> Table { get; set; }
        public D Db { get; set; }
        
       

        public EFBusiness(D db, DbSet<T> table)
        {
          
            Table = table;
            Db = db;

        }



        public virtual T Find(int id)
        {
            return this.FindFirst("id", new Dictionary<string, dynamic> { {"id", id } });
        }

        public virtual bool Delete(int id)
        {
            var toDelete = this.Find(id);

            if (toDelete != null)
            {
                return this.Delete(toDelete);
            }
            return false;
        }

        [BusinessFind("all")]
        public virtual IQueryable<T> DefaultFind(IDictionary<string, dynamic> parameters)
        {

            IQueryable<T> target = Table;
            if(parameters != null && parameters.Keys.Contains("query"))
            {
                EFBusinessQuery<T> query = parameters["query"];

                if (query.Where != null)
                {
                    target = target.Where(query.Where);
                }

                if (query.OrderBy != null)
                {
                    target = target.OrderBy(query.OrderBy) ;
                }

            
            }


            return target;
        }



        [BusinessFind("id")]
        public IQueryable<T> IdFind(IDictionary<string, dynamic> parameters)
        {
            var table = (IEnumerable<dynamic>)Table;
            int id = parameters["id"];
            
            return Table.Where(p => p.ID == id);
        }


        protected override bool SaveHelper(T toSave)
        {
            dynamic toSaveTmp = toSave;
            if (toSaveTmp.ID == null)
            {
                Table.Add(toSave);
                Db.SaveChanges();
            }
            else
            {
                Db.Entry(toSave).State = EntityState.Modified;
                Db.SaveChanges();
            }

            return true;
        }

        protected override bool DeleteHelper(T toDelete)
        {
            Table.Remove(toDelete);
            Db.SaveChanges();
            return true;
        }





    }
}