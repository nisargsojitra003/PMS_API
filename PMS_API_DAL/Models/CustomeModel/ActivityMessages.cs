namespace PMS_API_DAL.Models.CustomeModel
{
    public class ActivityMessages
    {
        //createAccount Activity message.
        public string createAccountRecord = "Account has been created using Email : {1}";

        //Common Activity messages.
        public string add = "{1} {2} has been created";
        public string edit = "{1} {2} has been updated";
        public string delete = "{1} {2} has been deleted";
    }
    /// <summary>
    ///Total Entity of Application.
    /// </summary>
    public enum EntityNameEnum 
    {
        category,
        product
    }

    /// <summary>
    ///Total roles of Application.
    /// </summary>
    public enum RoleEnum 
    {
        Admin,
        User
    }
}