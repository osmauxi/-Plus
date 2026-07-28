using System;

namespace ProjectGame.HotFix.Core.NetworkEvents
{
    //允许标记结构体，不允许重复，不能继承，不能重复使用    
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class NetEventIdAttribute : Attribute
    {
        //只使用这个二字节的ushort作为事件ID
        public ushort Id { get; }

        public NetEventIdAttribute(ushort id)
        {
            Id = id;
        }
    }
}