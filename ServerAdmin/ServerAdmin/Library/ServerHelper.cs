using Newtonsoft.Json;
using ServerAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
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
                    tcpClient = new TcpClient();

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
                        data = new Byte[1024];

                        // String to store the response ASCII representation.
                        String responseData = String.Empty;
                        String secondData = String.Empty;

                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes); //(**This converts it to string**)

                        if (request.Equals("DeleteSpread") || request.Equals("CreateSpread")) {
                            if (responseData != null && responseData != "")
                                return responseData; 
                            else
                                return "Error: Login Credentials not valid";
                        }
                        else
                        {
                            data = new byte[1024];
                           
                            // Read the second batch of the TcpServer response bytes.
                            Int32 bytes2 = stream.Read(data, 0, data.Length); //(**This receives the data using the byte method**)
                            secondData = System.Text.Encoding.ASCII.GetString(data, 0, bytes2); //(**This converts it to string**)

                            secondData = secondData.TrimStart('\n');
                            secondData = secondData.TrimEnd('\n');

                            tcpClient.Close();
                            if (secondData != null && secondData != "")
                                return secondData;
                            else
                                return "Error: Login Credentials not valid";
                        }
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
