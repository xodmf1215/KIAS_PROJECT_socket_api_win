using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Data.SqlClient;

namespace KIAS_PROJECT_socket_api_win
{
    public class MiniServer
    {
        private Form1 window = null;
        private Dictionary<string, string> sessionData = new Dictionary<string, string>();
        private string sessionCookieName = "sessionId";

        public MiniServer(Form1 form)
        {
            this.window = form;
        }
        
        private AuthPair ParseQueryString(string queryString)
        {
            AuthPair result = null;
            if (!string.IsNullOrEmpty(queryString))
            {
                string[] parts = queryString.Split('&');
                if(parts.Length == 2)
                {
                    result.id = parts[0];
                    result.password = parts[1];
                }
            }
            return result;
        }
        private bool AuthenticateUser(string id, string password)
        {
            // Add code to authenticate the user credentials against the database
            // Return true if the credentials are valid, false otherwise

            // Example authentication using a SQL Server database
            /*
            string connectionString = "Data Source=localhost:10010;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM auth WHERE id = @id AND password = @password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@password", password);
                    int result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
            */
            if (id.Equals("xodmf") && password.Equals("xodmf"))
            {
                return true;
            }
            else
                return false;
        }
        private static string GenerateSessionId()
        {
            // Generate a random session ID
            byte[] sessionIdBytes = new byte[16];
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(sessionIdBytes);
            }
            string sessionId = BitConverter.ToString(sessionIdBytes).Replace("-", string.Empty);

            return sessionId;
        }

        public async void StartServer()
        {
            // Create a new HttpListener instance
            HttpListener listener = new HttpListener();

            // Add the prefixes for the URLs that the listener should respond to
            listener.Prefixes.Add("http://localhost:10010/");
            listener.Prefixes.Add("https://localhost:10011/");

            // Start listening for incoming requests
            listener.Start();

            Console.WriteLine("Listening for incoming requests...");

            while (true)
            {
                // Wait for a new request to arrive
                HttpListenerContext context = await listener.GetContextAsync();

                // Get the request object and read the request data
                HttpListenerRequest request = context.Request;
                string requestData = new StreamReader(request.InputStream).ReadToEnd();
                //Console.WriteLine("received");
                if (request.Url.LocalPath == "/connect")
                {
                    Console.WriteLine("Connect to api");
                    // Return a 200 OK response
                    
                    if(window.Connect()==0)
                    {
                        HttpListenerResponse response = context.Response;
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        response.ContentType = "text/html";
                        string responseString = "<html><body><h1>connection success</h1></body></html>";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
                        response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                        response.OutputStream.Close();
                    }
                    else
                    {
                        HttpListenerResponse response = context.Response;
                        response.StatusCode = 200;
                        response.StatusDescription = "OK";
                        response.OutputStream.Close();
                    }
                }
                // Check if the request method is POST and the URL is /login
                if (request.HttpMethod == "POST" && request.Url.LocalPath == "/login")
                {
                    // Parse the request data into a dictionary
                    AuthPair authPair = ParseQueryString(requestData);

                    // Check if the request data contains an "id" and "password" parameter
                    if (authPair != null)
                    {
                        string id = authPair.id;
                        string password = authPair.password;
                        // Authenticate the user credentials against the database
                        if (AuthenticateUser(id, password))
                        {
                            Console.WriteLine("User authenticated successfully: {0}", id);

                            // Generate a new session ID
                            string sessionId = GenerateSessionId();

                            // Store the session data in memory
                            sessionData[sessionId] = id;

                            // Set the session ID as a session cookie in the response
                            HttpListenerResponse response = context.Response;
                            Cookie sessionCookie = new Cookie(sessionCookieName, sessionId);
                            sessionCookie.HttpOnly = true;
                            sessionCookie.Path = "/";
                            response.SetCookie(sessionCookie);

                            // Return a 200 OK response
                            response.StatusCode = 200;
                            response.StatusDescription = "OK";
                            response.OutputStream.Close();
                        }
                        else
                        {
                            Console.WriteLine("User authentication failed: {0}", id);

                            // Return a 401 Unauthorized response
                            HttpListenerResponse response = context.Response;
                            response.StatusCode = 401;
                            response.StatusDescription = "Unauthorized";
                            response.OutputStream.Close();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid login request data: {0}", requestData);

                        // Return a 400 Bad Request response
                        HttpListenerResponse response = context.Response;
                        response.StatusCode = 400;
                        response.StatusDescription = "Bad Request";
                        response.OutputStream.Close();
                    }
                }
                else
                {
                    // Check if the request contains a session cookie
                    if (request.Cookies[sessionCookieName] != null)
                    {
                        string sessionId = request.Cookies[sessionCookieName].Value;
                        string id;

                        // Retrieve the user ID from the session data
                        if (sessionData.TryGetValue(sessionId, out id))
                        {
                            Console.WriteLine("User session found: {0}", id);

                            // Process the request and generate a response
                            string responseString = "Hello, " + id + "!";
                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                        }
                    }
                }
            }
        }
    }
}
