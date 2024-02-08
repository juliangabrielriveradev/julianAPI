using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using julianapi.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Cryptography;
using julianapi.Data;
using Dapper;

namespace julianapi.Helpers


{
    public class AuthHelper{

        private readonly IConfiguration _config;
          private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }
// make this constructor public so that when we create an instance of this class, we can pass our config into instance
      
          public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
        }

        public string CreateToken(int userId){
                Claim[] claims = new Claim[]{
                    new Claim("userId", userId.ToString())
                };

                SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:TokenKey").Value));

                SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha512Signature);

                SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(claims),
                    SigningCredentials = credentials,
                    Expires = DateTime.Now.AddDays(1)
                };
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                SecurityToken token = tokenHandler.CreateToken(descriptor);
                
                return tokenHandler.WriteToken(token);
        }
        public bool SetPassword(UserForLoginDto userforSetPassword){
             byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }
                    // string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

                    byte[] passwordHash = GetPasswordHash(userforSetPassword.Password, passwordSalt);
                    
                    // Left Side is stored procedure param, Right Hand side is the code parameters being passed in.
                    // protects us against SQL injections, by making sure the values match the value types, SQL injections wont be possible as easy.
                    string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert
                    @Email = @EmailParam, @PasswordHash = @PasswordHashParam, @PasswordSalt = @PasswordSaltParam";
                    

                    // List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    // SqlParameter emailParameter = new SqlParameter("@EmailParam", System.Data.SqlDbType.VarChar);
                    // emailParameter.Value = userforSetPassword.Email;
                    // sqlParameters.Add(emailParameter);

                    // SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", System.Data.SqlDbType.VarBinary);
                    // passwordHashParameter.Value = passwordHash;
                    // sqlParameters.Add(passwordHashParameter);

                    // SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", System.Data.SqlDbType.VarBinary);
                    // passwordSaltParameter.Value = passwordSalt;
                    // sqlParameters.Add(passwordSaltParameter);

                    // alternative to up top
                    DynamicParameters sqlParameters = new DynamicParameters();

                
                    sqlParameters.Add("@EmailParam", userforSetPassword.Email, DbType.String);
                    sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);

                    sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);


                    

                   

                    return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
                 
        }

    }
}