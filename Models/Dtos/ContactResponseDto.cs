namespace Contacts.Models.Dtos
{
    public class ContactResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Company { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}