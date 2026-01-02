namespace Web.Admin.Areas.Admin.Models
{
    public class PageEditViewModel
    {
        public int PageId { get; set; }
        public string Slug { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public bool IsMenuItem { get; set; }
        public int? MenuSortOrder { get; set; }

        public List<PageLanguageContentViewModel> Languages { get; set; } = new();
    }

}
