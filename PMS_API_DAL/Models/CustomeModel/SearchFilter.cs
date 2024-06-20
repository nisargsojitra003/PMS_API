namespace PMS_API_DAL.Models.CustomeModel
{
    public class SearchFilter
    {
        //Category List property of Search filter.
        public string? SearchName { get; set; }
        public string? SearchCode { get; set; }
        public string? description { get; set; }
        public int? sortType { get; set; }  
        public string? categoryPageSize {get; set;}
        public string? categoryPageNumber {get; set;}
        public int? userId {  get; set;}

        //Product List Property of Search filter.
        public string? searchProduct { get; set; }
        public string? searchCategoryTag { get; set; }
        public string? searchDescription { get; set; }
        public int? sortTypeProduct { get; set; }
        public int? searchCategory { get; set; } = 0;
        public string? productPageSize {get; set;}
        public string? productPageNumber {get; set;}
    }
}
