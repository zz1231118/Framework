﻿using System;
using System.Net.Sockets;

namespace Framework.Net.Sockets
{
    public readonly struct KeepAlive : IEquatable<KeepAlive>
    {
        public static readonly KeepAlive OFF = new KeepAlive(0);
        public static readonly KeepAlive One = new KeepAlive(1000);
        public static readonly KeepAlive Two = new KeepAlive(2000);
        public static readonly KeepAlive Three = new KeepAlive(3000);
        public static readonly KeepAlive Four = new KeepAlive(4000);
        public static readonly KeepAlive Five = new KeepAlive(5000);
        public static readonly KeepAlive Six = new KeepAlive(6000);
        public static readonly KeepAlive Seven = new KeepAlive(7000);
        public static readonly KeepAlive Eight = new KeepAlive(8000);
        public static readonly KeepAlive Nine = new KeepAlive(9000);
        public static readonly KeepAlive Ten = new KeepAlive(10000);

        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;

        public KeepAlive(TimeSpan value)
        {
            _dueTime = value;
            _period = value;
        }

        public KeepAlive(uint milliseconds)
        {
            _dueTime = TimeSpan.FromMilliseconds(milliseconds);
            _period = TimeSpan.FromMilliseconds(milliseconds);
        }

        public KeepAlive(TimeSpan dueTime, TimeSpan period)
        {
            _dueTime = dueTime;
            _period = period;
        }

        public KeepAlive(uint dueTime, uint period)
        {
            _dueTime = TimeSpan.FromMilliseconds(dueTime);
            _period = TimeSpan.FromMilliseconds(period);
        }

        /// <summary>
        /// 多长时间没有活跃就开始心跳包
        /// </summary>
        public TimeSpan DueTime => _dueTime;

        /// <summary>
        /// 心跳包间隔
        /// </summary>
        public TimeSpan Period => _period;

        public static bool operator ==(KeepAlive left, KeepAlive right)
        {
            return left._dueTime == right._dueTime && left._period == right._period;
        }

        public static bool operator !=(KeepAlive left, KeepAlive right)
        {
            return left._dueTime != right._dueTime || left._period != right._period;
        }

        private void GetBytes(byte[] bytes, int offset, int value)
        {
            bytes[offset + 0] = (byte)((value >> 0) & 0xFF);
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
            bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
            bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private byte[] GetBytes()
        {
            var @switch = this == OFF ? 0 : 1;
            var byteArray = new byte[sizeof(int) * 3];
            GetBytes(byteArray, sizeof(int) * 0, @switch);
            GetBytes(byteArray, sizeof(int) * 1, (int)DueTime.TotalMilliseconds);
            GetBytes(byteArray, sizeof(int) * 2, (int)Period.TotalMilliseconds);
            return byteArray;
        }

        public void IOControl(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            socket.IOControl(IOControlCode.KeepAliveValues, GetBytes(), null);
        }

        public bool Equals(KeepAlive other)
        {
            return other._dueTime == _dueTime && other._period == _period;
        }

        public override bool Equals(object obj)
        {
            return obj is KeepAlive other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (_dueTime.GetHashCode() << 16) ^ _period.GetHashCode();
        }
    }
}
