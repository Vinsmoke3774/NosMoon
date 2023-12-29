using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<OpenNosContext>
    {
        #region Instantiation

        // Stupid Automatic Migration
        public Configuration() => AutomaticMigrationsEnabled = false;

        #endregion
    }
}