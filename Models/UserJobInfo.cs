
// we added .models as an extra subname instead of just julianapi because when the app is big enough then it will slow everything down
namespace julianapi.Models
{
// partial because if we want to add inside of this class from another file then we are able to
    public partial class UserJobInfo
    {
       public int UserId{get; set;}
       public string JobTitle{get; set;} = "";
       public string Department{get; set;}= "";
    


    }
}