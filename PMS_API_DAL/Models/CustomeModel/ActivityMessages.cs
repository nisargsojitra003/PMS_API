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

    public enum ProductSortType 
    {
        NameAsc = 1,
        NameDesc = 2,
        PriceAsc = 3,
        PriceDesc = 4,
        DescriptionAsc = 5,
        DescriptionDesc = 6,
        CreatedAtAsc = 7,
        CreatedAtDesc = 8,
        ModifiedAtAsc = 9,
        ModifiedAtDesc = 10,
        CategoryTagAsc = 11,
        CategoryTagDesc = 12,
        CategoryNameAsc = 13,
        CategoryNameDesc = 14
    }

    public enum ActivitySortType 
    {
        CreatedAtAsc = 1,
        CreatedAtDesc = 2,
        DescriptionAsc = 3,
        DescriptionDesc = 4,
    }



}