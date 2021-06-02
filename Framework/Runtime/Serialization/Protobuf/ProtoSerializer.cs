using System;
using Framework.IO;

namespace Framework.Runtime.Serialization.Protobuf
{
    public static class ProtoSerializer
    {
        private static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

        public static byte[] Serialize(IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            using (var output = recyclableMemoryStreamManager.GetStream(nameof(ProtoSerializer)))
            {
                using (var writer = new ProtoWriter(output))
                {
                    message.WriteTo(writer);
                }
                return output.ToArray();
            }
        }

        public static void Deserialize(IMessage message, byte[] bytes, int offset, int count)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using (var input = recyclableMemoryStreamManager.GetStream(nameof(ProtoSerializer), bytes, offset, count))
            {
                using (var reader = new ProtoReader(input))
                {
                    message.ReadFrom(reader);
                }
            }
        }

        public static T Deserialize<T>(byte[] bytes, int offset, int count)
            where T : IMessage
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            //var message = (T)FormatterServices.GetUninitializedObject(typeof(T));
            var message = Activator.CreateInstance<T>();
            using (var input = recyclableMemoryStreamManager.GetStream(nameof(ProtoSerializer), bytes, offset, count))
            {
                using (var reader = new ProtoReader(input))
                {
                    message.ReadFrom(reader);
                }
            }
            return message;
        }

        public static T Deserialize<T>(byte[] bytes, int offset)
            where T : IMessage
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Deserialize<T>(bytes, offset, bytes.Length - offset);
        }

        public static T Deserialize<T>(byte[] bytes)
            where T : IMessage
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Deserialize<T>(bytes, 0, bytes.Length);
        }

        public static object Deserialize(Type type, byte[] bytes, int offset, int count)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IMessage).IsAssignableFrom(type))
                throw new ArgumentException(nameof(type));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            //var message = (IMessage)FormatterServices.GetUninitializedObject(type);
            var message = (IMessage)Activator.CreateInstance(type);
            using (var input = recyclableMemoryStreamManager.GetStream(nameof(ProtoSerializer), bytes, offset, count))
            {
                using (var reader = new ProtoReader(input))
                {
                    message.ReadFrom(reader);
                }
            }
            return message;
        }

        public static object Deserialize(Type type, byte[] bytes, int offset)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Deserialize(type, bytes, offset, bytes.Length - offset);
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Deserialize(type, bytes, 0, bytes.Length);
        }
    }
}
