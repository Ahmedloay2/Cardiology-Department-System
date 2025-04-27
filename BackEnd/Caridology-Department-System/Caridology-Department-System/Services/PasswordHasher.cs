using BCrypt.Net;
using Caridology_Department_System;
public class PasswordHasher
{
    private const int WorkFactor = 12; // Good balance between security and performance

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        try
        {
            // Explicitly use BCrypt.Net.BCrypt to avoid ambiguity
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }
        catch (SaltParseException ex)
        {
            throw new Exception("Failed to generate password hash", ex);
        }
        catch (ArgumentException ex)
        {
            throw new Exception("Invalid password format", ex);
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            // Explicit namespace to avoid conflicts
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch (SaltParseException)
        {
            // Invalid hash format
            return false;
        }
        catch (ArgumentException)
        {
            // Invalid password format
            return false;
        }
    }
}