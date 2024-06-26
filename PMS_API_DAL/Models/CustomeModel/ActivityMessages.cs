namespace PMS_API_DAL.Models.CustomeModel
{
    public class ActivityMessages
    {
        //createAccount Activity message.
        public string createAccountRecord = "Account has been created using Email : {1}";

        //Category Activity messages.
        public string add = "{1} {2} has been created";
        public string edit = "{1} {2} has been updated";
        public string delete = "{1} {2} has been deleted";
    }

    public enum TypeOfItem 
    {
        category,
        product
    }

}
