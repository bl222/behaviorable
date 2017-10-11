namespace Behaviorable.Behaviors.EntityFramework
{
    /// <summary>
    /// Any Poco object that needs to be manipulated by a behavior has to implement either the IBasePoco interface.
    /// The IBasePoco interface  specifies which
    /// collumns are necessary for the database table represented by the Poco
    /// </summary>
    public interface IBasePoco
    {
        /// <summary>
        /// Any behavior needs the table to have at least an ID collumn that is an integer and can be null. The null
        /// is used to indicate a new Row that is being created
        /// </summary>
        int? ID { get; set; }
    }

}
