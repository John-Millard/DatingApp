namespace API.Helpers
{
    public class UserParameters
    {
        private const int maxPageSize = 50;

        public int PageNumber { get; set; } = 1;

        private int pageSize = 10;

        public int PageSize
        {
            get => this.pageSize;
            set => this.pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string CurrentUserName { get; set; }

        public string Gender { get; set; }

        public int MinAge { get; set; } = 18;

        public int MaxAge { get; set; } = 150;

        public string OrderBy { get; set; } = "lastActive";
    }
}