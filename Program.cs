using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CruatechTestClientApp
{
    [ServiceContract]
    public interface ITestService
    {
        [OperationContract]
        bool SendEventData(string message);
        [OperationContract]
        int GetTCPPort();
        [OperationContract]
        Stream GetImage();
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            ReadServerMessage("localhost", 11111);
            using (var channelFactory = ConnectService("http://localhost:8848/CruatechTest"))
            {
                SendServerMessage(channelFactory, "Hello Server Thank you for sending me message!");
                GetImage(channelFactory);
            }

            Console.WriteLine("\n Press Enter to close...");
            Console.Read();




        }
        private static void ReadServerMessage(string server, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient(server, port))
                {
                    var byteData = new Byte[256];


                    var responseData = String.Empty;

                    var stream = client.GetStream();

                    var bytes = stream.Read(byteData, 0, byteData.Length);
                    responseData = Encoding.ASCII.GetString(byteData, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);


                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }


           
        }

        private static ChannelFactory<ITestService> ConnectService(string serviceAddress)
        {
            ChannelFactory<ITestService> channelFactory = null;

            try
            {

                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;

                EndpointAddress endpointAddress = new EndpointAddress(serviceAddress);

                
                channelFactory = new ChannelFactory<ITestService>(binding, endpointAddress);

                return channelFactory;


            }
            catch (TimeoutException)
            {
                
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (FaultException)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (CommunicationException)
            {
               
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
            catch (Exception)
            {
                if (channelFactory != null)
                    channelFactory.Abort();

                throw;
            }
           
        }

        private static void SendServerMessage(ChannelFactory<ITestService> channelFactory,string message)
        {
            var channel = channelFactory.CreateChannel();
            channel.SendEventData(message);
            Console.WriteLine(message);
        }

        private static void GetImage(ChannelFactory<ITestService> channelFactory)
        {
            var channel = channelFactory.CreateChannel();
            var imageStream = channel.GetImage();
            SaveStreamAsFile("../../Images", imageStream, "TestImage.PNG");

            Console.WriteLine("image saved in Images folder");

        }
        private static void SaveStreamAsFile(string filePath, Stream inputStream, string fileName)
        {
            var info = new DirectoryInfo(filePath);
            if (!info.Exists)
            {
                info.Create();
            }

            var path = Path.Combine(filePath, fileName);
            using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
            }
        }

    }
}
