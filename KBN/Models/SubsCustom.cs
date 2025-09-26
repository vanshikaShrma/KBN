using System.ComponentModel.DataAnnotations;

namespace KBN.Models
{
    public class SubsCustom:Subscriber
    {
        [Required]
        [Display(Name = "Customer Name")]
        public string customer_name { get; set; }
    }
}
