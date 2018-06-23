using System;
using System.Collections.Generic;
using SpringHello.model;
using SpringHello.utillty;

namespace SpringHello.controller
{
    public class CotrollerSpring

    {
        public YYAccountModel model = new YYAccountModel();

        public bool Register()
        {
            while (true)
            {
                YYAccount yyAccount = GetAccountInformation();
                Dictionary<string, string> errors = yyAccount.CheckValidate();
                if (errors.Count > 0)
                {
                    Console.WriteLine("Please fix errors below and try again.");
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }
                }
                else
                {
                    yyAccount.EncryptPassWord();
                    model.Save(yyAccount);
                    return true;
                }
            }
        }

        public void ShowAccountInformation()
        {
            var currentAccount = model.GetByUserName(Program.currentLoggedInYyAccount.Username);
            if (currentAccount == null)
            {
                Console.WriteLine();
                
            }

            Console.WriteLine("Số tài khoản: ");
            Console.WriteLine(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("Số dư hiện tại: ");
            Console.WriteLine(Program.currentLoggedInYyAccount.Balance);
        }

        public bool LoginAccount()
        {
            Console.WriteLine("Enter account infor.");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            YYAccount account = model.Login(username);
            if (account == null)
            {
                Console.WriteLine("Invalid account information.");
                return false;
            }

            if (!account.CheckEncryptPassWord(password))
            {
                Console.WriteLine("Invalid account information.");
                return false;
            }

            Program.currentLoggedInYyAccount = account;
            return true;
        }

        private YYAccount GetAccountInformation()
        {
            while (true)
            {
                Console.WriteLine("---------------Register  Information---------------: ");
                Console.WriteLine("Nhap Username: ");
                var username = Console.ReadLine();
                Console.WriteLine("Nhap Password: ");
                var password = Console.ReadLine();
                Console.WriteLine("Comfirm Password: ");
                var cpassword = Console.ReadLine();
                Console.WriteLine("Nhap Balance: ");
                var balance = Utillty.GetDecimalNumber();
                Console.WriteLine("Nhap IdentityCard: ");
                var identityCard = Console.ReadLine();
                Console.WriteLine("Full Name: ");
                var fullName = Console.ReadLine();
                Console.WriteLine("Nhap Birdthday: ");
                var birdthday = Console.ReadLine();
                Console.WriteLine("Nhap Gender(1. male | 2.temale | 3. rather not say ): ");
                var gender = Utillty.GetInt32Number();
                Console.WriteLine("Nhap Email: ");
                var email = Console.ReadLine();
                Console.WriteLine("Nhap PhoneNumber: ");
                var phone = Console.ReadLine();
                Console.WriteLine("Nhap Address: ");
                var address = Console.ReadLine();
                YYAccount yyAccount = new YYAccount
                {
                    Username = username,
                    Password = password,
                    Cpassword = cpassword,
                    IdentityCard = identityCard,
                    Gender = gender,
                    Balance = balance,
                    Address = address,
                    Dob = birdthday,
                    Name = fullName,
                    Email = email,
                    PhoneNumber = phone
                };
                return yyAccount;
            }
        }


        /*
         * Tiến hành chuyển khoản, mặc định là trong ngân hàng.
         * 1. Yêu cầu nhập số tài khoản cần chuyển.(số tài khoản của người nhận.)
         *     1.1. Xác minh thông tin tài khoản và hiển thị tên người cần chuyển.
         * 2. Nhập số tiền cần chuyển.
         *     2.1. Kiểm tra số dư tài khoản.
         * 3. Nhập nội dung chuyển tiền.
         *     3.1 Xác nhận nội dung chuyển tiền.
         * 4. Thực hiện chuyển tiền.
         *     4.1. Mở transaction. Mở block try catch.
         *     4.2. Trừ tiền người gửi.
         *         4.2.1. Lấy thông tin tài khoản gửi tiền một lần nữa. Đảm bảo thông tin là mới nhất.
         *         4.2.2. Kiểm tra lại một lần nữa số dư xem có đủ tiền để chuyển không.
         *             4.2.2.1. Nếu không đủ thì rollback.
         *             4.2.2.2. Nếu đủ thì trừ tiền và update vào bảng `accounts`.
         *     4.3. Cộng tiền người nhận.
         *         4.3.1. Lấy thông tin tài khoản nhận, đảm bảo tài khoản không bị khoá hoặc inactive.
         *         4.3.1.1. Nếu ok thì update số tiền cho người nhận.
         *         4.3.1.2. Nếu không ok thì rollback.
         *     4.4. Lưu lịch sử giao dịch.
         *     4.5. Kiểm tra lại trạng thái của 3 câu lệnh trên.
         *         4.5.1. Nếu cả 3 cùng thành công thì commit transaction.
         *         4.5.2. Nếu bất kỳ một câu lệnh nào bị lỗi thì rollback.
         *     4.x. Đóng, commit transaction.
         */
        public void Transfer()
        {
            Console.WriteLine("Nhập số tài khoản cần chuyển: ");
            var accountNumber = Console.ReadLine();
            YYAccount account = model.GetByAccountNumber(accountNumber);
            if (account == null)
            {
                Console.WriteLine("Invalid account information");
                return;
            }
            Console.WriteLine("Tên người cần chuyển: " + account.Name);
            Console.WriteLine("Nhập số tiền cần chuyển: ");
            var amount = Utillty.GetDecimalNumber();
            
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.TRANSFER,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = accountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.Tranfer(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.Login(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
           
        }


        public void Deposit()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to deposit: ");
            var amount = Utillty.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.DEPOSIT,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.Login(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }
        
        public void Whithdrow()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to Withdrow: ");
            var amount = Utillty.GetDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new YYTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.WITHDRAW,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.Login(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }
        
        
    }
}