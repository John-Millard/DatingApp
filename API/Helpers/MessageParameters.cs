namespace API.Helpers
{
    public class MessageParameters : PaginationParameters
    {
        public string UserName { get; set; }

        public string Container { get; set; } = "Unread";
    }
}