using AutoMapper;
using BinksNoSake.Application.Contratos;
using BinksNoSake.Application.Dtos;
using BinksNoSake.Domain.Identity;
using BinksNoSake.Persistence.Contratos;
using Microsoft.AspNetCore.Identity;

namespace BinksNoSake.Application.Services;
public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly SignInManager<Account> _signInManager;
    private readonly IAccountPersist _accountPersist;
    private readonly IMapper _mapper;

    public AccountService(UserManager<Account> userManager, SignInManager<Account> signInManager, IMapper mapper, IAccountPersist accountPersist)
    {
        _accountPersist = accountPersist;
        _signInManager = signInManager;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<SignInResult> CheckUserPasswordAsync(AccountUpdateDto accountUpdateDto, string password)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(accountUpdateDto.Username.ToLower());

            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }
        catch (System.Exception e)
        {

            throw new Exception($"Erro ao verificar senha. {e.Message}");
        }
    }

    public async Task<AccountDto> CreateAccountAsync(AccountDto accountDto)
    {
        try
        {
            var user = _mapper.Map<Account>(accountDto);
            var result = await _userManager.CreateAsync(user, accountDto.Password);

            if (result.Succeeded)
            {
                var userToReturn = _mapper.Map<AccountDto>(user);
                return userToReturn;
            }
            return null;
        }
        catch (System.Exception e)
        {

            throw new Exception($"Erro ao criar usuário. {e.Message}");
        }
    }

    public async Task<AccountUpdateDto> GetUserByUsernameAsync(string username)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username.ToLower());
            if (user == null) return null;
            return _mapper.Map<AccountUpdateDto>(user);
        }
        catch (System.Exception e)
        {

            throw new Exception($"Erro ao obter usuário. {e.Message}");
        }
    }

    public async Task<AccountUpdateDto> UpdateAccount(AccountUpdateDto accountUpdateDto)
    {
        try
        {
            var user = await _accountPersist.GetUserByUsernameAsync(accountUpdateDto.Username);
            if (user == null) return null;
            accountUpdateDto.Id = user.Id;

            if (accountUpdateDto.Password != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, accountUpdateDto.Password);
            }
            _accountPersist.Update<Account>(user);

            if (await _accountPersist.SaveChangesAsync())
            {
                var userToReturn = await _accountPersist.GetUserByUsernameAsync(accountUpdateDto.Username);
                return _mapper.Map<AccountUpdateDto>(userToReturn);
            }
            return null;
        }
        catch (System.Exception e)
        {

            throw new Exception($"Erro ao atualizar usuário. {e.Message}");
        }
    }

    public async Task<bool> UserExists(string username)
    {
        try
        {
            return await _userManager.FindByNameAsync(username.ToLower()) != null;
        }
        catch (System.Exception e)
        {

            throw new Exception($"Erro ao verificar usuário. {e.Message}");
        }
    }
}