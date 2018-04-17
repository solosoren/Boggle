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
            public Message(StringSocket.ReceiveCallback callback, object payload)
            {
                RecCallback = callback;
                Payload = payload;
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
        }

        // Underlying socket
        private Socket socket;

        // Encoding used for sending and receiving
        private Encoding encoding;

        private string textToSend;
        private string textReceivedSoFar;

        private Message sendingMessage;
        private byte[] pendingBytes = new byte[0];
        private int pendingIndex = 0;

        Queue<Message> messagesToSend;
        Queue<Message> messagesReceived;

        private Boolean isSending;
        private Boolean isReceiving;

        // For syncing
        private readonly object lockSend = new object();
        private readonly object lockReceive = new object();

        // Received message but not dealt with yet.
        private Message messageNotGotten;

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
            textToSend = "";
            textReceivedSoFar = "";
            messagesToSend = new Queue<Message>();
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

                if (!isSending)
                {
                    isSending = true;
                    SendMessage();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void SendMessage()
        {

            if (pendingIndex < pendingBytes.Length)
            {
                try
                {
                    socket.BeginSend(pendingBytes, pendingIndex, pendingBytes.Length - pendingIndex,
                                     SocketFlags.None, MessageSent, null);
                }
                catch (ObjectDisposedException) { }
            }
            else if (messagesToSend.Count > 0)
            {
                {
                    sendingMessage = messagesToSend.Dequeue();
                    pendingBytes = encoding.GetBytes(sendingMessage.Text);
                    pendingIndex = 0;
                    try
                    {
                        socket.BeginSend(pendingBytes, 0, pendingBytes.Length,
                                         SocketFlags.None, MessageSent, null);
                    }
                    catch (ObjectDisposedException) { }
                }
            }
            else
            {
                isSending = false;
                
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        public void MessageSent(IAsyncResult ar)
        {
            int numOfBytes = socket.EndSend(ar);

            lock (lockSend)
            {
                if (numOfBytes == 0)
                {
                    if (pendingIndex < pendingBytes.Length)
                    {
                        Thread thread  = new Thread(() => sendingMessage.Callback(false, sendingMessage.Payload));
                        thread.Start();
                    }
                    socket.Close();
                }
                else if (numOfBytes == pendingBytes.Length && messagesToSend.Count == 0)
                {
                    Thread thread = new Thread(() => sendingMessage.Callback(true, sendingMessage.Payload));
                    thread.Start();
                }
                else
                {
                    pendingIndex += numOfBytes;
                    SendMessage();
                }
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
                //if (length <= 0)
                //{
                messagesReceived.Enqueue(new Message(callback, payload));

                if (!isReceiving)
                {
                    isReceiving = true;
                    try
                    {
                        MessageReceived();
                    }
                    catch (Exception e)
                    {
                        messageNotGotten.RecCallback(null, messageNotGotten.Payload);
                    }
                }
                //}
                //else
                //{

                //}

            }

        }

        //Helper method to send all received callbacks and payloads
        private void MessageReceived()
        {
            int index;
            // Check data for new line chars if have more messages
            if (messagesReceived.Count > 0)
            {

                if ((index = textReceivedSoFar.IndexOf('\n')) >= 0)
                {
                    textToSend = textReceivedSoFar.Substring(0, index);
                    if (textToSend.EndsWith("\r"))
                    {
                        textToSend = textToSend.Substring(0, index - 1);

                    }
                    textReceivedSoFar = textReceivedSoFar.Substring(index + 1);
                    messageNotGotten = messagesReceived.Dequeue();

                    messageNotGotten.RecCallback(null, messageNotGotten.Payload);
                    Thread thread = new Thread(() => messageNotGotten.RecCallback(textToSend, messageNotGotten.Payload));
                    thread.Start();

                }
                byte[] buffer = new byte[1];
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, MessageReceivedCallback, buffer);
            }
            else
            {
                isReceiving = false;
            }
        }


        // Called when data has been received
        private void MessageReceivedCallback(IAsyncResult ar)
        {

            int numOfBytes = socket.EndReceive(ar);

            if (numOfBytes == 0)
            {
                // Nothing to do
                socket.Close();
            }
            else
            {
                byte[] buffer = (byte[])(ar.AsyncState);

                textReceivedSoFar += encoding.GetString(buffer, 0, numOfBytes);

                try
                {
                    MessageReceived();
                }
                catch (Exception e)
                {
                    messageNotGotten.RecCallback(null, messageNotGotten.Payload);
                }

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