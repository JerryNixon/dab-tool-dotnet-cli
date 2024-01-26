    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;

    public partial class authors
    {
        [Key]
        public int id { get; set; }

        public string? first_name { get; set; }

        public string? middle_name { get; set; }

        public string? last_name { get; set; }

    }
