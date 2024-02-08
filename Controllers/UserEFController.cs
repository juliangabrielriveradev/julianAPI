using AutoMapper;
using julianapi.Data;
using julianapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace julianapi.Controllers;
// gives us built in functionality to make it work and makes us able to send back and receive json data and other nuances
[ApiController]
// the logic that will reach in and find the name of our class BEFORE the word controller.
[Route("[controller]")]
public class UserEFController : ControllerBase
{

    IUserRepository _userRepository;
    IMapper _mapper;
    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
      

    _userRepository = userRepository;
    _mapper = new Mapper(new MapperConfiguration(cfg =>{
        cfg.CreateMap<UserToAddDto, User>();
    }));
    }


    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
      
      IEnumerable<User> users = _userRepository.GetUsers();
      return users;

    }

       [HttpGet("GetSingleUser/{userId}")]
    //   Users is the name of our users model.
    public User GetSingleUser(int userId)
    {
    
   
    return _userRepository.GetSingleUser(userId);

     

    }

    [HttpPut("EditUser")]
// We can add the [FromBody] but if we also use the User model then that will also act as its coming from the payload.
    public IActionResult EditUser(User user)
    {
         User? userDb = _userRepository.GetSingleUser(user.UserId);
// entity framework doesnt know if we'll get any results back and we get yellow line above if we dont include this null statement so we should add this.
      if(userDb != null){
       userDb.Active = user.Active;
       userDb.FirstName = user.FirstName;
       userDb.LastName = user.LastName;
       userDb.Email = user.Email;
       userDb.Gender = user.Gender;
      

        if(_userRepository.SaveChanges()){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        
        }
               throw new Exception("Failed to Update User");

      }
       throw new Exception("Failed to Update User");
      }
     [HttpPost("AddUser")]

    public IActionResult AddUser(UserToAddDto user)
    {

         User userDb = _mapper.Map<User>(user);
// entity framework doesnt know if we'll get any results back and we get yellow line above if we dont include this null statement so we should add this.
   
 
    _userRepository.AddEntity<User>(userDb);
      

        if(_userRepository.SaveChanges()){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        
        }
               throw new Exception("Failed to Add User");
}

    [HttpDelete("DeleteUser/{userId}")]

    public IActionResult DeleteUser(int userId)
    {

       User? userDb = _userRepository.GetSingleUser(userId);
// entity framework doesnt know if we'll get any results back and we get yellow line above if we dont include this null statement so we should add this.
      if(userDb != null){

       _userRepository.RemoveEntity<User>(userDb);

        if(_userRepository.SaveChanges()){
 // built in method that comes from our controllerbase class that we are inheriting from
        return Ok();
        
        }
               throw new Exception("Failed to Delete User");

      }
       throw new Exception("Failed to Get User");
}
}