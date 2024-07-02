namespace PMS_API_DAL.Models.CustomeModel
{
    public class ActivityMessages
    {
        //createAccount Activity message.
        public string createAccountRecord = "Account has been created using Email : {1}";

        //Common Activity messages.
        public string add = "{1} {2} has been created.";
        public string edit = "{1} {2} has been updated.";
        public string delete = "{1} {2} has been deleted.";
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

    public enum CategorySortType 
    { 
        NameAsc = 1,
        NameDesc = 2,   
        CodeAsc = 3,
        CodeDesc = 4,
        CreatedAtAsc = 5,
        CreatedAtDesc = 6,
        ModifiedAtAsc = 7,
        ModifiedAtDesc = 8,
        DescriptionAsc = 9,
        DescriptionDesc = 10
    }

}