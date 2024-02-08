using julianapi.Models;

namespace julianapi.Data
{

    public interface IUserRepository{
// calls to the methods
 public bool SaveChanges();
   public void AddEntity<T>(T entityToAdd);
      public void RemoveEntity<T>(T entityToRemove);
      public IEnumerable<User> GetUsers();
       public User GetSingleUser(int userId);
    }

    
}