using System;
using MySql.Data.MySqlClient;

namespace SpringHello.model
{
    public class YYAccountModel
    {
        public bool Save(YYAccount account)
        {
            DbConnection.Instance().OpenConnection();
            string queryString =
                "insert into `accounts` (accountnumber,name , username, password, salt, balance, identityCard," +
                "email, phoneNumber, address, gender, dob) values " +
                "(@accountnumber,@name , @username, @password, @salt, @balance, @indentityCard," +
                "@email, @phoneNumber, @address, @gender,@dob)";
            MySqlCommand command = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            command.Parameters.AddWithValue("@accountnumber", account.AccountNumber);
            command.Parameters.AddWithValue("@name", account.Name);
            command.Parameters.AddWithValue("@username", account.Username);
            command.Parameters.AddWithValue("@password", account.Password);
            command.Parameters.AddWithValue("@salt", account.Salt);
            command.Parameters.AddWithValue("@balance", account.Balance);
            command.Parameters.AddWithValue("@indentityCard", account.IdentityCard);
            command.Parameters.AddWithValue("@email", account.Email);
            command.Parameters.AddWithValue("@dob", account.Dob);
            command.Parameters.AddWithValue("@phoneNumber", account.PhoneNumber);
            command.Parameters.AddWithValue("@address", account.Address);
            command.Parameters.AddWithValue("@gender", account.Gender);
            command.ExecuteNonQuery();
            DbConnection.Instance().CloseConnection();
            return true;
        }

        public bool GetByUserName(string username)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "select * from `accounts` where username = @username and status = 1";
            MySqlCommand command = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            var isExit = reader.Read();
            DbConnection.Instance().CloseConnection();
            return isExit;
        }

        public YYAccount Login(string username)
        {
            YYAccount account = null;
            DbConnection.Instance().OpenConnection();
            string queryString = "select * from `accounts` where username = @username";
            MySqlCommand command = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            command.Parameters.AddWithValue("@username", username);
            MySqlDataReader reader = command.ExecuteReader();
            var isExit = reader.Read();
            if (isExit)
            {
                account = new YYAccount();
                account.AccountNumber = reader.GetString("accountnumber");
                account.Username = reader.GetString("username");
                account.Email = reader.GetString("email");
                account.Name = reader.GetString("name");
                account.PhoneNumber = reader.GetString("phoneNumber");
                account.Address = reader.GetString("address");
                account.Password = reader.GetString("password");
                account.Salt = reader.GetString("salt");
                account.Balance = reader.GetInt32("balance");
                account.IdentityCard = reader.GetString("identityCard");
                account.Gender = reader.GetInt32("gender");
                account.Status = reader.GetInt32("status");
                DbConnection.Instance().CloseConnection();
                return account;
            }

            return null;
        }

        public YYAccount GetByAccountNumber(string accountnumber)
        {
            YYAccount account = null;
            DbConnection.Instance().OpenConnection();
            string queryString = "select * from `accounts` where accountnumber = @accountnumber and status = 1";
            MySqlCommand command = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            command.Parameters.AddWithValue("@accountnumber", accountnumber);
            MySqlDataReader reader = command.ExecuteReader();
            var isExit = reader.Read();
            if (isExit)
            {
                account = new YYAccount();
                account.AccountNumber = reader.GetString("accountnumber");
                account.Username = reader.GetString("username");
                account.Email = reader.GetString("email");
                account.Name = reader.GetString("name");
                account.PhoneNumber = reader.GetString("phoneNumber");
                account.Address = reader.GetString("address");
                account.Password = reader.GetString("password");
                account.Salt = reader.GetString("salt");
                account.Balance = reader.GetInt32("balance");
                account.IdentityCard = reader.GetString("identityCard");
                account.Gender = reader.GetInt32("gender");
                account.Status = reader.GetInt32("status");
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }


        public bool UpdateBalance(YYAccount account, YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.

            try
            {
                /**
                 * 1. Lấy thông tin số dư mới nhất của tài khoản.
                 * 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw.
                 *     2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.                 
                 * 3. Update số dư vào tài khoản.
                 *     3.1. Tính toán lại số tiền trong tài khoản.
                 *     3.2. Update số tiền vào database.
                 * 4. Lưu thông tin transaction vào bảng transaction.
                 */

                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `accounts` where username = @username and status = @status";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (historyTransaction.Type != YYTransaction.TransactionType.DEPOSIT
                    && historyTransaction.Type != YYTransaction.TransactionType.WITHDRAW)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (historyTransaction.Type == YYTransaction.TransactionType.WITHDRAW &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (historyTransaction.Type == YYTransaction.TransactionType.DEPOSIT)
                {
                    currentBalance -= historyTransaction.Amount;
                }
                else
                {
                    currentBalance
                        += historyTransaction.Amount;
                }

                // 3.2. Update số dư vào database.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `accounts` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        public bool Tranfer(YYAccount currentLoggedInYyAccount, YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction();
            try
            {
                
                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance =
                    "select balance from `accounts` where accountnumber = @accountnumber and status = @status";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@accountnumber", currentLoggedInYyAccount.AccountNumber);
                queryBalanceCommand.Parameters.AddWithValue("@status", currentLoggedInYyAccount.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                if (historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                currentBalance -= historyTransaction.Amount;

                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `accounts` set balance = @balance where accountnumber = @accountnumber and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@accountnumber",
                    currentLoggedInYyAccount.AccountNumber);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();


                var queryBalanceRecever =
                    "select balance from `accounts` where accountnumber = @accountnumber and status = @status";
                MySqlCommand queryBalanceCommandRecever =
                    new MySqlCommand(queryBalanceRecever, DbConnection.Instance().Connection);
                queryBalanceCommandRecever.Parameters.AddWithValue("@accountnumber",
                    historyTransaction.ReceiverAccountNumber);
                queryBalanceCommandRecever.Parameters.AddWithValue("@status", currentLoggedInYyAccount.Status);
                var balanceReaderRecever = queryBalanceCommandRecever.ExecuteReader();


                if (!balanceReaderRecever.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalanceRecever = balanceReaderRecever.GetDecimal("balance");
                balanceReaderRecever.Close();

                if (historyTransaction.Amount > currentBalanceRecever)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                currentBalanceRecever += historyTransaction.Amount;

                var updateAccountResultRecever = 0;
                var queryUpdateAccountBalanceRecever =
                    "update `accounts` set balance = @balance where accountnumber = @accountnumber and status = 1";
                var cmdUpdateAccountBalanceRecever =
                    new MySqlCommand(queryUpdateAccountBalanceRecever, DbConnection.Instance().Connection);
                cmdUpdateAccountBalanceRecever.Parameters.AddWithValue("@accountnumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdUpdateAccountBalanceRecever.Parameters.AddWithValue("@balance", currentBalanceRecever);
                updateAccountResultRecever = cmdUpdateAccountBalanceRecever.ExecuteNonQuery();


                // 4. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1 && updateAccountResultRecever == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return true;
        }
    }

    public class SpringHeroTransactionException : Exception
    {
        public SpringHeroTransactionException(string invalidTransactionType)
        {
            throw new NotImplementedException();
        }
    }
}