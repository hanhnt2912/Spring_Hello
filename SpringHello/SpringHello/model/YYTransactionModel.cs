using System;
using System.Collections.Generic;
using System.Transactions;
using MySql.Data.MySqlClient;

namespace SpringHello.model
{
    public class YYTransactionModel
    {
        public List<YYTransaction> getTransactionByAccountNumber(string accountNumber)
        {
            YYTransaction transaction = null;
            var lisTransaction = new List<YYTransaction>();
            var queryString = "select * from `accounts` where senderAccountNumber = @accountnumber or receiverAccountNumber = @accountnumber";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountnumber", accountNumber);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                transaction = new YYTransaction()
                {
                    Id = reader.GetString("id"),
                    CreatedAt = reader.GetString("createAt"),
                    UpdatedAt = reader.GetString("updateAt"),
                    Amount = reader.GetDecimal("amount"),
                    Content = reader.GetString("content"),
                    SenderAccountNumber = reader.GetString("senderAccountNumber"),
                    ReceiverAccountNumber = reader.GetString("receiverAccountNumber"),
                    Type = (YYTransaction.TransactionType) reader.GetInt32("type"),
                    Status = (YYTransaction.ActiveStatus) reader.GetInt32("status")
                     
                    
                };
                
                lisTransaction.Add(transaction);
                
            }
            
            
            return lisTransaction;
        }
    }
}