namespace PMS_API_DAL.Models.CustomeModel
{
    public class UserResponse
    {
        public string ActionName {  get; set; }
        public string ControllerName {  get; set; }
        public string JwtToken {  get; set; }
        public string UserRole {  get; set; }
        public int UserId {  get; set; }
    }
}
