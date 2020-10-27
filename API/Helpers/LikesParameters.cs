namespace API.Helpers
{
    public class LikesParameters : PaginationParameters
    {
        public int UserId { get; set; }

        public string Predicate { get; set; }
    }
}