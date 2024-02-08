using System.Data;
using System.Diagnostics;
using Dapper;
using julianapi.Data;
using julianapi.Dtos;
using julianapi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace julianapi.Controllers

{
    // [Authorize]
    [ApiController]
    // pulls whatever is in class name before controller and set it as the base of route, so all routes inside of this controller will be /Post/whateverwesetthemto
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
            private readonly DataContextDapper _dapper;
            public PostController(IConfiguration config){
                _dapper = new DataContextDapper(config);
            }
            [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
// if we put none in the searchParam like we do here then we know we can skip the searchParam and not add it to the filter.
            public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
            {

                string sql = @"EXEC TutorialAppSchema.spPosts_Get";
                string stringParameters = "";
                DynamicParameters sqlParameters = new DynamicParameters();

                if(postId != 0){
                    stringParameters += ", @PostId=@PostIdParameter";
                    sqlParameters.Add("@PostIdParameter",postId,DbType.Int32);
                }
                 if(userId != 0){
                    stringParameters += ", @UserId= @UserIdParameter";
                    sqlParameters.Add("@UserIdParameter",userId,DbType.Int32);

                }
                  if(searchParam.ToLower() != "none"){
                    stringParameters += ", @SearchValue=@SearchParameter";
                    sqlParameters.Add("@SearchParameter",searchParam,DbType.String);

                }

                if(stringParameters.Length >0)
                {
                     sql += stringParameters.Substring(1);
                }
                return _dapper.LoadDataWithParameters<Post>(sql,sqlParameters);
            }

              

          
            [HttpGet("MyPosts")]

            public IEnumerable<Post> GetMyPosts()
            {

                string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId=@UserIdParameter";
                // the this above means we want to acces it from this class
                DynamicParameters sqlParameters = new DynamicParameters();
               sqlParameters.Add("@UserIdParameter",this.User.FindFirst("userId")?.Value,DbType.String);

                

                return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
            }
            
           
            [HttpPut("UpsertPost")]
            public IActionResult UpsertPost(Post postToUpsert){
//  single quotations protect from sql injections but in theory someone could break out of the single quotation with a value in their post title that has a single quotation and once they break out of that they can type in their own query and run something we dont want them to run
                string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                             @UserId =@UserIdParameter,
                             @PostTitle = @PostTitleParameter,
                             @PostContent = @PostContentParameter";
                            
                            
                DynamicParameters sqlParameters = new DynamicParameters();

                sqlParameters.Add("@UserIdParameter",this.User.FindFirst("userId")?.Value,DbType.Int32);
                sqlParameters.Add("@PostTitleParameter",postToUpsert.PostTitle,DbType.String);
                sqlParameters.Add("@PostContentParameter",postToUpsert.PostContent,DbType.String);





                if(postToUpsert.PostId > 0){
                    sql +=  ", @PostId=@PostIdParameter";
                    sqlParameters.Add("@PostIdParameter",postToUpsert.PostId,DbType.Int32);

                    }
                             
             

                if(_dapper.ExecuteSqlWithParameters(sql,sqlParameters)){
                    return Ok();
                }
                throw new Exception("Failed to upsert post!");
            }
          

            [HttpDelete("Post/{postId}")]

            public IActionResult DeletePost(int postId)
            {
                    string sql = @"EXEC TutorialAppSchema.spPost_Delete 
                                    @PostId = @PostIdParameter,
                                    @UserId = @UserIdParameter";
                DynamicParameters sqlParameters = new DynamicParameters();

                sqlParameters.Add("@UserIdParameter",this.User.FindFirst("userId")?.Value,DbType.Int32);
                sqlParameters.Add("@PostIdParameter",postId,DbType.Int32);


                if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters)){
                    return Ok();
                }
                throw new Exception("Failed to delete post!");            
     }
    }
}