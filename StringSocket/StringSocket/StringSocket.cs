// Written by Joe Zachary for CS 3500, November 2012
// Revised by Joe Zachary April 2016
// Revised extensively by Joe Zachary April 2017

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CustomNetworking
{

    /// <summary> 
    /// A StringSocket is a wrapper around a Socket.  It provides methods that
    /// asynchronously read lines of text (strings terminated by newlines) and 
    /// write strings. (As opposed to Sockets, which read and write raw bytes.)  
    ///
    /// StringSockets are thread safe.  This means that two or more threads may
    /// invoke methods on a shared StringSocket without restriction.  The
    /// StringSocket takes care of the synchronization.
    /// 
    /// Each StringSocket contains a Socket object that is provided by the client.  
    /// A StringSocket will work properly only if the client refrains from calling
    /// the contained Socket's read and write methods.
    /// 
    /// We can write a string to a StringSocket ss by doing
    /// 
    ///    ss.BeginSend("Hello world", callback, payload);
    ///    
    /// where callback is a SendCallback (see below) and payload is an arbitrary object.
    /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
    /// successfully written the string to the underlying Socket, or failed in the 
    /// attempt, it invokes the callback.  The parameter to the callback is the payload.  
    /// 
    /// We can read a string from a StringSocket ss by doing
    /// 
    ///     ss.BeginReceive(callback, payload)
    ///     
    /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
    /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
    /// string of text terminated by a newline character from the underlying Socket, or
    /// failed in the attempt, it invokes the callback.  The parameters to the callback are
    /// a string and the payload.  The string is the requested string (with the newline removed).
    /// </summary>

    public class StringSocket : IDisposable
    {
        /// <summary>
        /// The type of delegate that is called when a StringSocket send has completed.
        /// </summary>
        public delegate void SendCallback(bool wasSent, object payload);

        /// <summary>
        /// The type of delegate that is called when a receive has completed.
        /// </summary>
        public delegate void ReceiveCallback(String s, object payload);

        // Data structure for messages that'll be sent
        private class Message
        {
            public Message(string text, StringSocket.SendCallback callback, object payload)
            {
                Text = text;
                Callback = callback;
                Payload = payload;
            }
            /// <summary>
            /// Constructs a new message object with the given parameters
            /// </summary>
            /// <param name="callback"></param>
            /// <param name="payload"></param>
            /// <param name="length"></param>
            public Message(StringSocket.ReceiveCallback callback, object payload, int length)
            {
                RecCallback = callback;
                Payload = payload;
                Length = length;
            }



            /// <summary>
            /// Property to setup string that is either received or needing to be sent
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Property to setup SendCallback
            /// </summary>
            public SendCallback Callback { get; set; }

            /// <summary>
            /// Property to setup ReceiveCallback
            /// </summary>
            public StringSocket.ReceiveCallback RecCallback { get; set; }
            /// <summary>
            /// Property to setup Payload
            /// </summary>
            public object Payload { get; set; }

            // for receive
            public int Length
            {
                get;
                set;
            }
        }

        // Underlying socket
        private Socket socket;

        // Encoding used for sending and receiving
        private Encoding encoding;

        private string textReceivedSoFar;

        private byte[] sendingBytes;
        private byte[] pendingBytes;
        private char[] receiveChars;
        private int amountToSend;

        Queue<Message> messagesToSend;
        Queue<Message> messagesReceived;
        private LinkedList<string> receivedLines;
        private Boolean isReceiving = true;

        // For syncing
        private readonly object lockSend = new object();
        private readonly object lockReceive = new object();

        /// <summary>
        /// Creates a StringSocket from a regular Socket, which should already be connected.  
        /// The read and write methods of the regular Socket must not be called after the
        /// StringSocket is created.  Otherwise, the StringSocket will not behave properly.  
        /// The encoding to use to convert between raw bytes and strings is also provided.
        /// </summary>
        internal StringSocket(Socket s, Encoding e)
        {
            socket = s;
            encoding = e;
            textReceivedSoFar = "";
            messagesToSend = new Queue<Message>();
            receivedLines = new LinkedList<string>();
            pendingBytes = new byte[1024];
            receiveChars = new char[1024];
            messagesReceived = new Queue<Message>();
        }

        /// <summary>
        /// Shuts down this StringSocket.
        /// </summary>
        public void Shutdown(SocketShutdown mode)
        {
            socket.Shutdown(mode);
        }

        /// <summary>
        /// Closes this StringSocket.
        /// </summary>
        public void Close()
        {
            socket.Close();
        }

        /// <summary>
        /// We can write a string to a StringSocket ss by doing
        /// 
        ///    ss.BeginSend("Hello world", callback, payload);
        ///    
        /// where callback is a SendCallback (see below) and payload is an arbitrary object.
        /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
        /// successfully written the string to the underlying Socket it invokes the callback.  
        /// The parameters to the callback are true and the payload.
        /// 
        /// If it is impossible to send because the underlying Socket has closed, the callback 
        /// is invoked with false and the payload as parameters.
        ///
        /// This method is non-blocking.  This means that it does not wait until the string
        /// has been sent before returning.  Instead, it arranges for the string to be sent
        /// and then returns.  When the send is completed (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginSend
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginSend must take care of synchronization instead.  On a given StringSocket, each
        /// string arriving via a BeginSend method call must be sent (in its entirety) before
        /// a later arriving string can be sent.
        /// </summary>
        public void BeginSend(String s, SendCallback callback, object payload)
        {
            lock (lockSend)
            {
                messagesToSend.Enqueue(new Message(s, callback, payload));

                if (messagesToSend.Count == 1)
                {
                    SendMessage();
                }
            }
        }


        /// <summary>
        /// Sends the Message. Calls the callback Message Sent when message is sent.
        /// </summary>
        public void SendMessage()
        {

            if (messagesToSend.Count > 0)
            {
                sendingBytes = encoding.GetBytes(messagesToSend.First().Text);
                socket.BeginSend(sendingBytes, amountToSend = 0, sendingBytes.Length, SocketFlags.None, MessageSent, null);
            }
        }

        /// <summary>
        /// Checks whether whole string was sent. If so calls callback on message, if not finished sending the message.
        /// </summary>
        /// <param name="ar"></param>
        public void MessageSent(IAsyncResult ar)
        {
            int numOfBytes = socket.EndSend(ar);
            amountToSend += numOfBytes;


            if (numOfBytes == 0)
            {
                lock (lockSend)
                {
                    while (messagesToSend.Count > 0)
                    {
                        Message req = messagesToSend.Dequeue();
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            req.Callback(false, req.Payload);
                        });
                    }
                }
            }
            else if (amountToSend == sendingBytes.Length)
            {
                lock (lockSend)
                {
                    Message req2 = messagesToSend.Dequeue();
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        req2.Callback(true, req2.Payload);
                    });
                    SendMessage();
                }
            }
            else
            {
                socket.BeginSend(sendingBytes, amountToSend, sendingBytes.Length - amountToSend, SocketFlags.None, MessageSent, null);
            }
        }


        /// <summary>
        /// We can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload)
        ///     
        /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
        /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
        /// string of text terminated by a newline character from the underlying Socket, it 
        /// invokes the callback.  The parameters to the callback are a string and the payload.  
        /// The string is the requested string (with the newline removed).
        /// 
        /// Alternatively, we can read a string from the StringSocket by doing
        /// 
        ///     ss.BeginReceive(callback, payload, length)
        ///     
        /// If length is negative or zero, this behaves identically to the first case.  If length
        /// is positive, then it reads and decodes length bytes from the underlying Socket, yielding
        /// a string s.  The parameters to the callback are s and the payload
        ///
        /// In either case, if there are insufficient bytes to service a request because the underlying
        /// Socket has closed, the callback is invoked with null and the payload.
        /// 
        /// This method is non-blocking.  This means that it does not wait until a line of text
        /// has been received before returning.  Instead, it arranges for a line to be received
        /// and then returns.  When the line is actually received (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginReceive
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginReceive must take care of synchronization instead.  On a given StringSocket, each
        /// arriving line of text must be passed to callbacks in the order in which the corresponding
        /// BeginReceive call arrived.
        /// 
        /// Note that it is possible for there to be incoming bytes arriving at the underlying Socket
        /// even when there are no pending callbacks.  StringSocket implementations should refrain
        /// from buffering an unbounded number of incoming bytes beyond what is required to service
        /// the pending callbacks.
        /// </summary>
        /// 

        /// If length is negative or zero, this behaves identically to the first case.  If length
        /// is positive, then it reads and decodes length bytes from the underlying Socket, yielding
        /// a string s.  The parameters to the callback are s and the payload
        public void BeginReceive(ReceiveCallback callback, object payload, int length = 0)
        {
            lock (lockReceive)
            {
                messagesReceived.Enqueue(new Message(callback, payload, length));

                if (messagesReceived.Count == 1)
                {
                    MessageReceived();
                }
            }

        }

        //Helper method to send all received callbacks and payloads
        private void MessageReceived()
        {
            lock (lockReceive)
            {
                while (messagesReceived.Count() > 0)
                {
                    Message receiveRequest = messagesReceived.Peek();
                    if (receiveRequest.Length <= 0)
                    {
                        if (receivedLines.Count <= 0)
                        {
                            break;
                        }
                        string line = receivedLines.First();
                        receivedLines.RemoveFirst();
                        Message message1 = messagesReceived.Dequeue();
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            message1.RecCallback(line, message1.Payload);
                        });
                    }
                    else
                    {
                        receiveRequest = messagesReceived.Peek();
                        string relay = convertToAbstract(receiveRequest.Length);
                        if (relay == null)
                        {
                            break;
                        }
                        Message message2 = messagesReceived.Dequeue();
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            message2.RecCallback(relay, message2.Payload);
                        });
                    }
                }
                if (messagesReceived.Count > 0)
                {
                    if (isReceiving)
                    {
                        socket.BeginReceive(pendingBytes, 0, pendingBytes.Length, SocketFlags.None, MessageReceivedCallback, null);
                    }
                    else
                    {
                        while (messagesReceived.Count > 0)
                        {
                            Message message3 = messagesReceived.Dequeue();
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                message3.RecCallback(null, message3.Payload);
                            });
                        }
                    }
                }
            }
        }

        private string convertToAbstract(int length)
        {
            int num = encoding.GetByteCount(textReceivedSoFar);
            foreach (string receivedLine in receivedLines)
            {
                if (num >= length)
                {
                    break;
                }
                num += encoding.GetByteCount(receivedLine) + encoding.GetByteCount("\n");
            }
            if (num < length)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            int counter = 0;
            while (receivedLines.Count > 0)
            {
                string text = receivedLines.First();
                int byteCount = encoding.GetByteCount(text) + encoding.GetByteCount("\n");
                if (counter + byteCount > length)
                {
                    break;
                }
                counter += byteCount;
                stringBuilder.Append(text).Append("\n");
            }
            if (receivedLines.Count > 0)
            {
                string text2 = receivedLines.First();
                receivedLines.RemoveFirst();
                int split1 = Split(text2, length - counter);
                stringBuilder.Append(text2.Substring(0, split1));
                receivedLines.AddFirst(text2.Substring(split1));
            }
            else
            {
                int split2 = Split(textReceivedSoFar, length - counter);
                stringBuilder.Append(textReceivedSoFar.Substring(0, split2));
                textReceivedSoFar = textReceivedSoFar.Substring(split2);
            }
            return stringBuilder.ToString();
        }

        private int Split(string line, int bytes)
        {
            int num = 0;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                int byteCount = encoding.GetByteCount(c.ToString());
                if (bytes - byteCount >= 0)
                {
                    bytes = bytes - byteCount;
                    num++;
                }
                if (bytes == 0)
                {
                    break;
                }
            }
            return num;
        }

        // Called when data has been received
        private void MessageReceivedCallback(IAsyncResult ar)
        {
            int numOfBytes = socket.EndReceive(ar);
            if (numOfBytes == 0)
            {
                isReceiving = false;
                MessageReceived();
            }
            else
            {
                int chars = encoding.GetDecoder().GetChars(pendingBytes, 0, numOfBytes, receiveChars, 0, false);
                textReceivedSoFar += new string(receiveChars, 0, chars);
                int num2 = 0;
                int num3;
                while ((num3 = textReceivedSoFar.IndexOf('\n', num2)) >= 0)
                {
                    receivedLines.AddLast(textReceivedSoFar.Substring(num2, num3 - num2));
                    num2 = num3 + 1;
                }
                textReceivedSoFar = textReceivedSoFar.Substring(num2);
                MessageReceived();
            }

        }

        /// <summary>
        /// Frees resources associated with this StringSocket.
        /// </summary>
        public void Dispose()
        {
            Shutdown(SocketShutdown.Both);
            Close();
        }
    }
}