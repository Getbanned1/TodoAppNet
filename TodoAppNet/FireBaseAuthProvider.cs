using Firebase.Auth;
using System;
using System.Threading.Tasks;

public class FirebaseAuthService
{
    private readonly FirebaseAuthClient _firebaseAuth;

    public FirebaseAuthService(FirebaseAuthClient firebaseAuth)
    {
        _firebaseAuth = firebaseAuth;
    }

    public async Task<UserCredential> SignUpAsync(string email, string password)
    {
        try
        {
            return await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception($"Ошибка регистрации: {ex.Reason} (код: {ex.Source})", ex);
        }
    }

    public async Task<UserCredential> LoginAsync(string email, string password)
    {
        try
        {
            return await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception($"Ошибка входа: {ex.Reason} (код: {ex.Source})", ex);
        }
    }

    public void SignOut() => _firebaseAuth.SignOut();
}
