namespace julianapi.Models
{
// partial because if we want to add inside of this class from another file then we are able to
    public partial class User
    {
       public int UserId{get; set;}
       public string FirstName{get; set;} = "";
       public string LastName{get; set;}= "";
       public string Email{get; set;}= "";
       public string Gender{get; set;}= "";
       public bool Active{get; set;}

       public string TestingBranch {get; set;}= "";
    public string TestingBranch2 {get; set;}= "";
    public string TestingBranch3 {get; set;}= "";

       


    }
}