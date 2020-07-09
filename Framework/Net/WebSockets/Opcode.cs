namespace Framework.Net.WebSockets
{
    public static class Opcode
    {
        public const sbyte Empty = -1;

        public const sbyte Continuation = 0;

        public const sbyte Text = 1;

        public const sbyte Binary = 2;

        public const sbyte Close = 8;

        public const sbyte Ping = 9;

        public const sbyte Pong = 10;
    }
}
