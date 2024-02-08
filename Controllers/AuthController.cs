
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetApi.Helpers;
using julianapi.Data;
using julianapi.Dtos;
using julianapi.Helpers;
using julianapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
namespace julianapi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly DataContextDapper _dapper;

        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        
            // with the underscore we have a private fields name, just authHelper would be local(without underscore)
            _authHelper = new AuthHelper(config);

            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg =>{
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
            }));
        }
        [AllowAnonymous]
        [HttpPost("Register")]

        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" +
                userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto(){
                            Email = userForRegistration.Email,
                            Password = userForRegistration.Password
                    };
                    if(_authHelper.SetPassword(userForSetPassword))

                  

                   

                    {
                        // had to map this because reusablesql takes in a usercomplete object so I mapped userforRegistration to userComplete
                            UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                            // had to set this to true when we create user because we dont want it set to false when creating user and we also dont have this value on the userforregistrationDTO
                            userComplete.Active = true;


                        //   string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert
                        //     @FirstName = '" + userForRegistration.FirstName + 
                        //     "', @LastName = '" + userForRegistration.LastName +
                        //     "',@Email = '" + userForRegistration.Email + 
                        //     "', @Gender = '" + userForRegistration.Gender + 
                        //     "', @JobTitle = '" + userForRegistration.JobTitle + 
                        //     "', @Department = '" + userForRegistration.Department + 
                        //     "', @Salary = '" + userForRegistration.Salary + 
                        //     "', @Active = 1";
                        //             string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
                            
                        //     [FirstName],
                        //     [LastName],
                        //     [Email],
                        //     [Gender],
                        //     [Active]
                        //     ) VALUES (" +
                        //    "'" + userForRegistration.FirstName +
                        //    "', '" + userForRegistration.LastName +
                        //    "', '" + userForRegistration.Email +
                        //    "', '" + userForRegistration.Gender +
                        //    "',1)";

                        if(_reusableSql.UpsertUser(userComplete))
                       { return Ok();}
                          throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");

                }
                throw new Exception("Users with this email aldready exists!");

            }
            throw new Exception("Passwords do not match!");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword){
                    if(_authHelper.SetPassword(userForSetPassword)){
                        return Ok();
                    }
            throw new Exception("Failed to update password!");


        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {

            string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get
            @Email = @EmailParam";

             DynamicParameters sqlParameters = new DynamicParameters();

                    // SqlParameter emailParameter = new SqlParameter("@EmailParam", System.Data.SqlDbType.VarChar);
                    // emailParameter.Value = userForLogin.Email;
                    // sqlParameters.Add(emailParameter);
                
            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);
            UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt,sqlParameters);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            // if(passwordHash = userForConfirmation.PasswordHash) // wont work because this is comparing the memory address stored so it wont ever be true.
            for (int index = 0; index < passwordHash.Length; index++)
            {

                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password!");
                    // this works too 
                    // throw new Exception("Incorrect password!");
                }

            }

            // change to stored procedure
            // string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users WHERE Email =  '" + userForLogin.Email + "'";
            string userIdSql = @"EXEC TutorialAppSchema.spUserId_Get @Email = @EmailParam";
            int userId = _dapper.LoadDataSingleWithParameters<int>(userIdSql,sqlParameters);
            Console.WriteLine(userId);
            return Ok( new Dictionary<string, string>{
                {
                    "token", _authHelper.CreateToken(userId)

                }
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken(){

            // string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users WHERE UserId =  '" + User.FindFirst("userId")?.Value + "'";
            string userIdSql = @"EXEC TutorialAppSchema.spUserId_Get @UserId=@UserIdParameter";
                // the this above means we want to access it from this class
            DynamicParameters sqlParameters = new DynamicParameters();
            
            sqlParameters.Add("@UserIdParameter",this.User.FindFirst("userId")?.Value,DbType.String);

            int userId = _dapper.LoadDataSingleWithParameters<int>(userIdSql, sqlParameters);

            return _authHelper.CreateToken(userId);
        }






    }
}