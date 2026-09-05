using System;

namespace ProjectGame.HotFix.Gameplay.Map.View
{
    /// <summary>
    /// 网格房间的四方向连接掩码 
    /// 可以通过位运算同时表示多个方向 
    /// </summary>
    //flags标签标明该枚举可以作为位域使用，允许组合多个值
    [Flags]
    public enum ConnectorMask : byte
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3,
        All = North | East | South | West
    }

    /// <summary>
    /// 房间模板允许使用的旋转角度 
    /// 一个Quarter Turn等于顺时针旋转 90 度 
    /// </summary>
    [Flags]
    public enum QuarterTurnMask : byte
    {
        None = 0,
        Turn0 = 1 << 0,
        Turn90 = 1 << 1,
        Turn180 = 1 << 2,
        Turn270 = 1 << 3,
        All = Turn0 | Turn90 | Turn180 | Turn270
    }
    /// <summary>
    /// 负责方向表示、方向组合和房间旋转之间的转换与判断 
    /// </summary>
    public static class ConnectorMaskUtility
    {
        /// <summary>
        /// 把一个单独的ConnectorDirection转换成对应的ConnectorMask 
        /// </summary>
        public static ConnectorMask FromDirection(ConnectorDirection direction)
        {
            return direction switch
            {
                ConnectorDirection.North => ConnectorMask.North,
                ConnectorDirection.East => ConnectorMask.East,
                ConnectorDirection.South => ConnectorMask.South,
                ConnectorDirection.West => ConnectorMask.West,
                _ => ConnectorMask.None
            };
        }

        /// <summary>
        /// /将房间局部连接结构顺时针旋转指定次数 
        /// </summary>
        /// <param name="mask"></param> 旋转前的房间局部连接掩码
        /// <param name="quarterTurns"></param> 顺时针旋转的次数（每次旋转90度）
        /// <returns></returns> 旋转后的房间局部连接掩码
        public static ConnectorMask RotateClockwise(ConnectorMask mask, int quarterTurns)
        {
            //会出现负数旋转的情况，这里两次取模保证旋转次数在0~3之间
            quarterTurns = ((quarterTurns % 4) + 4) % 4;

            for (int i = 0; i < quarterTurns; i++)
            {
                ConnectorMask rotated = ConnectorMask.None;

                if ((mask & ConnectorMask.North) != 0)//存在北方向连接，顺时针转后是东方向连接
                    rotated |= ConnectorMask.East; //在新的掩码上加入旋转后的方向

                if ((mask & ConnectorMask.East) != 0)
                    rotated |= ConnectorMask.South;

                if ((mask & ConnectorMask.South) != 0)
                    rotated |= ConnectorMask.West;

                if ((mask & ConnectorMask.West) != 0)
                    rotated |= ConnectorMask.North;

                mask = rotated;
            }

            return mask;
        }

        /// <summary>
        /// 将世界方向掩码逆向转换为旋转前的房间局部掩码 
        /// </summary>
        //存在原因为房间生成会旋转，但是我们又需要预制体未旋转时的连接方向，以确定门的位置等，所以要反向把世界方向转换为房间局部方向
        //roomRotationIndex是房间旋转的次数，这里是反向转回来，所以是4-roomRotationIndex
        public static ConnectorMask WorldToLocal(ConnectorMask worldMask, int roomRotationIndex)
        {
            return RotateClockwise(worldMask, 4 - roomRotationIndex);
        }

        /// <summary>
        /// 判断available是否包含required中的全部方向 
        /// 用在判定房间门插槽是否支持所有当前需要的连接方向 
        /// </summary>
        public static bool ContainsAll(ConnectorMask available, ConnectorMask required)
        {
            return (available & required) == required;
        }
        /// <summary>
        /// 统计一个连接掩码中包含多少个方向 
        /// </summary>
        public static int Count(ConnectorMask mask)
        {
            int value = (int)mask;
            int count = 0;

            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }

        /// <summary>
        /// 判断一个房间模板是否允许使用指定的旋转角度
        /// </summary>
        /// <param name="mask"></param> 当前房间模板允许的旋转角度掩码
        /// <param name="rotationIndex"></param> 房间请求的旋转角度索引（0表示0度，1表示90度，2表示180度，3表示270度）
        /// <returns></returns>
        public static bool IsRotationAllowed(QuarterTurnMask mask, int rotationIndex)
        {
            //1左移rotationIndex位，得到对应的旋转标志，然后与mask进行按位与运算，如果结果不为0，则表示允许该旋转
            QuarterTurnMask rotationFlag = (QuarterTurnMask)(1 << rotationIndex);
            return (mask & rotationFlag) != 0;
        }
    }
}