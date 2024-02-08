using System.Data;
using System.Diagnostics;
using Dapper;
using DotnetApi.Helpers;
using julianapi.Data;
using julianapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace julianapi.Controllers;
// [Authorize]
// gives us built in functionality to make it work and makes us able to send back and receive json data and other nuances
[ApiController]
// the logic that will reach in and find the name of our class BEFORE the word controller.
[Route("[controller]")]
public class UserCompleteController : Controller
{

    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public UserCompleteController(IConfiguration config)
    {
        Console.WriteLine(config.GetConnectionString("DefaultConnection"));

    _dapper = new DataContextDapper(config);
    // pass in config to be able to create an instance of dapper here
    _reusableSql = new ReusableSql(config);
    }

    [HttpGet("TestConnection")]

    public DateTime TestConnection()

    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }
    // [HttpGet]
    // public IActionResult Index(){
    //   var users = UserCompleteController.GetUsers(int userId, Active);
    //   return View();
    // }

    [HttpGet("GetUsers")]
    public IActionResult GetUsers(int userId, bool Active)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string stringParameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if(userId !=0){
        stringParameters += ", @UserId=@UserIdParameter";
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
          if(Active){
        stringParameters  += ", @Active=@ActiveParameter";
        sqlParameters.Add("@ActiveParameter", Active, DbType.Boolean);

        }
        if(stringParameters.Length > 0){
        sql += stringParameters.Substring(1);

        }
       
      IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
      Console.WriteLine(users);
      foreach(var user in users){
              Console.WriteLine(user.UserId);
Debug.WriteLine(user.UserId);
      }
      Debug.WriteLine(users);
      return View(users);

    }

      

    [HttpPut("UpsertUser")]
// We can add the [FromBody] but if we also use the User model then that will also act as its coming from the payload.
    public IActionResult UpsertUser(UserComplete user)
    {

 
     
        if(_reusableSql.UpsertUser(user)){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        }
       throw new Exception("Failed to Update User");
    }
   
    [HttpDelete("DeleteUser/{userId}")]

    public IActionResult DeleteUser(int userId)
    {

        string sql = @"TutorialAppSchema.spUser_Delete @UserId = @UserIdParameter";
                
        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

          Console.WriteLine(sql);
        if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters)){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        }
       throw new Exception("Failed to Delete User");
    }
}
