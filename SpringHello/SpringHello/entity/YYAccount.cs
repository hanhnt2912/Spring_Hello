using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SpringHello.model;
using SpringHello.utillty;

namespace SpringHello
{
    public class YYAccount
    {
        private string _accountNumber;
        private string _username; //  unique
        private string _password;
        private string _cpassword;
        private string _salt;
        private decimal _balance;
        private string _identityCard; // unique
        private string _name;
        private string _email; // unique
        private string _phoneNumber; // unique
        private string _address;
        private string _dob;
        private int _gender; // 1. male | 2.temale | 3. rather not say  
        private string _createdAt;
        private string _updateAt;
        private int _status; // 1. active | 2. locked | 0. inactive

        public enum AccountStatus
        {
            
            ACTIVE = 1,
            
        }

        public YYAccount()
        {
            GenerateAccountNumber();
            GenerateSalt();
        }

        public void GenerateAccountNumber()
        {
            _accountNumber = Guid.NewGuid().ToString();
        }

        public Boolean CheckEncryptPassWord(string password)
        {
            var checkpassword = Hash.EncryptedString(password, _salt);
            return checkpassword == _password;
        }

       

        public void EncryptPassWord()
        {
            if (string.IsNullOrEmpty(_password))
            {
                throw new ArgumentNullException("Password is null or empyt.");
            }

            _password = Hash.EncryptedString(_password, _salt);
        }

        public void GenerateSalt()
        {
            _salt = Guid.NewGuid().ToString().Substring(0, 7);
        }

        public YYAccount(string accountNumber, string name, string email, string phoneNumber, string address)
        {
            _accountNumber = accountNumber;
            _name = name;
            _email = email;
            _phoneNumber = phoneNumber;
            _address = address;
        }

        public string Cpassword
        {
            get => _cpassword;
            set => _cpassword = value;
        }

        public string Salt
        {
            get => _salt;
            set => _salt = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public string Password
        {
            get => _password;
            set => _password = value;
        }

        public string AccountNumber
        {
            get => _accountNumber;
            set => _accountNumber = value;
        }

        public decimal Balance
        {
            get => _balance;
            set => _balance = value;
        }

        public string IdentityCard
        {
            get => _identityCard;
            set => _identityCard = value;
        }

        public string Email
        {
            get => _email;
            set => _email = value;
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => _phoneNumber = value;
        }

        public string Address
        {
            get => _address;
            set => _address = value;
        }

        public string Dob
        {
            get => _dob;
            set => _dob = value;
        }

        public int Gender
        {
            get => _gender;
            set => _gender = value;
        }

        public string CreatedAt
        {
            get => _createdAt;
            set => _createdAt = value;
        }

        public string UpdateAt
        {
            get => _updateAt;
            set => _updateAt = value;
        }

        public int Status
        {
            get => _status;
            set => _status = value;
        }

        public Dictionary<string, string> CheckValidate()
        {
            YYAccountModel model = new YYAccountModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(this._username))
            {
                errors.Add("username", "UserName can not be left blank");
            }
            else if (this._username.Length < 6)
            {
                errors.Add("username", "Username is to short");
            }else if (model.GetByUserName(this._username))
            {
                errors.Add("username", "This account already exists. Please enter another account");
            }

            if (string.IsNullOrEmpty(this._password))
            {
                errors.Add("password", "Password can not be left blank");    
            }
            else if(_username.Length < 5)
            {
                errors.Add("password", "Password not less than 5 characters");
            }
            if (_cpassword != _password)
            {
                errors.Add("password", "Confirm password dose not match.");
            }
//
//            if (_balance != Utillty.GetDecimalNumber())
//            {
//                errors.Add("balance", "You must enter the amount");
//            }
            
            return errors;
        }
    }
}