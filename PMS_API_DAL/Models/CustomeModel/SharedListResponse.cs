namespace PMS_API_DAL.Models.CustomeModel
{
    public class SharedListResponse<T> 
    {
        public int TotalRecords {  get; set; }
        public List<T> List {  get; set; }
    }
}
