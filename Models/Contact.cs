using System;
using System.ComponentModel.DataAnnotations;

namespace Contacts.Models
{
    public class Contact
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [StringLength(100)]
        public string Company { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string OwnerId { get; set; }
    }
}