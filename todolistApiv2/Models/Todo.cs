namespace todolistApiv2.Models
{
    public class Todo
    {
        public int TodoId { get;}
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set;}
        public DateTime UpdatedDate { get; set;}

        // Foreign key property
        public int UserId { get; set; }
/*        public User? User { get; set; }*/
    }
}
