using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Caridology_Department_System.Models;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryInMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var key = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        _audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
        _expiryInMinutes = Convert.ToInt32(configuration["Jwt:ExpiryInMinutes"] ?? "60");
    }
    /// <summary>
    /// Generates a JSON Web Token (JWT) that contains the user's ID, email, first name, last name, and role name.
    /// The token is signed using the HMAC SHA-256 algorithm.
    /// </summary>
    /// <param name="user">The user whose data will be included in the token claims.</param>
    /// <returns>The generated JWT token as a string.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="user"/> is <c>null</c>.
    /// </exception>
    public string GenerateToken(UserModel user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LName ?? string.Empty),
            new Claim(ClaimTypes.Role,user.Role.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token identifier
        };

        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}