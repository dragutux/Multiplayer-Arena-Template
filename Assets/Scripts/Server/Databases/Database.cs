﻿using UnityEngine;
using DarkRift.Server;
using MySql.Data.MySqlClient;
using System.Collections;
/*
using System.Collections.Generic;
using System.Threading;
using DarkRift;
*/
namespace Server
{
    public class Database : MonoBehaviour
    {
        public static Database getInstance;
        void Awake()
        {
            getInstance = this;
            Connect();
            StartCoroutine(SaveAccountsThread());
        }

        [SerializeField] string DbServer = "192.168.1.2";
        [SerializeField] string DbName = "arena";
        [SerializeField] string DbUsername = "arena";
        [SerializeField] string DbPassword = "arena";
        [SerializeField] string DBSSslMode = "none";

        public void Login(IClient client, string nickname/*, string password*/)
        {
            //password = Cryptography.Encrypt_Custom(password);

            Debug.Log("Login");

            try
            {
                if (isLoggedIn(client))
                {
                    Server.getInstance.LoginResponse(false, "Client already logged in!", client);
                    return;
                }
                else if (isLoggedIn(nickname))
                {
                    Server.getInstance.LoginResponse(false, "Account already logged in!", client);
                    return;
                }
                else if (!accountExists(nickname))
                {
                    Server.getInstance.LoginResponse(false, "Account not found!", client);
                    return;
                }

                if (openConnection())
                {
                    // switch to nick, keep orig request as ref
                    /*
                    string query = "SELECT * FROM accounts WHERE email='" + email + "' AND password='" + password + "'";
                    */

                    string query = "SELECT * FROM accounts WHERE name='" + nickname + "'";//  /* AND password='" + password */ + "'";
                    Debug.Log("loginq : " + query);

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool done = false;

                    if (reader.Read())
                    {
                        string name = reader.GetString(reader.GetOrdinal("name"));
                        Gender gender = (Gender)reader.GetInt32(reader.GetOrdinal("gender"));

                        Account account = new Account(client, nickname, name, /*password,*/ gender, AccountState.InLobby, -1, "None");

                        Holder.accounts.Add(client, account);

                        done = true;
                    }

                    reader.Close();
                    closeConnection();

                    if (done) Server.getInstance.LoginResponse(true, "", client);
                    else Server.getInstance.LoginResponse(false, "Wrong password!", client);
                }
                else
                {
                    Server.getInstance.LoginResponse(false, "Server error!", client);
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to login from client with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
            }
        }
        public void Register(IClient client, string email,/* string password, */ string name, Gender gender)
        {
            //password = Cryptography.Encrypt_Custom(password);

            try
            {
                /*if (!email.Contains("@") || !email.Contains(".") || email.Length < 2)
                {
                    Server.getInstance.RegistrationResponse(false, "Wrong email form!", client);
                    return;
                }
                */
                if (gender != Gender.Male && gender != Gender.Female)
                {
                    Server.getInstance.RegistrationResponse(false, "Invalid gender!", client);
                    return;
                }
                /*
                if (password.Length <= 4)
                {
                    Server.getInstance.RegistrationResponse(false, "Password's length sould be greater than 4!", client);
                    return;
                }
                
                if (accountExists(email))
                {
                    Server.getInstance.RegistrationResponse(false, "Account already exists!", client);
                    return;
                }
                */
                if (accountNameExists(name))
                {
                    Server.getInstance.RegistrationResponse(false, "Account name already exists!", client);
                    return;
                }

                if (openConnection())
                {
                    // remove email later add steamID
                    string query = "INSERT INTO accounts(email, password, name, gender) VALUES ('" + email + "', '" /*+ password +  "', '" */+ name + "', '" + (int)gender + "')";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
                else
                {
                    Server.getInstance.RegistrationResponse(false, "Server error!", client);
                    return;
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to register from account with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
                return;
            }

            Server.getInstance.RegistrationResponse(true, "", client);
        }
        public void SaveAccount(IClient client)
        {
            if (!Holder.accounts.ContainsKey(client)) return;

            try
            {
                if (openConnection())
                {
                    string query = "UPDATE accounts SET "/*password='" + Holder.accounts[client].Password */ + "', gender='" + (int)Holder.accounts[client].Gender + "' WHERE email='" + Holder.accounts[client].Email + "'";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on saving account from client with id: " + client.ID + " with error: " + ex.Message, LogType.Error);
            }
        }
        public void DeleteAccount(string email)
        {
            try
            {
                if (openConnection())
                {
                    string query = "DELETE FROM accounts WHERE email='" + email + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.ExecuteNonQuery();

                    closeConnection();
                }
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on delete account with email: " + email + " with error: " + ex.Message, LogType.Error); }
        }
        public bool isLoggedIn(string email)
        {
            foreach (var account in Holder.accounts.Values)
            {
                if (account.Email == email)
                    return true;
            }

            return false;
        }
        bool isLoggedIn(IClient client)
        {
            foreach (var account in Holder.accounts.Keys)
            {
                if (account == client)
                    return true;
            }

            return false;
        }
        public bool accountExists(string nickname)
        {
            try
            {
                if (openConnection())
                {
                    Debug.Log("openConnection returned " + openConnection());
                    string query = "SELECT * FROM accounts WHERE name='" + nickname + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool exists = false;

                    if (reader.Read())
                        exists = true;

                    reader.Close();
                    closeConnection();

                    return exists;
                }
                else return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to seach for an account with email: " + nickname + " with error: " + ex.Message, LogType.Error);
                return true;
            }
        }
        public bool accountNameExists(string name)
        {
            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM accounts WHERE name='" + name + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    bool exists = false;

                    if (reader.Read())
                        exists = true;

                    reader.Close();
                    closeConnection();

                    return exists;
                }
                else return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on trying to seach for an account with name: " + name + " with error: " + ex.Message, LogType.Error);
                return true;
            }
        }
        IEnumerator SaveAccountsThread()
        {
            while (true)
            {
                yield return new WaitForSeconds(60 * 60 * 10);

                foreach (var account in Holder.accounts.Keys)
                    SaveAccount(account);

                yield return null;
            }
        }
        public string getPassword(string nickname)
        {
            string password = string.Empty;

            try
            {
                if (openConnection())
                {
                    string query = "SELECT * FROM accounts WHERE name='" + nickname + "'";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                        password = Cryptography.Encrypt_Custom(reader.GetString(reader.GetOrdinal("password")));

                    reader.Close();
                    closeConnection();
                }
            }
            catch (MySqlException ex) { Server.getInstance.Log("Error on searching for a password for email: " + nickname + " with error: " + ex.Message, LogType.Error); }
            return password;
        }

        MySqlConnection connection;
        void Connect()
        {
            string connectionString = "Server=" + DbServer + ";Database=" + DbName + ";Uid=" + DbUsername + ";Pwd=" + DbPassword + ";SslMode=" + DBSSslMode + ";";
            connection = new MySqlConnection(connectionString);

            Debug.Log("connection parameters is : " + connectionString);
        }
        bool openConnection()
        {
            if (connection == null)
            {
                Debug.Log("no link, opening DB");
                Connect();
            }                

            try
            {
                connection.OpenAsync();
                Debug.Log("can open DB");
                return true;
            }
            catch (MySqlException ex)
            {
                Debug.Log("cant open DB");
                Server.getInstance.Log("Error on opening a database connection! " + ex.Message, LogType.Error);
                return false;
            }
        }
        bool closeConnection()
        {
            if (connection == null)
            {
                Server.getInstance.Log("Trying to close a null database connection!", LogType.Error);
                return false;
            }

            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Server.getInstance.Log("Error on closing a database connection: " + ex.Message, LogType.Error);
                return false;
            }
        }
    }
}