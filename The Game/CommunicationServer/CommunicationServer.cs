﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using CommunicationServerLibrary.Messages;
using System.Net;
using CommunicationServerLibrary.Interfaces;

namespace CommunicationServer
{
    public class CommunicationServer
    {
        private IConnectionListener server;
        private int portNumber;
        private IPAddress ipAddress;

        private void Listen() { }

        private ThreadSafeVariable<CSState> state;
        private ThreadSafeVariable<int> gmId;

        private List<Task> tasks;
        private List<ManagedClient> clients;
        private readonly object clientsLocker;

        private readonly object gmRegisteringLocker;
        private int nextId;

        public CommunicationServer(IConnectionListener listener, string ipAddress, int portNumber)
        {
            this.server = listener ?? throw new ArgumentNullException(nameof(listener));
            this.portNumber = portNumber;
            this.ipAddress = IPAddress.Parse(ipAddress);

            this.state = new ThreadSafeVariable<CSState>();
            this.gmId = new ThreadSafeVariable<int> { Value = -1 };

            this.tasks = new List<Task>();
            this.clients = new List<ManagedClient>();
            this.clientsLocker = new object();

            this.gmRegisteringLocker = new object();
            this.nextId = 0;
        }


        public void Run()
        {
            if (!TryStartListening())
                return;
            while (true)
            {
                try
                {
                    IConnectionClient client = server.Accept();
                    if (client == null) break;
                    ManagedClient managedClient = new ManagedClient(client, nextId++);
                    lock (clientsLocker)
                    {
                        if (state.Value == CSState.Listening || state.Value == CSState.AgentsAccepting)
                        {
                            clients.Add(managedClient);
                            tasks.Add(Task.Run(() => HandleCommunication(managedClient)));
                        }
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine("Some error with TCPListener occured.\n");
                    Kill();
                }
            }
            Console.WriteLine($"Communication Server stopped listening for clients.\n");
            Task.WaitAll(tasks.ToArray());
        }


        private void HandleCommunication(ManagedClient client)
        {
           
        }

        private void HandleGmCommunication(ManagedClient client)
        {
           
        }


        #region MessagesProcessing
        private void ProcessClientMessageDuringListening(Message message, ManagedClient client)
        {
            
        }


        private void ProcessAgentMessageDuringAgentsAccepting(Message message, ManagedClient client)
        {

        }

        private void ProcessAgentMessageDuringGameInProgress(Message message, ManagedClient client)
        {
         
        }

        private void ProcessGmMessageDuringAgentsAccepting(Message message, ManagedClient client)
        {
           
        }

        private void ProcessGmMessageDuringGameInProgress(Message message, ManagedClient client)
        {
            
        }

        private void ProcessGmMessageDuringGameFinished(Message message, ManagedClient client)
        {
            
        }
        #endregion

        private bool TryStartListening()
        {
            try
            {
                server.StartListening(this.ipAddress, this.portNumber);
                state.Value = CSState.Listening;
                return true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.WriteLine("TryStartListening: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryStartListening: "+e.Message);
            }
            return false;
        }

        private bool RegisterGM(ManagedClient client)
        {
            return false;
        }

        private void StartGame()
        {
            state.Value = CSState.GameInProgress;
            server.StopListening();
            Console.WriteLine("Start of the game");

            lock (clientsLocker)
            {
                clients.ForEach(c =>
                {
                    if (!c.IsInGame && c.Id != gmId.Value)
                    {
                        c.Disconnect();
                    }
                });
            }
        }

        private void ForwardMessageFromGM(Message msg)
        {
            int id = 0; // Player ID has to be retrieved from the message

            if (id < 0 || id >= clients.Count || id == gmId.Value)
            {
                Console.WriteLine("CS ForwardMessageFromGM: "+msg.ToString());
                Kill();
            }

            clients[id].SendMessage(msg);
        }

        private void HandlePlayerDisconnected(ManagedClient client)
        {
            client.Disconnect();
        }

        private void HandleGMDisconnected()
        {
            clients[gmId.Value].Disconnect();
            state.Value = CSState.GameFinished;
            server.StopListening();

            lock (clientsLocker)
            {
                clients.ForEach(c =>
                {
                    if (c.IsInGame)
                    {
                        c.IsInGame = false;
                    }
                });
            }
        }

        public void Kill()
        {
            gmId.Value = -1;
            state.Value = CSState.GameFinished;
            server.StopListening();

            lock (clientsLocker)
            {
                clients.ForEach(c =>
                {
                    c.IsInGame = false;
                    c.Disconnect();
                });
            }
        }
    }
}