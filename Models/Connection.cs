using System;
using System.ComponentModel.DataAnnotations;

namespace VPN_RDP_Manager_Web.Models
{
    public class Connection
    {
        [Key]
        public int SYS_NO { get; set; }

        [Required]
        public string? KURUM { get; set; }

        [Required]
        public string? TIP { get; set; }

        [Required]
        public string? IP { get; set; }

        public int? PORT { get; set; }

        [Required]
        public string? KULLANICI { get; set; }

        [Required]
        public string? SIFRE { get; set; }

        public string? NOTLAR { get; set; }

        public DateTime KAYIT_ANI { get; set; }
    }
}
