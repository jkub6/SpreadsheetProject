using Newtonsoft.Json;
using ServerAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerAdmin.Library
{
    public class ServerComm
    {
        /// <summary>
        /// Connects to the server with the ipAddress, username, and password
        /// 
        /// Returns out true for authentication if valid data was returned
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="authenticated"></param>
        public static string ConnectToServer(TcpClient tcpClient, String ipAddress, String username, String password, String request)
        {
            try
            {
                {
                    //Port should always be 2112
                    var result = tcpClient.BeginConnect(ipAddress, 2112, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false);

                    if (!success)
                    {
                        return "Error: Server Connection Timed Out";
                    }
                    //Succesfully connects
                    else
                    {
                        Console.WriteLine("Connected");
                        // use the ipaddress as in the server program
                        UserRequest ur = new UserRequest
                        {
                            username = username,
                            type = request
                        };

                        if (password != null && password != "")
                            ur.password = password;

                        //Sends the JSON Request to the server
                        string message = JsonConvert.SerializeObject(ur) + '\n' + '\n';

                        // Translate the passed message into ASCII and store it as a Byte array.
                        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                        // Get a client stream for reading and writing. 
                        NetworkStream stream = tcpClient.GetStream();

                        // Send the message to the connected TcpServer. 
                        stream.Write(data, 0, data.Length); //(**This is to send data using the byte method**) 
                        Console.WriteLine("Transmitting.....");

                        // Buffer to store the response bytes.
                        data = new Byte[256];

                        // String to store the response ASCII representation.
                        String responseData = String.Empty;

                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)

                        tcpClient.Close();
                        if (responseData != null && responseData != "")
                            return responseData;
                        else
                            return "Error: Login Credentials not valid";
                    }
                }
            }
            catch (Exception e)
            {
                return "Error: Failed to Connect to Server";
            }
        }
    }
}
