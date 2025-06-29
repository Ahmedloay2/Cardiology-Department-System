using BCrypt.Net;
using Caridology_Department_System;
public class PasswordHasher
{
    private const int WorkFactor = 12; // Good balance between security and performance
    /// <summary>
    /// Hashes the provided password using the BCrypt algorithm for secure storage.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password string.</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    /// <summary>
    /// Verifies whether the provided plain text password matches the stored hashed password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hashedPassword">The previously stored hashed password.</param>
    /// <returns><c>true</c> if the password is correct; otherwise, <c>false</c>.</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}