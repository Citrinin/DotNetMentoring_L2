namespace ProfileSample.DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ImgSource
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public byte[] Data { get; set; }
    }
}
