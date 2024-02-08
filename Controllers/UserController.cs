using julianapi.Data;
using julianapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace julianapi.Controllers;
// gives us built in functionality to make it work and makes us able to send back and receive json data and other nuances
[ApiController]
// the logic that will reach in and find the name of our class BEFORE the word controller.
[Route("[controller]")]
public class UserController : ControllerBase
{

    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        Console.WriteLine(config.GetConnectionString("DefaultConnection"));

    _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]

    public DateTime TestConnection()

    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @"
        SELECT [UserId]
        , [FirstName]
        , [LastName]
        , [Email]
        , [Gender]
        , [Active] FROM TutorialAppSchema.Users
        ";
      IEnumerable<User> users = _dapper.LoadData<User>(sql);
      return users;

    }

       [HttpGet("GetSingleUser/{userId}")]
    //   Users is the name of our users model.
    public User GetSingleUser(int userId)
    {
    string sql = @"
        SELECT [UserId]
        , [FirstName]
        , [LastName]
        , [Email]
        , [Gender]
        , [Active] FROM TutorialAppSchema.Users
        WHERE UserId = " + userId.ToString(); // "7"
      User user = _dapper.LoadDataSingle<User>(sql);
      return user;

    }

    [HttpPut("EditUser")]
// We can add the [FromBody] but if we also use the User model then that will also act as its coming from the payload.
    public IActionResult EditUser(User user)
    {
        string sql = @"
        UPDATE TutorialAppSchema.Users
         SET [FirstName] = '" + user.FirstName + 
         "', [LastName] = '" + user.LastName +
         "',[Email] = '" + user.Email + 
         "', [Gender] = '" + user.Gender + 
         "',[Active] = '"  + user.Active + 
        "' WHERE UserId = " + user.UserId;
Console.WriteLine(sql);
        if(_dapper.ExecuteSql(sql)){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        }
       throw new Exception("Failed to Update User");
    }
     [HttpPost("AddUser")]

    public IActionResult AddUser(UserToAddDto user)
    {

        string sql = @"INSERT INTO TutorialAppSchema.Users(
                
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
                ) VALUES (" +
                "'" + user.FirstName + 
                "', '" + user.LastName +
                "', '" + user.Email + 
                "', '" + user.Gender + 
                "','"  + user.Active + 
                "')";
        Console.WriteLine(sql);
        if(_dapper.ExecuteSql(sql)){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        }
       throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]

    public IActionResult DeleteUser(int userId)
    {

        string sql = @"DELETE FROM TutorialAppSchema.Users WHERE UserId = " + userId.ToString();
          Console.WriteLine(sql);
        if(_dapper.ExecuteSql(sql)){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        }
       throw new Exception("Failed to Delete User");
    }
}
