using Behaviorable.Behaviors.EntityFramework;
using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using System.Linq;

namespace Behaviorable.Behaviors
{
    /// <summary>
    /// Instead of creating a behavior class by implementing the IBehavior interface, it is possible to inherit
    /// from the BaseBehavior class. BaseBehavior is there for convenience, it implements all the callbacks defined in
    /// the IBehavior interace as empty methods so when a class inherit from BaseBehavior, you only have to write the code
    /// for the callbacks you wish to implement. In other words, inheriting from BaseBehavoir means that 
    /// </summary>
    /// <typeparam name="T">The type of data managed by the business to which the behavior is attached</typeparam>
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
}