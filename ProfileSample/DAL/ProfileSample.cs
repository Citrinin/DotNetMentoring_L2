namespace ProfileSample.DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ProfileSample : DbContext
    {
        public ProfileSample()
            : base("name=ProfileSample")
        {
        }

        public virtual DbSet<ImgSource> ImgSources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
