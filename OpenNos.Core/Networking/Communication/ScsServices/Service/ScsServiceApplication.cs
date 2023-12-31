﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.Scs.Server;
using OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// Implements IScsServiceApplication and provides all functionallity.
    /// </summary>
    public class ScsServiceApplication : IScsServiceApplication, IDisposable
    {
        #region Members

        /// <summary>
        /// Underlying IScsServer object to accept and manage client connections.
        /// </summary>
        private readonly IScsServer _scsServer;

        /// <summary>
        /// All connected clients to service.
        /// Key: Client's unique Id.
        /// Value: Reference to the client.
        /// </summary>
        private readonly ThreadSafeSortedList<long, IScsServiceClient> _serviceClients;

        /// <summary>
        /// User service objects that is used to invoke incoming method invocation requests.
        /// Key: Service interface type's name.
        /// Value: Service object.
        /// </summary>
        private readonly ThreadSafeSortedList<string, ServiceObject> _serviceObjects;

        private bool _disposed;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsServiceApplication object.
        /// </summary>
        /// <param name="scsServer">Underlying IScsServer object to accept and manage client connections</param>
        /// <exception cref="ArgumentNullException">
        /// Throws ArgumentNullException if scsServer argument is null
        /// </exception>
        public ScsServiceApplication(IScsServer scsServer)
        {
            _scsServer = scsServer ?? throw new ArgumentNullException(nameof(scsServer));
            _scsServer.ClientConnected += ScsServer_ClientConnected;
            _scsServer.ClientDisconnected += ScsServer_ClientDisconnected;
            _serviceObjects = new ThreadSafeSortedList<string, ServiceObject>();
            _serviceClients = new ThreadSafeSortedList<long, IScsServiceClient>();
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when a new client connected to the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ClientDisconnected;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a service object to this service application. Only single service object can be
        /// added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">
        /// Service class type. Must be delivered from ScsService and must implement TServiceInterface.
        /// </typeparam>
        /// <param name="service">An instance of TServiceClass.</param>
        /// <exception cref="ArgumentNullException">
        /// Throws ArgumentNullException if service argument is null
        /// </exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        public void AddService<TServiceInterface, TServiceClass>(TServiceClass service) where TServiceInterface : class where TServiceClass : ScsService, TServiceInterface
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type type = typeof(TServiceInterface);
            if (_serviceObjects[type.Name] != null)
            {
                throw new Exception("Service '" + type.Name + "' is already added before.");
            }

            _serviceObjects[type.Name] = new ServiceObject(type, service);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        /// <summary>
        /// Removes a previously added service object from this service application. It removes
        /// object according to interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <returns>True: removed. False: no service object with this interface</returns>
        public bool RemoveService<TServiceInterface>() where TServiceInterface : class => _serviceObjects.Remove(typeof(TServiceInterface).Name);

        /// <summary>
        /// Starts service application.
        /// </summary>
        public void Start() => _scsServer.Start();

        /// <summary>
        /// Stops service application.
        /// </summary>
        public void Stop() => _scsServer.Stop();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serviceClients.Dispose();
                _serviceObjects.Dispose();
            }
        }

        /// <summary>
        /// Sends response to the remote application that invoked a service method.
        /// </summary>
        /// <param name="client">Client that sent invoke message</param>
        /// <param name="requestMessage">Request message</param>
        /// <param name="returnValue">Return value to send</param>
        /// <param name="exception">Exception to send</param>
        /// <param name="parameters">
        /// Parameters possibly modified in the method call by out or ref
        /// </param>
        private static void SendInvokeResponse(IMessenger client, IScsMessage requestMessage, object returnValue, ScsRemoteException exception, object[] parameters = null)
        {
            try
            {
                client.SendMessage(new ScsRemoteInvokeReturnMessage
                {
                    RepliedMessageId = requestMessage.MessageId,
                    ReturnValue = returnValue,
                    RemoteException = exception,
                    Parameters = parameters
                }, 10);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("Invoke response send failed", ex);
            }
        }

        /// <summary>
        /// Handles MessageReceived events of all clients, evaluates each message, finds appropriate
        /// service object and invokes appropriate method.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            // Get RequestReplyMessenger object (sender of event) to get client
            RequestReplyMessenger<IScsServerClient> requestReplyMessenger = (RequestReplyMessenger<IScsServerClient>)sender;

            // Cast message to ScsRemoteInvokeMessage and check it
            ScsRemoteInvokeMessage invokeMessage = e.Message as ScsRemoteInvokeMessage;
            if (invokeMessage == null)
            {
                return;
            }

            try
            {
                // Get client object
                IScsServiceClient client = _serviceClients[requestReplyMessenger.Messenger.ClientId];
                if (client == null)
                {
                    requestReplyMessenger.Messenger.Disconnect();
                    return;
                }

                // Get service object
                ServiceObject serviceObject = _serviceObjects[invokeMessage.ServiceClassName];
                if (serviceObject == null)
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("There is no service with name '" + invokeMessage.ServiceClassName + "'"));
                    return;
                }

                // Invoke method
                try
                {
                    // Set client to service, so user service can get client in service method using
                    // CurrentClient property.
                    object returnValue;
                    serviceObject.Service.CurrentClient = client;
                    try
                    {
                        returnValue = serviceObject.InvokeMethod(invokeMessage.MethodName, invokeMessage.Parameters);
                    }
                    finally
                    {
                        // Set CurrentClient as null since method call completed
                        serviceObject.Service.CurrentClient = null;
                    }

                    // Send method invocation return value to the client
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, returnValue, null, invokeMessage.Parameters);
                }
                catch (TargetInvocationException ex)
                {
                    Exception innerEx = ex.InnerException;
                    if (innerEx != null)
                    {
                        SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteInvocationException(invokeMessage.ServiceClassName, serviceObject.ServiceAttribute?.Version ?? "", invokeMessage.MethodName, innerEx.Message, innerEx));
                    }
                }
                catch (Exception ex)
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteInvocationException(invokeMessage.ServiceClassName, serviceObject.ServiceAttribute?.Version ?? "", invokeMessage.MethodName, ex.Message, ex));
                }
            }
            catch (Exception ex)
            {
                SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("An error occured during remote service method call.", ex));
            }
        }

        /// <summary>
        /// Raises ClientConnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientConnected(IScsServiceClient client)
        {
            EventHandler<ServiceClientEventArgs> handler = ClientConnected;
            handler?.Invoke(this, new ServiceClientEventArgs(client));
        }

        /// <summary>
        /// Raises ClientDisconnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientDisconnected(IScsServiceClient client)
        {
            EventHandler<ServiceClientEventArgs> handler = ClientDisconnected;
            handler?.Invoke(this, new ServiceClientEventArgs(client));
        }

        /// <summary>
        /// Handles ClientConnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            RequestReplyMessenger<IScsServerClient> requestReplyMessenger = new RequestReplyMessenger<IScsServerClient>(e.Client);
            requestReplyMessenger.MessageReceived += Client_MessageReceived;
            requestReplyMessenger.Start();

            IScsServiceClient serviceClient = ScsServiceClientFactory.CreateServiceClient(e.Client, requestReplyMessenger);
            _serviceClients[serviceClient.ClientId] = serviceClient;
            OnClientConnected(serviceClient);
        }

        /// <summary>
        /// Handles ClientDisconnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            IScsServiceClient serviceClient = _serviceClients[e.Client.ClientId];
            if (serviceClient == null)
            {
                return;
            }
            e.Client.Disconnect();
            _serviceClients.Remove(e.Client.ClientId);
            OnClientDisconnected(serviceClient);
        }

        #endregion

        #region Classes

        /// <summary>
        /// Represents a user service object. It is used to invoke methods on a ScsService object.
        /// </summary>
        private sealed class ServiceObject
        {
            #region Members

            /// <summary>
            /// This collection stores a list of all methods of service object.
            /// Key: Method name
            /// Value: Informations about method.
            /// </summary>
            private readonly SortedList<string, MethodInfo> _methods;

            #endregion

            #region Instantiation

            /// <summary>
            /// Creates a new ServiceObject.
            /// </summary>
            /// <param name="serviceInterfaceType">Type of service interface</param>
            /// <param name="service">The service object that is used to invoke methods on</param>
            public ServiceObject(Type serviceInterfaceType, ScsService service)
            {
                Service = service;
                object[] classAttributes = serviceInterfaceType.GetCustomAttributes(typeof(ScsServiceAttribute), true);

                ServiceAttribute = classAttributes.Length > 0
                    ? classAttributes[0] as ScsServiceAttribute
                    : new ScsServiceAttribute { Version = serviceInterfaceType.Assembly.GetName().Version.ToString() };

                _methods = new SortedList<string, MethodInfo>();
                foreach (Type serviceInterface in serviceInterfaceType.GetInterfaces().Union(new[] { serviceInterfaceType }))
                {
                    foreach (MethodInfo methodInfo in serviceInterface.GetMethods())
                    {
                        _methods[methodInfo.Name] = methodInfo;
                    }
                }
            }

            #endregion

            #region Properties

            /// <summary>
            /// The service object that is used to invoke methods on.
            /// </summary>
            public ScsService Service { get; }

            /// <summary>
            /// ScsService attribute of Service object's class.
            /// </summary>
            public ScsServiceAttribute ServiceAttribute { get; }

            #endregion

            #region Methods

            /// <summary>
            /// Invokes a method of Service object.
            /// </summary>
            /// <param name="methodName">Name of the method to invoke</param>
            /// <param name="parameters">Parameters of method</param>
            /// <returns>Return value of method</returns>
            public object InvokeMethod(string methodName, params object[] parameters)
            {
                // Check if there is a method with name methodName
                if (!_methods.ContainsKey(methodName))
                {
                    throw new Exception("There is not a method with name '" + methodName + "' in service class.");
                }

                // Get method
                MethodInfo method = _methods[methodName];

                // Invoke method and return invoke result
                return method.Invoke(Service, parameters);
            }

            #endregion
        }

        #endregion
    }
}