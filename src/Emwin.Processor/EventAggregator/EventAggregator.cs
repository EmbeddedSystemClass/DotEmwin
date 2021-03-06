﻿/*
 * Microsoft Public License (MS-PL)
 * Copyright (c) 2015 Jonathan Bradshaw <jonathan@nrgup.net>
 *     
 * This license governs use of the accompanying software. If you use the software, you
 * accept this license. If you do not accept the license, do not use the software.
 *     
 * 1. Definitions
 *     The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
 *     same meaning here as under U.S. copyright law.
 *     A "contribution" is the original software, or any additions or changes to the software.
 *     A "contributor" is any person that distributes its contribution under this license.
 *     "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 *     
 * 2. Grant of Rights
 *     (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 *     (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 *     
 * 3. Conditions and Limitations
 *     (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 *     (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 *     (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 *     (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 *     (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
 */

// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Emwin.Processor.EventAggregator
{
    /// <summary>
    /// Class EventAggregator.
    /// </summary>
    internal class EventAggregator : IEventAggregator
    {
        /// <summary>
        /// The _listeners
        /// </summary>
        private readonly ListenerWrapperCollection _listeners;

        /// <summary>
        /// The _config
        /// </summary>
        private readonly Config _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAggregator"/> class.
        /// </summary>
        public EventAggregator()
            : this(new Config())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAggregator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public EventAggregator(Config config)
        {
            _config = config;
            _listeners = new ListenerWrapperCollection();
        }

        /// <summary>
        /// This will send the message to each IHandle that is subscribed to TMessage.
        /// </summary>
        /// <typeparam name="TMessage">The type of message being sent</typeparam>
        /// <param name="message">The message instance</param>
        /// <param name="marshal">You can optionally override how the message publication action is marshalled</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void SendMessage<TMessage>(TMessage message, Action<Action> marshal = null)
        {
            if (marshal == null)
                marshal = _config.DefaultThreadMarshaler;

            Call<IHandle<TMessage>>(message, marshal);
        }

        /// <summary>
        /// This will create a new default instance of TMessage and send the message to each IHandle that is subscribed to TMessage.
        /// </summary>
        /// <typeparam name="TMessage">The type of message being sent</typeparam>
        /// <param name="marshal">You can optionally override how the message publication action is marshalled</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
             "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public void SendMessage<TMessage>(Action<Action> marshal = null)
            where TMessage : new()
        {
            SendMessage(new TMessage(), marshal);
        }

        /// <summary>
        /// Calls the specified message.
        /// </summary>
        /// <typeparam name="TListener">The type of the t listener.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="marshaller">The marshaller.</param>
        private void Call<TListener>(object message, Action<Action> marshaller)
            where TListener : class
        {
            var listenerCalledCount = 0;
            marshaller(() =>
            {
                foreach (var o in _listeners.Where(o => o.Handles<TListener>() || o.HandlesMessage(message)))
                {
                    bool wasThisOneCalled;
                    o.TryHandle<TListener>(this, message, out wasThisOneCalled);
                    if (wasThisOneCalled)
                        listenerCalledCount++;
                }
            });

            var wasAnyListenerCalled = listenerCalledCount > 0;

            if (!wasAnyListenerCalled)
            {
                _config.OnMessageNotPublishedBecauseZeroListeners(message);
            }
        }

        /// <summary>
        /// Adds the listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns>Emwin.Core.EventAggregator.IEventSubscriptionManager.</returns>
        public IEventSubscriptionManager AddListener(object listener) => AddListener(listener, null);

        /// <summary>
        /// Adds the listener.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Emwin.Core.EventAggregator.IEventSubscriptionManager.</returns>
        public IEventSubscriptionManager AddListener<T>()
        {
            AddListener(Activator.CreateInstance<T>(), true);

            return this;
        }

        /// <summary>
        /// Adds the listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <param name="holdStrongReference">The hold strong reference.</param>
        /// <returns>Emwin.Core.EventAggregator.IEventSubscriptionManager.</returns>
        public IEventSubscriptionManager AddListener(object listener, bool? holdStrongReference)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            var holdRef = _config.HoldReferences;
            if (holdStrongReference.HasValue)
                holdRef = holdStrongReference.Value;
            var supportMessageInheritance = _config.SupportMessageInheritance;
            _listeners.AddListener(listener, holdRef, supportMessageInheritance);

            return this;
        }

        /// <summary>
        /// Adds the listener.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listener">The listener.</param>
        /// <param name="holdStrongReference">The hold strong reference.</param>
        /// <returns>Emwin.Core.EventAggregator.IEventSubscriptionManager.</returns>
        public IEventSubscriptionManager AddListener<T>(IHandle<T> listener, bool? holdStrongReference)
        {
            AddListener((object)listener, holdStrongReference);

            return this;
        }

        /// <summary>
        /// Removes the listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns>Emwin.Core.EventAggregator.IEventSubscriptionManager.</returns>
        public IEventSubscriptionManager RemoveListener(object listener)
        {
            _listeners.RemoveListener(listener);
            return this;
        }

        /// <summary>
        /// Wrapper collection of ListenerWrappers to manage things like 
        /// threadsafe manipulation to the collection, and convenience 
        /// methods to configure the collection
        /// </summary>
        private class ListenerWrapperCollection : IEnumerable<ListenerWrapper>
        {
            private readonly List<ListenerWrapper> _listeners = new List<ListenerWrapper>();
            private readonly object _sync = new object();

            public void RemoveListener(object listener)
            {
                ListenerWrapper listenerWrapper;
                lock (_sync)
                    if (TryGetListenerWrapperByListener(listener, out listenerWrapper))
                        _listeners.Remove(listenerWrapper);
            }

            private void RemoveListenerWrapper(ListenerWrapper listenerWrapper)
            {
                lock (_sync)
                    _listeners.Remove(listenerWrapper);
            }

            public IEnumerator<ListenerWrapper> GetEnumerator()
            {
                lock (_sync)
                    return _listeners.ToList().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private bool ContainsListener(object listener)
            {
                ListenerWrapper listenerWrapper;
                return TryGetListenerWrapperByListener(listener, out listenerWrapper);
            }

            private bool TryGetListenerWrapperByListener(object listener, out ListenerWrapper listenerWrapper)
            {
                lock (_sync)
                    listenerWrapper = _listeners.SingleOrDefault(x => x.ListenerInstance == listener);

                return listenerWrapper != null;
            }

            public void AddListener(object listener, bool holdStrongReference, bool supportMessageInheritance)
            {
                lock (_sync)
                {

                    if (ContainsListener(listener))
                        return;

                    var listenerWrapper = new ListenerWrapper(listener, RemoveListenerWrapper, holdStrongReference, supportMessageInheritance);
                    if (listenerWrapper.Count == 0)
                        throw new ArgumentException(@"IListener<T> is not implemented", nameof(listener));
                    _listeners.Add(listenerWrapper);
                }
            }
        }

        #region IReference

        private interface IReference
        {
            object Target { get; }
        }

        private class WeakReferenceImpl : IReference
        {
            private readonly WeakReference _reference;

            public WeakReferenceImpl(object listener)
            {
                _reference = new WeakReference(listener);
            }

            public object Target => _reference.Target;
        }

        private class StrongReferenceImpl : IReference
        {
            public StrongReferenceImpl(object target)
            {
                Target = target;
            }

            public object Target { get; }
        }

        #endregion

        private class ListenerWrapper
        {
            private const string HandleMethodName = "Handle";
            private readonly Action<ListenerWrapper> _onRemoveCallback;
            private readonly List<HandleMethodWrapper> _handlers = new List<HandleMethodWrapper>();
            private readonly IReference _reference;

            public ListenerWrapper(object listener, Action<ListenerWrapper> onRemoveCallback, bool holdReferences, bool supportMessageInheritance)
            {
                _onRemoveCallback = onRemoveCallback;

                if (holdReferences)
                    _reference = new StrongReferenceImpl(listener);
                else
                    _reference = new WeakReferenceImpl(listener);

                var listenerInterfaces = TypeHelper.GetBaseInterfaceType(listener.GetType())
                                                   .Where(w => TypeHelper.DirectlyClosesGeneric(w, typeof(IHandle<>)));

                foreach (var listenerInterface in listenerInterfaces)
                {
                    var messageType = TypeHelper.GetFirstGenericType(listenerInterface);
                    var handleMethod = TypeHelper.GetMethod(listenerInterface, HandleMethodName);

                    var handler = new HandleMethodWrapper(handleMethod, listenerInterface, messageType, supportMessageInheritance);
                    _handlers.Add(handler);
                }
            }

            public object ListenerInstance => _reference.Target;

            public bool Handles<TListener>() where TListener : class =>
                _handlers.Aggregate(false, (current, handler) => current | handler.Handles<TListener>());

            public bool HandlesMessage(object message) => 
                message != null && _handlers.Aggregate(false, (current, handler) => current | handler.HandlesMessage(message));

            public void TryHandle<TListener>(IEventAggregator eventAggregator, object message, out bool wasHandled)
                where TListener : class
            {
                var target = _reference.Target;
                wasHandled = false;
                if (target == null)
                {
                    _onRemoveCallback(this);
                    return;
                }

                foreach (var handler in _handlers)
                {
                    bool thisOneHandled;
                    handler.TryHandle<TListener>(eventAggregator, target, message, out thisOneHandled);
                    wasHandled |= thisOneHandled;
                }
            }

            public int Count => _handlers.Count;
        }

        private class HandleMethodWrapper
        {
            private readonly Type _listenerInterface;
            private readonly Type _messageType;
            private readonly MethodInfo _handlerMethod;
            private readonly bool _supportMessageInheritance;
            private readonly Dictionary<Type, bool> supportedMessageTypes = new Dictionary<Type, bool>();

            public HandleMethodWrapper(MethodInfo handlerMethod, Type listenerInterface, Type messageType, bool supportMessageInheritance)
            {
                _handlerMethod = handlerMethod;
                _listenerInterface = listenerInterface;
                _messageType = messageType;
                _supportMessageInheritance = supportMessageInheritance;
                supportedMessageTypes[messageType] = true;
            }

            public bool Handles<TListener>() where TListener : class => _listenerInterface == typeof(TListener);

            public bool HandlesMessage(object message)
            {
                if (message == null)
                {
                    return false;
                }

                bool handled;
                var messageType = message.GetType();
                var previousMessageType = supportedMessageTypes.TryGetValue(messageType, out handled);
                if (!previousMessageType && _supportMessageInheritance)
                {
                    handled = TypeHelper.IsAssignableFrom(_messageType, messageType);
                    supportedMessageTypes[messageType] = handled;
                }
                return handled;
            }

            public void TryHandle<TListener>(IEventAggregator eventAggregator, object target, object message, out bool wasHandled)
                where TListener : class
            {
                wasHandled = false;
                if (target == null)
                {
                    return;
                }

                if (!Handles<TListener>() && !HandlesMessage(message)) return;

                _handlerMethod.Invoke(target, new[] { message, eventAggregator });
                wasHandled = true;
            }
        }

        internal static class TypeHelper
        {
            internal static IEnumerable<Type> GetBaseInterfaceType(Type type)
            {
                if (type == null)
                    return new Type[0];

#if NETFX_CORE
                var interfaces = type.GetTypeInfo().ImplementedInterfaces.ToList();
#else
                var interfaces = type.GetInterfaces().ToList();
#endif

                foreach (var @interface in interfaces.ToArray())
                {
                    interfaces.AddRange(GetBaseInterfaceType(@interface));
                }

#if NETFX_CORE
                if (type.GetTypeInfo().IsInterface)
#else
                if (type.IsInterface)
#endif
                {
                    interfaces.Add(type);
                }

                return interfaces.Distinct();
            }

            internal static bool DirectlyClosesGeneric(Type type, Type openType)
            {
                if (type == null)
                    return false;
#if NETFX_CORE
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == openType)
#else
                if (type.IsGenericType && type.GetGenericTypeDefinition() == openType)
#endif
                {
                    return true;
                }

                return false;
            }

            internal static Type GetFirstGenericType<T>() where T : class => GetFirstGenericType(typeof(T));

            internal static Type GetFirstGenericType(Type type)
            {
#if NETFX_CORE
                var messageType = type.GetTypeInfo().GenericTypeArguments.First();
#else
                var messageType = type.GetGenericArguments().First();
#endif
                return messageType;
            }

            internal static MethodInfo GetMethod(Type type, string methodName)
            {
#if NETFX_CORE
                var typeInfo = type.GetTypeInfo();
                var handleMethod = typeInfo.GetDeclaredMethod(methodName);
#else
                var handleMethod = type.GetMethod(methodName);

#endif
                return handleMethod;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Language", "CSE0003:Use expression-bodied members")]
            internal static bool IsAssignableFrom(Type type, Type specifiedType)
            {
#if NETFX_CORE
                return type.GetTypeInfo().IsAssignableFrom(specifiedType.GetTypeInfo());
#else
                return type.IsAssignableFrom(specifiedType);
#endif
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class Config
        {
            public Action<object> OnMessageNotPublishedBecauseZeroListeners { get; set; } = msg =>
            {
                /* TODO: possibly Trace message?*/
            };

            public Action<Action> DefaultThreadMarshaler { get; set; } = action => action();

            /// <summary>
            /// If true instructs the EventAggregator to hold onto a reference to all listener objects. You will then have to explicitly remove them from the EventAggrator.
            /// If false then a WeakReference is used and the garbage collector can remove the listener when not in scope any longer.
            /// </summary>
            public bool HoldReferences { get; set; }

            /// <summary>
            /// If true then EventAggregator will support registering listeners for base messages. 
            /// If false then EventAggregator will only match the message type to the listener.
            /// </summary>
            public bool SupportMessageInheritance { get; set; }
        }
    }


}

// ReSharper enable InconsistentNaming