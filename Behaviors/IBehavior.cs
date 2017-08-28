using System.Linq;
using Behaviorable.Businesses;

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

}
