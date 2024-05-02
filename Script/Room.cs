using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Room
{
   public enum Type{
       NOT_ASSIGNED, MONSTER, TREASURE, CAMPFIRE, SHOP, BOSS
   }

   //房间的类型
   public Type type;
   //房间所处棋盘的行数
   public int row;
   //房间所属的棋盘列数
   public int column;
   //房间当前2d位置
   public Vector2 position;
   //与房间相连的其他房间
   public List<Room> nextRooms;
   //是否已被选择
   public bool selected;

   public Room() {
       nextRooms = new List<Room>();
   }

   public override string ToString() {
       return column + $"( {type.ToString().ToCharArray()[0]} )";
   }
}
