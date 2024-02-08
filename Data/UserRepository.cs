
using julianapi.Models;
namespace julianapi.Data

{

    public class UserRepository : IUserRepository
    {
    DataContextEF _entityFramework;
   
    public UserRepository(IConfiguration config)
    {
      

    _entityFramework = new DataContextEF(config);

  
    }

    public bool SaveChanges(){

        return _entityFramework.SaveChanges() >0;
    }
    // T here means that it will allow us to pass in any type that we need to pass to add method.
    // The first T is what type it expects and then pass in the type for the second T
    public void AddEntity<T>(T entityToAdd){
        if(entityToAdd!= null){
 _entityFramework.Add(entityToAdd);
        }
           
    }

       public void RemoveEntity<T>(T entityToRemove){
        if(entityToRemove!= null){
 _entityFramework.Remove(entityToRemove);
        }
           
    }
public IEnumerable<User> GetUsers()
    {
      
      IEnumerable<User> users = _entityFramework.Users.ToList<User>();
      return users;

    }

     public User GetSingleUser(int userId)
    {
    
      User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();
// entity framework doesnt know if we'll get any results back and we get yellow line above if we dont include this null statement so we should add this.
      if(user != null){
 return user;
      }
             throw new Exception("Failed to Get User");


     

    }
    }
}